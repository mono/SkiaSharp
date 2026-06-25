using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests.Visual.Tests
{
	/// <summary>
	/// The shared engine behind every visual-regression cell. A "cell" is one
	/// <c>(renderer × scene)</c> pair: it renders the scene with the renderer and
	/// compares the pixels to a committed golden via
	/// <see cref="SkiaSharp.Extended.SKPixelComparer"/> with a per-renderer
	/// tolerance (see <see cref="GoldenTolerance"/>). Golden lookup is handled by
	/// <see cref="GoldenStore"/>.
	///
	/// <para>
	/// This base class owns the per-cell pipeline (<see cref="RunCellAsync"/>) so
	/// it can be reused by hosts that contribute their own renderers. The shared
	/// <see cref="VisualMatrixTests"/> drives every auto-discovered renderer in the
	/// base test assembly; a satellite host project (Vulkan, Direct3D) adds a thin
	/// subclass that drives only the renderers it compiles in. Both share the exact
	/// same emit / compare / fail discipline below.
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
	public abstract class VisualMatrixTestsBase : SKTest
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

		/// <summary>
		/// Line prefix for a per-cell outcome marker emitted into the test log after
		/// the comparison decision. The full line is
		/// <c>##SKIA-VISUAL-CELL## path={renderer}.{platform}/{scene}.png outcome={pass|mismatch|unseeded}</c>.
		/// It lets the triage extractor (<c>extract-visual-goldens.py --failures-out</c>)
		/// tell an <i>unseeded</i> cell (harvest its golden) apart from a <i>mismatch</i>
		/// (a regression to investigate) — both otherwise emit the same captured PNG.
		/// </summary>
		public const string VisualCellMarker = "##SKIA-VISUAL-CELL##";

		/// <summary>
		/// Line prefix for the golden/diff images emitted alongside a failing cell,
		/// so a red cell is triageable as browsable PNGs straight from the published
		/// TRX. The full line is
		/// <c>##SKIA-VISUAL-IMAGE## path={renderer}.{platform}/{scene}.{golden|diff}.png size=WxH base64=...</c>.
		/// The captured (actual) image is the existing <see cref="GoldenImageMarker"/>.
		/// </summary>
		public const string VisualImageMarker = "##SKIA-VISUAL-IMAGE##";

		protected VisualMatrixTestsBase(ITestOutputHelper output)
			: base(output)
		{
		}

		/// <summary>
		/// Renders one cell and asserts the pixels match the committed golden.
		/// This is the single shared code path for every host: the base matrix and
		/// each satellite subclass call straight into it. The
		/// <paramref name="renderer"/> is owned by its catalog (constructed once,
		/// reused); this method never disposes it.
		/// </summary>
		protected async Task RunCellAsync(IRenderer renderer, ISkiaScene scene)
		{
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

		// Records this cell's verdict so the triage extractor can separate an
		// unseeded cell (harvest the captured PNG into a golden) from a real
		// mismatch (investigate the regression) — both otherwise look identical.
		private void EmitOutcome(string rendererName, string sceneName, string outcome) =>
			WriteOutput($"{VisualCellMarker} path={GoldenStore.Key(rendererName, sceneName)} outcome={outcome}");

		// Emits a failing cell's golden or diff image (kind = "golden" | "diff") as
		// a single-line marker so the published TRX carries everything needed to
		// triage the failure as browsable PNGs, on every host.
		private void EmitVisualImage(string rendererName, string sceneName, string kind, SKImageInfo info, SKImage image)
		{
			var normalized = RendererPixels.NormalizedInfo(info);
			using var data = image.Encode(SKEncodedImageFormat.Png, 100);
			var key = $"{rendererName}.{VisualPlatform.Tag}/{sceneName}.{kind}.png";
			WriteOutput(
				$"{VisualImageMarker} path={key} " +
				$"size={normalized.Width}x{normalized.Height} base64={Convert.ToBase64String(data.ToArray())}");
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
			{
				EmitOutcome(rendererName, sceneName, "pass");
				return;
			}

			using var diffImage = SkiaSharp.Extended.SKPixelComparer.GenerateDifferenceImage(goldenImage, actualImage, tolerance.ChannelTolerance);

			// Emit the golden and diff as structured markers (red = over tolerance,
			// amber = minor) so the published TRX carries actual+golden+diff for
			// browsable triage, then tag the cell as a mismatch.
			EmitVisualImage(rendererName, sceneName, "golden", info, goldenImage);
			EmitVisualImage(rendererName, sceneName, "diff", info, diffImage);
			EmitOutcome(rendererName, sceneName, "mismatch");

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
			EmitOutcome(rendererName, sceneName, "unseeded");

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
