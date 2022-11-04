using System.Collections.Immutable;
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
using Bicep.Decompiler;

namespace BicepBuild;

public class BicepCompiler
{
    private readonly IFileResolver fileResolver;
    private readonly IModuleDispatcher moduleDispatcher;
    private readonly IFeatureProviderFactory featureProviderFactory;
    private readonly INamespaceProvider namespaceProvider;
    private readonly IConfigurationManager configurationManager;
    private readonly IApiVersionProviderFactory apiVersionProviderFactory;
    private readonly IBicepAnalyzer bicepAnalyzer;
    private readonly IModuleRegistryProvider moduleRegistryProvider;

    public BicepCompiler(
        IFileResolver fileResolver,
        IModuleDispatcher moduleDispatcher,
        IFeatureProviderFactory featureProviderFactory,
        INamespaceProvider namespaceProvider,
        IConfigurationManager configurationManager,
        IApiVersionProviderFactory apiVersionProviderFactory,
        IBicepAnalyzer bicepAnalyzer,
        IModuleRegistryProvider moduleRegistryProvider)
    {
        this.fileResolver = fileResolver;
        this.moduleDispatcher = moduleDispatcher;
        this.featureProviderFactory = featureProviderFactory;
        this.namespaceProvider = namespaceProvider;
        this.configurationManager = configurationManager;
        this.apiVersionProviderFactory = apiVersionProviderFactory;
        this.bicepAnalyzer = bicepAnalyzer;
        this.moduleRegistryProvider = moduleRegistryProvider;
    }

    public (EmitResult emitResult, string json) Compile(string bicepContents)
    {
        var bicepUri = new Uri("file:///main.bicep");
        var fileResolver = new InMemoryFileResolver(new Dictionary<Uri, string>
        {
            [bicepUri] = bicepContents,
        });

        var sourceFileGrouping = SourceFileGroupingBuilder.Build(fileResolver, moduleDispatcher, new Workspace(), bicepUri, false);
        var compilation = new Compilation(featureProviderFactory, namespaceProvider, sourceFileGrouping, configurationManager, apiVersionProviderFactory, bicepAnalyzer);

        var emitter = new TemplateEmitter(compilation.GetEntrypointSemanticModel());

        using var stringWriter = new StringWriter();
        var emitResult = emitter.Emit(stringWriter);

        return (emitResult, stringWriter.ToString());
    }

    public (string entryFile, ImmutableDictionary<string, string> filesDict) Decompile(string jsonContents)
    {
        var bicepUri = new Uri("file:///main.bicep");
        var jsonUri = new Uri("file:///main.json");
        var fileResolver = new InMemoryFileResolver(new Dictionary<Uri, string>
        {
            [jsonUri] = jsonContents,
        });

        var decompiler = new TemplateDecompiler(featureProviderFactory, namespaceProvider, fileResolver, moduleRegistryProvider, apiVersionProviderFactory, bicepAnalyzer);
        var (entrypointUri, filesToSave) = decompiler.DecompileFileWithModules(jsonUri, bicepUri);

        var entryFile = "main.bicep";
        var filesDict = filesToSave.ToImmutableDictionary(
            x => x.Key == entrypointUri ? entryFile : entrypointUri.MakeRelativeUri(x.Key).ToString(),
            x => x.Value);

        return (entryFile, filesDict);
    }
}