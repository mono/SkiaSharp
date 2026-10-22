using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using SkiaSharp.Tests.Goldens;

namespace SkiaSharp.Tests.Visual
{
	// Visual tests own enumeration, loading and decoding; the resolver sees only keys.
	internal static class GoldenStore
	{
		internal const string ResourcePrefix = "SkiaSharp.Goldens/";

		public readonly struct ResolvedGolden
		{
			public ResolvedGolden(byte[] pixels, GoldenResolution resolution)
			{
				Pixels = pixels;
				Resolution = resolution;
			}

			public byte[] Pixels { get; }
			public GoldenResolution Resolution { get; }
			public string Location => Resolution.Path;
		}

		public static string CaptureKey(string rendererName, string sceneName)
		{
			var request = VisualPlatform.Request(rendererName, sceneName);
			return $"{request.Profile}/{request.Version}/{request.CaseName}.{request.Variant}.{request.Architecture}.png";
		}

		public static ResolvedGolden? TryLoad(string rendererName, string sceneName, SKImageInfo info, out GoldenResolution resolution)
		{
			var sources = new Dictionary<string, (string Location, Func<byte[]> Read)>(StringComparer.Ordinal);
			foreach (var root in DiskRoots.Distinct(StringComparer.Ordinal))
			{
				if (!Directory.Exists(root))
					continue;
				var prefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
				foreach (var path in Directory.EnumerateFiles(prefix, "*.png", SearchOption.AllDirectories))
				{
					var key = path.Substring(prefix.Length).Replace(Path.DirectorySeparatorChar, '/');
					if (!sources.ContainsKey(key))
						sources.Add(key, (path, () => File.ReadAllBytes(path)));
				}
			}

			foreach (var assembly in ResourceAssemblies)
			{
				foreach (var resource in assembly.GetManifestResourceNames()
					.Where(name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal)))
				{
					var key = resource.Substring(ResourcePrefix.Length);
					if (!sources.ContainsKey(key))
						sources.Add(key, ($"{assembly.GetName().Name}!{resource}", () => ReadEmbedded(assembly, key)));
				}
			}

			resolution = GoldenReference.Resolve(sources.Keys, VisualPlatform.Request(rendererName, sceneName));
			if (resolution.Key is null)
				return null;
			var source = sources[resolution.Key];
			resolution = new GoldenResolution(resolution.Request, resolution.Key, source.Location, resolution.Candidates);
			return new ResolvedGolden(Decode(source.Read(), info, source.Location), resolution);
		}

		internal static byte[] ReadEmbedded(Assembly assembly, string key)
		{
			using var stream = assembly.GetManifestResourceStream(ResourcePrefix + key)
				?? throw new InvalidOperationException($"Missing embedded golden '{ResourcePrefix + key}'.");
			using var memory = new MemoryStream();
			stream.CopyTo(memory);
			return memory.ToArray();
		}

		private static IEnumerable<Assembly> ResourceAssemblies
		{
			get
			{
				var assemblies = new HashSet<Assembly>();
				var entry = Assembly.GetEntryAssembly();
				if (entry is not null && assemblies.Add(entry))
					yield return entry;
				var host = TestConfig.Current.GetType().Assembly;
				if (assemblies.Add(host))
					yield return host;
				if (assemblies.Add(typeof(GoldenStore).Assembly))
					yield return typeof(GoldenStore).Assembly;
			}
		}

		private static IEnumerable<string> DiskRoots
		{
			get
			{
				yield return Path.Combine(RuntimeRoot, "Goldens");
				if (!string.IsNullOrEmpty(TestConfig.Current.PathRoot))
					yield return Path.Combine(TestConfig.Current.PathRoot, "Goldens");
			}
		}

		public static byte[] EncodePng(byte[] rgba, SKImageInfo info)
		{
			var normalized = RendererPixels.NormalizedInfo(info);
			var handle = GCHandle.Alloc(rgba, GCHandleType.Pinned);
			try
			{
				using var pixmap = new SKPixmap(normalized, handle.AddrOfPinnedObject());
				using var image = SKImage.FromPixels(pixmap);
				using var data = image.Encode(SKEncodedImageFormat.Png, 100);
				return data.ToArray();
			}
			finally
			{
				handle.Free();
			}
		}

		public static string SaveFailureArtifact(string rendererName, string sceneName, string suffix, byte[] rgba, SKImageInfo info)
		{
			var path = Path.Combine(FailuresRoot, rendererName, sceneName + suffix);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllBytes(path, EncodePng(rgba, info));
			return path;
		}

		public static string SaveFailureImage(string rendererName, string sceneName, string suffix, SKImage image)
		{
			var path = Path.Combine(FailuresRoot, rendererName, sceneName + suffix);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			using var data = image.Encode(SKEncodedImageFormat.Png, 100);
			File.WriteAllBytes(path, data.ToArray());
			return path;
		}

		private static byte[] Decode(byte[] png, SKImageInfo info, string location)
		{
			var normalized = RendererPixels.NormalizedInfo(info);
			using var data = SKData.CreateCopy(png);
			using var codec = SKCodec.Create(data)
				?? throw new InvalidOperationException($"Failed to decode golden image '{location}'.");

			if (codec.Info.Width != normalized.Width || codec.Info.Height != normalized.Height)
				throw new InvalidOperationException(
					$"Golden image '{location}' is {codec.Info.Width}x{codec.Info.Height} but the scene rendered {normalized.Width}x{normalized.Height}.");

			var pixels = new byte[normalized.BytesSize];
			var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
			try
			{
				var result = codec.GetPixels(normalized, handle.AddrOfPinnedObject());
				if (result is not SKCodecResult.Success and not SKCodecResult.IncompleteInput)
					throw new InvalidOperationException($"SKCodec.GetPixels failed for golden '{location}': {result}.");
			}
			finally
			{
				handle.Free();
			}

			return pixels;
		}

		private static string RuntimeRoot => AppContext.BaseDirectory
			.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		private static string FailuresRoot => Path.Combine(RuntimeRoot, "_visualfailures");
	}
}
