using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Resolves and loads golden images for the visual-regression matrix, and
	/// encodes captured pixels to PNG for emission into the test log.
	///
	/// <para><b>Layered lookup, generalizing over platform only.</b> A cell
	/// resolves the first of:</para>
	/// <list type="number">
	///   <item><c>Content/Goldens/{renderer}.{platform}/{scene}.png</c> —
	///         per-platform override, used when this OS/driver diverges.</item>
	///   <item><c>Content/Goldens/{renderer}/{scene}.png</c> — the renderer's
	///         golden shared across platforms, used when the backend produces the
	///         same pixels everywhere (the common case for CPU raster and for
	///         software GL).</item>
	/// </list>
	///
	/// <para>The fallback deliberately generalizes only over <b>platform</b>, never
	/// over <b>renderer</b>: different backends legitimately differ (antialiasing,
	/// driver), so a cell never falls back to another backend's bytes. That is what
	/// keeps a GPU cell from ever being compared against the CPU baseline.</para>
	///
	/// <para><b>No record mode.</b> Goldens are seeded by harvesting the captured
	/// PNGs that every cell emits into the test results (TRX) and committing them
	/// (see <c>scripts/infra/tests/extract-visual-goldens.py</c>). That works
	/// uniformly for desktop, device, and browser hosts — including the
	/// device/browser hosts where the filesystem is sandboxed/embedded and an
	/// in-process write-to-source-tree is impossible. The harvest writes the
	/// per-platform path by default; promoting identical per-platform goldens up to
	/// the shared <c>{renderer}/</c> folder is a manual dedupe (the harvest then
	/// stops re-creating the per-platform copies once they are byte-identical). The
	/// store therefore only reads.</para>
	///
	/// <para>Each candidate is checked on disk first (the build copies
	/// <c>Content</c> next to the test binary, and a source-tree walk lets the
	/// inner loop edit a golden without rebuilding) and then as an embedded
	/// resource (device and browser hosts embed <c>Content</c> rather than copying
	/// it to a readable filesystem).</para>
	/// </summary>
	internal static class GoldenStore
	{
		private const string GoldensFolder = "Goldens";

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

		/// <summary>
		/// The default golden key for a cell, relative to the <c>Goldens</c> root:
		/// <c>{renderer}.{platform}/{scene}.png</c>. This is the path the
		/// captured-image marker carries and the harvest script writes to by
		/// default; a promoted, platform-portable golden lives at the shared
		/// <c>{renderer}/{scene}.png</c> key instead.
		/// </summary>
		public static string Key(string rendererName, string sceneName) =>
			$"{rendererName}.{VisualPlatform.Tag}/{sceneName}.png";

		/// <summary>
		/// Golden keys for a cell in lookup order (most specific first):
		/// the per-platform override, then the platform-portable renderer golden.
		/// </summary>
		public static IEnumerable<string> Candidates(string rendererName, string sceneName)
		{
			yield return $"{rendererName}.{VisualPlatform.Tag}/{sceneName}.png";
			yield return $"{rendererName}/{sceneName}.png";
		}

		// Returns the decoded golden (RGBA8888/Premul, sized to info) or null when
		// no golden exists for this cell on this platform.
		public static ResolvedGolden? TryLoad(string rendererName, string sceneName, SKImageInfo info)
		{
			foreach (var key in Candidates(rendererName, sceneName))
			{
				var relative = $"{GoldensFolder}/{key}";

				var diskPath = FindOnDisk(relative);
				if (diskPath is not null)
					return new ResolvedGolden(Decode(File.ReadAllBytes(diskPath), info, diskPath), diskPath);

				var resource = FindEmbedded(relative);
				if (resource is not null)
					return new ResolvedGolden(Decode(resource.Value.Bytes, info, resource.Value.Name), resource.Value.Name);
			}

			return null;
		}

		// PNG-encodes captured RGBA8888/Premul pixels. Used to emit the actual
		// image (base64) into the test log and to write local failure artifacts.
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

		// ---- PNG decode ----

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
		// the inner loop can edit a golden in the source tree and re-run without a
		// rebuild. Unreachable on device/browser hosts (those read the embedded
		// resource instead).
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

		private static string ToPlatformPath(string relative) =>
			relative.Replace('/', Path.DirectorySeparatorChar);
	}
}
