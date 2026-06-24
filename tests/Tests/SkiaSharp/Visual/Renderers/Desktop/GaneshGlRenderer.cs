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
		public string Name => "ganesh-gl";

		public bool IsAvailable => UnavailableReason is null;

		public string UnavailableReason =>
			TestConfig.Current.IsMac || TestConfig.Current.IsWindows || TestConfig.Current.IsLinux
				? null
				: "OpenGL is only wired up for the desktop test hosts (macOS, Windows, Linux).";

		public Task<byte[]> RenderAsync(ISkiaScene scene, SKImageInfo info, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			lock (GpuRenderGate.Sync)
			{
				using var glContext = CreateContextOrSkip();
				glContext.MakeCurrent();

				using var grContext = GRContext.CreateGl()
					?? throw new InvalidOperationException("GRContext.CreateGl returned null.");
				using var surface = SKSurface.Create(grContext, budgeted: true, info)
					?? throw new InvalidOperationException("SKSurface.Create returned null on Ganesh/GL.");

				scene.Draw(surface.Canvas);
				grContext.Flush(submit: true, synchronous: true);

				return Task.FromResult(RendererPixels.ReadRgba(surface, info));
			}
		}

		public void Dispose()
		{
		}

		// Distinguishes "GL genuinely absent on this host" (legit skip) from a
		// broken binding (real failure). A missing native entry point or method
		// is a regression and MUST fail; an absent display/driver is an honest
		// skip.
		private static GlContext CreateContextOrSkip()
		{
			try
			{
				return TestConfig.Current.CreateGlContext();
			}
			catch (Exception ex) when (ex is not EntryPointNotFoundException and not MissingMethodException)
			{
				throw new RendererUnavailableException(
					$"Unable to create an OpenGL context on this host: {ex.Message}", ex);
			}
		}
	}
}
