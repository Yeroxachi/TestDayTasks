using System.Text.Json;
using TileGameEngine.Domain.Entities;
using TileGameEngine.Domain.Interfaces;
using TileGameEngine.Presentation.DTOs;
using TileGameEngine.Presentation.DTOs.Map;
using TileGameEngine.Presentation.DTOs.Region;
using TileGameEngine.Presentation.Models;

namespace TileGameEngine.Presentation.Controllers;

public record RegionController
{
    private readonly IRegionService _regionService;
    private readonly ILogger<RegionController> _logger;

    public RegionController(IRegionService regionService, ILogger<RegionController> logger)
    {
        _regionService = regionService;
        _logger = logger;
    }

    public async Task<object> HandleGetRegionByCoordinate(UdpApiMessage request)
    {
        if (request.Payload is JsonElement payloadElement)
        {
            var coordinateDto = payloadElement.Deserialize<TileCoordinateDto>();
            if (coordinateDto == null)
            {
                throw new ArgumentException("Invalid coordinate payload.");
            }

            var coordinate = new TileCoordinate(coordinateDto.X, coordinateDto.Y);
            var region = await _regionService.GetRegionIdByCoordinateAsync(coordinate);

            if (region == null)
            {
                return new
                {
                    Success = false, Message = $"No region found at coordinates ({coordinate.X}, {coordinate.Y})"
                };
            }

            return new
            {
                Success = true,
                Region = new
                {
                    Id = region.Value.Id,
                    Name = region.Value.Name,
                    Area = region.Value.Area
                }
            };
        }

        throw new ArgumentException("Missing or invalid payload for GetRegionByCoordinate.");
    }

    public async Task<object> HandleGetRegionById(UdpApiMessage request)
    {
        if (request.Payload is JsonElement payloadElement)
        {
            var regionIdDto = payloadElement.Deserialize<RegionIdDto>();
            if (regionIdDto == null || regionIdDto.Id == Guid.Empty)
            {
                throw new ArgumentException("Invalid region ID payload.");
            }

            var region = await _regionService.GetRegionIdByIdAsync(regionIdDto.Id);

            if (region == null)
            {
                return new { Success = false, Message = $"Region with ID {regionIdDto.Id} not found." };
            }

            return new
            {
                Success = true,
                Region = new
                {
                    Id = region.Value.Id,
                    Name = region.Value.Name,
                    Area = region.Value.Area
                }
            };
        }

        throw new ArgumentException("Missing or invalid payload for GetRegionById.");
    }

    public async Task<object> HandleCheckTileInRegion(UdpApiMessage request)
    {
        if (request.Payload is JsonElement payloadElement)
        {
            var checkTileDto = payloadElement.Deserialize<CheckTileInRegionDto>();
            if (checkTileDto == null || checkTileDto.RegionId == Guid.Empty)
            {
                throw new ArgumentException("Invalid check tile payload.");
            }

            var coordinate = new TileCoordinate(checkTileDto.X, checkTileDto.Y);
            var isInRegion = await _regionService.CheckTileInRegionAsync(checkTileDto.RegionId, coordinate);

            return new
            {
                Success = true,
                IsInRegion = isInRegion,
                RegionId = checkTileDto.RegionId,
                Coordinate = new { X = coordinate.X, Y = coordinate.Y }
            };
        }

        throw new ArgumentException("Missing or invalid payload for CheckTileInRegion.");
    }

    public async Task<object> HandleGetAllRegionsInArea(UdpApiMessage request)
    {
        if (request.Payload is JsonElement payloadElement)
        {
            var areaDto = payloadElement.Deserialize<AreaDto>();
            if (areaDto is not { X2: > 0 } || areaDto.Y2 <= 0)
            {
                throw new ArgumentException("Invalid area dimensions. Width and Height must be positive.");
            }

            var area = new Area(areaDto.X1, areaDto.Y1, areaDto.X2, areaDto.Y2);
            var regions = await _regionService.GetAllRegionsInAreaAsync(area);

            return new
            {
                Success = true,
                Area = area,
                Regions = regions.Select(r => new
                {
                    Id = r.Id,
                    Name = r.Name,
                    Area = r.Area
                }).ToArray(),
                Count = regions.Length
            };
        }

        throw new ArgumentException("Missing or invalid payload for GetAllRegionsInArea.");
    }

    public async Task<object> HandleGenerateRegions(UdpApiMessage request)
    {
        if (request.Payload is JsonElement payloadElement)
        {
            var generateRegionsDto = payloadElement.Deserialize<GenerateRegionsDto>();
            if (generateRegionsDto == null || generateRegionsDto.Names == null || generateRegionsDto.Names.Length == 0)
            {
                throw new ArgumentException("Invalid generate regions payload. Names array cannot be empty.");
            }

            try
            {
                await _regionService.GenerateRegionsAsync(generateRegionsDto.Names);
                return new
                {
                    Success = true, Message = $"Successfully generated {generateRegionsDto.Names.Length} regions."
                };
            }
            catch (NotImplementedException)
            {
                return new { Success = false, Message = "Region generation is not implemented yet." };
            }
        }

        throw new ArgumentException("Missing or invalid payload for GenerateRegions.");
    }
}