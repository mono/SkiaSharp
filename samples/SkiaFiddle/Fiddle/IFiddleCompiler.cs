using System.Threading.Tasks;

namespace SkiaFiddle.Fiddle;

public record FiddleCompileResult(FiddleCanvas.FiddleDelegate? Draw, string? Diagnostics, long ElapsedMs);

public interface IFiddleCompiler
{
    string Name { get; }

    Task<FiddleCompileResult> CompileAsync(string setupCode, string drawCode);
}
