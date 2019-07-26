using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using OpenTK.Graphics;
using SkiaSharp.Views.Desktop;
using Xamarin.Forms.Platform.WPF;

using SKFormsView = SkiaSharp.Views.Forms.SKGLView;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SkiaSharp.Views.Forms.SKGLViewRenderer))]

namespace SkiaSharp.Views.Forms
{
	public class SKHostedGLControl : WindowsFormsHost
	{
		public SKHostedGLControl()
		{
			Initialize(new SKGLControl(new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8)));
		}

		public SKHostedGLControl(GraphicsMode mode)
		{
			Initialize(new SKGLControl(mode));
		}

		public SKHostedGLControl(GraphicsMode mode, int major, int minor, GraphicsContextFlags flags)
		{
			Initialize(new SKGLControl(mode, major, minor, flags));
		}

		public void Initialize(SKGLControl control)
		{
			GLControl = control;
			GLControl.MakeCurrent();
			GLControl.Dock = DockStyle.Fill;

			Child = GLControl;
		}

		public SKGLControl GLControl { get; private set; }

		public GRContext GRContext => GLControl.GRContext;

		public SKSize CanvasSize => GLControl.CanvasSize;

		public event EventHandler<Desktop.SKPaintGLSurfaceEventArgs> PaintSurface
		{
			add => GLControl.PaintSurface += value;
			remove => GLControl.PaintSurface -= value;
		}

		public void Invalidate()
		{
			GLControl.Invalidate();
		}
	}
}
