using System;
using System.ComponentModel;
using System.Runtime.Versioning;
using SkiaSharp.Views.Desktop;

#nullable enable

namespace SkiaSharp.Views.Gtk
{
	/// <summary>A GTK4 OpenGL view that can be drawn on using SkiaSharp GPU commands.</summary>
	[SupportedOSPlatform("linux")]
	[SupportedOSPlatform("macos")]
	[SupportedOSPlatform("windows")]
	public class SKGLView : global::Gtk.GLArea
	{
		private const uint FramebufferBinding = 0x8CA6;
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
		private bool disposed;

		/// <summary>Creates a GTK4 OpenGL view.</summary>
		/// <remarks>Uses desktop OpenGL on Linux and macOS. On Windows, GTK may also select OpenGL ES through EGL/ANGLE. No software canvas fallback is provided.</remarks>
		public SKGLView() : base(new GObject.ConstructArgument[] { })
		{
			if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS() && !OperatingSystem.IsWindows())
				throw new PlatformNotSupportedException("GTK4 SKGLView requires Linux, macOS or Windows.");

			SetAllowedApis(OperatingSystem.IsWindows() ? Gdk.GLAPI.Gl | Gdk.GLAPI.Gles : Gdk.GLAPI.Gl);
			SetAutoRender(false);
			SetHasStencilBuffer(true);
			OnRender += Render;
			OnRealize += Realize;
			OnUnrealize += Unrealize;
		}

		/// <summary>Gets the physical or logical canvas size, according to <see cref="IgnorePixelScaling" />.</summary>
		public SKSize CanvasSize => lastCanvasSize;

		/// <summary>Gets the SkiaSharp GPU context, or <see langword="null" /> before the first paint and after the view is unrealized.</summary>
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

		/// <summary>Gets or sets whether paint coordinates are expressed in logical pixels.</summary>
		/// <remarks>By default, paint coordinates use physical pixels. GTK input events use logical pixels independently of this property. <c>RawInfo</c> always describes the physical backing surface.</remarks>
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

		/// <summary>Occurs when the GPU surface needs to be painted.</summary>
		/// <remarks>Draw by handling this event or overriding <see cref="OnPaintSurface" />.</remarks>
		[Category("Appearance")]
		public event EventHandler<SKPaintGLSurfaceEventArgs>? PaintSurface;

		/// <summary>Raises the <see cref="PaintSurface" /> event.</summary>
		/// <param name="e">The drawing surface and its physical and logical dimensions.</param>
		/// <remarks>Overrides must call the base method to raise the event.</remarks>
		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e) =>
			PaintSurface?.Invoke(this, e);

		/// <summary>Releases this view's SkiaSharp GPU resources.</summary>
		public override void Dispose()
		{
			if (disposed)
				return;

			EnableRenderLoop = false;
			OnRender -= Render;
			OnRealize -= Realize;
			OnUnrealize -= Unrealize;
			ReleaseContext();
			disposed = true;
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
			var nativeContext = GetContext()?.Handle.DangerousGetHandle() ?? nint.Zero;
			if (nativeContext == nint.Zero)
				throw new InvalidOperationException("GTK4 could not create an OpenGL context.");

			if (context is null || lastNativeContext != nativeContext)
			{
				ReleaseContext(abandon: true);
				using var glInterface = GRGlInterface.Create(GtkGl.GetProcedureAddress)
					?? throw new InvalidOperationException("SkiaSharp could not resolve GTK4 OpenGL entry points.");
				context = GRContext.CreateGl(glInterface)
					?? throw new InvalidOperationException("SkiaSharp could not initialize the GTK4 OpenGL context.");
				lastNativeContext = nativeContext;
			}

			var scale = GetScaleFactor();
			var size = new SKSizeI(checked(GetWidth() * scale), checked(GetHeight() * scale));
			if (size.Width <= 0 || size.Height <= 0)
				return true;

			// GTK changes GL state and framebuffer attachments between paint events.
			context.ResetContext();
			var framebuffer = GtkGl.GetInteger(FramebufferBinding);
			var stencil = GtkGl.GetStencilBits();
			var samples = GtkGl.GetInteger(Samples);
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
				OnPaintSurface(new SKPaintGLSurfaceEventArgs(surface, renderTarget!,
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
			{
				MakeCurrent();
				if (GetError() != null)
					context.AbandonContext();
			}

			ReleaseSurface();
			context.Dispose();
			context = null;
			lastNativeContext = nint.Zero;
			lastCanvasSize = default;
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
	}
}
