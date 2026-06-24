using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Pure-CPU rasterizer. Always available and fully deterministic across every
	/// platform — its output is the portable baseline stored under
	/// <c>Content/Goldens/_shared/</c>.
	/// </summary>
	public sealed class RasterRenderer : IRenderer
	{
		public string Name => "raster";

		public bool IsAvailable => true;

		public string UnavailableReason => null;

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
