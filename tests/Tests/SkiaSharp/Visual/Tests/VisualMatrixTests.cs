using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests.Visual.Tests
{
	/// <summary>
	/// The visual-regression matrix: one theory cell per
	/// <c>(renderer × scene)</c>. xUnit fans the test out across every
	/// combination, so adding a scene gives every renderer a new row and adding a
	/// renderer gives every scene a new column — the single seam the Graphite
	/// backend plugs into.
	///
	/// <para>
	/// Each cell renders the scene with the renderer and compares the pixels to a
	/// committed golden via <see cref="SkiaSharp.Extended.SKPixelComparer"/> with a
	/// per-renderer tolerance (see <see cref="GoldenTolerance"/>). Golden lookup is
	/// handled by <see cref="GoldenStore"/>.
	/// </para>
	///
	/// <para><b>Every cell emits its rendered PNG into the test results</b>, on
	/// pass <i>and</i> fail, as a single-line <c>##SKIA-GOLDEN-IMAGE##</c> marker
	/// carrying the golden path and base64 bytes. The TRX is the one output channel
	/// that exists uniformly on every host — desktop, device, and browser — so it
	/// is how goldens are seeded: run the matrix, harvest the captured PNGs from the
	/// TRX with <c>scripts/infra/tests/extract-visual-goldens.py</c>, commit them,
	/// and re-run. There is no in-process record mode and no environment variable;
	/// the device/browser hosts have no writable source tree, so a write-to-disk
	/// record path could never seed them.</para>
	///
	/// <para><b>Discipline.</b> A cell <i>skips</i> only when the backend is
	/// genuinely absent on this host — the renderer reports
	/// <see cref="IRenderer.IsAvailable"/> = <see langword="false"/>, or
	/// <see cref="IRenderer.RenderAsync"/> throws
	/// <see cref="RendererUnavailableException"/>. Every other outcome is a hard
	/// <b>failure</b>: a render that throws anything else, pixels that differ from a
	/// golden that exists, or <i>no golden recorded yet</i> for this cell on this
	/// platform (the captured PNG is in the TRX — harvest and commit it, then the
	/// cell goes green). There is no path that downgrades a real regression to a
	/// skip or a warning.</para>
	///
	/// <para><b>Selecting the suite.</b> Every cell is tagged
	/// <c>[Trait("Category", "Visual")]</c>. Run only the visual matrix with
	/// <c>--filter-trait "Category=Visual"</c> (Microsoft.Testing.Platform) or
	/// <c>--filter "Category=Visual"</c> (VSTest); skip it everywhere else with
	/// <c>--filter-not-trait "Category=Visual"</c>. The cells run as part of the
	/// normal test run when no filter is supplied.</para>
	/// </summary>
	[Trait("Category", VisualCategory)]
	public class VisualMatrixTests : SKTest
	{
		/// <summary>
		/// Trait value tagging every visual-matrix cell. Lets CI and developers
		/// run or skip just the visual suite without naming individual classes.
		/// </summary>
		public const string VisualCategory = "Visual";

		/// <summary>
		/// Line prefix for the captured-image marker emitted into the test log.
		/// The full line is
		/// <c>##SKIA-GOLDEN-IMAGE## path={renderer}.{platform}/{scene}.png size=WxH base64=...</c>.
		/// Kept on a single line (base64 has no whitespace and is XML-safe) so the
		/// harvest script can extract it from the TRX with a simple per-line scan.
		/// </summary>
		public const string GoldenImageMarker = "##SKIA-GOLDEN-IMAGE##";

		public VisualMatrixTests(ITestOutputHelper output)
			: base(output)
		{
		}

		[Theory]
		[MemberData(nameof(Matrix))]
		public async Task RenderMatchesGolden(string rendererName, string sceneName)
		{
			var renderer = RendererCatalog.Get(rendererName);
			var scene = SceneCatalog.Get(sceneName);

			if (!renderer.IsAvailable)
				Assert.Skip($"Renderer '{renderer.Name}' is unavailable on this host: {renderer.UnavailableReason}");

			var info = scene.Info;

			byte[] actual;
			try
			{
				actual = await renderer.RenderAsync(scene, info, CancellationToken.None);
			}
			catch (RendererUnavailableException ex)
			{
				Assert.Skip($"Renderer '{renderer.Name}' could not run scene '{scene.Name}' on this host: {ex.Message}");
				return;
			}

			// Always publish the rendered PNG into the test results, pass or fail.
			// This is the seed channel (harvest from the TRX) and lets a passing
			// cell still be eyeballed against its golden when tolerance is in play.
			EmitGoldenImage(renderer.Name, scene.Name, info, actual);

			var golden = GoldenStore.TryLoad(renderer.Name, scene.Name, info);
			if (golden is null)
			{
				FailUnseeded(renderer.Name, scene.Name, info, actual);
				return;
			}

			CompareOrFail(renderer.Name, scene.Name, info, actual, golden.Value);
		}

		public static IEnumerable<object[]> Matrix()
		{
			foreach (var rendererName in RendererCatalog.AllNames)
				foreach (var sceneName in SceneCatalog.AllNames)
					yield return new object[] { rendererName, sceneName };
		}

		// Emits the captured pixels as a single-line, machine-parseable marker so
		// the harvest script can reconstruct the golden file verbatim from the TRX.
		private void EmitGoldenImage(string rendererName, string sceneName, SKImageInfo info, byte[] actual)
		{
			var normalized = RendererPixels.NormalizedInfo(info);
			var png = GoldenStore.EncodePng(actual, info);
			var base64 = Convert.ToBase64String(png);
			WriteOutput(
				$"{GoldenImageMarker} path={GoldenStore.Key(rendererName, sceneName)} " +
				$"size={normalized.Width}x{normalized.Height} base64={base64}");
		}

		private void CompareOrFail(string rendererName, string sceneName, SKImageInfo info, byte[] actual, GoldenStore.ResolvedGolden golden)
		{
			var normalized = RendererPixels.NormalizedInfo(info);
			var tolerance = GoldenTolerance.For(rendererName, sceneName);

			using var actualImage = ToImage(actual, normalized);
			using var goldenImage = ToImage(golden.Pixels, normalized);

			var result = SkiaSharp.Extended.SKPixelComparer.Compare(goldenImage, actualImage, tolerance.ChannelTolerance);
			var allowedOutliers = (long)Math.Floor(result.TotalPixels * tolerance.MaxOutlierFraction);

			if (result.ErrorPixelCount <= allowedOutliers)
				return;

			using var diffImage = SkiaSharp.Extended.SKPixelComparer.GenerateDifferenceImage(goldenImage, actualImage, tolerance.ChannelTolerance);

			WriteOutput(goldenImage, $"GOLDEN {rendererName}/{sceneName} ({golden.Location})");
			WriteOutput(diffImage, $"DIFF {rendererName}/{sceneName} (red = over tolerance, amber = minor)");

			var actualPath = TrySave(() => GoldenStore.SaveFailureArtifact(rendererName, sceneName, ".actual.png", actual, info));
			var diffPath = TrySave(() => GoldenStore.SaveFailureImage(rendererName, sceneName, ".diff.png", diffImage));

			Assert.Fail(
				$"Visual regression for '{rendererName}/{sceneName}' against golden '{golden.Location}'. " +
				$"{result.ErrorPixelCount}/{result.TotalPixels} pixels exceed the per-channel tolerance of {tolerance.ChannelTolerance} " +
				$"(allowed outliers: {allowedOutliers}); max observed channel delta {result.MaxChannelDelta}. " +
				ArtifactSuffix(actualPath, diffPath) +
				"The rendered PNG is in the test results as a ##SKIA-GOLDEN-IMAGE## marker. " +
				"If this change is expected, harvest it with scripts/infra/tests/extract-visual-goldens.py and commit; " +
				"otherwise fix the regression.");
		}

		private void FailUnseeded(string rendererName, string sceneName, SKImageInfo info, byte[] actual)
		{
			var actualPath = TrySave(() => GoldenStore.SaveFailureArtifact(rendererName, sceneName, ".actual.png", actual, info));
			var looked = string.Join(" or ", GoldenStore.Candidates(rendererName, sceneName).Select(k => "Goldens/" + k));

			// The backend ran and produced pixels, but no reference is committed for
			// this cell on this platform. That is a hard failure, not a skip: the
			// captured PNG is already in the test results (the ##SKIA-GOLDEN-IMAGE##
			// marker above), so seed it by harvesting the TRX and committing the
			// result, after which the cell compares strictly and goes green.
			Assert.Fail(
				$"No golden recorded yet for '{rendererName}/{sceneName}' on '{VisualPlatform.Tag}' " +
				$"(looked for {looked}). " +
				"The rendered PNG is in the test results as a ##SKIA-GOLDEN-IMAGE## marker; " +
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
