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
	/// compares the pixels to a committed golden resolved by <see cref="GoldenStore"/>.
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
		private static readonly Lazy<GoldenStore> goldenStore = new(CreateStore);

		/// <summary>
		/// Trait value tagging every visual test. Lets CI and developers run or skip
		/// just the visual suite without naming individual classes.
		/// </summary>
		public const string VisualCategory = "Visual";

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
			var raster = renderer.Name == GpuBackends.Raster;
			Resolve(renderer.Name, scene.Name).Verify(
				actual,
				RendererPixels.NormalizedInfo(info),
				channelTolerance: raster ? 2 : 12,
				maxErrorPixelFraction: raster ? 0.002 : 0.03);
		}

		internal static Golden Resolve(string rendererName, string sceneName) =>
			goldenStore.Value.Resolve(
				TestConfig.Current.PlatformName,
				sceneName,
				architecture: RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant(),
				variant: rendererName);

		private static GoldenStore CreateStore()
		{
			var roots = new List<string> { Path.Combine(AppContext.BaseDirectory, "Goldens") };
			if (!string.IsNullOrEmpty(TestConfig.Current.PathRoot))
				roots.Add(Path.Combine(TestConfig.Current.PathRoot, "Goldens"));
			var assemblies = new List<Assembly>();
			if (Assembly.GetEntryAssembly() is { } entry) assemblies.Add(entry);
			assemblies.Add(TestConfig.Current.GetType().Assembly);
			assemblies.Add(typeof(VisualMatrixTestsBase).Assembly);
			return new GoldenStore(roots, assemblies);
		}
	}
}
