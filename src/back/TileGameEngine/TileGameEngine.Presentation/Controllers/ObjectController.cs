using System.Text.Json;
using TileGameEngine.Domain.Entities;
using TileGameEngine.Domain.Interfaces;
using TileGameEngine.Presentation.DTOs;
using TileGameEngine.Presentation.DTOs.GameObject;
using TileGameEngine.Presentation.DTOs.Map;
using TileGameEngine.Presentation.Models;

namespace TileGameEngine.Presentation.Controllers;

public class ObjectController
{
    private readonly IObjectService _objectService;
    private readonly ILogger<ObjectController> _logger;

    public ObjectController(IObjectService objectService, ILogger<ObjectController> logger)
    {
        _objectService = objectService;
        _logger = logger;
    }

    public async Task<object> HandleAddObject(UdpApiMessage request)
    {
        if (request.Payload is JsonElement payloadElement)
        {
            var gameObjectDto = payloadElement.Deserialize<CreateGameObjectDto>();

            if (gameObjectDto != null)
            {
                var gameObject = new GameObject(new TileCoordinate(gameObjectDto.X, gameObjectDto.Y), gameObjectDto.Width, gameObjectDto.Height);

                try
                {
                    await _objectService.AddObjectAsync(gameObject);
                    return new
                    {
                        Success = true,
                        Message = $"Game object '{gameObject.Id}' added successfully.",
                        ObjectId = gameObject.Id
                    };
                }
                catch (InvalidOperationException ex)
                {
                    return new { Success = false, Message = ex.Message };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding game object");
                    return new { Success = false, Message = "Internal server error while adding object." };
                }
            }
        }

        throw new ArgumentException("Missing or invalid payload for AddObject.");
    }

    public async Task<object> HandleGetGameObject(UdpApiMessage request)
    {
        if (request.Payload is JsonElement payloadElement)
        {
            var objectIdDto = payloadElement.Deserialize<GameObjectIdDto>();
            if (objectIdDto == null || objectIdDto.Id == Guid.Empty)
            {
                throw new ArgumentException("Invalid object ID payload.");
            }

            var gameObject = await _objectService.GetGameObjectAsync(objectIdDto.Id);

            if (!gameObject.HasValue)
            {
                return new { Success = false, Message = $"Game object with ID {objectIdDto.Id} not found." };
            }

            return new
            {
                Success = true,
                GameObject = gameObject.Value
            };
        }

        throw new ArgumentException("Missing or invalid payload for GetGameObject.");
    }

    public async Task<object> HandleDeleteGameObject(UdpApiMessage request)
    {
        if (request.Payload is JsonElement payloadElement)
        {
            var objectIdDto = payloadElement.Deserialize<GameObjectIdDto>();
            if (objectIdDto == null || objectIdDto.Id == Guid.Empty)
            {
                throw new ArgumentException("Invalid object ID payload.");
            }

            await _objectService.DeleteGameObjectAsync(objectIdDto.Id);

            return new
            {
                Success = true,
                Message = $"Game object with ID {objectIdDto.Id} deleted successfully."
            };
        }

        throw new ArgumentException("Missing or invalid payload for DeleteGameObject.");
    }

    public async Task<object> HandleGetGameObjectByCoordinate(UdpApiMessage request)
    {
        if (request.Payload is JsonElement payloadElement)
        {
            var coordinateDto = payloadElement.Deserialize<TileCoordinateDto>();
            if (coordinateDto == null)
            {
                throw new ArgumentException("Invalid coordinate payload.");
            }

            var coordinate = new TileCoordinate(coordinateDto.X, coordinateDto.Y);
            var gameObject = await _objectService.GetGameObjectByCoordinateAsync(coordinate);

            if (!gameObject.HasValue)
            {
                return new
                {
                    Success = false, Message = $"No game object found at coordinates ({coordinate.X}, {coordinate.Y})"
                };
            }

            return new
            {
                Success = true,
                GameObject = gameObject.Value
            };
        }

        throw new ArgumentException("Missing or invalid payload for GetGameObjectByCoordinate.");
    }

    public async Task<object> HandleCheckGameObjectInArea(UdpApiMessage request)
    {
        if (request.Payload is JsonElement payloadElement)
        {
            var checkObjectDto = payloadElement.Deserialize<CheckObjectInAreaDto>();
            if (checkObjectDto == null || checkObjectDto.Id == Guid.Empty)
            {
                throw new ArgumentException("Invalid check object payload.");
            }

            if (checkObjectDto.Area is null)
            {
                throw new ArgumentException("Invalid check object area");
            }

            var area = new Area(checkObjectDto.Area.X1, checkObjectDto.Area.Y1, checkObjectDto.Area.X2, checkObjectDto.Area.Y2);
            var isInArea = await _objectService.CheckGameObjectInAreaAsync(checkObjectDto.Id, area);

            return new
            {
                Success = true,
                IsInArea = isInArea,
                ObjectId = checkObjectDto.Id,
                Area = area
            };
        }

        throw new ArgumentException("Missing or invalid payload for CheckGameObjectInArea.");
    }

    public async Task<object> HandleGetAllGameObjectsInArea(UdpApiMessage request)
    {
        if (request.Payload is JsonElement payloadElement)
        {
            var areaDto = payloadElement.Deserialize<AreaDto>();
            if (areaDto == null || areaDto.X2 <= 0 || areaDto.Y2 <= 0)
            {
                throw new ArgumentException("Invalid area dimensions. Width and Height must be positive.");
            }

            var area = new Area(areaDto.X1, areaDto.Y1, areaDto.X2, areaDto.Y2);
            var gameObjects = await _objectService.GetAllGameObjectsInAreaAsync(area);

            return new
            {
                Success = true,
                Area = area,
                GameObjects = gameObjects,
                Count = gameObjects.Length
            };
        }

        throw new ArgumentException("Missing or invalid payload for GetAllGameObjectsInArea.");
    }
}