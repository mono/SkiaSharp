#nullable enable

using System;
using System.Threading;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidSurface = Android.Views.Surface;

namespace SkiaSharp.Views.Android
{
	/// <summary>A hardware-accelerated view that renders SkiaSharp drawing commands using the Graphite GPU backend.</summary>
	/// <remarks>On Android, this view uses Vulkan for GPU rendering and presentation.</remarks>
	public class SKGraphiteView : FrameLayout
	{
		private Presenter? presenter;
		private SKGraphiteContext? context;
		private bool ignorePixelScaling;
		private bool hasRenderLoop;

		/// <summary>Initializes a new instance of the <see cref="SKGraphiteView" /> class.</summary>
		/// <param name="context">The <see cref="Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		/// <remarks>Use this constructor when creating the view programmatically from code.</remarks>
		public SKGraphiteView(Context context)
			: base(context)
		{
			Initialize();
		}

		/// <summary>Initializes a new instance of the <see cref="SKGraphiteView" /> class with the specified XML attributes.</summary>
		/// <param name="context">The <see cref="Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		/// <param name="attrs">The attributes of the XML tag that is inflating the view.</param>
		/// <remarks>This constructor is called when inflating the view from an Android XML layout file.</remarks>
		public SKGraphiteView(Context context, IAttributeSet? attrs)
			: base(context, attrs)
		{
			Initialize();
		}

		/// <summary>Initializes a new instance of the <see cref="SKGraphiteView" /> class with the specified XML attributes and style.</summary>
		/// <param name="context">The <see cref="Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		/// <param name="attrs">The attributes of the XML tag that is inflating the view.</param>
		/// <param name="defStyleAttr">An attribute in the current theme that contains a reference to a style resource that supplies default values for the view. Can be 0 to not look for defaults.</param>
		/// <remarks>This constructor is called when inflating the view from an Android XML layout file and applying a class-specific base style from a theme attribute.</remarks>
		public SKGraphiteView(Context context, IAttributeSet? attrs, int defStyleAttr)
			: base(context, attrs, defStyleAttr)
		{
			Initialize();
		}

		/// <summary>Initializes a new instance of the <see cref="SKGraphiteView" /> class from a JNI object reference.</summary>
		/// <param name="javaReference">A <see cref="IntPtr" /> to a Java Native Interface (JNI) object reference.</param>
		/// <param name="transfer">A <see cref="JniHandleOwnership" /> indicating how to handle the Java reference.</param>
		/// <remarks>This constructor is used by the Android runtime when creating managed representations of JNI objects. It is not intended to be called directly from user code.</remarks>
		protected SKGraphiteView(IntPtr javaReference, JniHandleOwnership transfer)
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
		public SKSize CanvasSize { get; internal set; }

		/// <summary>Gets the current Graphite GPU context.</summary>
		/// <value>The <see cref="SkiaSharp.SKGraphiteContext" /> currently used for rendering, or <see langword="null" /> if the surface has not been initialized.</value>
		/// <remarks />
		public SKGraphiteContext? GraphiteContext => Volatile.Read(ref context);

		/// <summary>Gets or sets a value indicating whether the drawing canvas should use device-independent pixel dimensions instead of physical pixels.</summary>
		/// <value><see langword="true" /> to ignore pixel scaling; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IgnorePixelScaling
		{
			get => Volatile.Read(ref ignorePixelScaling);
			set
			{
				if (Volatile.Read(ref ignorePixelScaling) == value)
					return;

				Volatile.Write(ref ignorePixelScaling, value);
				InvalidateSurface();
			}
		}

		/// <summary>Gets or sets a value indicating whether the view renders continuously on the display refresh loop.</summary>
		/// <value><see langword="true" /> to enable continuous rendering; <see langword="false" /> to render only on demand.</value>
		/// <remarks />
		public bool HasRenderLoop
		{
			get => Volatile.Read(ref hasRenderLoop);
			set
			{
				if (Volatile.Read(ref hasRenderLoop) == value)
					return;

				Volatile.Write(ref hasRenderLoop, value);
				presenter?.Worker?.Wake();
			}
		}

		/// <summary>Occurs when the surface needs to be redrawn.</summary>
		/// <remarks>There are two ways to draw on this surface: by overriding <see cref="OnPaintSurface" /> or by attaching a handler to <see cref="PaintSurface" />.</remarks>
		public event EventHandler<SKPaintGraphiteSurfaceEventArgs>? PaintSurface;

		/// <summary>Informs the view that its drawing surface needs to be redrawn.</summary>
		/// <remarks />
		public void InvalidateSurface()
		{
			presenter?.RequestRender();
		}

		/// <summary>Implement this to draw on the canvas.</summary>
		/// <param name="e">The event arguments that contain the drawing surface and GPU context information.</param>
		/// <remarks>There are two ways to draw on this surface: by overriding <see cref="OnPaintSurface" /> or by attaching a handler to <see cref="PaintSurface" />. If this method is overridden, then the base must be called, otherwise the event will not be fired.</remarks>
		protected virtual void OnPaintSurface(SKPaintGraphiteSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		internal void RaisePaintSurface(SKPaintGraphiteSurfaceEventArgs e)
		{
			OnPaintSurface(e);
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

		/// <summary>Releases the unmanaged resources used by the <see cref="SKGraphiteView" /> and optionally releases the managed resources.</summary>
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

			if (Build.VERSION.SdkInt < BuildVersionCodes.N)
			{
				Console.WriteLine("Graphite Vulkan presentation requires Android API 24 or later.");
				return;
			}

			presenter = new Presenter(this);
			AddView(presenter, new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent));
		}

