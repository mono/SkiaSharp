using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests.Visual.Tests
{
	/// <summary>
	/// The shared visual-regression matrix: one test per <c>(renderer × scene)</c>
	/// over the renderers declared in <i>this</i> test assembly (raster and Metal
	/// everywhere; plus the desktop GL renderer in the Console host). xUnit fans
	/// the theory out across every combination, so adding a scene gives every
	/// renderer a new row and adding a renderer gives every scene a new column.
	///
	/// <para>Drop a portable renderer under <c>Visual/Renderers/</c> (or a desktop
	/// one under <c>Visual/Renderers/Desktop/</c>) and it joins automatically.
	/// Backends needing an extra NuGet package — Vulkan (Silk.NET/SharpVk),
	/// Direct3D (Vortice) — live in a satellite assembly with its own subclass of
	/// <see cref="VisualMatrixTestsBase"/>, so this project never takes that
	/// dependency. Each class runs only the renderers in its <b>own</b> assembly
	/// (via <see cref="RendererCatalog.NamesIn"/>), so a satellite and this class
	/// never double-run a test even when both load in one process (the device/MAUI
	/// host).</para>
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
			RunTestAsync(RendererCatalog.Get(rendererName), SceneCatalog.Get(sceneName));

		public static IEnumerable<object[]> Matrix()
		{
			foreach (var rendererName in RendererCatalog.NamesIn(Assembly.GetExecutingAssembly()))
				foreach (var sceneName in SceneCatalog.AllNames)
					yield return new object[] { rendererName, sceneName };
		}
	}
}
