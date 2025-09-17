using StackExchange.Redis;
using TileGameEngine.Application.Services;
using TileGameEngine.Domain.Interfaces;
using TileGameEngine.Persistence.Cache;
using TileGameEngine.Presentation.Controllers;
using TileGameEngine.Presentation.Helpers;
using TileGameEngine.Presentation.Server;

namespace TileGameEngine.Presentation.Extensions;

public static class StartupExtensions
{
    public static IServiceCollection AddCustomServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IMapService, MapService>();
        serviceCollection.AddScoped<ICacheService, RedisCacheService>();
        serviceCollection.AddScoped<IGameObjectCache, GameObjectCache>();
        serviceCollection.AddScoped<IObjectService, ObjectService>();
        serviceCollection.AddScoped<IRegionService, RegionService>();
        serviceCollection.AddScoped<IRegionCache, RegionCache>();
        return serviceCollection;
    }

    public static IServiceCollection AddUdpServer(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<UdpApiServer>>();
            var server = new UdpApiServer(UdpServerHelper.Port,logger);
            
            var mapController = sp.GetRequiredService<MapController>();
            server.RegisterEndpoint("/map/gettiletype", "GET", mapController.HandleGetTileType);
            server.RegisterEndpoint("/map/settiletype", "POST", mapController.HandleSetTileType);
            server.RegisterEndpoint("/map/fillinarray", "POST", mapController.HandleFillArea);


            var objectController = sp.GetRequiredService<ObjectController>();
            server.RegisterEndpoint("/objects/add", "POST", objectController.HandleAddObject);
            server.RegisterEndpoint("/objects/get", "GET", objectController.HandleGetGameObject);
            server.RegisterEndpoint("/objects/delete", "POST", objectController.HandleDeleteGameObject);
            server.RegisterEndpoint("/objects/getbycoordinate", "GET", objectController.HandleGetGameObjectByCoordinate);
            server.RegisterEndpoint("/objects/getallinarea", "GET", objectController.HandleGetAllGameObjectsInArea);
            server.RegisterEndpoint("/objects/checkobjectinarea", "GET", objectController.HandleCheckGameObjectInArea);
            
            var regionController = sp.GetRequiredService<RegionController>();
            server.RegisterEndpoint("/regions/generate", "POST", regionController.HandleGenerateRegions);
            server.RegisterEndpoint("/regions/getbycoordinate", "POST", regionController.HandleGetRegionByCoordinate);
            server.RegisterEndpoint("/regions/cheregioninarea", "GET", regionController.HandleCheckTileInRegion);
            server.RegisterEndpoint("/regions/getallinarea", "GET", regionController.HandleGetAllRegionsInArea);
            server.RegisterEndpoint("/regions/getbyid", "GET", regionController.HandleGetAllRegionsInArea);
            
            return server;
        });
        
        return serviceCollection;
    }
    
    public static IServiceCollection AddRedis(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisConnectionString = configuration.GetConnectionString("Redis") 
                                        ?? throw new ArgumentException("Redis connection string is not configured");
            return ConnectionMultiplexer.Connect(redisConnectionString);
        });
        return serviceCollection;
    }

    public static IServiceCollection AddUdpControllers(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<MapController>();
        serviceCollection.AddSingleton<ObjectController>();
        serviceCollection.AddSingleton<RegionController>();
        
        return serviceCollection;
    }
}