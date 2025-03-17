using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Sitecore.Abstractions;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.DependencyInjection;
using Sitecore.Globalization;
using Sitecore.Pipelines;
using Sitecore.Web;
using Sitecore.Web.UI.WebControls.Presentation;
using SitecoreMvcOtel.Pipelines;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SitecoreMvcOtel;

public class ServiceConfigurator : IServicesConfigurator
{
    public void Configure(IServiceCollection services)
    {
        // TEST
        //services.AddSingleton<BaseItemManager>(sp =>
        //{
        //    var helper = sp.GetRequiredService<ProviderHelper<ItemProviderBase, ItemProviderCollection>>();
        //    var pipelineManager = sp.GetRequiredService<BaseCorePipelineManager>();
        //    var baseFactory = sp.GetRequiredService<BaseFactory>();

        //    return new OtelBaseItemManager(new DefaultItemManager(helper, pipelineManager, baseFactory));
        //});

        services.AddSingleton<BaseCorePipelineManager>(sp =>
        {
            var pipelineProvider = sp.GetRequiredService<IPipelineProvider>();

            return new OtelCorePipelineManager(new DefaultCorePipelineManager(pipelineProvider));
        });
        // TEST

        // parse and add configuration
        Enum.TryParse<OpenTelemetry.Exporter.OtlpExportProtocol>(SafeGetSetting("SitecoreMvcOtel.Exporter.Protocol"), true, out var exporterProtocol);

        var configuration = new Configuration(SafeGetSetting("SitecoreMvcOtel.ServiceName"),
            new Exporter(new Uri(SafeGetSetting("SitecoreMvcOtel.Exporter.EndpointUri")), exporterProtocol),
            new Traces(bool.Parse(SafeGetSetting("SitecoreMvcOtel.Traces.UseAlwaysOnSampler"))),
            new Instrumentation(bool.Parse(SafeGetSetting("SitecoreMvcOtel.Instrumentation.UseSqlClientInstrumentation")))
        );

        services.AddSingleton(configuration);

        // build and add resouce builder
        var resourceBuilder = ResourceBuilder.CreateDefault().AddService(
            serviceName: configuration.ServiceName,
            serviceInstanceId: Environment.MachineName.ToLowerInvariant(),
            autoGenerateServiceInstanceId: false)
                .AddProcessRuntimeDetector();

        services.AddSingleton(resourceBuilder);

        // configure logging
        services.AddLogging(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;

                options.AddOtlpExporter(options =>
                {
                    options.Endpoint = configuration.Exporter.EndpointUri;
                    options.Protocol = configuration.Exporter.Protocol;
                });

                options.SetResourceBuilder(resourceBuilder);
            });

            // remove "Sitecore.Diagnostics.SitecoreLoggerProvider" so we can reverse the flow from "mslog -> log4net" to "log4net -> mslog"
            services.Remove(services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(ILoggerProvider)));
        });
    }

    private string SafeGetSetting(string name)
    {
        var node = ConfigReader.GetConfiguration().SelectSingleNode($"sitecore/settings/setting[@name='{name}']");

        if (node == null)
        {
            return null;
        }

        var attribute = node.Attributes["value"];

        if (attribute == null)
        {
            return null;
        }

        return attribute.Value;
    }
}


internal class OtelCorePipelineManager : BaseCorePipelineManager
{
    private readonly BaseCorePipelineManager _original;

    public OtelCorePipelineManager(DefaultCorePipelineManager original)
    {
        _original = original;
    }

    public override void ClearCache()
    {
        _original.ClearCache();
    }

    public override CorePipeline GetPipeline(string pipelineName, string pipelineGroup) => _original.GetPipeline(pipelineName, pipelineGroup);
    public override void Run(string pipelineName, PipelineArgs args) => Run(pipelineName, args, string.Empty);
    public override void Run(string pipelineName, PipelineArgs args, bool failIfNotExists) => Run(pipelineName, args, string.Empty, failIfNotExists);
    public override void Run(string pipelineName, PipelineArgs args, string pipelineDomain) => Run(pipelineName, args, pipelineDomain, failIfNotExists: true);
    
    public override void Run(string pipelineName, PipelineArgs args, string pipelineDomain, bool failIfNotExists)
    {
        using var source = Activity.Current?.Source.StartActivity($"BaseCorePipelineManager ({pipelineName})");

        if (source != null)
        {
            source.AddTag("pipelinemanager.pipelinename", pipelineName);
            source.AddTag("pipelinemanager.pipelinedomain", pipelineDomain);
        }

        _original.Run(pipelineName, args, pipelineDomain, failIfNotExists);
    }
}

//internal class OtelBaseItemManager : BaseItemManager
//{
//    private readonly BaseItemManager _original;

//    public OtelBaseItemManager(BaseItemManager original) => _original = original;

