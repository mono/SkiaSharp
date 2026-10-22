using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp.Tests.Goldens;
using Xunit;

namespace SkiaSharp.Tests.Visual.Tests
{
	/// <summary>
	/// The shared engine behind every visual-regression test. One test is one
	/// <c>(renderer × scene)</c> pair: it renders the scene with the renderer and
	/// compares the pixels to a committed golden via
	/// <see cref="SkiaSharp.Extended.SKPixelComparer"/> with a per-renderer
	/// tolerance (see <see cref="GoldenTolerance"/>). Golden lookup is handled by
	/// <see cref="GoldenStore"/>.
	///
	/// <para>The pipeline lives here so hosts can contribute their own renderers:
	/// <see cref="VisualMatrixTests"/> drives every auto-discovered renderer in the
	/// base test assembly, and a satellite host project (Vulkan, Direct3D) adds a
	/// thin subclass that drives only the renderers it compiles in.</para>
	///
	/// <para>Results are published as base64 markers in the TRX, the one output
	/// channel that exists uniformly on desktop, device and browser. That is how
	/// goldens are seeded — run the suite, harvest with
	/// <c>scripts/infra/tests/extract-visual-goldens.py</c>, commit, re-run. A
	/// write-to-disk record mode could never seed the device and browser hosts,
	/// which have no writable source tree.</para>
	///
	/// <para>A test skips only when <see cref="SkiaSharp.Tests.GpuPolicy"/> says the
	/// backend is not required on this host. A render that throws, pixels outside
	/// tolerance, and a missing golden are all failures.</para>
	///
	/// <para>Every test is tagged <c>[Trait("Category", "Visual")]</c>: select the
	/// suite with <c>--filter-trait "Category=Visual"</c>
	/// (Microsoft.Testing.Platform) or <c>--filter "Category=Visual"</c> (VSTest),
	/// and exclude it with <c>--filter-not-trait "Category=Visual"</c>.</para>
	/// </summary>
	public abstract class VisualMatrixTestsBase : SKTest
	{
		/// <summary>
		/// Trait value tagging every visual test. Lets CI and developers run or skip
		/// just the visual suite without naming individual classes.
		/// </summary>
		public const string VisualCategory = "Visual";

		// Capture identity and the chosen reference are separate fields, so export
		// never overwrites a shared reference just because this host used it.
		public const string ActualImageMarker = "##SKIA-GOLDEN## role=actual";
		public const string GoldenImageMarker = "##SKIA-GOLDEN## role=reference";
		public const string DiffImageMarker = "##SKIA-GOLDEN## role=diff";

		protected VisualMatrixTestsBase(ITestOutputHelper output)
			: base(output)
		{
		}

		/// <summary>
		/// Renders one scene with one renderer and asserts the pixels match the
		/// committed golden. This is the single shared code path for every host: the
		/// base matrix and each satellite subclass call straight into it. The
		/// <paramref name="renderer"/> is owned by its catalog (constructed once,
		/// reused); this method never disposes it.
		/// </summary>
		protected async Task RunTestAsync(IRenderer renderer, ISkiaScene scene)
		{
			GpuPolicy.RequireOrSkip(renderer.Name);

			var info = scene.Info;

			var actual = await renderer.RenderAsync(scene, info, CancellationToken.None);

			var resolution = GoldenStore.Resolve(renderer.Name, scene.Name);
			WriteOutput(resolution.Describe());
			EmitActual(renderer.Name, scene.Name, info, actual, resolution);

			if (resolution.Key is null)
			{
				FailUnseeded(renderer.Name, scene.Name, info, actual, resolution);
				return;
			}

			CompareOrFail(renderer.Name, scene.Name, info, actual, GoldenStore.Load(resolution, info));
		}

		// Harvesting this marker from the TRX is how goldens are created, so it
		// carries the pixels verbatim.
		private void EmitActual(string rendererName, string sceneName, SKImageInfo info, byte[] actual, GoldenResolution resolution) =>
			Emit(ActualImageMarker, info, GoldenStore.EncodePng(actual, info), resolution);

