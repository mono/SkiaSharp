#nullable enable

using System;
using System.Threading;
using Android.Content;
using Android.Opengl;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using Javax.Microedition.Khronos.Opengles;
using EGLConfig = Javax.Microedition.Khronos.Egl.EGLConfig;

namespace SkiaSharp.Views.Android
{
	/// <summary>A hardware-accelerated view that renders SkiaSharp drawing commands using the Ganesh GPU backend.</summary>
	/// <remarks>On Android, this view uses OpenGL ES for GPU rendering and presentation.</remarks>
	public class SKGaneshView : FrameLayout
	{
		private GLTextureView? presenter;
		private Renderer? renderer;
		private GRContext? context;
		private bool ignorePixelScaling;
		private bool hasRenderLoop;

		/// <summary>Initializes a new instance of the <see cref="SKGaneshView" /> class.</summary>
		/// <param name="context">The <see cref="Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		/// <remarks>Use this constructor when creating the view programmatically from code.</remarks>
		public SKGaneshView(Context context)
			: base(context)
		{
			Initialize();
		}

		/// <summary>Initializes a new instance of the <see cref="SKGaneshView" /> class with the specified XML attributes.</summary>
		/// <param name="context">The <see cref="Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		/// <param name="attrs">The attributes of the XML tag that is inflating the view.</param>
		/// <remarks>This constructor is called when inflating the view from an Android XML layout file.</remarks>
		public SKGaneshView(Context context, IAttributeSet? attrs)
			: base(context, attrs)
		{
			Initialize();
		}

		/// <summary>Initializes a new instance of the <see cref="SKGaneshView" /> class with the specified XML attributes and style.</summary>
		/// <param name="context">The <see cref="Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		/// <param name="attrs">The attributes of the XML tag that is inflating the view.</param>
		/// <param name="defStyleAttr">An attribute in the current theme that contains a reference to a style resource that supplies default values for the view. Can be 0 to not look for defaults.</param>
		/// <remarks>This constructor is called when inflating the view from an Android XML layout file and applying a class-specific base style from a theme attribute.</remarks>
		public SKGaneshView(Context context, IAttributeSet? attrs, int defStyleAttr)
			: base(context, attrs, defStyleAttr)
		{
			Initialize();
		}

		/// <summary>Initializes a new instance of the <see cref="SKGaneshView" /> class from a JNI object reference.</summary>
		/// <param name="javaReference">A <see cref="IntPtr" /> to a Java Native Interface (JNI) object reference.</param>
		/// <param name="transfer">A <see cref="JniHandleOwnership" /> indicating how to handle the Java reference.</param>
		/// <remarks>This constructor is used by the Android runtime when creating managed representations of JNI objects. It is not intended to be called directly from user code.</remarks>
		protected SKGaneshView(IntPtr javaReference, JniHandleOwnership transfer)
			: base(javaReference, transfer)
		{
			Initialize();
		}

		private void Initialize()
		{
			SetClipChildren(true);
		}

		/// <summary>Gets the current canvas size.</summary>
		/// <value>The current canvas size in pixels, or in device-independent units when <see cref="IgnorePixelScaling" /> is <see langword="true" />.</value>
		/// <remarks />
		public SKSize CanvasSize { get; private set; }

		/// <summary>Gets the current Ganesh GPU context.</summary>
		/// <value>The <see cref="SkiaSharp.GRContext" /> currently used for rendering, or <see langword="null" /> if the surface has not been initialized.</value>
		/// <remarks />
		public GRContext? GRContext => Volatile.Read(ref context);

		/// <summary>Gets or sets a value indicating whether the drawing canvas should use device-independent pixel dimensions instead of physical pixels.</summary>
		/// <value><see langword="true" /> to ignore pixel scaling; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
			set
			{
				if (ignorePixelScaling == value)
					return;

				ignorePixelScaling = value;
				InvalidateSurface();
			}
		}

		/// <summary>Gets or sets a value indicating whether the view renders continuously on the display refresh loop.</summary>
		/// <value><see langword="true" /> to enable continuous rendering; <see langword="false" /> to render only on demand.</value>
		/// <remarks />
		public bool HasRenderLoop
		{
			get => hasRenderLoop;
			set
			{
				if (hasRenderLoop == value)
					return;

				hasRenderLoop = value;
				if (presenter is not null)
				{
					presenter.RenderMode = value ? Rendermode.Continuously : Rendermode.WhenDirty;
				}
			}
		}

		/// <summary>Occurs when the surface needs to be redrawn.</summary>
		/// <remarks>There are two ways to draw on this surface: by overriding <see cref="OnPaintSurface" /> or by attaching a handler to <see cref="PaintSurface" />.</remarks>
		public event EventHandler<SKPaintGaneshSurfaceEventArgs>? PaintSurface;

		/// <summary>Informs the view that its drawing surface needs to be redrawn.</summary>
		/// <remarks />
		public void InvalidateSurface()
		{
			presenter?.RequestRender();
		}

