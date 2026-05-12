using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Base class for visual tests. Provides the matrix theory data
	/// (<see cref="Matrix"/>) — every (renderer × scene) pair where the
	/// renderer is compatible with the scene's requirements — and the
	/// per-cell comparison harness (<see cref="VerifyScene"/>).
	///
	/// <para>
	/// Theory data is passed as <c>(string rendererName, string sceneName)</c>
	/// (xUnit-serializable). The catalog lookup happens inside the test body,
	/// so discovery never instantiates GPU contexts.
	/// </para>
	///
	/// <para>
	/// <b>Golden file lookup</b>: per-renderer override first, shared fallback:
	/// <list type="number">
	///   <item><c>Goldens/{renderer.Name}/{scene.Name}.png</c> — explicit override</item>
	///   <item><c>Goldens/_shared/{scene.Name}.png</c> — canonical baseline</item>
	/// </list>
	/// </para>
	///
	/// <para>
	/// <b>Update mode</b>: set <c>SKIASHARP_UPDATE_GOLDENS=1</c>. By default
	/// writes to <c>_shared/</c>. To record a per-renderer override (accepted
	/// platform divergence), also set <c>SKIASHARP_GOLDEN_SCOPE=renderer</c>.
	/// </para>
	/// </summary>
	public abstract class VisualTestBase : BaseTest
	{
		private const string EnvUpdateGoldens   = "SKIASHARP_UPDATE_GOLDENS";
		private const string EnvGoldenScope     = "SKIASHARP_GOLDEN_SCOPE";
		private const string EnvRunVisualTests  = "SKIASHARP_VISUAL_TESTS";
		private const string SharedGoldensDir   = "_shared";

		/// <summary>Per-channel max delta. Software ICDs are typically
		/// bit-stable across hosts; 2 rides out 1-bit rounding while still
		/// catching real regressions.</summary>
		protected virtual int MaxChannelDelta => 2;

		/// <summary>Full (renderer × scene) matrix.</summary>
		public static IEnumerable<object[]> Matrix ()
		{
			// Note: this is evaluated AT DISCOVERY TIME. Cheap probes only —
			// no GPU resource allocation. RendererCatalog and SceneCatalog
			// both lazy-instantiate on first access, but constructors are
			// metadata-only by contract (see IRenderer doc).
			foreach (var renderer in RendererCatalog.All)
				foreach (var scene in SceneCatalog.All)
					yield return new object[] { renderer.Name, scene.Name };
		}

		/// <summary>
		/// Convenience: every available renderer's name. Useful for scenes
		/// that want to write one theory per scene (with per-scene overrides
		/// like tolerance) and fan it across renderers.
		/// </summary>
		public static IEnumerable<object[]> AllRenderers () =>
			RendererCatalog.AllNames.Select<string, object[]> (n => new object[] { n });

		/// <summary>Run a single (renderer, scene) cell.</summary>
		protected async Task VerifyScene (string rendererName, string sceneName, CancellationToken ct = default)
		{
			// Visual tests are opt-in: they need a published WASM payload, a
			// built Android APK or iOS .app, etc., and on machines without
			// them the matrix is mostly skips. Gate on an env var so a plain
			// `dotnet test` doesn't have to think about visual coverage.
			Skip.IfNot (IsTrue (Environment.GetEnvironmentVariable (EnvRunVisualTests)),
				$"Visual tests are opt-in. Set {EnvRunVisualTests}=1 to enable them " +
				$"(see documentation/dev/visual-tests.md).");

			var renderer = RendererCatalog.Get (rendererName);
			var scene    = SceneCatalog.Get (sceneName);

			Skip.IfNot (renderer.IsAvailable,
				$"Renderer '{renderer.Name}' unavailable: {renderer.UnavailableReason}");

			byte[] actual;
			try {
				actual = await renderer.RenderAsync (scene, scene.SuggestedInfo, ct);
			} catch (RendererUnavailableException ex) {
				// The renderer (or its transport) decided at runtime that
				// this host can't run it — missing driver feature, no
				// device attached, etc. Skip rather than fail; the matrix
				// is supposed to honestly report what each host can do.
				Skip.If (true, $"Renderer '{renderer.Name}' unavailable at runtime: {ex.Message}");
				return; // unreachable — Skip.If throws
			}
			var rgba = new SKImageInfo (scene.SuggestedInfo.Width, scene.SuggestedInfo.Height,
				SKColorType.Rgba8888, SKAlphaType.Premul);
			Compare (renderer.Name, scene.Name, rgba, actual);
		}

		// ---- Comparison ----

		private void Compare (string rendererName, string sceneName, SKImageInfo info, byte[] actual)
		{
			// Lookup: renderer-specific override → shared baseline.
			var goldenPath = ResolveGoldenForRead (rendererName, sceneName);

			var updateRequested = IsTrue (Environment.GetEnvironmentVariable (EnvUpdateGoldens));
			if (goldenPath == null) {
				if (updateRequested) {
					var writeTo = ResolveGoldenForWrite (rendererName, sceneName, exists: false);
					WritePng (writeTo, info, actual);
					return;
				}
				var actualMissingOut = WritePng (FailurePath (rendererName, sceneName, ".actual.png"), info, actual);
				var sharedPath   = SourceGoldenPath (SharedGoldensDir, sceneName);
				throw new Xunit.Sdk.XunitException (
					$"No golden image found for renderer '{rendererName}' scene '{sceneName}'. " +
					$"Looked in '{SourceGoldenPath (rendererName, sceneName)}' and '{sharedPath}'. " +
					$"Actual output saved to '{actualMissingOut}'. " +
					$"Re-run with {EnvUpdateGoldens}=1 to record as the shared golden (default), " +
					$"or {EnvUpdateGoldens}=1 {EnvGoldenScope}=renderer for a per-renderer override.");
			}

			var golden = LoadPngAsRgba (goldenPath, info);
			var diff   = ComputeDiff (actual, golden, info, MaxChannelDelta);
			if (!diff.Failed)
				return;

			if (updateRequested) {
				var writeTo = ResolveGoldenForWrite (rendererName, sceneName, exists: true);
				WritePng (writeTo, info, actual);
				return;
			}

			var actualOut = WritePng (FailurePath (rendererName, sceneName, ".actual.png"), info, actual);
			var diffOut   = WritePng (FailurePath (rendererName, sceneName, ".diff.png"),   info, diff.DiffImage);
			throw new Xunit.Sdk.XunitException (
				$"Visual diff against '{goldenPath}': max channel delta {diff.MaxDelta} exceeds tolerance {MaxChannelDelta}. " +
				$"Mismatched pixels: {diff.MismatchCount}/{info.Width * info.Height}. " +
				$"Actual: '{actualOut}', diff: '{diffOut}'. " +
				$"If this divergence is expected for this renderer, re-run with " +
				$"{EnvUpdateGoldens}=1 {EnvGoldenScope}=renderer to record a per-renderer override; " +
				$"otherwise fix the underlying regression.");
		}

		private static bool IsTrue (string v)
		{
			if (string.IsNullOrEmpty (v)) return false;
			v = v.Trim ();
			return v == "1" || v.Equals ("true", StringComparison.OrdinalIgnoreCase);
		}

		// ---- Path resolution ----

		private static string ResolveGoldenForRead (string rendererName, string sceneName)
		{
			// Source tree first (lets devs edit + run without rebuild), then
			// the build-copied runtime path. Renderer-specific override wins
			// over shared baseline.
			foreach (var dir in new[] { rendererName, SharedGoldensDir }) {
				var src = SourceGoldenPath (dir, sceneName);
				if (File.Exists (src)) return src;
				var rt = RuntimeGoldenPath (dir, sceneName);
				if (File.Exists (rt))  return rt;
			}
			return null;
		}

		private static string ResolveGoldenForWrite (string rendererName, string sceneName, bool exists)
		{
			// Default scope: shared (the common, expected case). Operators
			// explicitly opt into a per-renderer override for accepted
			// platform divergences.
			var scope = (Environment.GetEnvironmentVariable (EnvGoldenScope) ?? "").Trim ()
				.ToLowerInvariant ();
			var dir = scope == "renderer" ? rendererName : SharedGoldensDir;
			return SourceGoldenPath (dir, sceneName);
		}

		private static string RuntimeGoldenPath (string dir, string scene) =>
			Path.Combine (RuntimeRoot, "Goldens", dir, scene + ".png");

		private static string SourceGoldenPath (string dir, string scene) =>
			Path.Combine (SourceRoot, "Goldens", dir, scene + ".png");

		private static string FailurePath (string rendererName, string scene, string suffix) =>
			Path.Combine (RuntimeRoot, "_failures", rendererName, scene + suffix);

		// AppContext.BaseDirectory rather than Assembly.Location: net48's
		// vstest runner shadow-copies the test DLL to %TEMP%\<guid>\... and
		// Assembly.Location points there, but the Content goldens + the
		// source tree are reachable only from the original bin\ folder.
		// AppContext.BaseDirectory always returns the deployment directory.
		private static string RuntimeRoot => AppContext.BaseDirectory.TrimEnd (
			Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

		private static string sourceRootCache;
		private static string SourceRoot {
			get {
				if (sourceRootCache != null) return sourceRootCache;
				var dir = RuntimeRoot;
				for (int i = 0; i < 12 && dir != null; i++) {
					var candidate = Path.Combine (dir, "tests", "Tests", "SkiaSharp", "Visual");
					if (Directory.Exists (candidate)) {
						sourceRootCache = candidate;
						return sourceRootCache;
					}
					dir = Path.GetDirectoryName (dir);
				}
				sourceRootCache = Path.Combine (RuntimeRoot, "Goldens-source-fallback");
				return sourceRootCache;
			}
		}

		// ---- PNG read/write ----

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
			using var data  = SKData.Create (path);
			using var codec = SKCodec.Create (data)
				?? throw new InvalidOperationException ($"Failed to decode golden PNG at '{path}'");
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

		private static IntPtr PinPixels (byte[] pixels) =>
			System.Runtime.InteropServices.GCHandle.Alloc (
				pixels, System.Runtime.InteropServices.GCHandleType.Pinned).AddrOfPinnedObject ();

		// ---- Pixel comparison ----

		private struct DiffResult
		{
			public bool   Failed;
			public int    MaxDelta;
			public int    MismatchCount;
			public byte[] DiffImage;
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
					diffImage[i + 0] = 255; diffImage[i + 1] = 0;   diffImage[i + 2] = 0;   diffImage[i + 3] = 255;
				} else if (max > 0) {
					diffImage[i + 0] = 255; diffImage[i + 1] = 200; diffImage[i + 2] = 0;   diffImage[i + 3] = 255;
				} else {
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
