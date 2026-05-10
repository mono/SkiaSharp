using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Base class for visual tests. Subclass and write tests that take a
	/// <see cref="VisualSetup"/> from <see cref="AllSetups"/> (or one of the
	/// filtered MemberData sources) and call <see cref="VerifyDraw"/>:
	///
	/// <code>
	/// public class MyVisualTests : VisualTestBase
	/// {
	///     [Theory, MemberData(nameof(AllSetups))]
	///     public void RedSquare(VisualSetup setup) =>
	///         VerifyDraw (setup, "red_square", new SKImageInfo (64, 64), canvas => {
	///             canvas.Clear (SKColors.White);
	///             using var p = new SKPaint { Color = SKColors.Red };
	///             canvas.DrawRect (16, 16, 32, 32, p);
	///         });
	/// }
	/// </code>
	///
	/// First run with no golden file: the framework writes the actual image to
	/// the failure-inspection directory and fails the test. To accept a new
	/// golden, set <c>SKIASHARP_UPDATE_GOLDENS=1</c> and re-run — the framework
	/// will write the rendered output to the source-tree <c>Goldens/</c>
	/// directory. Subsequent runs without the env var will compare.
	///
	/// Tolerance: <see cref="MaxChannelDelta"/> is the per-channel max
	/// allowable difference. Default 2 (catch real regressions, ride out
	/// 1-bit rounding in software rasterizers). Override per-test if needed.
	/// </summary>
	public abstract class VisualTestBase : BaseTest
	{
		private const string EnvUpdateGoldens = "SKIASHARP_UPDATE_GOLDENS";

		// Setups are cheap value-y objects: they hold a Name + an availability
		// probe but NOT any SKObject GPU resources. Per-test surfaces (returned
		// by VisualSetup.CreateSurface) own the GRContext/SKGraphiteContext/etc.
		// and dispose them all together — keeps the assembly-level leak check
		// in GarbageCleanupFixture happy.
		//
		// MemberData methods are evaluated by xUnit at TEST DISCOVERY TIME (not
		// run time), so they must not allocate GPU resources.

		public static IEnumerable<object[]> AllSetups () => Setups (
			new RasterSetup (),
			new GaneshVulkanSetup (),
			new GraphiteVulkanSetup (),
			new MetalSetup ());

		public static IEnumerable<object[]> GpuSetups () => Setups (
			new GaneshVulkanSetup (),
			new GraphiteVulkanSetup (),
			new MetalSetup ());

		public static IEnumerable<object[]> RasterSetups () => Setups (
			new RasterSetup ());

		public static IEnumerable<object[]> GraphiteSetups () => Setups (
			new GraphiteVulkanSetup (),
			new MetalSetup ());

		public static IEnumerable<object[]> GaneshSetups () => Setups (
			new GaneshVulkanSetup ());

		private static IEnumerable<object[]> Setups (params VisualSetup[] setups) =>
			setups.Select (s => new object[] { s });

		/// <summary>Default per-channel tolerance. Override per-test as needed.</summary>
		protected virtual int MaxChannelDelta => 2;

		/// <summary>
		/// Render <paramref name="draw"/> into a fresh surface from
		/// <paramref name="setup"/> and compare against the golden file
		/// at <c>Goldens/{setup.Name}/{goldenName}.png</c>. Skips if the
		/// setup is unavailable. Fails with a clear message + saved actual/diff
		/// images if the comparison fails or the golden is missing.
		/// </summary>
		protected void VerifyDraw (VisualSetup setup, string goldenName, SKImageInfo info, Action<SKCanvas> draw)
		{
			Skip.IfNot (setup.IsAvailable, $"Setup '{setup.Name}' unavailable: {setup.UnavailableReason}");

			byte[] actualPixels;
			using (var surface = setup.CreateSurface (info)) {
				draw (surface.Canvas);
				actualPixels = surface.ReadPixels ();
			}

			var rgbaInfo = new SKImageInfo (info.Width, info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
			Compare (setup.Name, goldenName, rgbaInfo, actualPixels);
		}

		private void Compare (string setupName, string goldenName, SKImageInfo info, byte[] actual)
		{
			var sourceGoldenPath  = SourceGoldenPath (setupName, goldenName);
			var runtimeGoldenPath = RuntimeGoldenPath (setupName, goldenName);

			// Prefer the source-tree golden (lets devs edit + run without rebuild);
			// fall back to the runtime copy that the build pipeline copied.
			var goldenPath = File.Exists (sourceGoldenPath) ? sourceGoldenPath
			               : File.Exists (runtimeGoldenPath) ? runtimeGoldenPath
			               : null;

			var update = (Environment.GetEnvironmentVariable (EnvUpdateGoldens) ?? "").Trim ();
			var updateMode = update == "1" || update.Equals ("true", StringComparison.OrdinalIgnoreCase);

			if (goldenPath == null) {
				if (updateMode) {
					WritePng (sourceGoldenPath, info, actual);
					return;
				}
				var actualOut = WritePng (FailurePath (setupName, goldenName, ".actual.png"), info, actual);
				throw new Xunit.Sdk.XunitException (
					$"No golden image at '{sourceGoldenPath}'. Actual output saved to '{actualOut}'. " +
					$"To accept this output as the new golden, re-run with {EnvUpdateGoldens}=1.");
			}

			var golden = LoadPngAsRgba (goldenPath, info);
			var diff   = ComputeDiff (actual, golden, info, MaxChannelDelta);

			if (diff.Failed) {
				if (updateMode) {
					WritePng (sourceGoldenPath, info, actual);
					return;
				}
				var actualOut = WritePng (FailurePath (setupName, goldenName, ".actual.png"), info, actual);
				var diffOut   = WritePng (FailurePath (setupName, goldenName, ".diff.png"),   info, diff.DiffImage);
				throw new Xunit.Sdk.XunitException (
					$"Visual diff against '{goldenPath}': max channel delta {diff.MaxDelta} exceeds tolerance {MaxChannelDelta}. " +
					$"Mismatched pixels: {diff.MismatchCount}/{info.Width * info.Height}. " +
					$"Actual: '{actualOut}', diff: '{diffOut}'. " +
					$"If this output is correct, re-run with {EnvUpdateGoldens}=1 to update the golden.");
			}
		}

		// ---- Path resolution ----

		// The runtime location is where the test runner actually executes from
		// (bin/.../net10.0/Goldens/...). Goldens get copied there by the project.
		private static string RuntimeGoldenPath (string setup, string golden) =>
			Path.Combine (RuntimeRoot, "Goldens", setup, golden + ".png");

		// The source-tree location is where the goldens are committed
		// (tests/Tests/SkiaSharp/Visual/Goldens/...). Found by walking up from
		// the test assembly's directory until we hit the source-tree pattern.
		private static string SourceGoldenPath (string setup, string golden) =>
			Path.Combine (SourceRoot, "Goldens", setup, golden + ".png");

		private static string FailurePath (string setup, string golden, string suffix) =>
			Path.Combine (RuntimeRoot, "_failures", setup, golden + suffix);

		private static string RuntimeRoot =>
			Path.GetDirectoryName (typeof (VisualTestBase).Assembly.Location);

		private static string sourceRootCache;
		private static string SourceRoot {
			get {
				if (sourceRootCache != null) return sourceRootCache;
				// Walk up from the assembly until we find a `tests/Tests/SkiaSharp/Visual` directory.
				var dir = RuntimeRoot;
				for (int i = 0; i < 12 && dir != null; i++) {
					var candidate = Path.Combine (dir, "tests", "Tests", "SkiaSharp", "Visual");
					if (Directory.Exists (candidate)) {
						sourceRootCache = candidate;
						return sourceRootCache;
					}
					dir = Path.GetDirectoryName (dir);
				}
				// Fallback: keep goldens next to the test assembly. Update mode will
				// just write here and require a manual copy back to source.
				sourceRootCache = Path.Combine (RuntimeRoot, "Goldens-source-fallback");
				return sourceRootCache;
			}
		}

		// ---- PNG read/write via SkiaSharp itself ----

		private static string WritePng (string path, SKImageInfo info, byte[] rgbaPixels)
		{
			Directory.CreateDirectory (Path.GetDirectoryName (path));
			using var pixmap  = new SKPixmap (info, PinPixels (rgbaPixels));
			using var image   = SKImage.FromPixels (pixmap);
			using var encoded = image.Encode (SKEncodedImageFormat.Png, 100);
			File.WriteAllBytes (path, encoded.ToArray ());
			return path;
		}

		private static byte[] LoadPngAsRgba (string path, SKImageInfo expectedInfo)
		{
			using var data = SKData.Create (path);
			using var codec = SKCodec.Create (data);
			if (codec == null)
				throw new InvalidOperationException ($"Failed to decode golden PNG at '{path}'");
			if (codec.Info.Width != expectedInfo.Width || codec.Info.Height != expectedInfo.Height)
				throw new InvalidOperationException ($"Golden PNG '{path}' is {codec.Info.Width}x{codec.Info.Height} but the test rendered {expectedInfo.Width}x{expectedInfo.Height}");
			var pixels = new byte[expectedInfo.BytesSize];
			unsafe {
				fixed (byte* p = pixels) {
					var result = codec.GetPixels (expectedInfo, (IntPtr)p);
					if (result != SKCodecResult.Success)
						throw new InvalidOperationException ($"SKCodec.GetPixels failed for '{path}': {result}");
				}
			}
			return pixels;
		}

		private static IntPtr PinPixels (byte[] pixels)
		{
			// Caller guarantees pixels stays alive for the duration of the SKPixmap
			// (pixmap is used only inside WritePng, returns before pixels can be GC'd).
			return System.Runtime.InteropServices.GCHandle.Alloc (
				pixels, System.Runtime.InteropServices.GCHandleType.Pinned).AddrOfPinnedObject ();
		}

		// ---- Pixel comparison ----

		private struct DiffResult
		{
			public bool   Failed;
			public int    MaxDelta;
			public int    MismatchCount;
			public byte[] DiffImage;     // RGBA: red=high diff, green=in tolerance, original=identical
		}

		private static DiffResult ComputeDiff (byte[] actual, byte[] golden, SKImageInfo info, int tolerance)
		{
			if (actual.Length != golden.Length) {
				return new DiffResult {
					Failed = true,
					MaxDelta = 255,
					MismatchCount = info.Width * info.Height,
					DiffImage = new byte[actual.Length],
				};
			}
			var diffImage = new byte[actual.Length];
			int maxDelta = 0;
			int mismatch = 0;
			for (int i = 0; i < actual.Length; i += 4) {
				int dR = Math.Abs (actual[i + 0] - golden[i + 0]);
				int dG = Math.Abs (actual[i + 1] - golden[i + 1]);
				int dB = Math.Abs (actual[i + 2] - golden[i + 2]);
				int dA = Math.Abs (actual[i + 3] - golden[i + 3]);
				int max = Math.Max (Math.Max (dR, dG), Math.Max (dB, dA));
				if (max > maxDelta) maxDelta = max;
				if (max > tolerance) {
					mismatch++;
					// Highlight as bright red in the diff image.
					diffImage[i + 0] = 255;
					diffImage[i + 1] = 0;
					diffImage[i + 2] = 0;
					diffImage[i + 3] = 255;
				} else if (max > 0) {
					// In-tolerance differences: amber.
					diffImage[i + 0] = 255;
					diffImage[i + 1] = 200;
					diffImage[i + 2] = 0;
					diffImage[i + 3] = 255;
				} else {
					// Bit-exact: dim grey so the eye can see the shape of the rendered scene.
					diffImage[i + 0] = (byte)(actual[i + 0] / 4);
					diffImage[i + 1] = (byte)(actual[i + 1] / 4);
					diffImage[i + 2] = (byte)(actual[i + 2] / 4);
					diffImage[i + 3] = 255;
				}
			}
			return new DiffResult {
				Failed = mismatch > 0,
				MaxDelta = maxDelta,
				MismatchCount = mismatch,
				DiffImage = diffImage,
			};
		}
	}
}
