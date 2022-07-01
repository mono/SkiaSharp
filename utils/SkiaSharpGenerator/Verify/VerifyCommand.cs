using System.Collections.Generic;
using System.IO;
using CppAst;
using Mono.Options;

namespace SkiaSharpGenerator
{
	public class VerifyCommand : BaseCommand
	{
		public VerifyCommand()
			: base("verify", "Verify the C definitions have implementations.")
		{
		}

		public string? SourceRoot { get; set; }

		public string? ConfigPath { get; set; }

		protected override OptionSet OnCreateOptions() => new OptionSet
		{
			{ "s|skia=", "The root of the skia source", v => SourceRoot = v },
			{ "c|config=", "The config file path", v => ConfigPath = v },
		};

		protected override bool OnValidateArguments(IEnumerable<string> extras)
		{
			var hasError = false;

			if (string.IsNullOrEmpty(SourceRoot))
			{
				Program.Log.LogError($"{Program.Name}: Path to the skia source was not provided: `--skia=<path-to-skia>`.");
				hasError = true;
			}
			else if (!Directory.Exists(SourceRoot))
			{
				Program.Log.LogError($"{Program.Name}: Path to the skia source does not exist: `{SourceRoot}`.");
				hasError = true;
			}

			if (string.IsNullOrEmpty(ConfigPath))
			{
				Program.Log.LogError($"{Program.Name}: Path to config file was not provided: `--config=<path-to-config-json>`.");
				hasError = true;
			}
			else if (!File.Exists(ConfigPath))
			{
				Program.Log.LogError($"{Program.Name}: Path to config file does not exist: `{ConfigPath}`.");
				hasError = true;
			}

			return !hasError;
		}

		protected override bool OnInvoke(IEnumerable<string> extras)
		{
			var verifier = new Verifier(SourceRoot!, ConfigPath!);
			verifier.Log = Program.Log;

			try
			{
				verifier.DoVerify();
			}
			catch
			{
				if (verifier.HasErrors)
				{
					foreach (var dgn in verifier.Messages)
					{
						if (dgn.Type == CppLogMessageType.Error)
							Program.Log.LogError($"{dgn.Text} at {dgn.Location}");
					}
				}

				throw;
			}

			return true;
		}
	}
}