//    public override Item AddFromTemplate(string itemName, ID templateId, Item destination, ID newId) => _original.AddFromTemplate(itemName, templateId, destination, newId);

//    public override Item AddFromTemplate(string itemName, ID templateId, Item destination) => _original.AddFromTemplate(itemName, templateId, destination);

//    public override Item AddVersion(Item item, Sitecore.SecurityModel.SecurityCheck securityCheck) => _original.AddVersion(item, securityCheck);

//    public override Item AddVersion(Item item) => _original.AddVersion(item);

//    [Obsolete]
//    public override bool BlobStreamExists(Guid blobId, Database database) => _original.BlobStreamExists(blobId, database);

//    public override Item CopyItem(Item source, Item destination, bool deep, string copyName, ID copyId) => _original.CopyItem(source, destination, deep, copyName, copyId);

//    public override Item CopyItem(Item source, Item destination, bool deep) => _original.CopyItem(source, destination, deep);
//    public override Item CopyItem(Item source, Item destination, bool deep, string copyName) => _original.CopyItem(source, destination, deep, copyName);

//    public override Item CreateItem(string itemName, Item destination, ID templateId, ID newId, Sitecore.SecurityModel.SecurityCheck securityCheck) => _original.CreateItem(itemName, destination, templateId, newId, securityCheck);

//    public override Item CreateItem(string itemName, Item destination, ID templateId) => _original.CreateItem(itemName, destination, templateId);
//    public override Item CreateItem(string itemName, Item destination, ID templateId, ID newId) => _original.CreateItem(itemName, destination, templateId, newId);
//    public override Item CreateItem(string itemName, Item destination, ID templateId, ID newId, DateTime created) => _original.CreateItem(itemName, destination, templateId, newId, created);
//    public override Item CreateItem(string itemName, Item destination, ID templateId, ID newId, DateTime created, Sitecore.SecurityModel.SecurityCheck securityCheck) => _original.CreateItem(itemName, destination, templateId, newId, created, securityCheck);

//    public override bool DeleteItem(Item item, Sitecore.SecurityModel.SecurityCheck securityCheck) => _original.DeleteItem(item, securityCheck);

//    public override bool DeleteItem(Item item) => _original.DeleteItem(item);

//    [Obsolete]
//    public override Stream GetBlobStream(Guid blobId, Database database) => _original.GetBlobStream(blobId, database);

//    [Obsolete]
//    public override Stream GetBlobStream(Field field) => _original.GetBlobStream(field);

//    public override ChildList GetChildren(Item item, Sitecore.SecurityModel.SecurityCheck securityCheck)
//    {
//        using var source = Activity.Current?.Source.StartActivity("GetChildren");

//        if (source != null)
//        {
//            source.AddTag("itemmanager.itemId", item?.ID);
//            source.AddTag("itemmanager.securityCheck", securityCheck);
//        }

//        return _original.GetChildren(item, securityCheck);
//    }

//    public override ChildList GetChildren(Item item, Sitecore.SecurityModel.SecurityCheck securityCheck, ChildListOptions options)
//    {
//        using var source = Activity.Current?.Source.StartActivity("GetChildren");

//        if (source != null)
//        {
//            source.AddTag("itemmanager.itemId", item?.ID);
//            source.AddTag("itemmanager.securityCheck", securityCheck);
//        }

//        return _original.GetChildren(item, securityCheck, options);
//    }

//    public override ChildList GetChildren(Item item) => GetChildren(item, Sitecore.SecurityModel.SecurityCheck.Enable);

//    public override LanguageCollection GetContentLanguages(Item item)
//    {
//        using var source = Activity.Current?.Source.StartActivity("GetContentLanguages");

//        source?.AddTag("itemmanager.itemId", item?.ID);

//        return _original.GetContentLanguages(item);
//    }

//    public override Item GetItem(ID itemId, Language language, Sitecore.Data.Version version, Database database, Sitecore.SecurityModel.SecurityCheck securityCheck)
//    {
//        using var source = Activity.Current?.Source.StartActivity("GetItem");

//        if (source != null)
//        {
//            source.AddTag("itemmanager.itemId", itemId);
//            source.AddTag("itemmanager.language", language.Name);
//            source.AddTag("itemmanager.version", version.Number);
//            source.AddTag("itemmanager.database", database.Name);
//            source.AddTag("itemmanager.securityCheck", securityCheck);
//        }

//        return _original.GetItem(itemId, language, version, database, securityCheck);
//    }

//    public override Item GetItem(string itemPath, Language language, Sitecore.Data.Version version, Database database, Sitecore.SecurityModel.SecurityCheck securityCheck)
//    {
//        using var source = Activity.Current?.Source.StartActivity("GetItem");

