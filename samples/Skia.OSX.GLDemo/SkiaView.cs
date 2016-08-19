using System;
using AppKit;
using OpenTK.Graphics.OpenGL;

using SkiaSharp;

namespace Skia.OSX.Demo
{
	public partial class SkiaView : NSOpenGLView
	{
		Demos.Sample sample;

		GRContext grContext;

		public SkiaView (IntPtr handle) : base (handle)
		{
			AddGestureRecognizer (new NSClickGestureRecognizer (OnClicked));
		}

		public override void PrepareOpenGL ()
		{
			base.PrepareOpenGL ();

			grContext = GRContext.Create (GRBackend.OpenGL);
		}

		public override void DrawRect (CoreGraphics.CGRect dirtyRect)
		{
			base.DrawRect (dirtyRect);

			if (grContext != null)
			{
				var sampleCount = grContext.GetRecommendedSampleCount(GRPixelConfig.Rgba8888, 96.0f);

				var desc = new GRBackendRenderTargetDesc
				{
					Width = (int)Bounds.Width,
					Height = (int)Bounds.Height,
					Config = GRPixelConfig.Rgba8888,
					Origin = GRSurfaceOrigin.TopLeft,
					SampleCount = sampleCount,
					StencilBits = 0,
					RenderTargetHandle = IntPtr.Zero,
				};

				using (var surface = SKSurface.Create (grContext, desc))
				{
					var skcanvas = surface.Canvas;

					sample.Method (skcanvas, (int)Bounds.Width, (int)Bounds.Height);

					skcanvas.Flush ();
				}

				GL.Flush();
			}
		}

		void OnClicked ()
		{
			Sample?.TapMethod?.Invoke ();
		}

		public Demos.Sample Sample {
			get {
				return sample;
			}
			set {
				sample = value;
				SetNeedsDisplayInRect (Bounds);
			}
		}
	}
}
