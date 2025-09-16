using MemoryPack;
using StackExchange.Redis;
using TileGameEngine.Domain.Entities;
using TileGameEngine.Domain.Interfaces;

namespace Persistence.Repositories;

public class RedisGameObjectRepository : IGameObjectRepository
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        private readonly string _geoKey;
        private readonly string _objectKeyPrefix;
        
        private readonly double _originLon;
        private readonly double _originLat;
        private readonly double _lonPerTile;
        private readonly double _latPerTile;
        

        public RedisGameObjectRepository(
            ConnectionMultiplexer redis,
            string geoKey = "gameobjects:geo",
            string objectKeyPrefix = "gameobject:",
            double originLon = 0.0,
            double originLat = 0.0,
            double lonPerTile = 0.00001,
            double latPerTile = 0.00001,
            string channelPrefix = "gameobject")
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _db = _redis.GetDatabase();

            _geoKey = geoKey;
            _objectKeyPrefix = objectKeyPrefix;

            _originLon = originLon;
            _originLat = originLat;
            _lonPerTile = lonPerTile;
            _latPerTile = latPerTile;
        }

        private string ObjKey(uint id) => $"{_objectKeyPrefix}{id}";

        /// <summary>Преобразует координату тайла в lon/lat (double,double).</summary>
        private (double lon, double lat) TileToLonLat(double x, double y)
        {
            var lon = _originLon + x * _lonPerTile;
            var lat = _originLat + y * _latPerTile;
            return (lon, lat);
        }

        /// <summary>Сериализация GameObject в byte[] через MemoryPack</summary>
        private static byte[] Serialize(GameObject obj)
            => MemoryPackSerializer.Serialize(obj);

        private static GameObject? Deserialize(RedisValue value)
        {
            if (value.IsNullOrEmpty) return null;
            try
            {
                return MemoryPackSerializer.Deserialize<GameObject>((byte[])value);
            }
            catch
            {
                return null;
            }
        }

        public async Task AddOrUpdateAsync(GameObject obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            
            var key = ObjKey(obj.Id);
            var bytes = Serialize(obj);
            
            await _db.StringSetAsync(key, bytes).ConfigureAwait(false);
            
            var centroidX = obj.Position.X + (obj.Width - 1) / 2.0;
            var centroidY = obj.Position.Y + (obj.Height - 1) / 2.0;

            var (lon, lat) = TileToLonLat(centroidX, centroidY);
            await _db.GeoAddAsync(_geoKey, lon, lat, obj.Id.ToString()).ConfigureAwait(false);
        }

        public async Task<GameObject?> GetByIdAsync(uint id)
        {
            var key = ObjKey(id);
            var val = await _db.StringGetAsync(key).ConfigureAwait(false);
            return Deserialize(val);
        }

        public async Task<bool> RemoveAsync(uint id)
        {
            var key = ObjKey(id);
            
            var removed = await _db.KeyDeleteAsync(key).ConfigureAwait(false);
            
            await _db.SortedSetRemoveAsync(_geoKey, id.ToString()).ConfigureAwait(false);
            return removed;
        }
        
        public async Task<GameObject?> GetByCoordinateAsync(int x, int y)
        {
            var (lon, lat) = TileToLonLat(x, y);
            
            var searchRadiusMeters = Math.Max(10, Math.Max(Math.Abs(_lonPerTile), Math.Abs(_latPerTile)) * 111320); // approx meters per degree * deg-per-tile

            var members = await _db.GeoRadiusAsync(_geoKey, lon, lat, searchRadiusMeters, GeoUnit.Meters).ConfigureAwait(false);

            if (members.Length == 0) return null;

            foreach (var member in members)
            {
                if (!uint.TryParse(member.Member, out var id)) continue;
                var obj = await GetByIdAsync(id).ConfigureAwait(false);
                if (obj == null) continue;
                if (obj.IntersectsWith(new Area(x, y, x, y)))
                    return obj;
            }

            return null;
        }

        public async Task<IEnumerable<GameObject>> GetInAreaAsync(Area area)
        {
            var (lon1, lat1) = TileToLonLat(area.X1, area.Y1);
            var (lon2, lat2) = TileToLonLat(area.X2, area.Y2);
            
            var centerLon = (lon1 + lon2) / 2.0;
            var centerLat = (lat1 + lat2) / 2.0;
            
            var dx = Math.Abs(lon2 - centerLon);
            var dy = Math.Abs(lat2 - centerLat);
            var maxDeg = Math.Max(dx, dy);
            var radiusMeters = maxDeg * 111320 + 1;

            var members = await _db.GeoRadiusAsync(_geoKey, centerLon, centerLat, radiusMeters, GeoUnit.Meters).ConfigureAwait(false);

            var result = new List<GameObject>();
            if (members == null || members.Length == 0) return result;

            foreach (var m in members)
            {
                if (!uint.TryParse(m.Member, out var id)) continue;
                var obj = await GetByIdAsync(id).ConfigureAwait(false);
                if (obj == null) continue;

                if (obj.IntersectsWith(area))
                    result.Add(obj);
            }

            return result;
        }

        public async Task MoveObjectAsync(uint id, TileCoordinate newPosition)
        {
            var obj = await GetByIdAsync(id).ConfigureAwait(false);
            if (obj == null) throw new KeyNotFoundException($"GameObject with id {id} not found.");

            obj.MoveTo(newPosition);
            await AddOrUpdateAsync(obj).ConfigureAwait(false);
        }

        public void Dispose()
        { 
            _redis.Dispose();
        }
    }