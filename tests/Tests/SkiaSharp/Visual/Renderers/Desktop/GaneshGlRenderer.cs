using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Ganesh GPU backend over OpenGL. Reuses the existing
	/// <see cref="GlContext"/> abstraction (CGL on macOS, GLX/EGL on Linux, WGL
	/// on Windows) via <see cref="TestConfig.CreateGlContext"/> — the same path
	/// <c>GRContextTest</c> uses — rather than reinventing a platform loader.
	///
	/// <para>
	/// A fresh context is created per render and torn down afterwards; no
	/// long-lived GR state. This file lives under <c>Renderers/Desktop/</c> and is
	/// compiled only into the desktop host (Console), because the platform
	/// <see cref="GlContext"/> implementations are desktop-only.
	/// </para>
	/// </summary>
	public sealed class GaneshGlRenderer : IRenderer
	{
		public string Name => GpuBackends.GaneshGl;

		public Task<byte[]> RenderAsync(ISkiaScene scene, SKImageInfo info, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			// No catch: the policy already decided GL is required on this host, so
			// a context we cannot create is a failure to investigate, not a skip.
			using var glContext = TestConfig.Current.CreateGlContext();
			glContext.MakeCurrent();

			using var grContext = GRContext.CreateGl()
				?? throw new InvalidOperationException("GRContext.CreateGl returned null.");
			using var surface = SKSurface.Create(grContext, budgeted: true, info)
				?? throw new InvalidOperationException("SKSurface.Create returned null on Ganesh/GL.");

			scene.Draw(surface.Canvas);
			grContext.Flush(submit: true, synchronous: true);

			return Task.FromResult(RendererPixels.ReadRgba(surface, info));
		}

		public void Dispose()
		{
		}
	}
}
