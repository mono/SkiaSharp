using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests.Visual.Tests
{
	/// <summary>
	/// The shared visual-regression matrix: one theory cell per
	/// <c>(renderer × scene)</c> over every renderer auto-discovered in this test
	/// assembly (raster everywhere; plus the desktop GL/Metal renderers in the
	/// Console host). xUnit fans the test out across every combination, so adding a
	/// scene gives every renderer a new row and adding a renderer gives every scene
	/// a new column.
	///
	/// <para>
	/// This is the single seam most backends plug into: drop a portable renderer
	/// under <c>Visual/Renderers/</c> (or a desktop one under
	/// <c>Visual/Renderers/Desktop/</c>) and it joins this matrix automatically.
	/// Backends that need an extra NuGet package — Vulkan (SharpVk), Direct3D
	/// (Vortice) — live in their satellite host project instead and are driven by a
	/// thin subclass of <see cref="VisualMatrixTestsBase"/> there, so this base
	/// project never takes that dependency. All cells share the same render / emit /
	/// compare / fail discipline documented on <see cref="VisualMatrixTestsBase"/>.
	/// </para>
	/// </summary>
	[Trait("Category", VisualCategory)]
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
			foreach (var rendererName in RendererCatalog.AllNames)
				foreach (var sceneName in SceneCatalog.AllNames)
					yield return new object[] { rendererName, sceneName };
		}
	}
}
