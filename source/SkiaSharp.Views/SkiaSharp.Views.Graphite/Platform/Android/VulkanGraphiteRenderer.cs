using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Android.Runtime;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using VkSemaphore = Silk.NET.Vulkan.Semaphore;
using AndroidSurface = Android.Views.Surface;

namespace SkiaSharp.Views.Android
{
	internal sealed unsafe class VulkanGraphiteRenderer
	{
		private readonly Vk vk = Vk.GetApi ();
		private Instance instance;
		private PhysicalDevice physicalDevice;
		private Device device;
		private Queue queue;
		private uint queueFamily;
		private SurfaceKHR surface;
		private IntPtr nativeWindow;
		private SwapchainKHR swapchain;
		private SwapchainImage[] images = Array.Empty<SwapchainImage> ();
		private readonly List<FrameSlot> pendingFrames = new ();
		private const int MaxFramesInFlight = 3;
		private Format format;
		private SKColorType colorType;
		private ImageUsageFlags usage;
		private ulong frameId;
		private GraphiteSession? session;
		private bool released;
		private CreateAndroidSurfaceDelegate createSurface = null!;
		private DestroySurfaceDelegate destroySurface = null!;
		private SurfaceSupportDelegate surfaceSupport = null!;
		private SurfaceCapabilitiesDelegate surfaceCapabilities = null!;
		private SurfaceFormatsDelegate surfaceFormats = null!;
		private CreateSwapchainDelegate createSwapchain = null!;
		private DestroySwapchainDelegate destroySwapchain = null!;
		private GetSwapchainImagesDelegate getImages = null!;
		private AcquireNextImageDelegate acquire = null!;
		private QueuePresentDelegate present = null!;

		public VulkanGraphiteRenderer (AndroidSurface window, int width, int height)
		{
			try {
				CreateInstance ();
				createSurface = Load<CreateAndroidSurfaceDelegate> ("vkCreateAndroidSurfaceKHR", false);
				destroySurface = Load<DestroySurfaceDelegate> ("vkDestroySurfaceKHR", false);
				surfaceSupport = Load<SurfaceSupportDelegate> ("vkGetPhysicalDeviceSurfaceSupportKHR", false);
				surfaceCapabilities = Load<SurfaceCapabilitiesDelegate> ("vkGetPhysicalDeviceSurfaceCapabilitiesKHR", false);
				surfaceFormats = Load<SurfaceFormatsDelegate> ("vkGetPhysicalDeviceSurfaceFormatsKHR", false);
				nativeWindow = ANativeWindow_fromSurface (JNIEnv.Handle, window.Handle);
				if (nativeWindow == IntPtr.Zero) throw new InvalidOperationException ("Android native window acquisition failed.");
				var surfaceInfo = new AndroidSurfaceCreateInfoKHR {
					SType = StructureType.AndroidSurfaceCreateInfoKhr, Window = (nint*)nativeWindow,
				};
				SurfaceKHR result;
				Check (createSurface (instance, &surfaceInfo, null, &result));
				surface = result;
				SelectDevice ();
				CreateDevice ();
				createSwapchain = Load<CreateSwapchainDelegate> ("vkCreateSwapchainKHR", true);
				destroySwapchain = Load<DestroySwapchainDelegate> ("vkDestroySwapchainKHR", true);
				getImages = Load<GetSwapchainImagesDelegate> ("vkGetSwapchainImagesKHR", true);
				acquire = Load<AcquireNextImageDelegate> ("vkAcquireNextImageKHR", true);
				present = Load<QueuePresentDelegate> ("vkQueuePresentKHR", true);
				using var backend = new SKGraphiteVkBackendContext {
					VkInstance = instance.Handle, VkPhysicalDevice = physicalDevice.Handle,
					VkDevice = device.Handle, VkQueue = queue.Handle, GraphicsQueueIndex = queueFamily,
					MaxApiVersion = Vk.Version11, GetProcedureAddress = GetProcedureAddress,
				};
				var context = SKGraphiteContext.CreateVulkan (backend) ??
					throw new PlatformNotSupportedException ("This device cannot create a Graphite Vulkan context.");
				session = new GraphiteSession (context, ReleaseDevice);
				Resize (width, height);
			} catch {
				if (session is not null) { BeginClose (); TryFinishClose (); }
				else { ReleasePresentation (); ReleaseDevice (); }
				throw;
			}
		}
		public SKGraphiteContext Context => session!.Context;
		public int Width { get; private set; }
		public int Height { get; private set; }
		public SKGraphiteRecorder CreateRecorder () => session!.CreateRecorder ();
		public SKGraphiteRecording SnapRecording (SKGraphiteRecorder recorder) => session!.SnapRecording (recorder);
		public SKGraphiteInsertStatus InsertRecording (SKGraphiteRecording recording) => session!.InsertRecording (recording);
		public void CheckAsyncWorkCompletion ()
		{
			session!.Context.CheckAsyncWorkCompletion ();
			RetireCompletedFrames ();
		}
		public void BeginClose ()
		{
			try { ReleasePresentation (); }
			finally { session?.Close (); }
		}
		public bool TryFinishClose () => session?.TryFinishClose () ?? true;

