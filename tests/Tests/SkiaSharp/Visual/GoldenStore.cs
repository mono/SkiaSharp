using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using SkiaSharp.Tests.Goldens;

namespace SkiaSharp.Tests.Visual
{
	internal static class GoldenStore
	{
		private static readonly Lazy<GoldenStoreIndex> store = new(CreateStore);

		public readonly struct ResolvedGolden
		{
			public ResolvedGolden(byte[] pixels, GoldenResolution resolution)
			{
				Pixels = pixels;
				Resolution = resolution;
			}
			public byte[] Pixels { get; }
			public GoldenResolution Resolution { get; }
			public string Location => Resolution.Asset.Location;
		}

		public static GoldenResolution Resolve(string rendererName, string sceneName) =>
			store.Value.Resolve(VisualPlatform.Request(rendererName, sceneName));

		public static string CaptureKey(GoldenRequest request) =>
			$"{request.Profile}/{request.Version}/{request.CaseName}.{request.Variant}.{request.Architecture}.png";

		public static ResolvedGolden Load(GoldenResolution resolution, SKImageInfo info)
		{
			if (resolution.Asset == null)
				throw new InvalidOperationException(resolution.Describe());
			return new ResolvedGolden(Decode(resolution.Asset.ReadBytes(), info, resolution.Asset.Location), resolution);
		}

		private static GoldenStoreIndex CreateStore()
		{
			var stores = new List<GoldenStoreIndex>();
			var roots = new HashSet<string>(StringComparer.Ordinal);
			var assemblies = new HashSet<Assembly>
			{
				typeof(GoldenStore).Assembly,
				TestConfig.Current.GetType().Assembly
			};
			var entry = Assembly.GetEntryAssembly();
			if (entry != null)
				assemblies.Add(entry);

			AddDirectory(Path.Combine(RuntimeRoot, "Goldens", "v1"));
			if (!string.IsNullOrEmpty(TestConfig.Current.PathRoot))
				AddDirectory(Path.Combine(TestConfig.Current.PathRoot, "Goldens", "v1"));
			foreach (var assembly in assemblies)
			{
				foreach (var attribute in assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
					if (attribute.Key == "SkiaSharp.GoldenSourceRoot")
						AddDirectory(attribute.Value);
				if (assembly.GetManifestResourceNames().Any(n => n.StartsWith("SkiaSharp.Goldens.v1/", StringComparison.Ordinal)))
					stores.Add(GoldenStoreIndex.FromAssembly(assembly));
			}
			if (stores.Count == 0)
				throw new InvalidOperationException("No canonical golden assets were packaged or registered for this host.");
			return GoldenStoreIndex.Combine(stores.ToArray());

			void AddDirectory(string root)
			{
				if (root != null && Directory.Exists(root) && roots.Add(Path.GetFullPath(root)))
					stores.Add(GoldenStoreIndex.FromDirectory(root));
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
				if (result != SKCodecResult.Success)
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
