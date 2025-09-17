using TileGameEngine.Presentation.Extensions;
using TileGameEngine.Presentation.Helpers;
using TileGameEngine.Presentation.Server;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddCustomServices();

builder.Services.AddRedis(builder.Configuration);

builder.Services.AddUdpControllers();

builder.Services.AddUdpServer();

var udpServer = app.Services.GetRequiredService<UdpApiServer>();

_ = udpServer.StartAsync(); 

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation($"UDP API Presentation Layer started on port {UdpServerHelper.Port}.");
logger.LogInformation("Press Ctrl+C to stop the application.");

app.MapGet("/", () => "Hello World!");

app.Run();