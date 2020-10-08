using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;

namespace SkiaSharp.Tests
{
	public class Program
	{
		private static readonly Assembly assembly = typeof(Program).Assembly;

		public static int Main(string[] args)
		{
			Console.WriteLine($"args: {string.Join(" ", args)}");

			// copy resources out of assembly
			var prefix = "SkiaSharp.Tests.";
			var resources = assembly.GetManifestResourceNames();
			foreach (var resource in resources)
			{
				if (!resource.StartsWith(prefix))
					continue;

				if (resource.StartsWith(prefix + "fonts.") || resource.StartsWith(prefix + "images."))
				{
					var ext = Path.GetExtension(resource);
					var fn = Path.GetFileNameWithoutExtension(resource);
					var dest = fn.Substring(prefix.Length - 1).Replace(".", "/") + ext;
					var dir = Path.GetDirectoryName(dest);

					if (!Directory.Exists(dir))
						Directory.CreateDirectory(dir);

					using (var stream = assembly.GetManifestResourceStream(resource))
					using (var file = File.Create(dest))
						stream.CopyTo(file);

					Console.WriteLine($"Saved file to {dest}: {File.ReadAllBytes(dest)?.Length}");
				}
			}

			var filters = new XunitFilters
			{
				ExcludedTraits =
				{
					{ BaseTest.CategoryKey, new List<string> { BaseTest.GpuCategory } },
					{ BaseTest.PlatformKey, new List<string> { BaseTest.WasmPlatform } },
				},
				ExcludedClasses =
				{
					"SkiaSharp.Tests.ApiTest",
				},
			};

			var testRunner = new ThreadlessXunitTestRunner();
			var result = testRunner.Run(assembly.GetName().Name + ".dll", filters);
			return result ? 1 : 0;
		}
	}
}
