using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Bicep.Core;
using Bicep.Core.Emit;

namespace BicepBuild.Controllers;

[ApiController]
[Route("[controller]")]
public class BuildController : ControllerBase
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

    private readonly ILogger<BuildController> _logger;

    public BuildController(ILogger<BuildController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public CompileResponse Post(CompileRequest request)
    {
        var (emitResult, templateContents) = BicepCompiler.Compile(request.BicepContents);

        return new CompileResponse(
            emitResult.Status != EmitStatus.Failed,
            templateContents,
            emitResult.Diagnostics.Select(x => new CompileDiagnostic(x.Code, x.Message, x.Level.ToString())).ToImmutableArray());
    }
}
