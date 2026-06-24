using System;
using System.Collections.Generic;
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
	/// Each cell renders the scene with the renderer, then compares the pixels to
	/// a committed golden via <see cref="SkiaSharp.Extended.SKPixelComparer"/>
	/// with a per-renderer tolerance (see <see cref="GoldenTolerance"/>). Golden
	/// lookup and recording are handled by <see cref="GoldenStore"/>.
	/// </para>
	///
	/// <para><b>Discipline.</b> A cell <i>skips</i> only when the backend is
	/// genuinely absent on this host — the renderer reports
	/// <see cref="IRenderer.IsAvailable"/> = <see langword="false"/>, or
	/// <see cref="IRenderer.RenderAsync"/> throws
	/// <see cref="RendererUnavailableException"/>. Every other outcome —
	/// a render that throws, pixels out of tolerance, or a missing golden — is a
	/// hard <b>failure</b>. There is no path that downgrades a real regression to a
	/// skip or a warning.</para>
	///
	/// <para><b>Recording goldens.</b> Run with <c>SKIASHARP_UPDATE_GOLDENS=1</c>
	/// to write goldens instead of comparing. The destination directory follows
	/// <c>SKIASHARP_GOLDEN_SCOPE</c> (<c>shared</c>|<c>renderer</c>|<c>platform</c>);
	/// the default records portable CPU output to <c>_shared/</c> and GPU output to
	/// a per-platform folder. Recording only works where the source tree is
	/// reachable (desktop Console runs).</para>
	///
	/// <para><b>Selecting the suite.</b> Every cell is tagged
	/// <c>[Trait("Category", "Visual")]</c>. Run only the visual matrix with
	/// <c>--filter-trait "Category=Visual"</c> (Microsoft.Testing.Platform) or
	/// <c>--filter "Category=Visual"</c> (VSTest); skip it everywhere else with
	/// <c>--filter-not-trait "Category=Visual"</c>. The cells still run by default
	/// when no filter is supplied.</para>
	/// </summary>
	[Trait("Category", VisualCategory)]
	public class VisualMatrixTests : SKTest
	{
		/// <summary>
		/// Trait value tagging every visual-matrix cell. Lets CI and developers
		/// run or skip just the visual suite without naming individual classes.
		/// </summary>
		public const string VisualCategory = "Visual";
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

			if (GoldenStore.UpdateRequested)
			{
				RecordGolden(renderer.Name, scene.Name, info, actual);
				return;
			}

			var golden = GoldenStore.TryLoad(renderer.Name, scene.Name, info);
			if (golden is null)
			{
				FailMissingGolden(renderer.Name, scene.Name, info, actual);
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

		private void RecordGolden(string rendererName, string sceneName, SKImageInfo info, byte[] actual)
		{
			try
			{
				var path = GoldenStore.Record(rendererName, sceneName, actual, info);
				WriteOutput($"Recorded golden for '{rendererName}/{sceneName}' at '{path}'.");
			}
			catch (Exception ex)
			{
				// Recording needs a writable source tree, which device/browser
				// hosts don't have. Surface the captured pixels so they can still
				// be retrieved from the log, then make the inability to record an
				// explicit failure rather than a silent no-op.
				using var actualImage = ToImage(actual, info);
				WriteOutput(actualImage, $"ACTUAL {rendererName}/{sceneName}");
				Assert.Fail($"Could not record golden for '{rendererName}/{sceneName}': {ex.Message}. " +
					"Recording is only supported where the source tree is writable (desktop Console runs). " +
					"The captured output is logged above as base64.");
			}
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
			WriteOutput(actualImage, $"ACTUAL {rendererName}/{sceneName}");
			WriteOutput(diffImage, $"DIFF {rendererName}/{sceneName} (red = over tolerance, amber = minor)");

			var actualPath = TrySave(() => GoldenStore.SaveFailureArtifact(rendererName, sceneName, ".actual.png", actual, info));
			var diffPath = TrySave(() => GoldenStore.SaveFailureImage(rendererName, sceneName, ".diff.png", diffImage));

			Assert.Fail(
				$"Visual regression for '{rendererName}/{sceneName}' against golden '{golden.Location}'. " +
				$"{result.ErrorPixelCount}/{result.TotalPixels} pixels exceed the per-channel tolerance of {tolerance.ChannelTolerance} " +
				$"(allowed outliers: {allowedOutliers}); max observed channel delta {result.MaxChannelDelta}. " +
				ArtifactSuffix(actualPath, diffPath) +
				$"If this change is expected, re-record with {GoldenStore.UpdateEnvVar}=1 " +
				$"(use {GoldenStore.ScopeEnvVar}=renderer or =platform for an override); otherwise fix the regression.");
		}

		private void FailMissingGolden(string rendererName, string sceneName, SKImageInfo info, byte[] actual)
		{
			using var actualImage = ToImage(actual, info);
			WriteOutput(actualImage, $"ACTUAL {rendererName}/{sceneName} (no golden on record)");

			var actualPath = TrySave(() => GoldenStore.SaveFailureArtifact(rendererName, sceneName, ".actual.png", actual, info));
			var looked = string.Join(", ", GoldenStore.ReadLocations(rendererName, sceneName));

			Assert.Fail(
				$"No golden image found for '{rendererName}/{sceneName}'. Looked in: {looked}. " +
				ArtifactSuffix(actualPath, null) +
				$"Re-run with {GoldenStore.UpdateEnvVar}=1 to record it (the captured output is logged above as base64).");
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
