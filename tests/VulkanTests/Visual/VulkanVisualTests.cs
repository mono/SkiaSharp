using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using SkiaSharp.Tests.Visual;
using SkiaSharp.Tests.Visual.Tests;
using Xunit;

namespace SkiaSharp.Vulkan.Tests
{
	/// <summary>
	/// Visual-regression tests for the GPU backends contributed by the
	/// <c>SkiaSharp.Vulkan.Tests</c> satellite (<c>ganesh-vulkan</c> and
	/// <c>graphite-vulkan</c>). The shared raster / GL / Metal renderers belong to
	/// the base <see cref="VisualMatrixTests"/> and are not run here.
	///
	/// <para>The matrix is driven by reflection over <i>this</i> assembly
	/// (<see cref="RendererCatalog.NamesIn"/>): add a Vulkan-family renderer class
	/// plus its golden folder and it joins automatically.</para>
	/// </summary>
	[Trait("Category", VisualMatrixTestsBase.VisualCategory)]
	[Collection(VulkanGpuRenderingCollection.Name)]
	public class VulkanVisualTests : VisualMatrixTestsBase
	{
		public VulkanVisualTests(ITestOutputHelper output)
			: base(output)
		{
		}

		[Theory]
		[MemberData(nameof(Matrix))]
		public Task RenderMatchesGolden(string rendererName, string sceneName) =>
			RunTestAsync(RendererCatalog.Get(rendererName), SceneCatalog.Get(sceneName));

		public static IEnumerable<object[]> Matrix()
		{
			foreach (var rendererName in RendererCatalog.NamesIn(Assembly.GetExecutingAssembly()))
				foreach (var sceneName in SceneCatalog.AllNames)
					yield return new object[] { rendererName, sceneName };
		}
	}
}
