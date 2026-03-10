using System.Collections.Generic;
using System.IO;
using CppAst;
using Mono.Options;

namespace SkiaSharpGenerator
{
	public class GenerateCommand : BaseCommand
	{
		public GenerateCommand()
			: base("generate", "Generate the p/invoke bindings for SkiaSharp.")
		{
		}

		public string? SourceRoot { get; set; }

		public string? ConfigPath { get; set; }

		public string? OutputPath { get; set; }

		protected override OptionSet OnCreateOptions() => new OptionSet
		{
			{ "r|root=", "The root of the source", v => SourceRoot = v },
			{ "c|config=", "The config file path", v => ConfigPath = v },
			{ "o|output=", "The output path", v => OutputPath = v },
		};

		protected override bool OnValidateArguments(IEnumerable<string> extras)
		{
			var hasError = false;

			if (string.IsNullOrEmpty(SourceRoot))
			{
				Program.Log.LogError($"{Program.Name}: Path to the skia source was not provided: `--root=<path-to-skia-or-root>`.");
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

			if (string.IsNullOrEmpty(OutputPath))
				OutputPath = Path.Combine(Directory.GetCurrentDirectory(), "SkiaApi.generated.cs");

			return !hasError;
		}

		protected override bool OnInvoke(IEnumerable<string> extras)
		{
			var dir = Path.GetDirectoryName(OutputPath);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			var docStore = OutputPath is not null && File.Exists(OutputPath)
				? new DocumentationStore(OutputPath)
				: null;

			using var file = File.Create(OutputPath);
			using var writer = new StreamWriter(file);

			var generator = new Generator(SourceRoot!, ConfigPath!, writer, docStore);
			generator.Log = Program.Log;

			try
			{
				generator.GenerateAsync().Wait();
			}
			catch
			{
				if (generator.HasErrors)
				{
					foreach (var dgn in generator.Messages)
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
