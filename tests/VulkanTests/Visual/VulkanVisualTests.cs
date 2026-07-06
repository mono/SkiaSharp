using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using SkiaSharp.Tests.Visual;
using SkiaSharp.Tests.Visual.Tests;
using Xunit;

namespace SkiaSharp.Vulkan.Tests
{
	/// <summary>
	/// Visual-regression cells for the GPU backends contributed by the
	/// <c>SkiaSharp.Vulkan.Tests</c> satellite (today: <c>ganesh-vulkan</c>; the
	/// Graphite PR adds <c>graphite-vulkan</c> by simply dropping another renderer
	/// into this project). The shared raster / GL / Metal cells are <b>not</b> run
	/// here — they belong to the base <see cref="VisualMatrixTests"/> — so there is
	/// no double coverage.
	///
	/// <para>
	/// The matrix is driven by reflection over <i>this</i> assembly
	/// (<see cref="RendererCatalog.NamesIn"/>), so a new Vulkan-family renderer
	/// needs no edit here: add the renderer class plus its golden folder and it
	/// joins automatically. Each cell runs through the same render / emit / compare
	/// / fail engine as every other host via <see cref="VisualMatrixTestsBase"/>.
	/// </para>
	/// </summary>
	[Trait("Category", VisualMatrixTestsBase.VisualCategory)]
	public class VulkanVisualTests : VisualMatrixTestsBase
	{
		public VulkanVisualTests(ITestOutputHelper output)
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
