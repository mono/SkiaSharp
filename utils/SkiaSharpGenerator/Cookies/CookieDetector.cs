using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mono.Cecil;

namespace SkiaSharpGenerator
{
	public class CookieDetector
	{
		private const string SourceUrl = "https://github.com/mono/mono/raw/{0}/mcs/tools/wasm-tuner/InterpToNativeGenerator.cs";

		private readonly string branchUrl;
		private readonly string assemblyPath;
		private readonly string type;
		private CSharpSyntaxTree? compilation;

		public CookieDetector(string assembly, string interopType, string branchName)
		{
			assemblyPath = assembly;
			type = interopType;
			branchUrl = string.Format(SourceUrl, branchName);
		}

		public ILogger? Log { get; set; }

		public bool HasErrors =>
			compilation?.GetDiagnostics()?.Any(d => d.Severity == DiagnosticSeverity.Error) ?? false;

		public IEnumerable<Diagnostic> Messages =>
			compilation?.GetDiagnostics() ?? Array.Empty<Diagnostic>();

		public async Task DetectAsync()
		{
			Log?.LogVerbose("Downloading source...");

			var code = await DownloadLatestCodeAsync();
			if (string.IsNullOrEmpty(code))
			{
				Log?.LogError("Downloading source failed.");
				throw new Exception("Downloading source failed.");
			}

			compilation = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(code);
			if (compilation == null || HasErrors)
			{
				Log?.LogError("Parsing source failed.");
				throw new Exception("Parsing source failed.");
			}

			Log?.LogVerbose("Parsing cookies...");

			var cookies = GetExistingCookies();
			if (cookies == null || cookies.Length == 0)
			{
				Log?.LogError("Retreiving cookies failed.");
				throw new Exception("Retreiving cookies failed.");
			}

			Log?.LogVerbose($"Found {cookies.Length} cookies.");

			Log?.LogVerbose("Loading .NET assembly...");

			var signatures = ParseAssembly(type);

			Log?.LogVerbose($"Found {signatures.Length} signatures.");

			var newSignatures = signatures
				.Where(s => !cookies.Contains(s.Signature))
				.ToArray();

			if (newSignatures.Length > 0)
			{
				Log?.LogWarning($"Found {newSignatures.Length} NEW signatures.");

				foreach (var sig in newSignatures)
				{
					Log?.LogVerbose($"{sig}");
					Log?.LogVerbose($"Potential matches: {string.Join(", ", GetMatches(sig.Signature))}");
				}

				Log?.Log(string.Join(Environment.NewLine, newSignatures.Select(s => s.Signature).Distinct()));
			}
			else
			{
				Log?.LogVerbose($"Found NO new signatures.");
			}

			IEnumerable<string> GetMatches(string signature)
			{
				return cookies.Where(c =>
					c.Length == signature.Length &&
					c[0] == signature[0] &&
					string.Concat(c.Substring(1).ToCharArray().OrderBy(x => x)) == string.Concat(signature.Substring(1).ToCharArray().OrderBy(x => x)));
			}
		}

		private (string Method, string Signature)[] ParseAssembly(string typeFullName)
		{
			var resolver = new DefaultAssemblyResolver();
			var param = new ReaderParameters
			{
				AssemblyResolver = resolver
			};
			var module = AssemblyDefinition.ReadAssembly(assemblyPath, param);

			var type = module.MainModule.GetType(typeFullName);

			var methods = type.Methods.Select(m =>
			{
				var returnSig = GetSignature(m, m.ReturnType);
				var paramsSig = string.Concat(m.Parameters.Select(p => GetSignature(m, p.ParameterType)));
				return (Method: m.Name, Signature: returnSig + paramsSig);
			});

			return methods.OrderBy(s => s.Signature).ToArray();

			static string GetSignature(MethodDefinition method, TypeReference ret)
			{
				if (ret.IsByReference || ret.IsArray || ret.IsPointer)
					return "I";

				return GetTypeSignature(method, ret.Resolve());
			}
		}

		private static string GetTypeSignature(MethodDefinition method, TypeDefinition type)
		{
			if (type.IsEnum || type.IsPointer || type.IsArray)
				return "I";

			// special delegates
			if ((type.FullName.StartsWith("SkiaSharp.") || type.FullName.StartsWith("HarfBuzzSharp.")) && type.FullName.EndsWith("ProxyDelegate"))
				return "I";

			switch (type.FullName)
			{
				case "System.String":
				case "System.UInt16":
				case "System.Int64":
				case "System.UInt32":
				case "System.Int32":
				case "System.IntPtr":
				case "System.Byte":
				case "System.Boolean":
				case "System.Void*":
				case "SkiaSharp.SKManagedDrawableDelegates":
				case "SkiaSharp.SKManagedStreamDelegates":
				case "SkiaSharp.SKManagedWStreamDelegates":
					return "I";

				case "System.Double":
					return "D";

				case "System.Single":
					return "F";

				case "SkiaSharp.GRVkBackendContextNative":
					return "O";

				case "System.Void":
					return "V";

				default:
					throw new NotSupportedException($"Unsupported parameter type for {method.FullName}: {type}");
			}
		}

		private string[]? GetExistingCookies()
		{
			var root = compilation!.GetCompilationUnitRoot();

			var generatorClass = root.Members
				.OfType<ClassDeclarationSyntax>()
				.Single(m => m.Identifier.ValueText == "InterpToNativeGenerator");
			var cookiesField = generatorClass.Members
				.OfType<FieldDeclarationSyntax>()
				.SelectMany(m => m.Declaration.Variables)
				.Single(m => m.Identifier.ValueText == "cookies");
			var cookiesDeclaration = cookiesField.Initializer?.Value as ArrayCreationExpressionSyntax;

			return cookiesDeclaration?.Initializer?.Expressions
				.OfType<LiteralExpressionSyntax>()
				.Select(e => e.Token.ValueText)
				.ToArray();
		}

		private async Task<string?> DownloadLatestCodeAsync()
		{
			using var client = new HttpClient();
			return await client.GetStringAsync(branchUrl);
		}
	}
}
