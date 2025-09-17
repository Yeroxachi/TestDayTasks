using System.Text.Json.Serialization;

namespace TileGameEngine.Presentation.Models;

public record UdpApiMessage
{
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
    public string Endpoint { get; set; }
    public string Method { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Payload { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Status { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorMessage { get; set; }
}