using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog for generic host
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddLogging(lb =>
{
    lb.ClearProviders();
    lb.AddSerilog();
});

// TODO: register DbContext and background services to dispatch Outbox messages

var host = builder.Build();
await host.RunAsync();