		private void DisconnectPresenter()
		{
			var oldPresenter = presenter;
			if (oldPresenter is null)
				return;

			presenter = null;
			oldPresenter.Stop();
			RemoveView(oldPresenter);
			oldPresenter.Dispose();

			Volatile.Write(ref context, null);
			CanvasSize = SKSize.Empty;
		}

		private sealed class Presenter : TextureView, TextureView.ISurfaceTextureListener
		{
			private readonly SKGraphiteView owner;
			private AndroidSurface? surface;

			public RenderThread? Worker { get; private set; }

			public Presenter(SKGraphiteView owner)
				: base(owner.Context!)
			{
				this.owner = owner;
				SetOpaque(false);
				SurfaceTextureListener = this;
			}

			public void RequestRender()
			{
				Worker?.RequestFrame();
			}

			public void OnSurfaceTextureAvailable(SurfaceTexture texture, int width, int height)
			{
				if (Worker?.IsAlive == true)
				{
					Stop();
				}

				if (width > 0 && height > 0)
				{
					texture.SetDefaultBufferSize(width, height);
				}

				surface = new AndroidSurface(texture);
				Worker = new RenderThread(owner, surface, width, height);
				Worker.Start();
			}

			public void OnSurfaceTextureSizeChanged(SurfaceTexture texture, int width, int height)
			{
				if (width > 0 && height > 0)
				{
					texture.SetDefaultBufferSize(width, height);
				}

				Worker?.Resize(width, height);
			}

			public void OnSurfaceTextureUpdated(SurfaceTexture texture)
			{
			}

			public bool OnSurfaceTextureDestroyed(SurfaceTexture texture)
			{
				Stop();
				return true;
			}

			public void Stop()
			{
				Worker?.Stop();
				Worker = null;

				surface?.Dispose();
				surface = null;
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					Stop();
					SurfaceTextureListener = null;
				}

				base.Dispose(disposing);
			}
		}

		private sealed class RenderThread
		{
			private readonly SKGraphiteView owner;
			private readonly AndroidSurface surface;
			private readonly Thread thread;
			private readonly object wakeLock = new();
			private readonly AutoResetEvent wake = new(false);
			private readonly ManualResetEventSlim presentationReleased = new(false);
			private int width;
			private int height;
			private int resize;
			private int requested = 1;
			private int stopping;

			public bool IsAlive => thread.IsAlive;

			public RenderThread(SKGraphiteView owner, AndroidSurface surface, int width, int height)
			{
				this.owner = owner;
				this.surface = surface;
				this.width = width;
				this.height = height;
				thread = new Thread(Run)
				{
					IsBackground = true,
					Name = "SkiaSharp Graphite presentation",
				};
			}

			public void Start()
			{
				thread.Start();
			}

			public void Wake()
			{
				lock (wakeLock)
				{
					if (Volatile.Read(ref stopping) == 0)
					{
						wake.Set();
					}
				}
			}

			public void RequestFrame()
			{
				Interlocked.Exchange(ref requested, 1);
				Wake();
			}

			public void Resize(int width, int height)
			{
				Interlocked.Exchange(ref this.width, width);
				Interlocked.Exchange(ref this.height, height);
				Interlocked.Exchange(ref resize, 1);
				RequestFrame();
			}

			public void Stop()
			{
				lock (wakeLock)
				{
					if (Interlocked.Exchange(ref stopping, 1) == 0)
					{
						wake.Set();
					}
				}

				if (System.Environment.CurrentManagedThreadId == thread.ManagedThreadId)
					return;

				presentationReleased.Wait();
			}

			private void Run()
			{
				VulkanGraphiteRenderer? renderer = null;
				try
				{
					renderer = new VulkanGraphiteRenderer(
						surface,
						Volatile.Read(ref width),
						Volatile.Read(ref height));

					Volatile.Write(ref owner.context, renderer.Context);

					while (Volatile.Read(ref stopping) == 0)
					{
						if (Interlocked.Exchange(ref resize, 0) != 0)
						{
							renderer.Resize(
								Volatile.Read(ref width),
								Volatile.Read(ref height));
						}

						if (Volatile.Read(ref owner.hasRenderLoop) || Interlocked.Exchange(ref requested, 0) != 0)
						{
							if (!renderer.Render(owner))
							{
								Interlocked.Exchange(ref requested, 1);
							}
						}

						renderer.CheckAsyncWorkCompletion();
						wake.WaitOne(16);
					}
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine($"Graphite rendering failed: {ex}");
				}
				finally
				{
					Interlocked.Exchange(ref stopping, 1);

					try
					{
						renderer?.Dispose();
					}
					catch (Exception ex)
					{
						Console.Error.WriteLine($"Graphite cleanup failed: {ex}");
					}
					finally
					{
						Volatile.Write(ref owner.context, null);
						owner.CanvasSize = SKSize.Empty;
						presentationReleased.Set();
					}

					lock (wakeLock)
					{
						wake.Dispose();
					}
				}
			}
		}
	}
}
