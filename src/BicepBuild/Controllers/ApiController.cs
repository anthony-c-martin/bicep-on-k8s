using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Bicep.Core.Emit;

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

    public ApiController(BicepCompiler bicepCompiler)
    {
        this.bicepCompiler = bicepCompiler;
    }

    [HttpPost]
    [Route("build")]
    public CompileResponse Build(CompileRequest request)
    {
        var (emitResult, templateContents) = bicepCompiler.Compile(request.BicepContents);

        return new CompileResponse(
            emitResult.Status != EmitStatus.Failed,
            templateContents,
            emitResult.Diagnostics.Select(x => new CompileDiagnostic(x.Code, x.Message, x.Level.ToString())).ToImmutableArray());
    }

    [HttpPost]
    [Route("decompile")]
    public DecompileResponse Decompile(DecompileRequest request)
    {
        var (entryFile, filesDict) = bicepCompiler.Decompile(request.JsonContents);

        return new DecompileResponse(entryFile, filesDict);
    }
}