		private void CreateInstance ()
		{
			var extensions = SilkMarshal.StringArrayToPtr (new[] { "VK_KHR_surface", "VK_KHR_android_surface" });
			try {
				var app = new ApplicationInfo { SType = StructureType.ApplicationInfo, ApiVersion = Vk.Version11 };
				var info = new InstanceCreateInfo {
					SType = StructureType.InstanceCreateInfo, PApplicationInfo = &app,
					EnabledExtensionCount = 2, PpEnabledExtensionNames = (byte**)extensions,
				};
				Check (vk.CreateInstance (&info, null, out instance));
			} finally { SilkMarshal.Free (extensions); }
		}
		private void SelectDevice ()
		{
			uint count = 0;
			Check (vk.EnumeratePhysicalDevices (instance, &count, null));
			var devices = new PhysicalDevice[count];
			fixed (PhysicalDevice* ptr = devices) Check (vk.EnumeratePhysicalDevices (instance, &count, ptr));
			foreach (var candidate in devices) {
				PhysicalDeviceProperties properties;
				vk.GetPhysicalDeviceProperties (candidate, &properties);
				if (properties.ApiVersion < Vk.Version11) continue;
				uint familiesCount = 0;
				vk.GetPhysicalDeviceQueueFamilyProperties (candidate, &familiesCount, null);
				var families = new QueueFamilyProperties[familiesCount];
				fixed (QueueFamilyProperties* ptr = families)
					vk.GetPhysicalDeviceQueueFamilyProperties (candidate, &familiesCount, ptr);
				for (uint i = 0; i < families.Length; i++) {
					Bool32 supported;
					if ((families[i].QueueFlags & QueueFlags.GraphicsBit) != 0 &&
						surfaceSupport (candidate, i, surface, &supported) == Result.Success && supported) {
						physicalDevice = candidate; queueFamily = i; return;
					}
				}
			}
			throw new PlatformNotSupportedException ("No Vulkan 1.1 graphics queue can present to this Android window.");
		}
		private void CreateDevice ()
		{
			var extensions = SilkMarshal.StringArrayToPtr (new[] { "VK_KHR_swapchain" });
			try {
				var priority = 1f;
				var queueInfo = new DeviceQueueCreateInfo {
					SType = StructureType.DeviceQueueCreateInfo, QueueFamilyIndex = queueFamily,
					QueueCount = 1, PQueuePriorities = &priority,
				};
				var info = new DeviceCreateInfo {
					SType = StructureType.DeviceCreateInfo, QueueCreateInfoCount = 1,
					PQueueCreateInfos = &queueInfo, EnabledExtensionCount = 1,
					PpEnabledExtensionNames = (byte**)extensions,
				};
				Check (vk.CreateDevice (physicalDevice, &info, null, out device));
				vk.GetDeviceQueue (device, queueFamily, 0, out queue);
			} finally { SilkMarshal.Free (extensions); }
		}
		public void Resize (int width, int height)
		{
			session!.CheckThread ();
			if (width <= 0 || height <= 0) { Width = Height = 0; return; }
			Check (vk.DeviceWaitIdle (device));
			Context.CheckAsyncWorkCompletion ();
			ReleaseFrameSlots ();
			ReleaseSwapchain ();
			session.RestorePrimaryRecorder ();
			var geometryResult = ANativeWindow_setBuffersGeometry (nativeWindow, width, height, 0);
			if (geometryResult != 0) throw new InvalidOperationException ($"Android buffer resize failed: {geometryResult}.");
			SurfaceCapabilitiesKHR caps;
			Check (surfaceCapabilities (physicalDevice, surface, &caps));
			uint count = 0;
			Check (surfaceFormats (physicalDevice, surface, &count, null));
			var formats = new SurfaceFormatKHR[count];
			fixed (SurfaceFormatKHR* ptr = formats) Check (surfaceFormats (physicalDevice, surface, &count, ptr));
			SurfaceFormatKHR? selected = null;
			foreach (var candidate in formats)
				if (candidate.Format == Format.R8G8B8A8Unorm || candidate.Format == Format.B8G8R8A8Unorm) {
					selected = candidate; break;
				}
			if (selected is not { } selectedFormat)
				throw new PlatformNotSupportedException ("No Graphite-compatible RGBA8 presentation format is available.");
			format = selectedFormat.Format;
			colorType = format == Format.B8G8R8A8Unorm ? SKColorType.Bgra8888 : SKColorType.Rgba8888;
			usage = ImageUsageFlags.ColorAttachmentBit | ImageUsageFlags.TransferSrcBit | ImageUsageFlags.TransferDstBit;
			if ((caps.SupportedUsageFlags & usage) != usage)
				throw new PlatformNotSupportedException ("The window does not support Graphite's required texture usage.");
			if ((caps.SupportedUsageFlags & ImageUsageFlags.InputAttachmentBit) != 0) usage |= ImageUsageFlags.InputAttachmentBit;
			var extent = caps.CurrentExtent;
			if (extent.Width == uint.MaxValue) {
				extent.Width = Math.Clamp ((uint)width, caps.MinImageExtent.Width, caps.MaxImageExtent.Width);
				extent.Height = Math.Clamp ((uint)height, caps.MinImageExtent.Height, caps.MaxImageExtent.Height);
			}
			Width = (int)extent.Width; Height = (int)extent.Height;
			var imageCount = caps.MinImageCount + 1;
			if (caps.MaxImageCount > 0) imageCount = Math.Min (imageCount, caps.MaxImageCount);
			var alpha = (caps.SupportedCompositeAlpha & CompositeAlphaFlagsKHR.PreMultipliedBitKhr) != 0
				? CompositeAlphaFlagsKHR.PreMultipliedBitKhr : CompositeAlphaFlagsKHR.InheritBitKhr;
			if ((caps.SupportedCompositeAlpha & alpha) == 0)
				throw new PlatformNotSupportedException ("The Android window cannot present premultiplied transparent content.");
			var info = new SwapchainCreateInfoKHR {
				SType = StructureType.SwapchainCreateInfoKhr, Surface = surface, MinImageCount = imageCount,
				ImageFormat = format, ImageColorSpace = selectedFormat.ColorSpace,
				ImageExtent = extent, ImageArrayLayers = 1, ImageUsage = usage,
				ImageSharingMode = SharingMode.Exclusive, PreTransform = caps.CurrentTransform,
				CompositeAlpha = alpha, PresentMode = PresentModeKHR.FifoKhr, Clipped = true,
			};
			SwapchainKHR newSwapchain;
			Check (createSwapchain (device, &info, null, &newSwapchain)); swapchain = newSwapchain;
			uint actualCount = 0;
			Check (getImages (device, swapchain, &actualCount, null));
			var nativeImages = new Image[actualCount];
			fixed (Image* ptr = nativeImages) Check (getImages (device, swapchain, &actualCount, ptr));
			images = new SwapchainImage[actualCount];
			for (var i = 0; i < images.Length; i++) images[i] = CreateImage (nativeImages[i]);
		}
		private SwapchainImage CreateImage (Image image)
		{
			var textureInfo = new SKGraphiteVkTextureInfo {
				SampleCount = 1, Mipmapped = false, Format = (int)format,
				ImageTiling = (int)ImageTiling.Optimal, ImageUsageFlags = (uint)usage,
				SharingMode = (int)SharingMode.Exclusive, AspectMask = (uint)ImageAspectFlags.ColorBit,
			};
			var texture = SKGraphiteBackendTexture.CreateVulkan (Width, Height, textureInfo,
				(int)ImageLayout.Undefined, queueFamily, image.Handle) ??
				throw new InvalidOperationException ("Vulkan texture wrapping failed.");
			SKSurface? target = null;
			try {
				target = SKSurface.Create (session!.Recorder!, texture, colorType) ??
					throw new InvalidOperationException ("Graphite Vulkan surface creation failed.");
				return new SwapchainImage { Texture = texture, Surface = target, Finished = CreateSemaphore () };
			} catch { target?.Dispose (); texture.Dispose (); throw; }
		}
		public bool Render (Action<SKPaintGraphiteSurfaceEventArgs> paint)
		{
			session!.CheckThread ();
			if (Width <= 0 || Height <= 0 || images.Length == 0) return true;
			if (Context.IsDeviceLost) throw new InvalidOperationException ("The Graphite Vulkan device was lost.");
			CheckAsyncWorkCompletion ();
			if (pendingFrames.Count >= Math.Min (MaxFramesInFlight, images.Length)) return false;
			var slot = new FrameSlot { Acquired = CreateSemaphore () };
			var presentedFrame = false;
			SKPaintGraphiteSurfaceEventArgs? frame = null;
			try {
				var fenceInfo = new FenceCreateInfo { SType = StructureType.FenceCreateInfo };
				Check (vk.CreateFence (device, &fenceInfo, null, out slot.AcquireFence));
				uint index = 0;
				var result = acquire (device, swapchain, 0, slot.Acquired, slot.AcquireFence, &index);
				if (result == Result.Timeout || result == Result.NotReady) return false;
				if (result == Result.ErrorOutOfDateKhr) { Resize (Width, Height); return false; }
				if (result != Result.Success && result != Result.SuboptimalKhr) Check (result);
				slot.ImageAcquired = true;
				pendingFrames.Add (slot);
				var image = images[index];
				using (var restore = new SKAutoCanvasRestore (image.Surface.Canvas, true)) {
					image.Surface.Canvas.Clear (SKColors.Transparent);
					frame = new SKPaintGraphiteSurfaceEventArgs (image.Surface, image.Texture, Context,
						new SKImageInfo (Width, Height, colorType, SKAlphaType.Premul));
					paint (frame);
				}
				using var recording = session.Recorder!.Snap () ?? throw new InvalidOperationException ("Graphite snap failed.");
				using var wait = SKGraphiteBackendSemaphore.CreateVulkan (slot.Acquired.Handle) ??
					throw new InvalidOperationException ("Vulkan acquisition semaphore wrapping failed.");
				using var signal = SKGraphiteBackendSemaphore.CreateVulkan (image.Finished.Handle) ??
					throw new InvalidOperationException ("Vulkan presentation semaphore wrapping failed.");
				using var state = SKGraphiteMutableTextureState.CreateVulkan ((int)ImageLayout.PresentSrcKhr, queueFamily) ??
					throw new InvalidOperationException ("Vulkan presentation state creation failed.");
				var hasReadbacks = frame.HasReadbacks;
				SKGraphiteFinishedDelegate finished = success => { slot.Succeeded = success; slot.Completed = true; };
				var options = new SKGraphiteInsertRecordingOptions {
					TargetSurface = image.Surface, TargetTextureState = hasReadbacks ? null : state,
					WaitSemaphores = new[] { wait },
					SignalSemaphores = hasReadbacks ? ReadOnlySpan<SKGraphiteBackendSemaphore>.Empty : new[] { signal },
					Finished = hasReadbacks ? null : finished,
				};
				var status = Context.InsertRecording (recording, options);
				if (status != SKGraphiteInsertStatus.Success)
					throw new InvalidOperationException ($"Graphite Vulkan insertion failed: {status}.");
				frame.CompleteReadbacks (true);
				if (hasReadbacks) {
					// Readback can transition the image away from its presentation layout.
					// An empty recording restores that state and signals only after the read.
					using var transition = session.Recorder.Snap () ?? throw new InvalidOperationException ("Graphite presentation transition snap failed.");
					var transitionStatus = Context.InsertRecording (transition, new SKGraphiteInsertRecordingOptions {
						TargetSurface = image.Surface, TargetTextureState = state,
						SignalSemaphores = new[] { signal }, Finished = finished,
					});
					if (transitionStatus != SKGraphiteInsertStatus.Success)
						throw new InvalidOperationException ($"Graphite presentation transition failed: {transitionStatus}.");
				}
				if (!Context.Submit (new SKGraphiteSubmitInfo { Sync = false, MarkBoundary = true, FrameID = ++frameId }))
					throw new InvalidOperationException ("Graphite Vulkan submission failed.");
				var signalSemaphore = image.Finished;
				var currentSwapchain = swapchain;
				var presentInfo = new PresentInfoKHR {
					SType = StructureType.PresentInfoKhr, WaitSemaphoreCount = 1, PWaitSemaphores = &signalSemaphore,
					SwapchainCount = 1, PSwapchains = &currentSwapchain, PImageIndices = &index,
				};
				var presented = present (queue, &presentInfo);
				// The render-finished semaphore belongs to the image and is reused only
				// after reacquisition; that acquisition waits for the prior presentation.
				if (presented == Result.ErrorOutOfDateKhr || presented == Result.SuboptimalKhr || result == Result.SuboptimalKhr) {
					presentedFrame = true;
					Resize (Width, Height); return false;
				}
				Check (presented);
				presentedFrame = true;
				return true;
			} finally {
				try { frame?.CompleteReadbacks (false); }
				finally {
					if (!presentedFrame) {
						// Exceptional cleanup must cover WSI acquisition even if drawing
						// failed before the GPU submission consumed its semaphore.
						if (slot.ImageAcquired) {
							var fence = slot.AcquireFence;
							CheckCleanup (vk.WaitForFences (device, 1, &fence, true, ulong.MaxValue));
							CheckCleanup (vk.DeviceWaitIdle (device));
							Context.CheckAsyncWorkCompletion ();
						}
						pendingFrames.Remove (slot);
						ReleaseFrameSlot (slot);
					}
				}
			}
		}
		private void RetireCompletedFrames ()
		{
			for (var i = pendingFrames.Count - 1; i >= 0; i--) {
				var slot = pendingFrames[i];
				if (!slot.Completed) continue;
				pendingFrames.RemoveAt (i);
				ReleaseFrameSlot (slot);
				if (!slot.Succeeded) throw new InvalidOperationException ("Graphite Vulkan frame execution failed.");
			}
		}
		private void ReleaseFrameSlot (FrameSlot slot)
		{
			if (slot.AcquireFence.Handle != 0) {
				vk.DestroyFence (device, slot.AcquireFence, null);
				slot.AcquireFence = default;
			}
			if (slot.Acquired.Handle != 0) {
				vk.DestroySemaphore (device, slot.Acquired, null);
				slot.Acquired = default;
			}
		}
		private void ReleaseFrameSlots ()
		{
			foreach (var slot in pendingFrames) ReleaseFrameSlot (slot);
			pendingFrames.Clear ();
		}
		private static void CheckCleanup (Result result)
		{
			if (result != Result.Success && result != Result.ErrorDeviceLost) Check (result);
		}
		private VkSemaphore CreateSemaphore ()
		{
			var info = new SemaphoreCreateInfo { SType = StructureType.SemaphoreCreateInfo };
			Check (vk.CreateSemaphore (device, &info, null, out var result));
			return result;
		}
		private void ReleaseSwapchain ()
		{
			foreach (var image in images) {
				if (image is null) continue;
				image.Surface.Dispose (); image.Texture.Dispose ();
				vk.DestroySemaphore (device, image.Finished, null);
			}
			images = Array.Empty<SwapchainImage> ();
			// Frame surfaces die while their recorder is alive; its image/view caches
			// then die before the underlying Vulkan swapchain images are destroyed.
			session?.ReleasePrimaryRecorder ();
			if (swapchain.Handle != 0) { destroySwapchain (device, swapchain, null); swapchain = default; }
		}
		private void ReleasePresentation ()
		{
			if (device.Handle != 0) vk.DeviceWaitIdle (device);
			try {
				if (session is not null && !Context.IsDeviceLost) {
					Context.Submit (new SKGraphiteSubmitInfo { Sync = true });
					Context.CheckAsyncWorkCompletion ();
				}
			} finally {
				ReleaseFrameSlots ();
				ReleaseSwapchain ();
				if (surface.Handle != 0) { destroySurface (instance, surface, null); surface = default; }
				if (nativeWindow != IntPtr.Zero) { ANativeWindow_release (nativeWindow); nativeWindow = IntPtr.Zero; }
				Width = Height = 0;
			}
		}
		private void ReleaseDevice ()
		{
			if (released) return;
			released = true;
			if (device.Handle != 0) { vk.DestroyDevice (device, null); device = default; }
			if (instance.Handle != 0) { vk.DestroyInstance (instance, null); instance = default; }
			vk.Dispose ();
		}
		private IntPtr GetProcedureAddress (string name, IntPtr instanceHandle, IntPtr deviceHandle)
		{
			var bytes = Encoding.ASCII.GetBytes (name + "\0");
			fixed (byte* ptr = bytes) return deviceHandle != IntPtr.Zero ?
				vk.GetDeviceProcAddr (new Device (deviceHandle), ptr) : vk.GetInstanceProcAddr (new Instance (instanceHandle), ptr);
		}
		private T Load<T> (string name, bool deviceProcedure) where T : Delegate
		{
			var ptr = GetProcedureAddress (name, instance.Handle, deviceProcedure ? device.Handle : IntPtr.Zero);
			if (ptr == IntPtr.Zero) throw new PlatformNotSupportedException ($"{name} is unavailable.");
			return Marshal.GetDelegateForFunctionPointer<T> (ptr);
		}
		private static void Check (Result result)
		{
			if (result != Result.Success) throw new InvalidOperationException ($"Vulkan operation failed: {result}.");
		}
		private sealed class SwapchainImage
		{
			public SKGraphiteBackendTexture Texture = null!;
			public SKSurface Surface = null!;
			public VkSemaphore Finished;
		}
		private sealed class FrameSlot
		{
			public VkSemaphore Acquired;
			public Fence AcquireFence;
			public bool ImageAcquired;
			public bool Completed;
			public bool Succeeded;
		}
		[DllImport ("android")] private static extern IntPtr ANativeWindow_fromSurface (IntPtr environment, IntPtr surface);
		[DllImport ("android")] private static extern void ANativeWindow_release (IntPtr window);
		[DllImport ("android")] private static extern int ANativeWindow_setBuffersGeometry (IntPtr window, int width, int height, int format);
		[UnmanagedFunctionPointer (CallingConvention.Winapi)]
		private delegate Result CreateAndroidSurfaceDelegate (Instance instance, AndroidSurfaceCreateInfoKHR* info, AllocationCallbacks* allocator, SurfaceKHR* surface);
		[UnmanagedFunctionPointer (CallingConvention.Winapi)]
		private delegate void DestroySurfaceDelegate (Instance instance, SurfaceKHR surface, AllocationCallbacks* allocator);
		[UnmanagedFunctionPointer (CallingConvention.Winapi)]
		private delegate Result SurfaceSupportDelegate (PhysicalDevice device, uint family, SurfaceKHR surface, Bool32* supported);
		[UnmanagedFunctionPointer (CallingConvention.Winapi)]
		private delegate Result SurfaceCapabilitiesDelegate (PhysicalDevice device, SurfaceKHR surface, SurfaceCapabilitiesKHR* caps);
		[UnmanagedFunctionPointer (CallingConvention.Winapi)]
		private delegate Result SurfaceFormatsDelegate (PhysicalDevice device, SurfaceKHR surface, uint* count, SurfaceFormatKHR* formats);
		[UnmanagedFunctionPointer (CallingConvention.Winapi)]
		private delegate Result CreateSwapchainDelegate (Device device, SwapchainCreateInfoKHR* info, AllocationCallbacks* allocator, SwapchainKHR* swapchain);
		[UnmanagedFunctionPointer (CallingConvention.Winapi)]
		private delegate void DestroySwapchainDelegate (Device device, SwapchainKHR swapchain, AllocationCallbacks* allocator);
		[UnmanagedFunctionPointer (CallingConvention.Winapi)]
		private delegate Result GetSwapchainImagesDelegate (Device device, SwapchainKHR swapchain, uint* count, Image* images);
		[UnmanagedFunctionPointer (CallingConvention.Winapi)]
		private delegate Result AcquireNextImageDelegate (Device device, SwapchainKHR swapchain, ulong timeout, VkSemaphore semaphore, Fence fence, uint* index);
		[UnmanagedFunctionPointer (CallingConvention.Winapi)]
		private delegate Result QueuePresentDelegate (Queue queue, PresentInfoKHR* info);
	}
}
