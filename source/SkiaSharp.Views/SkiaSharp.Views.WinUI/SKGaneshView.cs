#nullable enable

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp.Views.GlesInterop;

#if WINDOWS
namespace SkiaSharp.Views.Windows
#else
namespace SkiaSharp.Views.UWP
#endif
{
	/// <summary>A hardware-accelerated XAML view that renders SkiaSharp drawing commands using the Ganesh GPU backend.</summary>
	/// <remarks>On Windows, this view uses OpenGL ES via ANGLE (Almost Native Graphics Layer Engine) for GPU rendering and presentation.</remarks>
	public class SKGaneshView : Grid
	{
		private Presenter? presenter;
		private bool ignorePixelScaling;
		private bool hasRenderLoop;

		/// <summary>Initializes a new instance of the <see cref="SKGaneshView" /> class.</summary>
		/// <remarks />
		public SKGaneshView()
		{
			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
		}

		/// <summary>Gets the current canvas size.</summary>
		/// <value>The current canvas size in pixels, or in device-independent units when <see cref="IgnorePixelScaling" /> is <see langword="true" />.</value>
		/// <remarks />
		public SKSize CanvasSize { get; private set; }

		/// <summary>Gets the current Ganesh GPU context.</summary>
		/// <value>The <see cref="SkiaSharp.GRContext" /> currently used for rendering, or <see langword="null" /> if the surface has not been initialized.</value>
		/// <remarks />
		public GRContext? GRContext => presenter?.Context;

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
				if (IsLoaded)
				{
					InvalidateSurface();
				}
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
				if (hasRenderLoop && IsLoaded && presenter is null)
				{
					InvalidateSurface();
				}
				else
				{
					presenter?.UpdateLoop();
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
			if (!IsLoaded)
				return;

			EnsurePresenter();
			presenter?.Render();
		}

		/// <summary>Implement this to draw on the canvas.</summary>
		/// <param name="e">The event arguments that contain the drawing surface and GPU context information.</param>
		/// <remarks>There are two ways to draw on this surface: by overriding <see cref="OnPaintSurface" /> or by attaching a handler to <see cref="PaintSurface" />. If this method is overridden, then the base must be called, otherwise the event will not be fired.</remarks>
		protected virtual void OnPaintSurface(SKPaintGaneshSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			EnsurePresenter();
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			DisconnectPresenter();
		}

		private void EnsurePresenter()
		{
			if (!IsLoaded || presenter is not null)
				return;

			presenter = new Presenter(this);
			Children.Add(presenter);
		}

		private void DisconnectPresenter()
		{
			var old = presenter;
			if (old is null)
				return;

			presenter = null;
			try
			{
				old.Disconnect();
			}
			finally
			{
				Children.Remove(old);
				CanvasSize = SKSize.Empty;
			}
		}

		private sealed class Presenter : SwapChainPanel
		{
			private const SKColorType colorType = SKColorType.Rgba8888;
			private const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

			private readonly SKGaneshView owner;
			private readonly DispatcherTimer timer = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(16),
			};
			private GlesContext? backend;
			private GRGlInterface? glInterface;
			private GRBackendRenderTarget? target;
			private SKSurface? surface;
			private SKSizeI size;
			private bool failed;
			private bool releasing;

			public GRContext? Context { get; private set; }

			public Presenter(SKGaneshView owner)
			{
				this.owner = owner;
				Loaded += OnLoaded;
				Unloaded += OnUnloaded;
				SizeChanged += OnSizeChanged;
				CompositionScaleChanged += OnCompositionScaleChanged;
				timer.Tick += OnTick;
			}

			public void UpdateLoop()
			{
				if (owner.hasRenderLoop && IsLoaded && !failed)
				{
					timer.Start();
				}
				else
				{
					timer.Stop();
				}
			}

			public unsafe void Render()
			{
				if (!IsLoaded || owner.Visibility != Visibility.Visible || ActualWidth <= 0 || ActualHeight <= 0)
					return;

				try
				{
					failed = false;
					backend ??= new GlesContext();
					if (!backend.HasSurface)
					{
						backend.CreateSurface(this, null, CompositionScaleX);
					}

					backend.MakeCurrent();
					backend.GetSurfaceDimensions(out var width, out var height);
					if (width <= 0 || height <= 0)
						return;

					backend.SetViewportSize(width, height);

					if (Context is null)
					{
						glInterface = GRGlInterface.CreateAngle();
						if (glInterface is null)
							return;

						Context = GRContext.CreateGl(glInterface);
						if (Context is null)
							return;
					}

					var nextSize = new SKSizeI(width, height);
					if (target is null || surface is null || nextSize != size || !target.IsValid)
					{
						surface?.Dispose();
						surface = null;

						target?.Dispose();
						target = null;

						int framebuffer, stencil, samples;
						Gles.glGetIntegerv(Gles.GL_FRAMEBUFFER_BINDING, &framebuffer);
						Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, &stencil);
						Gles.glGetIntegerv(Gles.GL_SAMPLES, &samples);

						var maxSamples = Context.GetMaxSurfaceSampleCount(colorType);
						if (samples > maxSamples)
							samples = maxSamples;

						var glInfo = new GRGlFramebufferInfo((uint)framebuffer, colorType.ToGlSizedFormat());
						target = new GRBackendRenderTarget(width, height, samples, stencil, glInfo);
						surface = SKSurface.Create(Context, target, surfaceOrigin, colorType);
						if (surface is null)
						{
							target.Dispose();
							target = null;
							return;
						}

						size = nextSize;
					}

					var raw = new SKImageInfo(width, height, colorType, SKAlphaType.Premul);
					var info = owner.IgnorePixelScaling
						? raw.WithSize((int)ActualWidth, (int)ActualHeight)
						: raw;

					owner.CanvasSize = info.Size;

					using (new SKAutoCanvasRestore(surface.Canvas, true))
					{
						surface.Canvas.Clear(SKColors.Transparent);

						if (owner.IgnorePixelScaling)
						{
							surface.Canvas.Scale(
								width / (float)Math.Max(1, info.Width),
								height / (float)Math.Max(1, info.Height));
						}

						var e = new SKPaintGaneshSurfaceEventArgs(surface, Context, target, surfaceOrigin, info, raw);
						owner.OnPaintSurface(e);
					}

					surface.Flush();
					Context.Flush();

					if (!backend.SwapBuffers())
					{
						throw new InvalidOperationException("ANGLE presentation failed.");
					}

					UpdateLoop();
				}
				catch (Exception ex)
				{
					failed = true;
					timer.Stop();
					Console.Error.WriteLine($"Ganesh rendering failed: {ex}");
					ReleaseContext();
				}
			}

			public void Disconnect()
			{
				timer.Stop();
				timer.Tick -= OnTick;
				Loaded -= OnLoaded;
				Unloaded -= OnUnloaded;
				SizeChanged -= OnSizeChanged;
				CompositionScaleChanged -= OnCompositionScaleChanged;
				ReleaseContext();
			}

			private void OnLoaded(object sender, RoutedEventArgs e)
			{
				UpdateLoop();
				Render();
			}

			private void OnUnloaded(object sender, RoutedEventArgs e)
			{
				owner.DisconnectPresenter();
			}

			private void OnTick(object? sender, object e)
			{
				Render();
			}

			private void OnSizeChanged(object sender, SizeChangedEventArgs e)
			{
				Resize();
			}

			private void OnCompositionScaleChanged(SwapChainPanel sender, object args)
			{
				Resize();
			}

			private void Resize()
			{
				try
				{
					if (backend is not null)
					{
						if (backend.HasSurface)
						{
							backend.MakeCurrent();
						}

						surface?.Dispose();
						surface = null;

						target?.Dispose();
						target = null;

						backend.DestroySurface();
						size = SKSizeI.Empty;
					}

					Render();
				}
				catch (Exception ex)
				{
					failed = true;
					timer.Stop();
					Console.Error.WriteLine($"Ganesh rendering failed: {ex}");
					ReleaseContext();
				}
			}

			private void ReleaseContext()
			{
				if (releasing)
					return;

				releasing = true;
				try
				{
					if (Context is not null && backend is { HasSurface: true })
					{
						try
						{
							backend.MakeCurrent();
						}
						catch
						{
							Context.AbandonContext(false);
						}
					}
				}
				finally
				{
					Context?.AbandonContext(false);

					surface?.Dispose();
					surface = null;

					target?.Dispose();
					target = null;

					Context?.Dispose();
					Context = null;

					glInterface?.Dispose();
					glInterface = null;

					backend?.Dispose();
					backend = null;

					size = SKSizeI.Empty;
					owner.CanvasSize = SKSize.Empty;
					releasing = false;
				}
			}
		}
	}
}
