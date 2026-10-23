using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp.Tests.Shared;
using Xunit;

namespace SkiaSharp.Tests.Visual.Tests
{
	/// <summary>
	/// The shared engine behind every visual-regression test. One test is one
	/// <c>(renderer × scene)</c> pair: it renders the scene with the renderer and
	/// compares the pixels to a committed golden via
	/// <see cref="GoldenImage"/> with per-renderer comparison options. Golden lookup is handled by
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

		public const string ImageMarker = GoldenDiagnostics.ImageMarker;

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

			EmitActual(renderer.Name, scene.Name, info, actual);

			var resolution = ResolveGolden(renderer.Name, scene.Name);
			WriteOutput(resolution.Describe());
			if (resolution.Key is null)
			{
				FailUnseeded(renderer.Name, scene.Name, info, actual, resolution);
				return;
			}

			using var golden = GoldenImage.Decode(resolution.ReadBytes(), resolution.Location);
			CompareOrFail(renderer.Name, scene.Name, info, actual, golden, resolution);
		}

		internal static GoldenRequest Request(string rendererName, string sceneName) =>
			new GoldenRequest(TestConfig.Current.PlatformName, sceneName, architecture:
				RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant(), variant: rendererName);

		internal static GoldenResolution ResolveGolden(string rendererName, string sceneName)
		{
			var roots = new List<string> { Path.Combine(AppContext.BaseDirectory, "Goldens") };
			if (!string.IsNullOrEmpty(TestConfig.Current.PathRoot))
				roots.Add(Path.Combine(TestConfig.Current.PathRoot, "Goldens"));
			var assemblies = new List<Assembly>();
			if (Assembly.GetEntryAssembly() is { } entry) assemblies.Add(entry);
			assemblies.Add(TestConfig.Current.GetType().Assembly);
			assemblies.Add(typeof(VisualMatrixTestsBase).Assembly);
			return new GoldenStore(roots, assemblies).Resolve(Request(rendererName, sceneName));
		}

		// Harvesting this marker from the TRX is how goldens are created, so it
		// carries the pixels verbatim.
		private void EmitActual(string rendererName, string sceneName, SKImageInfo info, byte[] actual) =>
			Emit("actual", rendererName, sceneName, info,
				GoldenImage.EncodePixels(actual, RendererPixels.NormalizedInfo(info)));

		private void EmitImage(string marker, string rendererName, string sceneName, SKImageInfo info, SKImage image)
		{
			Emit(marker, rendererName, sceneName, info, GoldenImage.Encode(image));
		}

		private void Emit(string marker, string rendererName, string sceneName, SKImageInfo info, byte[] png)
		{
			var normalized = RendererPixels.NormalizedInfo(info);
			WriteOutput(GoldenDiagnostics.Marker("Visual", marker, Request(rendererName, sceneName),
				normalized.Width, normalized.Height, png));
		}

		private void CompareOrFail(string rendererName, string sceneName, SKImageInfo info, byte[] actual, SKImage golden, GoldenResolution resolution)
		{
			var normalized = RendererPixels.NormalizedInfo(info);
			var options = ComparisonOptions(rendererName);

			using var actualImage = ToImage(actual, normalized);
			var comparison = GoldenImage.Compare(golden, actualImage, options);
			using var diffImage = GoldenImage.Difference(golden, actualImage, options);

			EmitImage("reference", rendererName, sceneName, info, golden);
			EmitImage("diff", rendererName, sceneName, info, diffImage);

			if (comparison.IsMatch)
				return;

			var actualPath = TrySave(() => SaveFailureArtifact(rendererName, sceneName, ".actual.png",
				GoldenImage.EncodePixels(actual, normalized)));
			var diffPath = TrySave(() => SaveFailureArtifact(rendererName, sceneName, ".diff.png",
				GoldenImage.Encode(diffImage)));

			Assert.Fail(
				$"Visual regression for '{rendererName}/{sceneName}' against golden '{resolution.Location}'. " +
				$"{comparison.Pixels.ErrorPixelCount}/{comparison.Pixels.TotalPixels} pixels exceed the per-channel tolerance of {options.ChannelTolerance} " +
				$"(allowed outliers: {comparison.AllowedErrorPixels}); max observed channel delta {comparison.Pixels.MaxChannelDelta}. " +
				ArtifactSuffix(actualPath, diffPath) +
				$"The rendered PNG is in the test results as an {ImageMarker} role=actual marker. " +
				"If this change is expected, harvest it with scripts/infra/tests/extract-visual-goldens.py and commit; " +
				"otherwise fix the regression.");
		}

		private void FailUnseeded(string rendererName, string sceneName, SKImageInfo info, byte[] actual, GoldenResolution resolution)
		{
			var actualPath = TrySave(() => SaveFailureArtifact(rendererName, sceneName, ".actual.png",
				GoldenImage.EncodePixels(actual, RendererPixels.NormalizedInfo(info))));

			Assert.Fail(
				$"No golden recorded yet for '{rendererName}/{sceneName}'. {resolution.Describe()} " +
				$"The rendered PNG is in the test results as an {ImageMarker} role=actual marker; " +
				"seed it with scripts/infra/tests/extract-visual-goldens.py and commit. " +
				ArtifactSuffix(actualPath, null));
		}

		private static SKImage ToImage(byte[] rgba, SKImageInfo info) =>
			SKImage.FromPixelCopy(RendererPixels.NormalizedInfo(info), rgba);

		private static GoldenComparisonOptions ComparisonOptions(string rendererName) =>
			rendererName == GpuBackends.Raster
				? new GoldenComparisonOptions { ChannelTolerance = 2, MaxErrorPixelFraction = 0.002 }
				: new GoldenComparisonOptions { ChannelTolerance = 12, MaxErrorPixelFraction = 0.03 };

		private static string SaveFailureArtifact(string rendererName, string sceneName, string suffix, byte[] png)
		{
			var path = Path.Combine(AppContext.BaseDirectory, "_visualfailures", rendererName, sceneName + suffix);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllBytes(path, png);
			return path;
		}

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
