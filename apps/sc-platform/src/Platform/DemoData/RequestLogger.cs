using Sitecore.Diagnostics;
using Sitecore.Pipelines.HttpRequest;

namespace Platform.DemoData;

public class RequestLogger : HttpRequestProcessor
{
    public override void Process(HttpRequestArgs args)
    {
        Assert.ArgumentNotNull(args, "args");

        Log.Info($"RequestLogger: HELLO SUGCON! url={args.Url.FilePathWithQueryString}", this);
    }
}
