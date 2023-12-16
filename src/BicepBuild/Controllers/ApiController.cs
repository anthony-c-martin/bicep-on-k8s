using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Bicep.Core.Emit;
using Bicep.Core;
using System.IO.Abstractions;
using Bicep.Decompiler;

namespace BicepBuild.Controllers;

[ApiController]
public class ApiController : ControllerBase
{
    public record CompileDiagnostic(
        string Code,
        string Message,
        string Level);
    public record CompileRequest(
        string BicepContents);
    public record CompileResponse(
        bool Success,
        string TemplateContents,
        ImmutableArray<CompileDiagnostic> Diagnostics);

    public record DecompileRequest(
        string Template);
    public record DecompileResponse(
        ImmutableArray<FileDefinition> Files,
        string EntryPoint);

    public record FileDefinition(
        string Path,
        string Contents);

    private readonly BicepCompiler bicepCompiler;
    private readonly BicepDecompiler bicepDecompiler;
    private readonly IFileSystem fileSystem;

    public ApiController(
        BicepCompiler bicepCompiler,
        BicepDecompiler bicepDecompiler,
        IFileSystem fileSystem)
    {
        this.bicepCompiler = bicepCompiler;
        this.bicepDecompiler = bicepDecompiler;
        this.fileSystem = fileSystem;
    }

    [HttpPost]
    [Route("build")]
    public async Task<CompileResponse> Build(CompileRequest request)
    {
        await fileSystem.File.WriteAllTextAsync("/main.bicep", request.BicepContents);
        var compilation = await bicepCompiler.CreateCompilation(new Uri("file:///main.bicep"));
        var emitter = new TemplateEmitter(compilation.GetEntrypointSemanticModel());

        using var stringWriter = new StringWriter();
        var emitResult = emitter.Emit(stringWriter);

        return new CompileResponse(
            emitResult.Status != EmitStatus.Failed,
            stringWriter.ToString(),
            emitResult.Diagnostics.Select(x => new CompileDiagnostic(x.Code, x.Message, x.Level.ToString())).ToImmutableArray());
    }

    [HttpPost]
    [Route("decompile")]
    public async Task<DecompileResponse> Decompile(DecompileRequest request)
    {
        var decompilation = await bicepDecompiler.Decompile(new Uri("file:///main.bicep"), request.Template);

        var files = decompilation.FilesToSave
            .Select(kvp => new FileDefinition(
                Path: kvp.Key.LocalPath.Replace("/", string.Empty),
                Contents: kvp.Value))
            .ToImmutableArray();

        return new DecompileResponse(files, "main.bicep");
    }
}