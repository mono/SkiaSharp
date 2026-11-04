using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Pure-CPU rasterizer. Always available and deterministic across platforms
	/// for the geometric scenes, so its output is the portable baseline stored
	/// under <c>tests/Goldens/Visual/portable/</c>, with a platform reference for
	/// any scene that diverges by OS, such as text.
	/// </summary>
	public sealed class RasterRenderer : IRenderer
	{
		public string Name => GpuBackends.Raster;

		public Task<byte[]> RenderAsync(ISkiaScene scene, SKImageInfo info, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			using var surface = SKSurface.Create(info)
				?? throw new InvalidOperationException("SKSurface.Create returned null for the raster backend.");

			scene.Draw(surface.Canvas);
			surface.Canvas.Flush();

			return Task.FromResult(RendererPixels.ReadRgba(surface, info));
		}

		public void Dispose()
		{
		}
	}
}