//        if (source != null)
//        {
//            source.AddTag("itemmanager.itemPath", itemPath);
//            source.AddTag("itemmanager.language", language.Name);
//            source.AddTag("itemmanager.version", version.Number);
//            source.AddTag("itemmanager.database", database.Name);
//            source.AddTag("itemmanager.securityCheck", securityCheck);
//        }

//        return _original.GetItem(itemPath, language, version, database, securityCheck);
//    }

//    public override Item GetItem(ID itemId, Language language, Sitecore.Data.Version version, Database database) => GetItem(itemId, language, version, database, Sitecore.SecurityModel.SecurityCheck.Enable);
//    public override Item GetItem(string itemPath, Language language, Sitecore.Data.Version version, Database database) => GetItem(itemPath, language, version, database, Sitecore.SecurityModel.SecurityCheck.Enable);

//    public override Item GetParent(Item item, Sitecore.SecurityModel.SecurityCheck securityCheck)
//    {
//        using var source = Activity.Current?.Source.StartActivity("GetParent");

//        source?.AddTag("itemmanager.itemId", item?.ID);

//        return _original.GetParent(item, securityCheck);
//    }

//    public override Item GetParent(Item item) => GetParent(item, Sitecore.SecurityModel.SecurityCheck.Enable);

//    public override Item GetRootItem(Language language, Sitecore.Data.Version version, Database database, Sitecore.SecurityModel.SecurityCheck securityCheck)
//    {
//        using var source = Activity.Current?.Source.StartActivity("GetRootItem");

//        if (source != null)
//        {
//            source.AddTag("itemmanager.language", language.Name);
//            source.AddTag("itemmanager.version", version.Number);
//            source.AddTag("itemmanager.database", database.Name);
//            source.AddTag("itemmanager.securityCheck", securityCheck);
//        }

//        return _original.GetRootItem(language, version, database, securityCheck);
//    }

//    public override Item GetRootItem(Language language, Sitecore.Data.Version version, Database database) => GetRootItem(language, version, database, Sitecore.SecurityModel.SecurityCheck.Enable);

//    public override VersionCollection GetVersions(Item item, Language language)
//    {
//        using var source = Activity.Current?.Source.StartActivity("GetVersions");

//        if (source != null)
//        {
//            source.AddTag("itemmanager.item", item?.ID);
//            source.AddTag("itemmanager.language", language.Name);
//        }

//        return _original.GetVersions(item, language);
//    }

//    public override VersionCollection GetVersions(Item item) => GetVersions(item, item.Language);

//    public override bool HasChildren(Item item, Sitecore.SecurityModel.SecurityCheck securityCheck)
//    {
//        using var source = Activity.Current?.Source.StartActivity("HasChildren");

//        if (source != null)
//        {
//            source.AddTag("itemmanager.item", item?.ID);
//            source.AddTag("itemmanager.securityCheck", securityCheck);
//        }

//        return _original.HasChildren(item, securityCheck);
//    }

//    public override bool HasChildren(Item item) => HasChildren(item, Sitecore.SecurityModel.SecurityCheck.Enable);
//    public override void Initialize() => _original.Initialize();

//    public override bool MoveItem(Item item, Item destination, Sitecore.SecurityModel.SecurityCheck securityCheck) => _original.MoveItem(item, destination, securityCheck);

//    public override bool MoveItem(Item item, Item destination) => _original.MoveItem(item, destination);

//    [Obsolete]
//    public override bool RemoveBlobStream(Guid blobId, Database database) => _original.RemoveBlobStream(blobId, database);

//    public override bool RemoveData(ID itemId, Language language, bool removeSharedData, Database database) => _original.RemoveData(itemId, language, removeSharedData, database);

//    public override bool RemoveData(ID itemId, Database database) => _original.RemoveData(itemId, database);

//    public override bool RemoveVersion(Item item, Sitecore.SecurityModel.SecurityCheck securityCheck) => _original.RemoveVersion(item, securityCheck);

//    public override bool RemoveVersion(Item item) => _original.RemoveVersion(item);

//    public override void RemoveVersions(Item item, Language language, Sitecore.SecurityModel.SecurityCheck securityCheck) => _original.RemoveVersions(item, language, securityCheck);

//    public override ID ResolvePath(string itemPath, Database database)
//    {
//        using var source = Activity.Current?.Source.StartActivity("ResolvePath");

//        if (source != null)
//        {
//            source.AddTag("itemmanager.itemPath", itemPath);
//            source.AddTag("itemmanager.database", database.Name);
//        }

//        return _original.ResolvePath(itemPath, database);
//    }

//    public override bool SaveItem(Item item) => _original.SaveItem(item);

//    [Obsolete]
//    public override bool SetBlobStream(Stream stream, Guid blobId, Database database) => _original.SetBlobStream(stream, blobId, database);
//}
