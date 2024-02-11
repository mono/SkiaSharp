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

		public void DoVerify()
		{
			Log?.Log("Starting C API verification...");

			config = LoadConfig(ConfigFile);

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

			var functionGroups = compilation.Functions
				.OrderBy(f => f.Name)
				.GroupBy(f => f.Span.Start.File.ToLower().Replace("\\", "/"))
				.OrderBy(g => Path.GetDirectoryName(g.Key) + "/" + Path.GetFileName(g.Key));

			var allSources = new List<string>();
			foreach (var source in config.Source)
			{
				var path = Path.Combine(SkiaRoot, source.Key);
				foreach (var filter in source.Value)
				{
					allSources.AddRange(Directory.EnumerateFiles(path, filter));
				}
			}

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
