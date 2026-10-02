using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using SkiaSharp.Views.Desktop;

#nullable enable

namespace SkiaSharp.Views.Gtk
{
	/// <summary>A GTK4 OpenGL view that can be drawn on using SkiaSharp GPU commands.</summary>
	[SupportedOSPlatform("linux")]
	public class SKGLView : global::Gtk.GLArea
	{
		private const uint FramebufferBinding = 0x8CA6;
		private const uint StencilBits = 0x0D57;
		private const uint Samples = 0x80A9;
		private const uint Rgba8 = 0x8058;

		private GRContext? context;
		private GRBackendRenderTarget? renderTarget;
		private SKSurface? surface;
		private SKSizeI lastCanvasSize;
		private int lastSamples;
		private int lastStencil;
		private nint lastNativeContext;
		private uint tickCallback;
		private bool enableRenderLoop;
		private bool ignorePixelScaling;

		/// <summary>Creates a GTK4 OpenGL view using desktop OpenGL (not OpenGL ES).</summary>
		public SKGLView() : base(new GObject.ConstructArgument[] { })
		{
			SetAllowedApis(Gdk.GLAPI.Gl);
			SetAutoRender(false);
			SetHasStencilBuffer(true);
			OnRender += Render;
			OnRealize += Realize;
			OnUnrealize += Unrealize;
		}

		/// <summary>Gets the physical or logical canvas size, according to <see cref="IgnorePixelScaling" />.</summary>
		public SKSize CanvasSize => lastCanvasSize;

		/// <summary>Gets the SkiaSharp GPU context while the GL view is realized.</summary>
		public GRContext? GRContext => context;

		/// <summary>Gets or sets whether to render continuously while the view is realized.</summary>
		public bool EnableRenderLoop
		{
			get => enableRenderLoop;
			set
			{
				if (enableRenderLoop == value)
					return;
				enableRenderLoop = value;
				UpdateRenderLoop();
			}
		}

		/// <summary>Gets or sets whether paint and touch coordinates are expressed in logical pixels.</summary>
		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
			set
			{
				if (ignorePixelScaling == value)
					return;
				ignorePixelScaling = value;
				QueueRender();
			}
		}

		/// <summary>Occurs when the OpenGL context is created or released.</summary>
		public event EventHandler? ContextChanged;

		/// <summary>Occurs when the GPU surface needs to be painted.</summary>
		public event EventHandler<SKPaintGLSurfaceEventArgs>? PaintSurface;

		/// <summary>Releases the GPU resources without disposing the GTK widget. A later render recreates them.</summary>
		public void ReleaseGlResources() => ReleaseContext();

		/// <summary>Releases this view's SkiaSharp GPU resources.</summary>
		public override void Dispose()
		{
			EnableRenderLoop = false;
			OnRender -= Render;
			OnRealize -= Realize;
			OnUnrealize -= Unrealize;
			ReleaseContext();
			base.Dispose();
		}

		private void UpdateRenderLoop()
		{
			if (enableRenderLoop && GetRealized() && tickCallback == 0)
				tickCallback = AddTickCallback((widget, clock) =>
				{
					QueueRender();
					return true;
				});
			else if ((!enableRenderLoop || !GetRealized()) && tickCallback != 0)
			{
				RemoveTickCallback(tickCallback);
				tickCallback = 0;
			}
		}

		private void Realize(global::Gtk.Widget widget, EventArgs args)
		{
			UpdateRenderLoop();
			QueueRender();
		}

		private void Unrealize(global::Gtk.Widget widget, EventArgs args)
		{
			if (tickCallback != 0)
			{
				RemoveTickCallback(tickCallback);
				tickCallback = 0;
			}
			ReleaseContext();
		}

		private bool Render(global::Gtk.GLArea area, global::Gtk.GLArea.RenderSignalArgs args)
		{
			if (GetApi() != Gdk.GLAPI.Gl)
				throw new NotSupportedException("GTK4 SKGLView requires desktop OpenGL; OpenGL ES is not supported.");

			var nativeContext = GetContext()?.Handle.DangerousGetHandle() ?? nint.Zero;
			if (nativeContext == nint.Zero)
				throw new InvalidOperationException("GTK4 could not create an OpenGL context.");

			if (context is null || lastNativeContext != nativeContext)
			{
				ReleaseContext(abandon: true);
				using var glInterface = GRGlInterface.Create()
					?? throw new InvalidOperationException("SkiaSharp could not resolve GTK4 OpenGL entry points.");
				context = GRContext.CreateGl(glInterface)
					?? throw new InvalidOperationException("SkiaSharp could not initialize the GTK4 OpenGL context.");
				lastNativeContext = nativeContext;
				ContextChanged?.Invoke(this, EventArgs.Empty);
			}

			var scale = GetScaleFactor();
			var size = new SKSizeI(checked(GetWidth() * scale), checked(GetHeight() * scale));
			if (size.Width <= 0 || size.Height <= 0)
				return true;

			GlGetIntegerv(FramebufferBinding, out var framebuffer);
			GlGetIntegerv(StencilBits, out var stencil);
			GlGetIntegerv(Samples, out var samples);
			samples = Math.Min(samples, context.GetMaxSurfaceSampleCount(SKColorType.Rgba8888));
			var info = new GRGlFramebufferInfo((uint)framebuffer, Rgba8);
			if (renderTarget is null || renderTarget.Width != size.Width || renderTarget.Height != size.Height ||
				renderTarget.GetGlFramebufferInfo().FramebufferObjectId != info.FramebufferObjectId ||
				lastSamples != samples || lastStencil != stencil)
			{
				ReleaseSurface();
				renderTarget = new GRBackendRenderTarget(size.Width, size.Height, samples, stencil, info);
				try
				{
					surface = SKSurface.Create(context, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888)
						?? throw new InvalidOperationException("SkiaSharp could not create a GTK4 GPU surface.");
				}
				catch
				{
					ReleaseSurface();
					throw;
				}
				lastSamples = samples;
				lastStencil = stencil;
			}

			var rawInfo = new SKImageInfo(size.Width, size.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
			var displayInfo = ignorePixelScaling
				? rawInfo.WithSize(new SKSizeI(size.Width / scale, size.Height / scale))
				: rawInfo;
			lastCanvasSize = displayInfo.Size;

			using (new SKAutoCanvasRestore(surface!.Canvas, true))
			{
				surface.Canvas.Clear(SKColors.Transparent);
				if (ignorePixelScaling)
					surface.Canvas.Scale(scale);
				PaintSurface?.Invoke(this, new SKPaintGLSurfaceEventArgs(surface, renderTarget!,
					GRSurfaceOrigin.BottomLeft, displayInfo, rawInfo));
			}
			surface.Canvas.Flush();
			return true;
		}

		private void ReleaseContext(bool abandon = false)
		{
			if (context is null)
				return;

			if (abandon || !GetRealized())
				context.AbandonContext();
			else
				MakeCurrent();

			ReleaseSurface();
			context.Dispose();
			context = null;
			lastNativeContext = nint.Zero;
			lastCanvasSize = default;
			ContextChanged?.Invoke(this, EventArgs.Empty);
		}

		private void ReleaseSurface()
		{
			surface?.Dispose();
			surface = null;
			renderTarget?.Dispose();
			renderTarget = null;
			lastSamples = 0;
			lastStencil = 0;
		}

		[DllImport("libGL.so.1", EntryPoint = "glGetIntegerv")]
		private static extern void GlGetIntegerv(uint name, out int value);
	}
}
