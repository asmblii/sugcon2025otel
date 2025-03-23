using Azure.Messaging.ServiceBus;
using ListenerApp;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;

public class AppMessageReceiver
{
    private readonly ServiceBusProcessor _processor;

    private readonly Instrumentation _instrumentation;
    private readonly ILogger<AppMessageReceiver> _logger;

    public AppMessageReceiver(ServiceBusProcessor processor, Instrumentation instrumentation, ILogger<AppMessageReceiver> logger)
    {
        _instrumentation = instrumentation;
        _logger = logger;
        _processor = processor;
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;
    }

    public async Task Start(CancellationToken ct) => await _processor.StartProcessingAsync(ct);

    public async Task Stop() => await _processor.StopProcessingAsync();

    private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
    {
        _logger.LogError(arg.Exception, arg.Exception.Message);

        return Task.CompletedTask;
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs arg)
    {
        var parentContext = Propagators.DefaultTextMapPropagator.Extract(default, arg.Message.ApplicationProperties, (qProps, key) =>
       {
           if (!qProps.TryGetValue(key, out var value) || value?.ToString() is null)
           {
               return Enumerable.Empty<string>();
           }

           return [value.ToString()!];
       });

        using var source = _instrumentation.ActivitySource.StartActivity(nameof(ProcessMessageAsync), ActivityKind.Consumer, parentContext.ActivityContext);

        if (source?.IsAllDataRequested == true)
        {
            foreach (var key in arg.Message.ApplicationProperties.Keys)
            {
                if (arg.Message.ApplicationProperties.TryGetValue(key, out var value))
                {
                    source.SetTag($"message.{key}", value);
                }
            }
        }

        try
        {
            await ProcessMessageInner(arg.Message, arg.CancellationToken);
            await arg.CompleteMessageAsync(arg.Message, arg.CancellationToken);

            _instrumentation.MessagesProcessed.Add(1);
        }
        catch (Exception ex)
        {
            source?.AddException(ex);

            _logger.LogError(ex, "Error while processing message: {ErrorMessage}", ex.Message);

            await arg.DeferMessageAsync(arg.Message);
        }
    }

    private async Task ProcessMessageInner(ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received message: {MessageBody}", message.Body);

        await Task.Delay(100, cancellationToken);
    }
}
