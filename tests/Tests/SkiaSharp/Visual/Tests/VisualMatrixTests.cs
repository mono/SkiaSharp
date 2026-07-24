using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests.Visual.Tests
{
	/// <summary>
	/// The shared visual-regression matrix: one theory cell per
	/// <c>(renderer × scene)</c> over the renderers declared in <i>this</i> test
	/// assembly (raster and Metal everywhere; plus the desktop GL renderer in the
	/// Console host). xUnit fans the test out across every combination, so adding a
	/// scene gives every renderer a new row and adding a renderer gives every scene
	/// a new column.
	///
	/// <para>
	/// This is the single seam most backends plug into: drop a portable renderer
	/// under <c>Visual/Renderers/</c> (or a desktop one under
	/// <c>Visual/Renderers/Desktop/</c>) and it joins this matrix automatically.
	/// Backends that need an extra NuGet package — Vulkan (Silk.NET/SharpVk),
	/// Direct3D (Vortice) — live in their own satellite assembly and are driven by a
	/// thin subclass of <see cref="VisualMatrixTestsBase"/> there, so this base
	/// project never takes that dependency. Each class runs only the renderers in
	/// its <b>own</b> assembly (via <see cref="RendererCatalog.NamesIn"/>), so a
	/// satellite and this base class never double-run the same cell even when both
	/// are loaded in one process (the device/MAUI host). All cells share the same
	/// render / emit / compare / fail discipline documented on
	/// <see cref="VisualMatrixTestsBase"/>.
	/// </para>
	/// </summary>
	[Trait("Category", VisualCategory)]
	[Collection(Visual.GpuRenderingCollection.Name)]
	public class VisualMatrixTests : VisualMatrixTestsBase
	{
		public VisualMatrixTests(ITestOutputHelper output)
			: base(output)
		{
		}

		[Theory]
		[MemberData(nameof(Matrix))]
		public Task RenderMatchesGolden(string rendererName, string sceneName) =>
			RunCellAsync(RendererCatalog.Get(rendererName), SceneCatalog.Get(sceneName));

		public static IEnumerable<object[]> Matrix()
		{
			foreach (var rendererName in RendererCatalog.NamesIn(Assembly.GetExecutingAssembly()))
				foreach (var sceneName in SceneCatalog.AllNames)
					yield return new object[] { rendererName, sceneName };
		}
	}
}
