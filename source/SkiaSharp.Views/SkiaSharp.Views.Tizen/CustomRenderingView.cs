using ElmSharp;
using System;

namespace SkiaSharp.Views.Tizen
{
	/// <summary>
	/// Widget which allows to implement custom rendering procedure.
	/// </summary>
	/// kkk
	public abstract class CustomRenderingView : Widget 
	{
		// called when image needs to be redrawn
		private readonly Interop.Evas.Image.ImagePixelsSetCallback redrawCallback;

		// used to redraw the surface at every animation tick (when render mode is set to RenderingMode.Continuously)
		private IntPtr animator;

		// type of rendering
		private RenderingMode renderingMode = RenderingMode.WhenDirty;

		/// <summary>
		/// Creates a new instance with the given object as its parent.
		/// </summary>
		/// <param name="parent">The parent object.</param>
		public CustomRenderingView(EvasObject parent) : base(parent)
		{
			// initialize the callbacks
			Resized += (sender, e) => OnResized();
			redrawCallback = (d, o) => OnDrawFrame();
		}

		/// <summary>
		/// The size of the surface.
		/// </summary>
		public SKSize SurfaceSize => new SKSize(SurfaceWidth, SurfaceHeight);

		public SKSize CanvasSize => SurfaceSize;

		/// <summary>
		/// Rendering mode used to control repainting the surface.
		/// </summary>
		/// <remarks>
		/// Default value is RenderingMode.WhenDirty.
		/// </remarks>
		public RenderingMode RenderingMode
		{
			get
			{
				return renderingMode;
			}

			set
			{
				if (renderingMode != value)
				{
					renderingMode = value;

					if (renderingMode == RenderingMode.Continuously)
					{
						CreateAnimator();
					}
					else
					{
						DestroyAnimator();
					}
				}
			}
		}

		/// <summary>
		/// Width of the drawing surface.
		/// </summary>
		protected abstract int SurfaceWidth
		{
			get;
		}

		/// <summary>
		/// Height of the drawing surface.
		/// </summary>
		protected abstract int SurfaceHeight
		{
			get;
		}

		/// <summary>
		/// Displays the rendered content.
		/// </summary>
		protected IntPtr EvasImage
		{
			get;
			private set;
		}

		/// <summary>
		/// Requests to repaint the surface.
		/// </summary>
		/// <remarks>
		/// Surface is repainted when RenderingMode is set to RenderingMode.WhenDirty, otherwise repainting is controlled by EFL.
		/// </remarks>
		public void Invalidate()
		{
			if (RenderingMode == RenderingMode.WhenDirty)
			{
				Repaint();
			}
		}

		/// <summary>
		/// Creates the native resources which should be present throughout whole life of the control.
		/// </summary>
		/// <param name="parent">The parent object.</param>
		/// <remarks>
		/// This method is empty.
		/// </remarks>
		protected virtual void CreateNativeResources(EvasObject parent)
		{
			// empty on purpose
		}

		/// <summary>
		/// Destroys the native resources.
		/// </summary>
		/// <remarks>
		/// This method is empty.
		/// </remarks>
		protected virtual void DestroyNativeResources()
		{
			// empty on purpose
		}

		/// <summary>
		/// Current frame should be drawn into the image.
		/// </summary>
		protected abstract void OnDrawFrame();

		/// <summary>
		/// Updates the drawing surface's size.
		/// </summary>
		/// <param name="geometry">Current geometry of the control.</param>
		/// <returns>true, if size has changed, false otherwise.</returns>
		protected abstract bool UpdateSurfaceSize(Rect geometry);

		/// <summary>
		/// Creates the drawing surface.
		/// </summary>
		/// <remarks>
		/// This method is empty.
		/// </remarks>
		protected virtual void CreateDrawingSurface()
		{
			// empty on purpose
		}

		/// <summary>
		/// Destroys the drawing surface.
		/// </summary>
		/// <remarks>
		/// This method is empty.
		/// </remarks>
		protected virtual void DestroyDrawingSurface()
		{
			// empty on purpose
		}

		/// <summary>
		/// Creates the EFL controls.
		/// </summary>
		/// <param name="parent">The parent object.</param>
		/// <returns>Pointer to the newly created control.</returns>
		protected sealed override IntPtr CreateHandle(EvasObject parent)
		{
			IntPtr handle = Interop.Elementary.elm_layout_add(parent);
			Interop.Elementary.elm_layout_theme_set(handle, "layout", "background", "default");

			EvasImage = Interop.Evas.Image.evas_object_image_filled_add(Interop.Evas.evas_object_evas_get(handle));
			Interop.Evas.Image.evas_object_image_colorspace_set(EvasImage, Interop.Evas.Image.Colorspace.ARGB8888);
			Interop.Evas.Image.evas_object_image_smooth_scale_set(EvasImage, true);
			Interop.Evas.Image.evas_object_image_alpha_set(EvasImage, true);

			Interop.Elementary.elm_object_part_content_set(handle, "elm.swallow.content", EvasImage);

			CreateNativeResources(parent);
			return handle;
		}

		/// <summary>
		/// Cleans up.
		/// </summary>
		protected sealed override void OnUnrealize()
		{
			DestroyAnimator();
			DestroyDrawingSurface();
			DestroyNativeResources();

			base.OnUnrealize();
		}

		/// <summary>
		/// Notifies that the size of the drawing surface has changed.
		/// </summary>
		protected void OnSurfaceSizeChanged()
		{
			OnResized();
		}

		private void OnResized()
		{
			var geometry = Geometry;

			if (geometry.Width <= 0 || geometry.Height <= 0)
			{
				// control is not yet fully initialized
				return;
			}

			if (UpdateSurfaceSize(geometry))
			{
				RemoveImageCallback();

				// recreate the drawing surface to match the new size
				DestroyDrawingSurface();
				ResizeImage();
				CreateDrawingSurface();

				SetImageCallback();

				// repaint
				Invalidate();
			}
		}

		private void ResizeImage()
		{
			// resize the image buffers
			Interop.Evas.Image.evas_object_image_size_set(EvasImage, SurfaceWidth, SurfaceHeight);
		}

		private void Repaint()
		{
			// mark the image as dirty
			Interop.Evas.Image.evas_object_image_pixels_dirty_set(EvasImage, true);
		}

		private void CreateAnimator()
		{
			if (animator == IntPtr.Zero)
			{
				animator = EcoreAnimator.AddAnimator(() =>
				{
					Repaint();
					return true;
				});
			}
		}

		private void DestroyAnimator()
		{
			if (animator != IntPtr.Zero)
			{
				EcoreAnimator.RemoveAnimator(animator);
				animator = IntPtr.Zero;
			}
		}

		private void SetImageCallback()
		{
			// set the image callback; will be invoked when image is marked as dirty
			Interop.Evas.Image.evas_object_image_pixels_get_callback_set(EvasImage, redrawCallback, IntPtr.Zero);
		}

		private void RemoveImageCallback()
		{
			// disconnect the callback
			Interop.Evas.Image.evas_object_image_native_surface_set(EvasImage, IntPtr.Zero);
		}
	}
}
