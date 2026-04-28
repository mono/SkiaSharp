using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using SkiaSharp;

namespace SkiaFiddle.Fiddle;

/// <summary>
/// Compiles a fiddle snippet that's split into a Setup block (class-scope —
/// fields, methods, ctor) and a Draw block (per-frame method body). Setup
/// runs once when the class is instantiated; Draw runs every frame.
/// </summary>
public class RoslynFiddleCompiler : IFiddleCompiler
{
    private static readonly CSharpCompilationOptions CompilationOptions =
        new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            optimizationLevel: OptimizationLevel.Release,
            concurrentBuild: false,
            allowUnsafe: false);

    private MetadataReference[]? _references;

    public string Name => "Roslyn (browser-wasm)";

    public Task<FiddleCompileResult> CompileAsync(string setupCode, string drawCode)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            var references = GetReferences();
            var fullSource = WrapSnippet(setupCode, drawCode);
            var parseOptions = new CSharpParseOptions(LanguageVersion.LatestMajor);
            var tree = CSharpSyntaxTree.ParseText(fullSource, parseOptions);

            var assemblyName = "SkiaFiddleSnippet_" + Guid.NewGuid().ToString("N");
            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { tree },
                references,
                CompilationOptions);

            using var ms = new MemoryStream();
            EmitResult emitResult = compilation.Emit(ms);

            if (!emitResult.Success)
            {
                var diagnostics = FormatDiagnostics(emitResult.Diagnostics);
                sw.Stop();
                return Task.FromResult(new FiddleCompileResult(null, diagnostics, sw.ElapsedMilliseconds));
            }

            ms.Position = 0;
            var loadContext = new AssemblyLoadContext(assemblyName, isCollectible: true);
            Assembly assembly = loadContext.LoadFromStream(ms);

            var type = assembly.GetType("SkiaFiddleSnippet.Snippet")
                       ?? throw new InvalidOperationException("Generated type not found.");
            var instance = Activator.CreateInstance(type)
                           ?? throw new InvalidOperationException("Could not instantiate snippet.");

            var method = type.GetMethod("Draw", BindingFlags.Public | BindingFlags.Instance)
                         ?? throw new InvalidOperationException("Draw entry point not found.");

            var del = (FiddleCanvas.FiddleDelegate)method.CreateDelegate(typeof(FiddleCanvas.FiddleDelegate), instance);

            sw.Stop();
            var warnings = FormatDiagnostics(emitResult.Diagnostics, includeErrors: false);
            var msg = $"Compiled in {sw.ElapsedMilliseconds} ms";
            if (!string.IsNullOrEmpty(warnings))
                msg += "\n" + warnings;
            return Task.FromResult(new FiddleCompileResult(del, msg, sw.ElapsedMilliseconds));
        }
        catch (Exception ex)
        {
            sw.Stop();
            return Task.FromResult(new FiddleCompileResult(null, ex.ToString(), sw.ElapsedMilliseconds));
        }
    }

    private MetadataReference[] GetReferences()
    {
        if (_references is not null)
            return _references;

        var refs = new List<MetadataReference>();
        refs.AddRange(Basic.Reference.Assemblies.Net100.References.All);
        AddRuntimeAssembly(refs, typeof(SKCanvas));

        _references = refs.ToArray();
        return _references;
    }

    private static void AddRuntimeAssembly(List<MetadataReference> list, Type type)
    {
        var asm = type.Assembly;
        unsafe
        {
            if (asm.TryGetRawMetadata(out byte* blob, out int length))
            {
                var moduleMetadata = ModuleMetadata.CreateFromMetadata((IntPtr)blob, length);
                var assemblyMetadata = AssemblyMetadata.Create(moduleMetadata);
                list.Add(assemblyMetadata.GetReference());
            }
        }
    }

    /// <summary>
    /// Wraps the user code in a host class. Uses #line directives so Roslyn
    /// reports diagnostics with paths "Setup" and "Draw" so we can map them
    /// back to the right editor box.
    /// </summary>
    private static string WrapSnippet(string setupCode, string drawCode)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using SkiaSharp;");
        sb.AppendLine("using static System.Math;");
        sb.AppendLine("namespace SkiaFiddleSnippet {");
        sb.AppendLine("public sealed class Snippet {");
        sb.AppendLine("#line 1 \"Setup\"");
        sb.AppendLine(setupCode);
        sb.AppendLine("#line hidden");
        sb.AppendLine("public void Draw(SkiaSharp.SKCanvas canvas, int width, int height, double t) {");
        sb.AppendLine("#line 1 \"Draw\"");
        sb.AppendLine(drawCode);
        sb.AppendLine("#line hidden");
        sb.AppendLine("} } }");
        return sb.ToString();
    }

    // Reference-identity warnings emitted because SkiaSharp's own transitive deps overlap
    // with Basic.Reference.Assemblies.Net100. They're harmless and noisy.
    private static readonly HashSet<string> SuppressedDiagnosticIds = new() { "CS1701", "CS1702", "CS8019" };

    private static string FormatDiagnostics(IEnumerable<Diagnostic> diagnostics, bool includeErrors = true)
    {
        var sb = new StringBuilder();
        foreach (var d in diagnostics)
        {
            if (d.Severity == DiagnosticSeverity.Hidden)
                continue;
            if (SuppressedDiagnosticIds.Contains(d.Id))
                continue;
            if (!includeErrors && d.Severity == DiagnosticSeverity.Error)
                continue;

            // GetMappedLineSpan honors #line directives, so Path is "Setup"/"Draw"/empty,
            // and the line is the 0-indexed user-line within that box.
            var mapped = d.Location.GetMappedLineSpan();
            var box = string.IsNullOrEmpty(mapped.Path) ? "wrapper" : mapped.Path;
            var userLine = mapped.StartLinePosition.Line + 1;
            var col = mapped.StartLinePosition.Character + 1;
            sb.Append('[').Append(d.Severity).Append("] ");
            sb.Append('<').Append(box).Append("> ");
            sb.Append('(').Append(userLine).Append(',').Append(col).Append(") ");
            sb.Append(d.Id).Append(": ").Append(d.GetMessage()).AppendLine();
        }
        return sb.ToString().TrimEnd();
    }
}
