using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Bicep.Core.Analyzers.Linter;
using Bicep.Core.Configuration;
using Bicep.Core.Diagnostics;
using Bicep.Core.Emit;
using Bicep.Core.Features;
using Bicep.Core.FileSystem;
using Bicep.Core.Registry;
using Bicep.Core.Registry.Auth;
using Bicep.Core.Semantics;
using Bicep.Core.Semantics.Namespaces;
using Bicep.Core.TypeSystem.Az;
using Bicep.Core.Workspaces;
using ConfigurationManager = Bicep.Core.Configuration.ConfigurationManager;

namespace BicepBuild.Controllers;

public static class BicepCompiler
{
    public static (EmitResult emitResult, string json) Compile(string bicepContents)
    {
        var bicepUri = new Uri("file:///main.bicep");

        var configurationManager = new ConfigurationManager(new FileSystem());
        var fileResolver = new InMemoryFileResolver(new Dictionary<Uri, string> { [bicepUri] = bicepContents, });
        var featureProvider = new FeatureProvider();

        var emitterSettings = new EmitterSettings(featureProvider);
        var moduleDispatcher = new ModuleDispatcher(
            new DefaultModuleRegistryProvider(
                fileResolver,
                new ContainerRegistryClientFactory(new TokenCredentialFactory()),
                new TemplateSpecRepositoryFactory(new TokenCredentialFactory()),
                featureProvider));
        var namespaceProvider = new DefaultNamespaceProvider(new AzResourceTypeLoader(), featureProvider);

        var configuration = configurationManager.GetConfiguration(bicepUri);
        var workspace = new Workspace();
        var sourceFileGrouping = SourceFileGroupingBuilder.Build(fileResolver, moduleDispatcher, workspace, bicepUri, configuration);
        var compilation = new Compilation(featureProvider, namespaceProvider, sourceFileGrouping, configuration, new LinterAnalyzer(configuration));
        var emitter = new TemplateEmitter(compilation.GetEntrypointSemanticModel(), emitterSettings);

        using var stringWriter = new StringWriter();
        var emitResult = emitter.Emit(stringWriter);

        return (emitResult, stringWriter.ToString());
    }
}