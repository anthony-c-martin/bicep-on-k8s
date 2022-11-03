using System.IO.Abstractions.TestingHelpers;
using Bicep.Core.Analyzers.Interfaces;
using Bicep.Core.Analyzers.Linter.ApiVersions;
using Bicep.Core.Configuration;
using Bicep.Core.Emit;
using Bicep.Core.Features;
using Bicep.Core.FileSystem;
using Bicep.Core.Registry;
using Bicep.Core.Semantics;
using Bicep.Core.Semantics.Namespaces;
using Bicep.Core.Workspaces;

namespace BicepBuild.Controllers;

public class BicepCompiler
{
    private readonly IFileResolver fileResolver;
    private readonly IModuleDispatcher moduleDispatcher;
    private readonly IFeatureProviderFactory featureProviderFactory;
    private readonly INamespaceProvider namespaceProvider;
    private readonly IConfigurationManager configurationManager;
    private readonly IApiVersionProviderFactory apiVersionProviderFactory;
    private readonly IBicepAnalyzer bicepAnalyzer;

    public BicepCompiler(
        IFileResolver fileResolver,
        IModuleDispatcher moduleDispatcher,
        IFeatureProviderFactory featureProviderFactory,
        INamespaceProvider namespaceProvider,
        IConfigurationManager configurationManager,
        IApiVersionProviderFactory apiVersionProviderFactory,
        IBicepAnalyzer bicepAnalyzer)
    {
        this.fileResolver = fileResolver;
        this.moduleDispatcher = moduleDispatcher;
        this.featureProviderFactory = featureProviderFactory;
        this.namespaceProvider = namespaceProvider;
        this.configurationManager = configurationManager;
        this.apiVersionProviderFactory = apiVersionProviderFactory;
        this.bicepAnalyzer = bicepAnalyzer;
    }

    public (EmitResult emitResult, string json) Compile(string bicepContents)
    {
        var bicepUri = new Uri("file:///main.bicep");
        var workspace = new Workspace();
        workspace.UpsertSourceFiles(new ISourceFile[] { SourceFileFactory.CreateBicepFile(bicepUri, bicepContents) });

        var sourceFileGrouping = SourceFileGroupingBuilder.Build(fileResolver, moduleDispatcher, workspace, bicepUri, false);
        var compilation = new Compilation(featureProviderFactory, namespaceProvider, sourceFileGrouping, configurationManager, apiVersionProviderFactory, bicepAnalyzer);

        var emitter = new TemplateEmitter(compilation.GetEntrypointSemanticModel());

        using var stringWriter = new StringWriter();
        var emitResult = emitter.Emit(stringWriter);

        return (emitResult, stringWriter.ToString());
    }
}