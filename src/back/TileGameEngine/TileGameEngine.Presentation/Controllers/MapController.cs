using System.Text.Json;
using TileGameEngine.Domain.Entities;
using TileGameEngine.Domain.Enums;
using TileGameEngine.Domain.Interfaces;
using TileGameEngine.Presentation.DTOs.Map;
using TileGameEngine.Presentation.Models;

namespace TileGameEngine.Presentation.Controllers;

public class MapController
{
    private readonly IMapService _mapService;
    private readonly ILogger<MapController> _logger;

    public MapController(IMapService mapService, ILogger<MapController> logger)
    {
        _mapService = mapService;
        _logger = logger;
    }

    public async Task<object> HandleGetTileType(UdpApiMessage request)
    {
        if (request.Payload is JsonElement payloadElement)
        {
            var coordinateDto = payloadElement.Deserialize<TileCoordinateDto>();
            if (coordinateDto == null)
            {
                throw new ArgumentException("Invalid coordinate payload.");
            }

            var coordinate = new TileCoordinate(coordinateDto.X, coordinateDto.Y);
            var tileType = _mapService.GetTileType(coordinate);
            return new { X = coordinate.X, Y = coordinate.Y, TileType = tileType.ToString() };
        }

        throw new ArgumentException("Missing or invalid payload for GetTileType.");
    }

    public async Task<object> HandleSetTileType(UdpApiMessage request)
    {
        if (request.Payload is JsonElement payloadElement)
        {
            var setTileTypeDto = payloadElement.Deserialize<SetTileTypeDto>();
            if (setTileTypeDto == null)
            {
                throw new ArgumentException("Invalid set tile type payload.");
            }

            var coordinate = new TileCoordinate(setTileTypeDto.X, setTileTypeDto.Y);
            if (!Enum.TryParse(setTileTypeDto.TileType, true, out SurfaceType tileType))
            {
                throw new ArgumentException($"Invalid TileType: {setTileTypeDto.TileType}");
            }

            _mapService.SetTileType(coordinate, tileType);
            return new { Success = true, X = coordinate.X, Y = coordinate.Y, NewTileType = tileType.ToString() };
        }

        throw new ArgumentException("Missing or invalid payload for SetTileType.");
    }

    public async Task<object> HandleFillArea(UdpApiMessage request)
    {
        if (request.Payload is JsonElement payloadElement)
        {
            var fillAreaDto = payloadElement.Deserialize<FillAreaDto>();
            if (fillAreaDto == null || fillAreaDto.Width <= 0 || fillAreaDto.Height <= 0)
            {
                throw new ArgumentException("Invalid area dimensions. Width and Height must be positive.");
            }

            var area = new Area(fillAreaDto.X, fillAreaDto.Y, fillAreaDto.Width, fillAreaDto.Height);
            if (!Enum.TryParse(fillAreaDto.TileType, true, out SurfaceType tileType))
            {
                throw new ArgumentException($"Invalid TileType: {fillAreaDto.TileType}");
            }

            _mapService.FillArea(area, tileType);

            return new { Success = true, Message = $"Area ({area}) filled with {tileType.ToString()}." };
        }

        throw new ArgumentException("Missing or invalid payload for FillArea.");
    }
}