		/// <summary>Implement this to draw on the canvas.</summary>
		/// <param name="e">The event arguments that contain the drawing surface and GPU context information.</param>
		/// <remarks>There are two ways to draw on this surface: by overriding <see cref="OnPaintSurface" /> or by attaching a handler to <see cref="PaintSurface" />. If this method is overridden, then the base must be called, otherwise the event will not be fired.</remarks>
		protected virtual void OnPaintSurface(SKPaintGaneshSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		/// <summary>Called when the view is attached to a window.</summary>
		/// <remarks />
		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			EnsurePresenter();
		}

		/// <summary>Called when the view is detached from a window.</summary>
		/// <remarks />
		protected override void OnDetachedFromWindow()
		{
			DisconnectPresenter();

			base.OnDetachedFromWindow();
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="SKGaneshView" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DisconnectPresenter();
			}

			base.Dispose(disposing);
		}

		private void EnsurePresenter()
		{
			if (presenter is not null || Context is null)
				return;

			presenter = new GLTextureView(Context);
			presenter.SetOpaque(false);
			presenter.SetEGLContextClientVersion(2);
			presenter.SetEGLConfigChooser(8, 8, 8, 8, 0, 8);

			renderer = new Renderer(this);
			presenter.SetRenderer(renderer);
			presenter.RenderMode = hasRenderLoop ? Rendermode.Continuously : Rendermode.WhenDirty;

			AddView(presenter, new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent));
		}

		private void DisconnectPresenter()
		{
			var oldPresenter = presenter;
			var oldRenderer = renderer;
			presenter = null;
			renderer = null;

			if (oldPresenter is not null)
			{
				RemoveView(oldPresenter);
				oldPresenter.Dispose();
			}

			oldRenderer?.Dispose();
			Volatile.Write(ref context, null);
			CanvasSize = SKSize.Empty;
		}

		private sealed class Renderer : Java.Lang.Object, GLTextureView.IRenderer
		{
			private readonly SKGaneshView owner;
			private GRGlInterface? glInterface;
			private GRBackendRenderTarget? target;
			private SKSurface? surface;
			private int width;
			private int height;

			public Renderer(SKGaneshView owner)
			{
				this.owner = owner;
			}

			public void OnSurfaceCreated(IGL10? gl, EGLConfig? config)
			{
				FreeContext();
			}

			public void OnSurfaceChanged(IGL10? gl, int width, int height)
			{
				this.width = width;
				this.height = height;

				surface?.Dispose();
				surface = null;

				target?.Dispose();
				target = null;

				GLES20.GlViewport(0, 0, width, height);
			}

			public void OnSurfaceDestroyed()
			{
				FreeContext();
			}

			public void OnDrawFrame(IGL10? gl)
			{
				if (width <= 0 || height <= 0)
					return;

				var currentContext = Volatile.Read(ref owner.context);
				if (currentContext is null)
				{
					glInterface ??= GRGlInterface.Create();
					if (glInterface is null)
						return;

					currentContext = GRContext.CreateGl(glInterface);
					if (currentContext is null)
						return;

					Volatile.Write(ref owner.context, currentContext);
				}

				const SKColorType colorType = SKColorType.Rgba8888;
				const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

				if (target is null || surface is null || !target.IsValid)
				{
					surface?.Dispose();
					surface = null;

					target?.Dispose();
					target = null;

					var buffer = new int[3];
					GLES20.GlGetIntegerv(GLES20.GlFramebufferBinding, buffer, 0);
					GLES20.GlGetIntegerv(GLES20.GlStencilBits, buffer, 1);
					GLES20.GlGetIntegerv(GLES20.GlSamples, buffer, 2);

					var samples = Math.Min(buffer[2], currentContext.GetMaxSurfaceSampleCount(colorType));
					var glInfo = new GRGlFramebufferInfo((uint)buffer[0], colorType.ToGlSizedFormat());

					target = new GRBackendRenderTarget(width, height, samples, buffer[1], glInfo);
					surface = SKSurface.Create(currentContext, target, surfaceOrigin, colorType);
					if (surface is null)
					{
						target.Dispose();
						target = null;
						return;
					}
				}

				var density = owner.Resources?.DisplayMetrics?.Density ?? 1f;
				if (density <= 0f)
					density = 1f;

				var raw = new SKImageInfo(width, height, colorType, SKAlphaType.Premul);
				var info = owner.IgnorePixelScaling
					? raw.WithSize((int)(width / density), (int)(height / density))
					: raw;

				owner.CanvasSize = info.Size;

				using (new SKAutoCanvasRestore(surface.Canvas, true))
				{
					surface.Canvas.Clear(SKColors.Transparent);

					if (owner.IgnorePixelScaling)
					{
						surface.Canvas.Scale(density);
					}

					var e = new SKPaintGaneshSurfaceEventArgs(surface, currentContext, target, surfaceOrigin, info, raw);
					owner.OnPaintSurface(e);
				}

				surface.Flush();
				currentContext.Flush();
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					FreeContext();
				}

				base.Dispose(disposing);
			}

			private void FreeContext()
			{
				surface?.Dispose();
				surface = null;

				target?.Dispose();
				target = null;

				var currentContext = Interlocked.Exchange(ref owner.context, null);
				currentContext?.Dispose();

				glInterface?.Dispose();
				glInterface = null;

				owner.CanvasSize = SKSize.Empty;
			}
		}
	}
}
