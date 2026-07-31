using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CppAst;

namespace SkiaSharpGenerator
{
	public class Verifier : BaseTool
	{
		private CppCompilation sourceCompilation = new CppCompilation();

		public Verifier(string skiaRoot, string configFile)
			: base(skiaRoot, configFile)
		{
		}

		public async Task VerifyAsync()
		{
			Log?.Log("Starting C API verification...");

			config = await LoadConfigAsync(ConfigFile);

			ParseSkiaHeaders();

			Verify();

			Log?.Log("C API verification complete.");
		}

		private void Verify()
		{
			Log?.LogVerbose("Verifying C API...");

			VerifyImplementations();
		}

		private void VerifyImplementations()
		{
			Log?.LogVerbose("  Making sure all declarations have an implementation...");

			var functions = StableOrdering.ByPathThenName(
				compilation.Functions,
				SkiaRoot,
				f => f.Span.Start.File,
				f => f.Name);
			var functionGroups = functions.GroupBy(
				f => StableOrdering.NormalizePath(SkiaRoot, f.Span.Start.File),
				StringComparer.Ordinal);

			var allSources = StableOrdering.EnumerateFiles(
				SkiaRoot,
				config.Source,
				Directory.EnumerateFiles);

			var sourcesContents = new Dictionary<string, string>();

			foreach (var group in functionGroups)
			{
				foreach (var function in group)
				{
					Log?.LogVerbose($"    {function.Name}");

					var found = false;

					foreach (var source in allSources)
					{
						if (!sourcesContents.TryGetValue(source, out var contents))
						{
							contents = File.ReadAllText(source);
							sourcesContents[source] = contents;
						}

						if (Regex.IsMatch(contents, $"\\s{function.Name}\\s*\\("))
						{
							found = true;
							break;
						}
					}

					if (!found)
						Log?.LogWarning($"Missing implementation for {function}");
				}
			}
		}
	}
}
