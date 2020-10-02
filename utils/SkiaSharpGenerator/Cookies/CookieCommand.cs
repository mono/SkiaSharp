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

		public string? InteropType { get; set; }

		public string? MonoBranch { get; set; } = "master";

		protected override OptionSet OnCreateOptions() => new OptionSet
		{
			{ "a|assembly=", "The .NET assembly", v => AssemblyPath = v },
			{ "t|type=", "The interop type", v => InteropType = v },
			{ "b|branch=", "The mono branch [master]", v => MonoBranch = v },
		};

		protected override bool OnValidateArguments(IEnumerable<string> extras)
		{
			var hasError = false;

			if (string.IsNullOrEmpty(AssemblyPath))
			{
				Program.Log.LogError($"{Program.Name}: Path to the .NET assembly was not provided: `--assembly=<path-to.dll>`.");
				hasError = true;
			}
			else if (!File.Exists(AssemblyPath))
			{
				Program.Log.LogError($"{Program.Name}: Path to the .NET assembly does not exist: `{AssemblyPath}`.");
				hasError = true;
			}

			if (string.IsNullOrEmpty(InteropType))
			{
				Program.Log.LogError($"{Program.Name}: The interop type was not specified: `{InteropType}`.");
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
			var detector = new CookieDetector(AssemblyPath!, InteropType!, MonoBranch!);
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
