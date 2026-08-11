#if __IOS__ || __MACOS__ || __TVOS__
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using CoreGraphics;
using Foundation;
using Metal;
using MetalKit;

#if __IOS__
namespace SkiaSharp.Views.iOS
#elif __MACOS__
namespace SkiaSharp.Views.Mac
#elif __TVOS__
namespace SkiaSharp.Views.tvOS
#endif
{
	[Register (nameof (SKGraphiteMetalView))]
	[DesignTimeVisible (true)]
	public class SKGraphiteMetalView : MTKView, IMTKViewDelegate, IComponent
	{
#pragma warning disable 67
		private event EventHandler? DisposedInternal;
#pragma warning restore 67

		private bool designMode;
		private IMTLCommandQueue? commandQueue;
		private SKGraphiteMtlBackendContext? backendContext;
		private SKGraphiteContext? context;
		private SKGraphiteRecorder? recorder;
		private ulong frameId;
		private bool contextUnrecoverable;

		ISite? IComponent.Site { get; set; }

		event EventHandler? IComponent.Disposed
		{
			add => DisposedInternal += value;
			remove => DisposedInternal -= value;
		}

		public SKGraphiteMetalView ()
			: this (CGRect.Empty)
		{
		}

		public SKGraphiteMetalView (CGRect frame)
			: base (frame, null)
		{
			Initialize ();
		}

		public SKGraphiteMetalView (CGRect frame, IMTLDevice device)
			: base (frame, device)
		{
			Initialize ();
		}

		public SKGraphiteMetalView (IntPtr handle)
			: base (handle)
		{
		}

		public override void AwakeFromNib ()
		{
			base.AwakeFromNib ();
			Initialize ();
		}

		public SKSize CanvasSize { get; private set; }

		public SKGraphiteContext? GraphiteContext => context;

		public event EventHandler<SKPaintGraphiteSurfaceEventArgs>? PaintSurface;

		public event EventHandler<SKGraphiteRenderFailedEventArgs>? RenderFailed;

		protected virtual void OnPaintSurface (SKPaintGraphiteSurfaceEventArgs e) =>
			PaintSurface?.Invoke (this, e);

		protected virtual void OnRenderFailed (Exception exception)
		{
			if (RenderFailed is null)
				throw new InvalidOperationException ("Graphite rendering failed.", exception);
			RenderFailed.Invoke (this, new SKGraphiteRenderFailedEventArgs (exception));
		}

		void IMTKViewDelegate.DrawableSizeWillChange (MTKView view, CGSize size)
		{
			CanvasSize = new SKSize ((float)size.Width, (float)size.Height);

			if (Paused && EnableSetNeedsDisplay)
#if __IOS__ || __TVOS__
				SetNeedsDisplay ();
#elif __MACOS__
				NeedsDisplay = true;
#endif
		}

		void IMTKViewDelegate.Draw (MTKView view)
		{
			if (designMode)
				return;

			try {
				EnsureGraphite ();
				if (context is null || recorder is null || commandQueue is null)
					return;

				var drawable = CurrentDrawable;
				var texture = drawable?.Texture;
				if (drawable is null || texture is null)
					return;

				CanvasSize = new SKSize ((float)DrawableSize.Width, (float)DrawableSize.Height);
				var width = (int)CanvasSize.Width;
				var height = (int)CanvasSize.Height;
				if (width <= 0 || height <= 0)
					return;

				using var backendTexture = SKGraphiteBackendTexture.CreateMetal (
					width, height, texture.Handle)
					?? throw new InvalidOperationException ("Unable to wrap the current Metal drawable.");
				using var surface = SKSurface.Create (
					recorder, backendTexture, SKColorType.Bgra8888)
					?? throw new InvalidOperationException ("Unable to create a Graphite surface.");
				using (var restore = new SKAutoCanvasRestore (surface.Canvas, true)) {
					var info = new SKImageInfo (
						width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
					OnPaintSurface (new SKPaintGraphiteSurfaceEventArgs (
						surface, backendTexture, context, info));
				}

				using var recording = recorder.Snap ()
					?? throw new InvalidOperationException ("Unable to snap the Graphite recording.");
				var status = context.InsertRecording (recording);
				if (status != SKGraphiteInsertStatus.Success) {
					if (status == SKGraphiteInsertStatus.AddCommandsFailed ||
						status == SKGraphiteInsertStatus.AsyncShaderCompilesFailed ||
						status == SKGraphiteInsertStatus.OutOfOrderRecording) {
						contextUnrecoverable = true;
					}
					throw new InvalidOperationException ($"Unable to insert the Graphite recording: {status}.");
				}
				if (!context.Submit (new SKGraphiteSubmitInfo {
					Sync = false,
					MarkBoundary = true,
					FrameID = ++frameId,
				})) {
					contextUnrecoverable = true;
					throw new InvalidOperationException ("Unable to submit the Graphite frame.");
				}

				using var commandBuffer = commandQueue.CommandBuffer ()
					?? throw new InvalidOperationException ("Unable to create a Metal presentation command buffer.");
				commandBuffer.PresentDrawable (drawable);
				commandBuffer.Commit ();
			} catch (Exception exception) {
				Paused = true;
				OnRenderFailed (exception);
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				Paused = true;
				Delegate = null;

				if (context is not null &&
					!context.IsDeviceLost &&
					!contextUnrecoverable) {
					context.Submit (new SKGraphiteSubmitInfo { Sync = true });
				}

				recorder?.Dispose ();
				recorder = null;
				context?.Dispose ();
				context = null;
				backendContext?.Dispose ();
				backendContext = null;
				commandQueue?.Dispose ();
				commandQueue = null;
			}

			base.Dispose (disposing);
		}

		private void Initialize ()
		{
			designMode = ((IComponent)this).Site?.DesignMode == true ||
				!EnvironmentExtensions.IsValidEnvironment;
			if (designMode)
				return;

			var device = Device ?? MTLDevice.SystemDefault;
			if (device is null)
				throw new PlatformNotSupportedException ("Metal is not supported on this device.");

			Device = device;
			ColorPixelFormat = MTLPixelFormat.BGRA8Unorm;
			DepthStencilPixelFormat = MTLPixelFormat.Invalid;
			SampleCount = 1;
			FramebufferOnly = false;
			Delegate = this;
		}

		private void EnsureGraphite ()
		{
			if (context is not null)
				return;

			var device = Device;
			if (device is null)
				return;
			if (!MetalCanDriveGraphite (device.Handle))
				throw new PlatformNotSupportedException (
					"The Metal device does not support a GPU family required by Graphite.");

			commandQueue = device.CreateCommandQueue ()
				?? throw new PlatformNotSupportedException ("Unable to create a Metal command queue.");
			backendContext = new SKGraphiteMtlBackendContext {
				Device = device,
				Queue = commandQueue,
			};
			context = SKGraphiteContext.CreateMetal (backendContext)
				?? throw new PlatformNotSupportedException ("Unable to create a Graphite Metal context.");

			var imageCache = new SKGraphiteImageCache ();
			recorder = context.CreateRecorder (
				-1,
				imageCache.FindOrCreate,
				imageCache.Dispose);
			if (recorder is null) {
				imageCache.Dispose ();
				throw new InvalidOperationException ("Unable to create a Graphite recorder.");
			}
		}

		private static bool MetalCanDriveGraphite (IntPtr device)
		{
			if (IsRunningOnAppleSimulator)
				return true;

#if __MACCATALYST__
			if (!OperatingSystem.IsMacCatalystVersionAtLeast (13, 1))
				return false;
#elif __IOS__
			if (!OperatingSystem.IsIOSVersionAtLeast (13))
				return false;
#elif __TVOS__
			if (!OperatingSystem.IsTvOSVersionAtLeast (13))
				return false;
#elif __MACOS__
			if (!OperatingSystem.IsMacOSVersionAtLeast (10, 15))
				return false;
#endif

			var selector = sel_registerName ("supportsFamily:");
#if (__IOS__ && !__MACCATALYST__) || __TVOS__
			var families = new ulong[] {
				1009, 1008, 1007, 1006, 1005, 1004, 1003, 1002, 2002,
			};
#else
			var families = new ulong[] { 1009, 1008, 1007, 2002 };
#endif
			foreach (var family in families) {
				if (objc_msgSend_supportsFamily (device, selector, family) != 0)
					return true;
			}
			return false;
		}

		private static bool IsRunningOnAppleSimulator =>
			!string.IsNullOrEmpty (
				Environment.GetEnvironmentVariable ("SIMULATOR_UDID")) ||
			!string.IsNullOrEmpty (
				Environment.GetEnvironmentVariable ("SIMULATOR_DEVICE_NAME"));

		[DllImport ("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		private static extern byte objc_msgSend_supportsFamily (
			IntPtr receiver,
			IntPtr selector,
			ulong family);

		[DllImport ("/usr/lib/libobjc.dylib", CharSet = CharSet.Ansi)]
		private static extern IntPtr sel_registerName (string name);
	}
}
#endif
