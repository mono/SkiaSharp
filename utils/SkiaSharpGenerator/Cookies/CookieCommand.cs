using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Mono.Options;

namespace SkiaSharpGenerator
{
	public class CookieCommand : BaseCommand
	{
		public CookieCommand()
			: base("cookie", "Ensure that all cookies have been defined in mono.")
		{
		}

		public string? AssemblyPath { get; set; }

		public string? MonoBranch { get; set; } = "master";

		protected override OptionSet OnCreateOptions() => new OptionSet
		{
			{ "a|assembly=", "The SkiaSharp assembly", v => AssemblyPath = v },
			{ "b|branch=", "The mono branch [master]", v => MonoBranch = v },
		};

		protected override bool OnValidateArguments(IEnumerable<string> extras)
		{
			var hasError = false;

			if (string.IsNullOrEmpty(AssemblyPath))
			{
				Program.Log.LogError($"{Program.Name}: Path to the SkiaSharp assembly was not provided: `--assembly=<path-to-SkiaSharp.dll>`.");
				hasError = true;
			}
			else if (!File.Exists(AssemblyPath))
			{
				Program.Log.LogError($"{Program.Name}: Path to the SkiaSharp assembly does not exist: `{AssemblyPath}`.");
				hasError = true;
			}

			if (string.IsNullOrEmpty(MonoBranch))
			{
				MonoBranch = "master";
			}

			return !hasError;
		}

		protected override bool OnInvoke(IEnumerable<string> extras)
		{
			var detector = new CookieDetector(AssemblyPath!, MonoBranch!);
			detector.Log = Program.Log;

			try
			{
				detector.DetectAsync().Wait();
			}
			catch
			{
				if (detector.HasErrors)
				{
					foreach (var dgn in detector.Messages)
					{
						if (dgn.Severity == DiagnosticSeverity.Error)
							Program.Log.LogError($"{dgn.Descriptor} at {dgn.Location}");
					}
				}

				throw;
			}

			return true;
		}
	}
}
