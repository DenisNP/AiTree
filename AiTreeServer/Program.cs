using AiTreeServer.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services
    .AddSingleton<AiService>()
    .AddSingleton<AliceService>()
    .AddSingleton<BusService>()
    .AddHostedService<PaletteService>();

WebApplication app = builder.Build();

app.MapControllers();
app.Run();