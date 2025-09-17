using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TileGameEngine.Presentation.Models;

namespace TileGameEngine.Presentation.Server;

public class UdpApiServer
{
    private readonly UdpClient _udpClient;
    private readonly int _port;
    private readonly ILogger<UdpApiServer> _logger;
    private bool _isRunning;
    private readonly Dictionary<string, Func<UdpApiMessage, Task<object>>> _endpointHandlers = new();

    public UdpApiServer(int port, ILogger<UdpApiServer> logger)
    {
        _port = port;
        _logger = logger;
        _udpClient = new UdpClient(_port);
        _isRunning = false;
        _logger.LogInformation($"UDP API Server initialized on port {_port}");
    }

    public void RegisterEndpoint(string endpoint, string method, Func<UdpApiMessage, Task<object>> handler)
    {
        var key = $"{method.ToUpper()} {endpoint.ToLower()}";
        _endpointHandlers[key] = handler;
        _logger.LogInformation($"Registered endpoint: {key}");
    }

    public async Task StartAsync()
    {
        _isRunning = true;
        _logger.LogInformation("UDP API Server started. Waiting for messages...");

        while (_isRunning)
        {
            try
            {
                var result = await _udpClient.ReceiveAsync();
                var remoteIpEndPoint = result.RemoteEndPoint;
                var receivedMessage = Encoding.UTF8.GetString(result.Buffer);

                _logger.LogDebug($"Received from {remoteIpEndPoint}: {receivedMessage}");

                UdpApiMessage requestMessage;
                try
                {
                    var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    requestMessage = JsonSerializer.Deserialize<UdpApiMessage>(receivedMessage, jsonOptions) ??
                                     new UdpApiMessage();
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Invalid JSON format received from {RemoteEndPoint}", remoteIpEndPoint);
                    await SendErrorResponse(remoteIpEndPoint, null, $"Invalid JSON format: {ex.Message}");
                    continue;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deserializing request from {RemoteEndPoint}", remoteIpEndPoint);
                    await SendErrorResponse(remoteIpEndPoint, null, $"Error parsing request: {ex.Message}");
                    continue;
                }
                
                _ = ProcessRequest(requestMessage, remoteIpEndPoint);
            }
            catch (SocketException ex) when (ex.ErrorCode == 10004)
            {
                _logger.LogInformation("Receive operation was cancelled, server is shutting down.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Server error during message reception.");
            }
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _udpClient.Close();
        _udpClient.Dispose();
        _logger.LogInformation("UDP API Server stopped.");
    }

    private async Task ProcessRequest(UdpApiMessage requestMessage, IPEndPoint remoteIpEndPoint)
    {
        var responseMessage = new UdpApiMessage
        {
            RequestId = requestMessage.RequestId
        };

        var endpointKey = $"{requestMessage.Method?.ToUpper()} {requestMessage.Endpoint?.ToLower()}";

        if (_endpointHandlers.TryGetValue(endpointKey, out var handler))
        {
            try
            {
                var handlerResult = await handler(requestMessage);
                responseMessage.Payload = handlerResult;
                responseMessage.Status = "success";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request for {EndpointKey} (Request ID: {RequestId})",
                    endpointKey, requestMessage.RequestId);
                responseMessage.Status = "error";
                responseMessage.ErrorMessage = $"Server error processing {endpointKey}: {ex.Message}";
            }
        }
        else
        {
            _logger.LogWarning("No handler found for {EndpointKey} (Request ID: {RequestId})", endpointKey,
                requestMessage.RequestId);
            responseMessage.Status = "error";
            responseMessage.ErrorMessage = $"Endpoint not found: {requestMessage.Endpoint} {requestMessage.Method}";
        }

        await SendResponse(remoteIpEndPoint, responseMessage);
    }

    private async Task SendResponse(IPEndPoint remoteIpEndPoint, UdpApiMessage responseMessage)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            var jsonResponse = JsonSerializer.Serialize(responseMessage, jsonOptions);
            var responseBytes = Encoding.UTF8.GetBytes(jsonResponse);

            await _udpClient.SendAsync(responseBytes, responseBytes.Length, remoteIpEndPoint);
            _logger.LogDebug($"Sent response (ID: {responseMessage.RequestId}) to {remoteIpEndPoint}: {jsonResponse}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending response (ID: {RequestId}) to {RemoteEndPoint}",
                responseMessage.RequestId, remoteIpEndPoint);
        }
    }

    private async Task SendErrorResponse(IPEndPoint remoteIpEndPoint, string? requestId, string errorMessage)
    {
        var errorResponse = new UdpApiMessage
        {
            RequestId = requestId ?? Guid.NewGuid().ToString(),
            Status = "error",
            ErrorMessage = errorMessage
        };
        await SendResponse(remoteIpEndPoint, errorResponse);
    }
}