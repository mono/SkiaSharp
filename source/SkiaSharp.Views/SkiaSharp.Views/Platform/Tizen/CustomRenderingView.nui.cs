using System;
using System.Threading;
using Tizen.NUI;
using NImageView = Tizen.NUI.BaseComponents.ImageView;

namespace SkiaSharp.Views.Tizen.NUI
{
	public abstract class CustomRenderingView : NImageView
	{
		bool _redrawRequest;

		protected SynchronizationContext MainloopContext { get; }

		protected CustomRenderingView()
		{
			Layout = new CustomLayout
			{
				SizeUpdated = OnResized
			};
			MainloopContext = SynchronizationContext.Current ?? throw new InvalidOperationException("Must create on main thread");
		}

		public SKSize CanvasSize => Size.ToSKSize();

		public event EventHandler<SKPaintSurfaceEventArgs>? PaintSurface;

		public void Invalidate()
		{
			if (!_redrawRequest)
			{
				_redrawRequest = true;
				MainloopContext.Post((s) =>
				{
					_redrawRequest = false;
					if (!Disposed)
					{
						OnDrawFrame();
					}
				}, null);
			}
		}

		protected abstract void OnResized();

		protected abstract void OnDrawFrame();

		protected void SendPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		class CustomLayout : AbsoluteLayout
		{
			float _width;
			float _height;

			public Action? SizeUpdated { get; set; }

			protected override void OnLayout(bool changed, LayoutLength left, LayoutLength top, LayoutLength right, LayoutLength bottom)
			{
				var sizeChanged = _width != Owner.SizeWidth || _height != Owner.SizeHeight;
				_width = Owner.SizeWidth;
				_height = Owner.SizeHeight;
				if (sizeChanged)
				{
					SizeUpdated?.Invoke();
				}
				base.OnLayout(changed, left, top, right, bottom);
			}
		}
	}
}
