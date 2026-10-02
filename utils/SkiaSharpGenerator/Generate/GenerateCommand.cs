using System;
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
			{ "o|output=", "The output directory", v => OutputPath = v },
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
				OutputPath = Path.Combine(Directory.GetCurrentDirectory(), "Generated");

			return !hasError;
		}

		protected override bool OnInvoke(IEnumerable<string> extras)
		{
			var outputPath = Path.GetFullPath(OutputPath!);
			var parent = Directory.GetParent(outputPath)?.FullName
				?? throw new InvalidOperationException("The output directory must have a parent directory.");
			var outputName = Path.GetFileName(outputPath);
			var temporaryOutputPath = Path.Combine(parent, $".{outputName}.{Guid.NewGuid():N}.generating");
			var backupOutputPath = Path.Combine(parent, $".{outputName}.{Guid.NewGuid():N}.previous");

			var docStore = Directory.Exists(outputPath)
				? new DocumentationStore(Directory.EnumerateFiles(outputPath, "*.cs", SearchOption.AllDirectories))
				: null;

			var generator = new Generator(SourceRoot!, ConfigPath!, temporaryOutputPath, docStore);
			generator.Log = Program.Log;

			try
			{
				generator.GenerateAsync().Wait();

				if (Directory.Exists(outputPath))
					Directory.Move(outputPath, backupOutputPath);

				try
				{
					Directory.Move(temporaryOutputPath, outputPath);
				}
				catch
				{
					if (Directory.Exists(backupOutputPath) && !Directory.Exists(outputPath))
						Directory.Move(backupOutputPath, outputPath);

					throw;
				}

				if (Directory.Exists(backupOutputPath))
					Directory.Delete(backupOutputPath, recursive: true);
			}
			catch
			{
				if (Directory.Exists(temporaryOutputPath))
					Directory.Delete(temporaryOutputPath, recursive: true);
				if (Directory.Exists(backupOutputPath) && !Directory.Exists(outputPath))
					Directory.Move(backupOutputPath, outputPath);

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

		internal static StreamWriter CreateOutputWriter(string outputPath)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
			var writer = new StreamWriter(File.Create(outputPath));
			writer.NewLine = "\n";
			return writer;
		}
	}
}
