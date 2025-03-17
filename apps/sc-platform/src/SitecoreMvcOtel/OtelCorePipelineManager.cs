using Sitecore.Abstractions;
using Sitecore.Pipelines;
using System.Diagnostics;

namespace SitecoreMvcOtel;

public class OtelCorePipelineManager : BaseCorePipelineManager
{
    private readonly BaseCorePipelineManager _original;

    public OtelCorePipelineManager(DefaultCorePipelineManager original) => _original = original;
    
    public override void ClearCache() => _original.ClearCache();
    public override CorePipeline GetPipeline(string pipelineName, string pipelineGroup) => _original.GetPipeline(pipelineName, pipelineGroup);
    public override void Run(string pipelineName, PipelineArgs args) => Run(pipelineName, args, string.Empty);
    public override void Run(string pipelineName, PipelineArgs args, bool failIfNotExists) => Run(pipelineName, args, string.Empty, failIfNotExists);
    public override void Run(string pipelineName, PipelineArgs args, string pipelineDomain) => Run(pipelineName, args, pipelineDomain, failIfNotExists: true);

    public override void Run(string pipelineName, PipelineArgs args, string pipelineDomain, bool failIfNotExists)
    {
        using var source = Activity.Current?.Source.StartActivity($"Pipeline [{pipelineName}]");

        source?.AddTag("pipeline.name", pipelineName);

        _original.Run(pipelineName, args, pipelineDomain, failIfNotExists);
    }
}