		private void EmitImage(string marker, SKImageInfo info, SKImage image, GoldenResolution resolution)
		{
			using var data = image.Encode(SKEncodedImageFormat.Png, 100);
			Emit(marker, info, data.ToArray(), resolution);
		}

		private void Emit(string marker, SKImageInfo info, byte[] png, GoldenResolution resolution)
		{
			var normalized = RendererPixels.NormalizedInfo(info);
			WriteOutput(
				$"{marker} key={GoldenStore.CaptureKey(resolution.Request)} selected={resolution.Key ?? "-"} " +
				$"size={normalized.Width}x{normalized.Height} base64={Convert.ToBase64String(png)}");
		}

		private void CompareOrFail(string rendererName, string sceneName, SKImageInfo info, byte[] actual, GoldenStore.ResolvedGolden golden)
		{
			var normalized = RendererPixels.NormalizedInfo(info);
			var tolerance = GoldenTolerance.For(rendererName);

			using var actualImage = ToImage(actual, normalized);
			using var goldenImage = ToImage(golden.Pixels, normalized);

			var result = SkiaSharp.Extended.SKPixelComparer.Compare(goldenImage, actualImage, tolerance.ChannelTolerance);
			var allowedOutliers = (long)Math.Floor(result.TotalPixels * tolerance.MaxOutlierFraction);

			using var diffImage = SkiaSharp.Extended.SKPixelComparer.GenerateDifferenceImage(goldenImage, actualImage, tolerance.ChannelTolerance);

			EmitImage(GoldenImageMarker, info, goldenImage, golden.Resolution);
			EmitImage(DiffImageMarker, info, diffImage, golden.Resolution);

			if (result.ErrorPixelCount <= allowedOutliers)
				return;

			var actualPath = TrySave(() => GoldenStore.SaveFailureArtifact(rendererName, sceneName, ".actual.png", actual, info));
			var diffPath = TrySave(() => GoldenStore.SaveFailureImage(rendererName, sceneName, ".diff.png", diffImage));

			Assert.Fail(
				$"Visual regression for '{rendererName}/{sceneName}' against golden '{golden.Location}'. " +
				$"{result.ErrorPixelCount}/{result.TotalPixels} pixels exceed the per-channel tolerance of {tolerance.ChannelTolerance} " +
				$"(allowed outliers: {allowedOutliers}); max observed channel delta {result.MaxChannelDelta}. " +
				ArtifactSuffix(actualPath, diffPath) +
				"The rendered PNG is retained in the test results. " +
				"If this change is expected, harvest it with scripts/infra/tests/extract-visual-goldens.py and commit; " +
				"otherwise fix the regression.");
		}

		private void FailUnseeded(string rendererName, string sceneName, SKImageInfo info, byte[] actual, GoldenResolution resolution)
		{
			var actualPath = TrySave(() => GoldenStore.SaveFailureArtifact(rendererName, sceneName, ".actual.png", actual, info));

			Assert.Fail(
				$"No golden recorded yet for '{rendererName}/{sceneName}'. {resolution.Describe()} " +
				"The rendered PNG is retained in the test results; " +
				"seed it with scripts/infra/tests/extract-visual-goldens.py and commit. " +
				ArtifactSuffix(actualPath, null));
		}

		private static SKImage ToImage(byte[] rgba, SKImageInfo info) =>
			SKImage.FromPixelCopy(RendererPixels.NormalizedInfo(info), rgba);

		private static string TrySave(Func<string> save)
		{
			try
			{
				return save();
			}
			catch
			{
				return null;
			}
		}

		private static string ArtifactSuffix(string actualPath, string diffPath)
		{
			var parts = new List<string>();
			if (actualPath is not null)
				parts.Add($"actual: '{actualPath}'");
			if (diffPath is not null)
				parts.Add($"diff: '{diffPath}'");
			return parts.Count == 0 ? "" : "Saved " + string.Join(", ", parts) + ". ";
		}
	}
}
