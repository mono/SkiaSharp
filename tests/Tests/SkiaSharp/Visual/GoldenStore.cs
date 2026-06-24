using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Resolves, loads, records, and (on failure) saves golden images for the
	/// visual-regression matrix.
	///
	/// <para><b>Lookup order</b> for a (renderer, scene) cell — first hit wins:</para>
	/// <list type="number">
	///   <item><c>Content/Goldens/{renderer}.{platform}/{scene}.png</c> — per-platform GPU override</item>
	///   <item><c>Content/Goldens/{renderer}/{scene}.png</c> — per-renderer override</item>
	///   <item><c>Content/Goldens/_shared/{scene}.png</c> — portable CPU baseline</item>
	/// </list>
	///
	/// <para>
	/// Each candidate directory is checked on disk first (the build copies
	/// <c>Content</c> next to the test binary, and a source-tree walk lets the
	/// inner-loop edit goldens without rebuilding) and then as an embedded
	/// resource (device and browser hosts embed <c>Content</c> rather than copying
	/// it to a readable filesystem).
	/// </para>
	/// </summary>
	internal static class GoldenStore
	{
		public const string UpdateEnvVar = "SKIASHARP_UPDATE_GOLDENS";
		public const string ScopeEnvVar = "SKIASHARP_GOLDEN_SCOPE";

		private const string GoldensFolder = "Goldens";
		private const string SharedDir = "_shared";

		public static bool UpdateRequested =>
			IsTrue(Environment.GetEnvironmentVariable(UpdateEnvVar));

		public readonly struct ResolvedGolden
		{
			public ResolvedGolden(byte[] pixels, string location)
			{
				Pixels = pixels;
				Location = location;
			}

			public byte[] Pixels { get; }

			public string Location { get; }
		}

		// Returns the decoded golden (RGBA8888/Premul, sized to info) or null when
		// no golden exists for any candidate directory.
		public static ResolvedGolden? TryLoad(string rendererName, string sceneName, SKImageInfo info)
		{
			foreach (var dir in ReadCandidates(rendererName))
			{
				var relative = RelativePath(dir, sceneName);

				var diskPath = FindOnDisk(relative);
				if (diskPath is not null)
					return new ResolvedGolden(Decode(File.ReadAllBytes(diskPath), info, diskPath), diskPath);

				var resource = FindEmbedded(relative);
				if (resource is not null)
					return new ResolvedGolden(Decode(resource.Value.Bytes, info, resource.Value.Name), resource.Value.Name);
			}

			return null;
		}

		// The list of "looked in" locations, for a helpful missing-golden message.
		public static IEnumerable<string> ReadLocations(string rendererName, string sceneName) =>
			ReadCandidates(rendererName).Select(dir => RelativePath(dir, sceneName));

		// Records a golden in the source tree so it can be committed. The target
		// directory follows the scope: raster defaults to the shared baseline,
		// other renderers default to a per-platform override; SKIASHARP_GOLDEN_SCOPE
		// (shared|renderer|platform) overrides this.
		public static string Record(string rendererName, string sceneName, byte[] rgba, SKImageInfo info)
		{
			var dir = WriteDirectory(rendererName);
			var path = Path.Combine(SourceGoldensRoot, dir, sceneName + ".png");
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllBytes(path, Encode(rgba, info));
			return path;
		}

		public static string SaveFailureArtifact(string rendererName, string sceneName, string suffix, byte[] rgba, SKImageInfo info)
		{
			var path = Path.Combine(FailuresRoot, rendererName, sceneName + suffix);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllBytes(path, Encode(rgba, info));
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

		// ---- candidate directories ----

		private static IEnumerable<string> ReadCandidates(string rendererName)
		{
			yield return rendererName + "." + VisualPlatform.Tag;
			yield return rendererName;
			yield return SharedDir;
		}

		private static string WriteDirectory(string rendererName)
		{
			var scope = (Environment.GetEnvironmentVariable(ScopeEnvVar) ?? "").Trim().ToLowerInvariant();
			return scope switch
			{
				"shared" => SharedDir,
				"renderer" => rendererName,
				"platform" => rendererName + "." + VisualPlatform.Tag,
				_ => rendererName == "raster" ? SharedDir : rendererName + "." + VisualPlatform.Tag,
			};
		}

		private static string RelativePath(string dir, string sceneName) =>
			$"{GoldensFolder}/{dir}/{sceneName}.png";

		// ---- disk ----

		private static string FindOnDisk(string relative)
		{
			foreach (var root in DiskRoots)
			{
				var path = Path.Combine(root, ToPlatformPath(relative));
				if (File.Exists(path))
					return path;
			}

			return null;
		}

		private static IEnumerable<string> DiskRoots
		{
			get
			{
				yield return Path.Combine(RuntimeRoot, "Content");
				if (!string.IsNullOrEmpty(TestConfig.Current.PathRoot))
					yield return Path.Combine(TestConfig.Current.PathRoot, "Content");
				yield return SourceContentRoot;
			}
		}

		// ---- embedded resources ----

		private static (byte[] Bytes, string Name)? FindEmbedded(string relative)
		{
			var suffix = ("Content." + relative.Replace('/', '.'));

			foreach (var assembly in ResourceAssemblies)
			{
				string match = null;
				foreach (var name in assembly.GetManifestResourceNames())
				{
					if (name.EndsWith(suffix, StringComparison.Ordinal))
					{
						match = name;
						break;
					}
				}

				if (match is null)
					continue;

				using var stream = assembly.GetManifestResourceStream(match);
				if (stream is null)
					continue;

				using var ms = new MemoryStream();
				stream.CopyTo(ms);
				return (ms.ToArray(), $"{assembly.GetName().Name}!{match}");
			}

			return null;
		}

		private static IEnumerable<Assembly> ResourceAssemblies
		{
			get
			{
				var entry = Assembly.GetEntryAssembly();
				if (entry is not null)
					yield return entry;
				if (typeof(GoldenStore).Assembly != entry)
					yield return typeof(GoldenStore).Assembly;
			}
		}

		// ---- PNG encode/decode ----

		private static byte[] Encode(byte[] rgba, SKImageInfo info)
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

		// ---- roots ----

		// AppContext.BaseDirectory (not Assembly.Location): net48's runner
		// shadow-copies the test assembly to a temp dir, but the copied Content
		// and the source tree are only reachable from the real output folder.
		private static string RuntimeRoot => AppContext.BaseDirectory
			.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

		private static string FailuresRoot => Path.Combine(RuntimeRoot, "_visualfailures");

		private static string sourceRootCache;

		// Walks up from the output folder to the repo's tests/Content directory so
		// recorded goldens land in a committable location during local runs. On
		// device/browser hosts the source tree is unreachable; record mode there
		// falls back to the runtime root.
		private static string SourceContentRoot
		{
			get
			{
				if (sourceRootCache is not null)
					return sourceRootCache;

				var dir = RuntimeRoot;
				for (var i = 0; i < 12 && dir is not null; i++)
				{
					var candidate = Path.Combine(dir, "tests", "Content");
					if (Directory.Exists(Path.Combine(candidate, GoldensFolder)) || Directory.Exists(candidate))
					{
						sourceRootCache = candidate;
						return sourceRootCache;
					}

					dir = Path.GetDirectoryName(dir);
				}

				sourceRootCache = Path.Combine(RuntimeRoot, "Content");
				return sourceRootCache;
			}
		}

		private static string SourceGoldensRoot => Path.Combine(SourceContentRoot, GoldensFolder);

		private static string ToPlatformPath(string relative) =>
			relative.Replace('/', Path.DirectorySeparatorChar);

		private static bool IsTrue(string value)
		{
			if (string.IsNullOrEmpty(value))
				return false;
			value = value.Trim();
			return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
		}
	}
}
