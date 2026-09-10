using System;
using System.Threading;
using Tizen.NUI;
using NImageView = Tizen.NUI.BaseComponents.ImageView;

namespace SkiaSharp.Views.Tizen.NUI
{
	/// <summary>An abstract base class for custom rendering views in the Tizen NUI framework.</summary>
	/// <remarks>This class extends Tizen.NUI.BaseComponents.ImageView and provides the foundation for SkiaSharp rendering on Tizen devices.</remarks>
	public abstract class CustomRenderingView : NImageView
	{
		bool _redrawRequest;

		/// <summary>Gets the synchronization context for the Tizen main loop.</summary>
		/// <value>The <see cref="T:System.Threading.SynchronizationContext" /> for the main loop.</value>
		/// <remarks>Use this context to post work to the main UI thread.</remarks>
		protected SynchronizationContext MainloopContext { get; }

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Tizen.NUI.CustomRenderingView" /> class.</summary>
		/// <remarks />
		protected CustomRenderingView()
		{
			Layout = new CustomLayout
			{
				SizeUpdated = OnResized
			};
			MainloopContext = SynchronizationContext.Current ?? throw new InvalidOperationException("Must create on main thread");
		}

		/// <summary>Gets the current canvas size in pixels.</summary>
		/// <value>The current size of the drawing canvas.</value>
		/// <remarks />
		public SKSize CanvasSize => Size.ToSKSize();

		/// <summary>Occurs when the surface needs to be painted.</summary>
		/// <remarks>Subscribe to this event to perform drawing operations on the canvas provided in the event arguments.</remarks>
		public event EventHandler<SKPaintSurfaceEventArgs>? PaintSurface;

		/// <summary>Requests that the view be redrawn.</summary>
		/// <remarks>Calling this method schedules a call to <see cref="M:SkiaSharp.Views.Tizen.NUI.CustomRenderingView.OnDrawFrame" /> on the next frame.</remarks>
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

		/// <summary>Called when the view has been resized.</summary>
		/// <remarks>Derived classes must override this method to handle size changes and reallocate any resources that depend on the view size.</remarks>
		protected abstract void OnResized();

		/// <summary>Called when the view needs to render a frame.</summary>
		/// <remarks>Derived classes must override this method to perform custom drawing operations.</remarks>
		protected abstract void OnDrawFrame();

		/// <param name="e">The event arguments containing the surface and canvas information.</param>
		/// <summary>Raises the <see cref="E:SkiaSharp.Views.Tizen.NUI.CustomRenderingView.PaintSurface" /> event.</summary>
		/// <remarks>Derived classes should call this method to notify subscribers that the surface is ready to be painted.</remarks>
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
