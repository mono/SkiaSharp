using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests.Visual.Tests
{
	/// <summary>
	/// One theory per <c>(renderer × scene)</c> cell. xUnit fans the test out
	/// across every compatible combination at discovery time; cells with an
	/// unavailable renderer (no Vulkan loader, wrong OS for Metal, …) skip
	/// cleanly.
	///
	/// Scenes are <see cref="ISkiaScene"/> impls in
	/// <c>tests/Tests/SkiaSharp/Visual/Scenes/</c>. Add a scene → it appears
	/// in every renderer's column automatically. Add a renderer → it appears
	/// in every scene's row automatically.
	/// </summary>
	public class VisualMatrixTests : VisualTestBase
	{
		[VisualTheory, MemberData (nameof (Matrix))]
		public Task RenderAndCompare (string rendererName, string sceneName) =>
			VerifyScene (rendererName, sceneName);
	}
}
