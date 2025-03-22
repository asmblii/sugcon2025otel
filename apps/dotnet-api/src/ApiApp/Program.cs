using ApiApp.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
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
var serviceName = builder.Configuration.GetValue<string>("OtelServiceName") ?? "dotnet-api-app";
var otlpExporterSection = builder.Configuration.GetSection("OtlpExporter");

builder.Logging.AddOpenTelemetry(options =>
{
    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
    options.AddOtlpExporter(options => otlpExporterSection.Bind(options));
});

var tracingProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource("Azure.*")
    .AddSource("Azure-Messaging-ServiceBus")
    .ConfigureResource(cfg => cfg.AddService(serviceName));


builder.Services.AddOpenTelemetry()
      .ConfigureResource(resource => resource.AddService(serviceName))
      .WithTracing(tracing =>
      {
          if (builder.Environment.IsDevelopment())
          {
              tracing.SetSampler<AlwaysOnSampler>();
          }

          tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("Azure.*")
            .AddSource("Azure-Messaging-ServiceBus")
            .AddRedisInstrumentation(options =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    options.SetVerboseDatabaseStatements = true;
                    options.FlushInterval = TimeSpan.FromSeconds(1);
                }
            })
            .SetErrorStatusOnException()
            .AddOtlpExporter(options => otlpExporterSection.Bind(options));
      })
      .WithMetrics(metrics => metrics
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddRuntimeInstrumentation()
          .AddOtlpExporter(options => otlpExporterSection.Bind(options)));

tracingProvider.Build();

// add services
builder.Services.AddSingleton<SitecoreGraphQLService>();
builder.Services.AddHttpClient<IDadJokeService, DadJokeService>(client =>
{
    var icanhazdadjokesAddress = builder.Configuration["ICanHazDadJokes:Uri"] ?? throw new Exception("Baseaddress for the joke service is not set.");
    var requestingParty = builder.Configuration["ICanHazDadJokes:RequestingParty"] ?? throw new Exception("If using ICanHazDadJoke they kindly request that contact details in form of a website or email is supplied to requests to their free API. See https://icanhazdadjoke.com/api#custom-user-agent for more info.");

    client.BaseAddress = new Uri(icanhazdadjokesAddress);
    client.DefaultRequestHeaders.UserAgent.ParseAdd(requestingParty);
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// map healthchecks
app.MapHealthChecks("/healthz");

// map joke endpoints
app.MapGet("/", () => "hello...").WithName("Index");
app.MapGet("/dadjoke/{id}", async (IDadJokeService dadJokeService, string id) =>
{
    var results = await dadJokeService.GetJokeAsync(id);
    return results;
});

app.MapGet("/randomdadjoke", (IDadJokeService dadJokeService) =>
{
    return dadJokeService.GetRandomJokeAsync();
});

app.MapGet("/randomdadjokes", (IDadJokeService dadJokeService) =>
{
    async IAsyncEnumerable<DadJoke> StreamJokesAsync()
    {
        for (var i = 0; i < 3; i++)
        {
            yield return await dadJokeService.GetRandomJokeAsync();
        }
    }

    return StreamJokesAsync();
});

// map Sitecore endpoints
app.MapGet("/sitecore", ([FromServices] SitecoreGraphQLService sitecore, CancellationToken cancellationToken) => sitecore.GetDataAsync(cancellationToken));

// map slow and throw endpoints
app.MapGet("/slow", async () =>
{
    var value = new Random().Next(400, 20000);

    await Task.Delay(value);

    return new { message = "Hi" };
});

app.MapGet("/throw", async ([FromServices] ILogger<Program> logger) =>
{
    var value = new Random().Next(100, 1000);

    logger.LogInformation("Testing throwing exception, let's wait {DelayMs} ms...", value);

    await Task.Delay(value);

    throw new NotImplementedException("Testing throwing exception.");
});

app.MapGet("/busrequest/{requestIdentifier}", async ([FromServices] ServiceBusSender sender, [FromServices] ILogger<BusRequestInstance> logger, string requestIdentifier) =>
{
    logger.LogInformation("Sending queue message {RequestIdentifier}", requestIdentifier);
    await sender.SendMessageAsync(new ServiceBusMessage($"Request: {requestIdentifier}"));
    return new { message = "Ok" };
});

// ready to run
app.Run();

public class BusRequestInstance { }
