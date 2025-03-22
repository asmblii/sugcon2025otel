using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using ListenerApp;
using Microsoft.Azure.Amqp.Framing;

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

    public async Task Start(CancellationToken ct)
    {
        await _processor.StartProcessingAsync(ct);
    }

    public async Task Stop()
    {
        await _processor.StopProcessingAsync();
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
    {
        _logger.LogError(arg.Exception, arg.Exception.Message);
        return Task.CompletedTask;
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs arg)
    {
        var message = arg.Message;

        const string activityName = nameof(ProcessMessageAsync);
        const string diagnosticsPropertyName = "Diagnostic-Id";

        using var source = (message.ApplicationProperties.TryGetValue(diagnosticsPropertyName, out var objectId) && objectId is string diagnosticId1)
                ? _instrumentation.ActivitySource.StartActivity(activityName, ActivityKind.Consumer, diagnosticId1)
                : _instrumentation.ActivitySource.StartActivity(activityName, ActivityKind.Consumer);

        if (source?.IsAllDataRequested == true)
        {
            foreach (var key in message.ApplicationProperties.Keys)
            {
                if (message.ApplicationProperties.TryGetValue(key, out var value))
                {
                    source.SetTag($"message.${value}", value);
                }
            }
        }

        using var activity = source?.Start();

        try
        {
            await ProcessMessageInner(arg.Message, arg.CancellationToken);
            await arg.CompleteMessageAsync(arg.Message, arg.CancellationToken);
            _instrumentation.MessagesProcessed.Add(1);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            _logger.LogError(ex, "Error while processing message: {ErrorMessage}", ex.Message);
            await arg.DeferMessageAsync(arg.Message);
        }
        finally
        {
            activity?.Stop();
        }
    }

    private async Task ProcessMessageInner(ServiceBusReceivedMessage message, CancellationToken ct)
    {

        _logger.LogInformation("Received message: {MessageBody}", message.Body);
        IReadOnlyDictionary<string, object> applicationProperties = message.ApplicationProperties;

        await Task.Delay(500);
    }
}
