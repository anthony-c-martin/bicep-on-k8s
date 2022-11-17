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
        string JsonContents);
    public record DecompileResponse(
        string FileName,
        ImmutableDictionary<string, string> FileLookup);

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
        await fileSystem.File.WriteAllTextAsync("/main.json", request.JsonContents);
        var (entrypointUri, filesToSave) = await bicepDecompiler.Decompile(new Uri("file:///main.json"), new Uri("file:///main.bicep"));

        var entryFile = "main.bicep";
        var filesDict = filesToSave.ToImmutableDictionary(
            x => x.Key == entrypointUri ? entryFile : entrypointUri.MakeRelativeUri(x.Key).ToString(),
            x => x.Value);

        return new DecompileResponse(entryFile, filesDict);
    }
}