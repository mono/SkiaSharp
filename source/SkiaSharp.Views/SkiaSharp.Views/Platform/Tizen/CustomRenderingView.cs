using System;
using ElmSharp;
using SkiaSharp.Views.Tizen.Interop;

namespace SkiaSharp.Views.Tizen
{
	/// <summary>An abstract view that can be inherited from to allow drawing on using SkiaSharp drawing commands.</summary>
	/// <remarks />
	public abstract class CustomRenderingView : Widget
	{
		private readonly Evas.ImagePixelsSetCallback redrawCallback;

		private IntPtr animator;
		private RenderingMode renderingMode = RenderingMode.WhenDirty;

		/// <summary>The pointer to the underlying control which provides the native drawing surface.</summary>
		/// <remarks />
		protected IntPtr evasImage;

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Tizen.CustomRenderingView" /> class.</summary>
		/// <param name="parent">The parent object.</param>
		/// <remarks>Use this constructor when creating the view programmatically from code.</remarks>
		public CustomRenderingView(EvasObject parent)
			: base(parent)
		{
			Resized += (sender, e) => OnResized();
			redrawCallback = (d, o) => OnDrawFrame();
		}

		/// <summary>Gets the current canvas size.</summary>
		/// <value>The current canvas size in pixels.</value>
		/// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
		public SKSize CanvasSize => GetSurfaceSize();

		/// <summary>Gets or sets the rendering mode.</summary>
		/// <value>The rendering mode.</value>
		/// <remarks />
		public RenderingMode RenderingMode
		{
			get { return renderingMode; }
			set
			{
				if (renderingMode != value)
				{
					renderingMode = value;

					if (renderingMode == RenderingMode.Continuously)
						CreateAnimator();
					else
						DestroyAnimator();
				}
			}
		}

		/// <summary>Invalidates the entire surface of the control and causes the control to be redrawn.</summary>
		/// <remarks />
		public void Invalidate()
		{
			if (RenderingMode == RenderingMode.WhenDirty)
				Evas.evas_object_image_pixels_dirty_set(evasImage, true);
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.Views.Tizen.CustomRenderingView" /> types to create the native resources which should be present throughout whole life of the control.</summary>
		/// <param name="parent">The parent object.</param>
		/// <remarks />
		protected virtual void CreateNativeResources(EvasObject parent)
		{
			// empty on purpose
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.Views.Tizen.CustomRenderingView" /> types to destroy the native resources.</summary>
		/// <remarks />
		protected virtual void DestroyNativeResources()
		{
			// empty on purpose
		}

		/// <summary>Invalidates the entire surface of the control and causes the control to be redrawn.</summary>
		/// <remarks />
		protected abstract void OnDrawFrame();

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.Views.Tizen.CustomRenderingView" /> types to update the drawing surface dimensions.</summary>
		/// <param name="geometry">The current geometry of the control.</param>
		/// <returns>Returns <see langword="true" /> if the size has changed, otherwise <see langword="false" />.</returns>
		/// <remarks />
		protected abstract bool UpdateSurfaceSize(Rect geometry);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.Views.Tizen.CustomRenderingView" /> types to provide the dimensions of the current drawing surface.</summary>
		/// <returns>Returns the current drawing surface dimensions.</returns>
		/// <remarks />
		protected abstract SKSizeI GetSurfaceSize();

		/// <summary>Gets the raw pixel size of the drawing surface.</summary>
		/// <returns>Returns the raw pixel size of the drawing surface.</returns>
		/// <remarks />
		protected virtual SKSizeI GetRawSurfaceSize() => GetSurfaceSize();

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.Views.Tizen.CustomRenderingView" /> types to construct the drawing surface.</summary>
		/// <remarks />
		protected virtual void CreateDrawingSurface()
		{
			// empty on purpose
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.Views.Tizen.CustomRenderingView" /> types to destroy the drawing surface.</summary>
		/// <remarks />
		protected virtual void DestroyDrawingSurface()
		{
			// empty on purpose
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.Views.Tizen.CustomRenderingView" /> types to creates a Widget handle.</summary>
		/// <param name="parent">The parent object.</param>
		/// <returns>Returns the pointer to the new handle.</returns>
		/// <remarks />
		protected sealed override IntPtr CreateHandle(EvasObject parent)
		{
			var handle = Interop.Elementary.elm_layout_add(parent);
			Interop.Elementary.elm_layout_theme_set(handle, "layout", "background", "default");

			evasImage = Evas.evas_object_image_filled_add(Interop.Evas.evas_object_evas_get(handle));
			Evas.evas_object_image_colorspace_set(evasImage, Evas.Colorspace.ARGB8888);
			Evas.evas_object_image_smooth_scale_set(evasImage, true);
			Evas.evas_object_image_alpha_set(evasImage, true);

			Interop.Elementary.elm_object_part_content_set(handle, "elm.swallow.content", evasImage);

			CreateNativeResources(parent);

			return handle;
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.Views.Tizen.CustomRenderingView" /> types to clean up resources used by the control.</summary>
		/// <remarks />
		protected sealed override void OnUnrealize()
		{
			DestroyAnimator();
			DestroyDrawingSurface();
			DestroyNativeResources();

			base.OnUnrealize();
		}

		/// <summary>Indicate to the control that the it has been resized.</summary>
		/// <remarks />
		protected void OnResized()
		{
			var geometry = Geometry;

			// control is not yet fully initialized
			if (geometry.Width <= 0 || geometry.Height <= 0)
				return;

			if (UpdateSurfaceSize(geometry))
			{
				// disconnect the callback
				Evas.evas_object_image_native_surface_set(evasImage, IntPtr.Zero);

				// recreate the drawing surface to match the new size
				DestroyDrawingSurface();

				var size = GetRawSurfaceSize();
				Evas.evas_object_image_size_set(evasImage, size.Width, size.Height);

				CreateDrawingSurface();

				// set the image callback; will be invoked when image is marked as dirty
				Evas.evas_object_image_pixels_get_callback_set(evasImage, redrawCallback, IntPtr.Zero);

				// repaint
				Invalidate();
			}
		}

		private void CreateAnimator()
		{
			if (animator == IntPtr.Zero)
			{
				animator = EcoreAnimator.AddAnimator(() =>
				{
					Evas.evas_object_image_pixels_dirty_set(evasImage, true);
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
	}
}
