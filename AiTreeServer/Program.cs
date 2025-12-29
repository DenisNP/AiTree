using AiTreeServer.Services;
using DeepSeek.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

string deepSeekApiKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY") ?? throw new ArgumentException("DEEPSEEK_API_KEY env not set");
builder.Services.AddDeepSeek(option =>
{
    option.BaseAddress = new Uri("https://api.deepseek.com");
    option.Timeout = TimeSpan.FromSeconds(30);
    option.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + deepSeekApiKey);
});

builder.Services
    .AddSingleton<AiService>()
    .AddSingleton<AliceService>()
    .AddSingleton<BusService>()
    .AddHostedService<PaletteService>();

WebApplication app = builder.Build();

app.MapControllers();
app.Run();