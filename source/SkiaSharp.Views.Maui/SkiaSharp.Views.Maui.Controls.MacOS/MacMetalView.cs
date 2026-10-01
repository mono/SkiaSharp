using AppKit;
using SkiaSharp.Views.Mac;
using SkiaSharp.Views.Maui;

namespace SkiaSharp.Views.Maui.Handlers
{
	internal sealed class MacMetalView : SKMetalView
	{
		private readonly MacTouchHandler touch;
		private bool disposed;

		public MacMetalView(MacTouchHandler touch) => this.touch = touch;

		public bool IgnorePixelScaling { get; set; }

		protected override void OnPaintSurface(SKPaintMetalSurfaceEventArgs e)
		{
			using var restore = new SKAutoCanvasRestore(e.Surface.Canvas, true);
			if (IgnorePixelScaling)
			{
				var logical = new SKSizeI((int)Bounds.Width, (int)Bounds.Height);
				if (logical.Width > 0 && logical.Height > 0)
				{
					e.Surface.Canvas.Scale(e.Info.Width / (float)logical.Width, e.Info.Height / (float)logical.Height);
					e = new SKPaintMetalSurfaceEventArgs(e.Surface, e.BackendRenderTarget,
						e.Origin, e.Info.WithSize(logical), e.Info);
				}
			}
			base.OnPaintSurface(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !disposed)
			{
				disposed = true;
				Paused = true;
				GRContext?.Dispose();
			}
			base.Dispose(disposing);
		}

		public override void MouseDown(NSEvent e)
		{
			if (!touch.Handle(e, SKTouchAction.Pressed, SKMouseButton.Left))
				base.MouseDown(e);
		}

		public override void MouseDragged(NSEvent e)
		{
			if (!touch.Handle(e, SKTouchAction.Moved, SKMouseButton.Left))
				base.MouseDragged(e);
		}

		public override void MouseUp(NSEvent e)
		{
			if (!touch.Handle(e, SKTouchAction.Released, SKMouseButton.Left))
				base.MouseUp(e);
		}

		public override void RightMouseDown(NSEvent e)
		{
			if (!touch.Handle(e, SKTouchAction.Pressed, SKMouseButton.Right))
				base.RightMouseDown(e);
		}

		public override void RightMouseDragged(NSEvent e)
		{
			if (!touch.Handle(e, SKTouchAction.Moved, SKMouseButton.Right))
				base.RightMouseDragged(e);
		}

		public override void RightMouseUp(NSEvent e)
		{
			if (!touch.Handle(e, SKTouchAction.Released, SKMouseButton.Right))
				base.RightMouseUp(e);
		}

		public override void OtherMouseDown(NSEvent e)
		{
			if (!touch.Handle(e, SKTouchAction.Pressed, SKMouseButton.Middle))
				base.OtherMouseDown(e);
		}

		public override void OtherMouseDragged(NSEvent e)
		{
			if (!touch.Handle(e, SKTouchAction.Moved, SKMouseButton.Middle))
				base.OtherMouseDragged(e);
		}

		public override void OtherMouseUp(NSEvent e)
		{
			if (!touch.Handle(e, SKTouchAction.Released, SKMouseButton.Middle))
				base.OtherMouseUp(e);
		}

		public override void MouseMoved(NSEvent e)
		{
			if (!touch.Handle(e, SKTouchAction.Moved))
				base.MouseMoved(e);
		}

		public override void MouseEntered(NSEvent e)
		{
			if (!touch.Handle(e, SKTouchAction.Entered))
				base.MouseEntered(e);
		}

		public override void MouseExited(NSEvent e)
		{
			if (!touch.Handle(e, SKTouchAction.Exited))
				base.MouseExited(e);
		}

		public override void ScrollWheel(NSEvent e)
		{
			if (!touch.Handle(e, SKTouchAction.WheelChanged))
				base.ScrollWheel(e);
		}
	}
}
