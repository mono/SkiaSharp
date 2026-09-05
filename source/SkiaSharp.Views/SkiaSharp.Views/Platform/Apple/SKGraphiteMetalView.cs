#nullable enable

#if __IOS__ || __MACOS__ || __TVOS__
using System;
using System.ComponentModel;
using System.Threading;
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
		private event EventHandler? DisposedInternal;

		private bool initialized;
		private bool designMode;
		private IMTLCommandQueue? commandQueue;
		private SKGraphiteMtlBackendContext? backendContext;
		private SKGraphiteContext? context;
		private SKGraphiteRecorder? recorder;
		private ulong frameId;
		private bool requiresContextRecreation;
		private bool requiresBackendRecreation;
		private IntPtr graphiteDeviceHandle;
		private int pendingMetalFailure;
#if __IOS__ || __TVOS__
		private NSObject? willResignActiveObserver;
		private NSObject? didEnterBackgroundObserver;
		private NSObject? didBecomeActiveObserver;
		private bool applicationActive = true;
		private bool resumeAfterActivation;
#endif

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
			if (RenderFailed is null) {
				Console.Error.WriteLine ($"Graphite rendering failed: {exception}");
				return;
			}

			RenderFailed.Invoke (this, new SKGraphiteRenderFailedEventArgs (
				exception,
				requiresContextRecreation || requiresBackendRecreation));
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
			using var autoreleasePool = new NSAutoreleasePool ();

			if (designMode)
				return;

			var insertedExternalRecording = false;
			try {
				if (requiresBackendRecreation)
					DisposeGraphite (submit: false);
				else if (requiresContextRecreation)
					DisposeGraphiteContext (submit: false);
				if (graphiteDeviceHandle != IntPtr.Zero &&
					Device?.Handle != graphiteDeviceHandle) {
					requiresBackendRecreation = true;
					throw new InvalidOperationException (
						"The Metal device changed. Dispose external Recorders before retrying.");
				}
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
					var eventArgs = new SKPaintGraphiteSurfaceEventArgs (
						surface, backendTexture, context, info);
					try {
						OnPaintSurface (eventArgs);
					} finally {
						insertedExternalRecording = eventArgs.HasInsertedRecording;
						requiresContextRecreation |= eventArgs.ContextFailureStatus.HasValue;
					}
					if (eventArgs.ContextFailureStatus is { } failureStatus)
						throw new InvalidOperationException (
							$"Unable to insert a Graphite recording: {failureStatus}.");
				}

				using var recording = recorder.Snap ()
					?? throw new InvalidOperationException ("Unable to snap the Graphite recording.");
				var status = InsertRecording (recording);
				if (status != SKGraphiteInsertStatus.Success)
					throw new InvalidOperationException ($"Unable to insert the Graphite recording: {status}.");
				if (!context.Submit (new SKGraphiteSubmitInfo {
					Sync = false,
					MarkBoundary = true,
					FrameID = ++frameId,
				})) {
					requiresContextRecreation = true;
					throw new InvalidOperationException ("Unable to submit the Graphite frame.");
				}

				using var commandBuffer = commandQueue.CommandBuffer ()
					?? throw new InvalidOperationException ("Unable to create a Metal presentation command buffer.");
				commandBuffer.AddCompletedHandler (HandleMetalCommandBufferCompleted);
				commandBuffer.PresentDrawable (drawable);
				commandBuffer.Commit ();
			} catch (Exception exception) {
				requiresContextRecreation |= insertedExternalRecording;
				requiresContextRecreation |= context?.IsDeviceLost == true;
				recorder?.Dispose ();
				recorder = null;
				Paused = true;
				OnRenderFailed (exception);
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				Paused = true;
				Delegate = null;
#if __IOS__ || __TVOS__
				willResignActiveObserver?.Dispose ();
				willResignActiveObserver = null;
				didEnterBackgroundObserver?.Dispose ();
				didEnterBackgroundObserver = null;
				didBecomeActiveObserver?.Dispose ();
				didBecomeActiveObserver = null;
#endif
				DisposeGraphite (
					submit: context?.IsDeviceLost != true &&
						!requiresContextRecreation &&
						!requiresBackendRecreation);

				DisposedInternal?.Invoke (this, EventArgs.Empty);
			}

			base.Dispose (disposing);
		}

		private SKGraphiteInsertStatus InsertRecording (SKGraphiteRecording recording)
		{
			if (context is null)
				throw new InvalidOperationException ("The Graphite context is unavailable.");

			var status = context.InsertRecording (recording);
			// The view exclusively controls its presentation Recorder, so an ordering
			// failure here cannot be repaired by external code.
			if (status == SKGraphiteInsertStatus.AddCommandsFailed ||
				status == SKGraphiteInsertStatus.AsyncShaderCompilesFailed ||
				status == SKGraphiteInsertStatus.OutOfOrderRecording) {
				requiresContextRecreation = true;
			}
			return status;
		}

		private void DisposeGraphiteContext (bool submit)
		{
			if (submit && context is not null)
				context.Submit (new SKGraphiteSubmitInfo { Sync = true });

			recorder?.Dispose ();
			recorder = null;
			context?.Dispose ();
			context = null;
			frameId = 0;
			requiresContextRecreation = false;
		}

		private void DisposeGraphite (bool submit)
		{
			DisposeGraphiteContext (submit);
			backendContext?.Dispose ();
			backendContext = null;
			commandQueue?.Dispose ();
			commandQueue = null;
			graphiteDeviceHandle = IntPtr.Zero;
			requiresBackendRecreation = false;
		}

		private void Initialize ()
		{
			if (initialized)
				return;

			designMode = ((IComponent)this).Site?.DesignMode == true ||
				!EnvironmentExtensions.IsValidEnvironment;
			if (designMode)
				return;
			initialized = true;

			var device = Device ?? MTLDevice.SystemDefault;
			if (device is null)
				throw new PlatformNotSupportedException ("Metal is not supported on this device.");

			Device = device;
			ColorPixelFormat = MTLPixelFormat.BGRA8Unorm;
			DepthStencilPixelFormat = MTLPixelFormat.Invalid;
			SampleCount = 1;
			FramebufferOnly = false;
			Delegate = this;
#if __IOS__ || __TVOS__
			willResignActiveObserver = UIKit.UIApplication.Notifications.ObserveWillResignActive (
				(_, _) => {
					if (!applicationActive)
						return;
					applicationActive = false;
					resumeAfterActivation = !Paused;
					Paused = true;
				});
			didEnterBackgroundObserver = UIKit.UIApplication.Notifications.ObserveDidEnterBackground (
				(_, _) => ReleaseDrawables ());
			didBecomeActiveObserver = UIKit.UIApplication.Notifications.ObserveDidBecomeActive (
				(_, _) => {
					if (applicationActive)
						return;
					applicationActive = true;
					if (!resumeAfterActivation)
						return;
					resumeAfterActivation = false;
					Paused = false;
					SetNeedsDisplay ();
				});
#endif
		}

		private void EnsureGraphite ()
		{
			if (recorder is not null)
				return;

			var device = Device;
			if (device is null)
				return;
			if (!MetalCanDriveGraphite (device))
				throw new PlatformNotSupportedException (
					"The Metal device does not support a GPU family required by Graphite.");

			commandQueue ??= device.CreateCommandQueue ()
				?? throw new PlatformNotSupportedException ("Unable to create a Metal command queue.");
			backendContext ??= new SKGraphiteMtlBackendContext {
				Device = device,
				Queue = commandQueue,
			};
			graphiteDeviceHandle = device.Handle;

			SKGraphiteContext? newContext = null;
			SKGraphiteRecorder? newRecorder = null;
			SKGraphiteImageCache? imageCache = null;
			try {
				var targetContext = context;
				if (targetContext is null) {
					newContext = SKGraphiteContext.CreateMetal (backendContext)
						?? throw new PlatformNotSupportedException ("Unable to create a Graphite Metal context.");
					targetContext = newContext;
				}

				imageCache = new SKGraphiteImageCache ();
				newRecorder = targetContext.CreateRecorder (
					-1,
					imageCache.FindOrCreate,
					imageCache.Dispose)
					?? throw new InvalidOperationException ("Unable to create a Graphite recorder.");

				context ??= newContext;
				recorder = newRecorder;

				newContext = null;
				newRecorder = null;
				imageCache = null;
			} finally {
				newRecorder?.Dispose ();
				if (newRecorder is null)
					imageCache?.Dispose ();
				newContext?.Dispose ();
			}
		}

		private static bool MetalCanDriveGraphite (IMTLDevice device)
		{
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
			if (!OperatingSystem.IsMacOSVersionAtLeast (12))
				return false;
#endif

			if (IsRunningOnAppleSimulator)
				return true;

#if (__IOS__ && !__MACCATALYST__) || __TVOS__
			var families = new ulong[] {
				1009, 1008, 1007, 1006, 1005, 1004, 1003, 1002, 2002,
			};
#else
			var families = new ulong[] { 1009, 1008, 1007, 2002 };
#endif
			foreach (var family in families) {
				if (device.SupportsFamily ((MTLGpuFamily)family))
					return true;
			}
			return false;
		}

		private void HandleMetalCommandBufferCompleted (IMTLCommandBuffer commandBuffer)
		{
			if (commandBuffer.Status != MTLCommandBufferStatus.Error ||
				Interlocked.Exchange (ref pendingMetalFailure, 1) != 0) {
				return;
			}

			var error = commandBuffer.Error;
			var code = error is null
				? MTLCommandBufferError.Internal
				: (MTLCommandBufferError)(long)error.Code;
			var message = error?.LocalizedDescription ??
				"The Metal presentation command buffer failed.";
#if __IOS__ || __TVOS__
			const long notPermittedErrorCode = 7;
			if (!applicationActive && (long)code == notPermittedErrorCode) {
				Interlocked.Exchange (ref pendingMetalFailure, 0);
				return;
			}
#endif
			var recreateBackend =
				code == MTLCommandBufferError.DeviceRemoved ||
				code == MTLCommandBufferError.Blacklisted;

			BeginInvokeOnMainThread (() => {
				try {
					if (Handle == IntPtr.Zero)
						return;

#if __MACOS__
					if (code == MTLCommandBufferError.DeviceRemoved &&
						PreferredDevice is { } preferredDevice) {
						Device = preferredDevice;
					}
#endif
					requiresBackendRecreation |= recreateBackend;
					recorder?.Dispose ();
					recorder = null;
					Paused = true;
					OnRenderFailed (new InvalidOperationException (
						$"Metal presentation failed ({code}): {message}"));
				} finally {
					Interlocked.Exchange (ref pendingMetalFailure, 0);
				}
			});
		}

		private static bool IsRunningOnAppleSimulator =>
#if __IOS__ || __TVOS__
			ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.SIMULATOR;
#else
			false;
#endif
	}
}
#endif
