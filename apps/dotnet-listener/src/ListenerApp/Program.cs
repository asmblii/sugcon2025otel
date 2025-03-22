using System.Runtime.Loader;
using Azure.Messaging.ServiceBus;
using ListenerApp;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();

// add healthchecks
builder.Services.AddHealthChecks();

// add opentelemetry
var serviceName = builder.Configuration.GetValue<string>("OtelServiceName") ?? "dotnet-listener-app";
var otlpExporterSection = builder.Configuration.GetSection("OtlpExporter");

// Add diagnostics providers
var tracingProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource(Instrumentation.ActivitySourceName)
    .AddSource("Azure-Messaging-ServiceBus")
    .AddSource("Azure.*")
    .ConfigureResource(cfg => cfg.AddService(serviceName));

var meterProvider = Sdk.CreateMeterProviderBuilder()
    .AddMeter(Instrumentation.MeterName)
    .ConfigureResource(cfg => cfg.AddService(serviceName));

builder.Logging.AddOpenTelemetry(options =>
{
    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
    options.AddOtlpExporter(options => otlpExporterSection.Bind(options));
});

builder.Services.AddOpenTelemetry()
      .ConfigureResource(resource => resource.AddService(serviceName))
      .WithTracing(tracing =>
      {
          if (builder.Environment.IsDevelopment())
          {
              tracing.SetSampler<AlwaysOnSampler>();
          }
          
          tracingProvider
            .AddInstrumentation<Instrumentation>()
            .AddSource(Instrumentation.ActivitySourceName)
            .AddSource("Azure-Messaging-ServiceBus")
            .AddSource("Azure.*")
            .AddHttpClientInstrumentation()
            .SetErrorStatusOnException()
            .AddOtlpExporter(options => otlpExporterSection.Bind(options))
            .AddConsoleExporter()
            ;
      })
      .WithMetrics(metrics => meterProvider
          .AddMeter(Instrumentation.MeterName)
          .AddHttpClientInstrumentation()
          .AddRuntimeInstrumentation()
          .AddOtlpExporter(options => otlpExporterSection.Bind(options)));

tracingProvider.Build();
meterProvider.Build();

builder.Services.AddSingleton<Instrumentation>();
builder.Services.AddTransient(sp =>
{
    var options = new ServiceBusClientOptions
    {
        TransportType = ServiceBusTransportType.AmqpTcp
    };

    var connectionString = builder.Configuration["ServiceBus:ConnectionString"] ?? throw new Exception("Missing service bus connection string");
    var client = new ServiceBusClient(connectionString, options);
    return client;
});
builder.Services.AddTransient(sp =>
{
    var queueName = builder.Configuration["ServiceBus:Queue"] ?? throw new Exception("Missing service bus Queue");
    return sp.GetRequiredService<ServiceBusClient>().CreateSender(queueName);
});
builder.Services.AddSingleton(sp =>
{
    var queueName = builder.Configuration["ServiceBus:Queue"] ?? throw new Exception("Missing service bus Queue");
    return sp.GetRequiredService<ServiceBusClient>().CreateProcessor(queueName);
});
builder.Services.AddSingleton<AppMessageReceiver>();

var app = builder.Build();

var cts = new CancellationTokenSource();
AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
Console.CancelKeyPress += (sender, eventArgs) =>
{
    eventArgs.Cancel = true;
    cts.Cancel();
    Console.WriteLine("Exiting...");
};

Console.WriteLine("Starting...");
var processor = app.Services.GetRequiredService<AppMessageReceiver>();
await processor.Start(cts.Token);
await WhenCancelled(cts.Token).WaitAsync(cts.Token);
await processor.Stop();

Task WhenCancelled(CancellationToken cancellationToken)
{
    Console.WriteLine("WhenCancelled called.");
    var tcs = new TaskCompletionSource<bool>();
    cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
    return tcs.Task;
}
