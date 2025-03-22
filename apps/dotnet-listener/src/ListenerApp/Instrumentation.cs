using System.Diagnostics.Metrics;
using System.Diagnostics;
using OpenTelemetry;

namespace ListenerApp;

public class Instrumentation : IDisposable
{
    internal const string ActivitySourceName = "ListenerApp";
    internal static string? Version => typeof(Instrumentation).Assembly.GetName().Version?.ToString();
    internal const string MeterName = "ListenerApp.MessagesProcessed";
    private readonly Meter _meter;

    public Instrumentation()
    {
        ActivitySource = new ActivitySource(ActivitySourceName, Version);
        _meter = new Meter(MeterName, Version);
        MessagesProcessed = _meter.CreateCounter<long>("messages.processed", description: "The number of messages processed");
    }

    public ActivitySource ActivitySource { get; }

    public Counter<long> MessagesProcessed { get; }

    public void Dispose()
    {
        ActivitySource.Dispose();
        _meter.Dispose();
    }
}