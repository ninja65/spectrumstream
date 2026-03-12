using AmbiMass.SpectrumStream.Communication;
using AmbiMass.SpectrumStream.Contracts.Interfaces;
using AmbiMass.SpectrumStream.Data.Config;
using AmbiMass.SpectrumStream.Services.Services;
using AmbiMass.SpectrumStream.Utils.CmdLine;
using AmbiMass.SpectrumStream.Utils.Json;
using AmbiMass.SpectrumStream.Utils.SysEnv;

CmdLineParser cmdLineParser = new CmdLineParser( args );

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

var urls = builder.Configuration["SignalR:Urls"];
if (!string.IsNullOrWhiteSpace(urls))
{
    builder.WebHost.UseUrls(urls);
}

// Services
builder.Services.Configure<SpectrumStreamSettings>(
    builder.Configuration.GetSection("SpectrumStream"));

builder.Services.AddSingleton<ISysEnvironment, SysEnvironment>();

builder.Services.AddSingleton<IJSONLoader, JSonLoaderImpl>();

builder.Services.AddSingleton<IJSONSaver, JSonSaverImpl>();

builder.Services.AddSingleton<AcquisitionStarterImpl>();

builder.Services.AddSingleton<StreamerImpl>();

builder.Services.AddHostedService<RdaService>();

builder.Services.AddSingleton<ISignalRHub, SignalRHubImpl>();

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

var app = builder.Build();

var hubPath = builder.Configuration["SignalR:HubPath"] ?? "/statusHub";

app.MapGet("/", () => Results.Ok(new
{
    Name = "Console app with SignalR",
    Hub = hubPath,
    Time = DateTimeOffset.Now
}));

app.MapHub<SpectrumStreamHub>(hubPath);

await app.RunAsync();


