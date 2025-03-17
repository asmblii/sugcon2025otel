using Sitecore.Diagnostics;
using Sitecore.Pipelines.HttpRequest;
using System;
using System.Diagnostics;

namespace SitecoreMvcOtel.Pipelines;

public class MagicDragon
{
    private readonly Activity _activity;

    public MagicDragon()
    {
        _activity = new Activity("WOOPASS");
        _activity.Start();
    }

    ~MagicDragon()
    {
        _activity.Stop();
        _activity.Dispose();
    }
}

public class OtelHttpRequestBeginStart : HttpRequestProcessor
{
    private readonly MagicDragon _test;

    public OtelHttpRequestBeginStart(MagicDragon test)
    {
        _test = test;
    }

    public override void Process(HttpRequestArgs args)
    {
        Assert.ArgumentNotNull(args, "args");
    }
}


public class OtelHttpRequestBeginStop : HttpRequestProcessor
{
    public override void Process(HttpRequestArgs args)
    {
        Assert.ArgumentNotNull(args, "args");
    }
}

public class DecorateActivityProcessor : HttpRequestProcessor
{
    public override void Process(HttpRequestArgs args)
    {
        Assert.ArgumentNotNull(args, "args");

        var activity = Activity.Current;

        // see best practices https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/docs/trace/README.md#activity
        if (!activity.IsAllDataRequested)
        {
            return;
        }

        // add database name
        var databaseName = "unknown";

        if (Sitecore.Context.Database != null)
        {
            databaseName = Sitecore.Context.Database.Name.ToLowerInvariant();
        }

        activity.SetTag("sitecore.database.name", databaseName);

        // add site name
        var siteName = "unknown";

        if (Sitecore.Context.Site != null)
        {
            siteName = Sitecore.Context.Site.Name.ToLowerInvariant();
        }

        activity.SetTag("sitecore.site.name", siteName);

        // add language name
        var languageName = "unknown";

        if (Sitecore.Context.Language != null)
        {
            languageName = Sitecore.Context.Language.Name.ToLowerInvariant();
        }

        activity.SetTag("sitecore.language.name", languageName);

        // set display name
        activity.DisplayName = $"{activity.DisplayName} [{siteName}, {databaseName}, {languageName}]";

        // add more context item data
        var item = Sitecore.Context.Item;

        if (item == null)
        {
            return;
        }

        activity.SetTag("sitecore.item.id", item.ID);
        activity.SetTag("sitecore.item.path", item.Paths.FullPath.ToLowerInvariant());
        activity.SetTag("sitecore.item.template.id", item.Template.ID);
        activity.SetTag("sitecore.item.template.path", item.Template.InnerItem.Paths.FullPath.ToLowerInvariant());
    }
}
