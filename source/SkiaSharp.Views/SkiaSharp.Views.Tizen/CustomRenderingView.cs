using System;
using ElmSharp;
using SkiaSharp.Views.Tizen.Interop;

namespace SkiaSharp.Views.Tizen
{
	public abstract class CustomRenderingView : Widget
	{
		private readonly Evas.ImagePixelsSetCallback redrawCallback;

		private IntPtr animator;
		private RenderingMode renderingMode = RenderingMode.WhenDirty;

		protected IntPtr evasImage;

		public CustomRenderingView(EvasObject parent)
			: base(parent)
		{
			Resized += (sender, e) => OnResized();
			redrawCallback = (d, o) => OnDrawFrame();
		}

		public SKSize CanvasSize => GetSurfaceSize();

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

		public void Invalidate()
		{
			if (RenderingMode == RenderingMode.WhenDirty)
				Evas.evas_object_image_pixels_dirty_set(evasImage, true);
		}

		protected virtual void CreateNativeResources(EvasObject parent)
		{
			// empty on purpose
		}

		protected virtual void DestroyNativeResources()
		{
			// empty on purpose
		}

		protected abstract void OnDrawFrame();

		protected abstract bool UpdateSurfaceSize(Rect geometry);

		protected abstract SKSizeI GetSurfaceSize();

		protected virtual SKSizeI GetRawSurfaceSize() => GetSurfaceSize();

		protected virtual void CreateDrawingSurface()
		{
			// empty on purpose
		}

		protected virtual void DestroyDrawingSurface()
		{
			// empty on purpose
		}

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

		protected sealed override void OnUnrealize()
		{
			DestroyAnimator();
			DestroyDrawingSurface();
			DestroyNativeResources();

			base.OnUnrealize();
		}

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
