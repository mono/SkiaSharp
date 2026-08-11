#pragma warning disable CS0618

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Android.Runtime;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

using AndroidSurface = Android.Views.Surface;
using VkSemaphore = Silk.NET.Vulkan.Semaphore;

namespace SkiaSharp.Views.Android
{
	internal sealed unsafe class VulkanGraphiteRenderer : IDisposable
	{
		private static readonly uint VulkanApiVersion = Vk.Version11;
		private static readonly Vk SharedVk = Vk.GetApi ();
		// A Context is expensive and some Android drivers cannot safely tear down
		// and recreate the Vulkan device during a view reattach. Keep at most one
		// idle core for reuse; recorders and all window/swapchain resources remain
		// view-owned and are always released on detach.
		private static readonly object CorePoolLock = new ();
		private static readonly Stack<VulkanCore> CorePool = new ();
		private static readonly List<VulkanGraphiteRenderer> RetiredFaultedRenderers = new ();
		private const int MaxPooledCoreCount = 1;
		private const string SurfaceExtensionName = "VK_KHR_surface";
		private const string AndroidSurfaceExtensionName = "VK_KHR_android_surface";
		private const string SwapchainExtensionName = "VK_KHR_swapchain";

		private readonly Vk vk;
		private CreateAndroidSurfaceDelegate createAndroidSurface = null!;
		private DestroySurfaceDelegate destroySurface = null!;
		private GetPhysicalDeviceSurfaceSupportDelegate getPhysicalDeviceSurfaceSupport = null!;
		private GetPhysicalDeviceSurfaceCapabilitiesDelegate getPhysicalDeviceSurfaceCapabilities = null!;
		private GetPhysicalDeviceSurfaceFormatsDelegate getPhysicalDeviceSurfaceFormats = null!;
		private GetPhysicalDeviceSurfacePresentModesDelegate getPhysicalDeviceSurfacePresentModes = null!;
		private CreateSwapchainDelegate createSwapchain = null!;
		private DestroySwapchainDelegate destroySwapchain = null!;
		private GetSwapchainImagesDelegate getSwapchainImages = null!;
		private AcquireNextImageDelegate acquireNextImage = null!;
		private QueuePresentDelegate queuePresent = null!;
		private readonly HashSet<ulong> pendingAcquireSemaphores = new ();
		private readonly object pendingSemaphoreLock = new ();

		private Instance instance;
		private SurfaceKHR surface;
		private PhysicalDevice physicalDevice;
		private Device device;
		private Queue queue;
		private uint queueFamily;
		private SwapchainKHR swapchain;
		private SwapchainImage[] images = Array.Empty<SwapchainImage> ();
		private Format format;
		private SKColorType colorType;
		private ImageUsageFlags imageUsage;
		private SharingMode sharingMode;
		private ulong frameId;
		private bool deviceLost;
		private bool contextUnrecoverable;

		private SKGraphiteVkBackendContext? backendContext;
		private SKGraphiteContext? context;
		private SKGraphiteRecorder? recorder;

		static VulkanGraphiteRenderer ()
		{
			AppDomain.CurrentDomain.ProcessExit += (_, _) => DisposeCorePool ();
		}

		public VulkanGraphiteRenderer (AndroidSurface androidSurface, int width, int height)
		{
			vk = SharedVk;
			try {
				if (TryTakeCore (out var core)) {
					RestoreCore (core);
					surface = CreateSurface (androidSurface);
					ValidateSurfaceSupport ();
				} else {
					instance = CreateInstance ();
					createAndroidSurface = LoadInstance<CreateAndroidSurfaceDelegate> (
						"vkCreateAndroidSurfaceKHR");
					destroySurface = LoadInstance<DestroySurfaceDelegate> ("vkDestroySurfaceKHR");
					getPhysicalDeviceSurfaceSupport =
						LoadInstance<GetPhysicalDeviceSurfaceSupportDelegate> (
							"vkGetPhysicalDeviceSurfaceSupportKHR");
					getPhysicalDeviceSurfaceCapabilities =
						LoadInstance<GetPhysicalDeviceSurfaceCapabilitiesDelegate> (
							"vkGetPhysicalDeviceSurfaceCapabilitiesKHR");
					getPhysicalDeviceSurfaceFormats =
						LoadInstance<GetPhysicalDeviceSurfaceFormatsDelegate> (
							"vkGetPhysicalDeviceSurfaceFormatsKHR");
					getPhysicalDeviceSurfacePresentModes =
						LoadInstance<GetPhysicalDeviceSurfacePresentModesDelegate> (
							"vkGetPhysicalDeviceSurfacePresentModesKHR");

					surface = CreateSurface (androidSurface);
					(physicalDevice, queueFamily) = SelectPhysicalDevice ();
					device = CreateDevice ();
					vk.GetDeviceQueue (device, queueFamily, 0, out queue);
					createSwapchain = LoadDevice<CreateSwapchainDelegate> ("vkCreateSwapchainKHR");
					destroySwapchain = LoadDevice<DestroySwapchainDelegate> ("vkDestroySwapchainKHR");
					getSwapchainImages = LoadDevice<GetSwapchainImagesDelegate> ("vkGetSwapchainImagesKHR");
					acquireNextImage = LoadDevice<AcquireNextImageDelegate> ("vkAcquireNextImageKHR");
					queuePresent = LoadDevice<QueuePresentDelegate> ("vkQueuePresentKHR");

					backendContext = new SKGraphiteVkBackendContext {
						VkInstance = instance.Handle,
						VkPhysicalDevice = physicalDevice.Handle,
						VkDevice = device.Handle,
						VkQueue = queue.Handle,
						GraphicsQueueIndex = queueFamily,
						MaxApiVersion = VulkanApiVersion,
						GetProcedureAddress = GetProcedureAddress,
					};
					context = SKGraphiteContext.CreateVulkan (backendContext)
						?? throw new PlatformNotSupportedException ("Unable to create a Graphite Vulkan context.");
					backendContext.Dispose ();
					backendContext = null;
				}

				var imageCache = new SKGraphiteImageCache ();
				recorder = context!.CreateRecorder (
					-1,
					imageCache.FindOrCreate,
					imageCache.Dispose);
				if (recorder is null) {
					imageCache.Dispose ();
					throw new InvalidOperationException ("Unable to create a Graphite recorder.");
				}

				CreateSwapchain (width, height);
			} catch {
				CleanupFailedInitialization ();
				throw;
			}
		}

		public SKGraphiteContext Context =>
			context ?? throw new ObjectDisposedException (nameof (VulkanGraphiteRenderer));

		public int Width { get; private set; }

		public int Height { get; private set; }

		public bool HasSurface => surface.Handle != 0;

		public void AttachSurface (
			AndroidSurface androidSurface,
			int width,
			int height)
		{
			ReleaseSurface ();
			surface = CreateSurface (androidSurface);
			ValidateSurfaceSupport ();

			CreateSwapchain (width, height);
		}

		public void ReleaseSurface ()
		{
			if (surface.Handle == 0)
				return;

			if (device.Handle != 0)
				TrackDeviceResult (vk.DeviceWaitIdle (device));
			if (context is not null && !context.IsDeviceLost) {
				context.Submit (new SKGraphiteSubmitInfo { Sync = true });
				for (var i = 0; i < 1000; i++)
					context.CheckAsyncWorkCompletion ();
			}

			DisposeSwapchainImages ();
			if (swapchain.Handle != 0) {
				destroySwapchain (device, swapchain, null);
				swapchain = default;
			}
			destroySurface (instance, surface, null);
			surface = default;
			Width = 0;
			Height = 0;
		}

		public void Resize (int width, int height)
		{
			if (!HasSurface || width <= 0 || height <= 0)
				return;
			CreateSwapchain (width, height);
		}

		public bool Render (Action<SKPaintGraphiteSurfaceEventArgs> paint)
		{
			if (context is null || recorder is null || images.Length == 0)
				return false;
			if (context.IsDeviceLost) {
				deviceLost = true;
				throw new InvalidOperationException ("The Graphite Vulkan device was lost.");
			}

			context.CheckAsyncWorkCompletion ();

			var acquireSemaphore = CreateSemaphore ();
			lock (pendingSemaphoreLock)
				pendingAcquireSemaphores.Add (acquireSemaphore.Handle);

			uint imageIndex = 0;
			var acquireResult = TrackDeviceResult (acquireNextImage (
				device,
				swapchain,
				ulong.MaxValue,
				acquireSemaphore,
				default,
				&imageIndex));

			if (acquireResult == Result.ErrorOutOfDateKhr) {
				ReleaseAcquireSemaphore (acquireSemaphore);
				CreateSwapchain (Width, Height);
				return false;
			}
			if (acquireResult != Result.Success && acquireResult != Result.SuboptimalKhr) {
				ReleaseAcquireSemaphore (acquireSemaphore);
				throw new InvalidOperationException ($"vkAcquireNextImageKHR failed: {acquireResult}.");
			}

			var image = images[imageIndex];
			using (var restore = new SKAutoCanvasRestore (image.Surface.Canvas, true)) {
				var info = new SKImageInfo (
					Width, Height, colorType, SKAlphaType.Premul);
				paint (new SKPaintGraphiteSurfaceEventArgs (
					image.Surface,
					image.BackendTexture,
					context,
					info));
			}

			using var recording = recorder.Snap ();
			if (recording is null) {
				TrackDeviceResult (vk.DeviceWaitIdle (device));
				ReleaseAcquireSemaphore (acquireSemaphore);
				throw new InvalidOperationException (
					"A Graphite frame must record at least one target-surface draw.");
			}
			using var wait = SKGraphiteBackendSemaphore.CreateVulkan (acquireSemaphore.Handle)
				?? throw new InvalidOperationException ("Unable to wrap the Vulkan acquire semaphore.");
			using var signal = SKGraphiteBackendSemaphore.CreateVulkan (image.RenderFinished.Handle)
				?? throw new InvalidOperationException ("Unable to wrap the Vulkan render semaphore.");
			using var presentState = SKGraphiteMutableTextureState.CreateVulkan (
				(int)ImageLayout.PresentSrcKhr, queueFamily)
				?? throw new InvalidOperationException ("Unable to create the Vulkan presentation state.");

			var waits = new[] { wait };
			var signals = new[] { signal };
			var insertCallReturned = false;
			var insertCallbackFailed = false;
			var options = new SKGraphiteInsertRecordingOptions {
				TargetSurface = image.Surface,
				TargetTextureState = presentState,
				WaitSemaphores = waits,
				SignalSemaphores = signals,
				Finished = success => {
					if (success || insertCallReturned) {
						if (!success)
							contextUnrecoverable = true;
						ReleaseAcquireSemaphore (acquireSemaphore);
					} else {
						insertCallbackFailed = true;
					}
				},
			};
			var status = context.InsertRecording (recording, options);
			insertCallReturned = true;
			if (status != SKGraphiteInsertStatus.Success) {
				if (status == SKGraphiteInsertStatus.AddCommandsFailed ||
					status == SKGraphiteInsertStatus.AsyncShaderCompilesFailed ||
					status == SKGraphiteInsertStatus.OutOfOrderRecording) {
					contextUnrecoverable = true;
				} else if (insertCallbackFailed) {
					TrackDeviceResult (vk.DeviceWaitIdle (device));
					ReleaseAcquireSemaphore (acquireSemaphore);
				}
				throw new InvalidOperationException ($"Unable to insert the Graphite recording: {status}.");
			}
			if (!context.Submit (new SKGraphiteSubmitInfo {
				Sync = false,
				MarkBoundary = true,
				FrameID = ++frameId,
			}))
				throw new InvalidOperationException ("Unable to submit the Graphite frame.");

			var renderFinished = image.RenderFinished;
			var localSwapchain = swapchain;
			var presentInfo = new PresentInfoKHR {
				SType = StructureType.PresentInfoKhr,
				WaitSemaphoreCount = 1,
				PWaitSemaphores = &renderFinished,
				SwapchainCount = 1,
				PSwapchains = &localSwapchain,
				PImageIndices = &imageIndex,
			};
			var presentResult = TrackDeviceResult (queuePresent (queue, &presentInfo));
			if (presentResult == Result.ErrorOutOfDateKhr ||
				presentResult == Result.SuboptimalKhr ||
				acquireResult == Result.SuboptimalKhr) {
				CreateSwapchain (Width, Height);
				return false;
			}
			if (presentResult != Result.Success)
				throw new InvalidOperationException ($"vkQueuePresentKHR failed: {presentResult}.");

			return true;
		}

		public void Dispose ()
		{
			if (deviceLost ||
				contextUnrecoverable ||
				context?.IsDeviceLost == true) {
				DisposeFaulted ();
				return;
			}

			ReleaseSurface ();
			if (deviceLost ||
				contextUnrecoverable ||
				context?.IsDeviceLost == true) {
				DisposeFaulted ();
				return;
			}

			recorder?.Dispose ();
			recorder = null;
			backendContext?.Dispose ();
			backendContext = null;

			lock (pendingSemaphoreLock) {
				foreach (var handle in pendingAcquireSemaphores)
					vk.DestroySemaphore (device, new VkSemaphore (handle), null);
				pendingAcquireSemaphores.Clear ();
			}

			if (context is not null) {
				ReturnCore (CaptureCore ());
				context = null;
			}
		}

		private void DisposeFaulted ()
		{
			// A client can create additional Recorders from the Context exposed by
			// the paint event. Once the Context or device is lost we cannot safely
			// destroy it before those client-owned wrappers are released. Retain
			// the faulted renderer until process exit; the OS then reclaims the
			// unusable Vulkan object graph without a use-after-free.
			lock (CorePoolLock)
				RetiredFaultedRenderers.Add (this);
		}

		private void CleanupFailedInitialization ()
		{
			if (device.Handle != 0)
				TrackDeviceResult (vk.DeviceWaitIdle (device));

			DisposeSwapchainImages ();
			if (swapchain.Handle != 0 && destroySwapchain is not null) {
				destroySwapchain (device, swapchain, null);
				swapchain = default;
			}
			if (surface.Handle != 0 && destroySurface is not null) {
				destroySurface (instance, surface, null);
				surface = default;
			}

			recorder?.Dispose ();
			recorder = null;
			backendContext?.Dispose ();
			backendContext = null;

			if (context is not null) {
				ReturnCore (CaptureCore ());
				context = null;
			} else {
				if (device.Handle != 0)
					vk.DestroyDevice (device, null);
				if (instance.Handle != 0)
					vk.DestroyInstance (instance, null);
			}
		}

		private Instance CreateInstance ()
		{
			var extensionNames = new[] {
				SurfaceExtensionName,
				AndroidSurfaceExtensionName,
			};
			var extensions = SilkMarshal.StringArrayToPtr (extensionNames);
			try {
				var appInfo = new ApplicationInfo {
					SType = StructureType.ApplicationInfo,
					ApiVersion = VulkanApiVersion,
				};
				var createInfo = new InstanceCreateInfo {
					SType = StructureType.InstanceCreateInfo,
					PApplicationInfo = &appInfo,
					EnabledExtensionCount = (uint)extensionNames.Length,
					PpEnabledExtensionNames = (byte**)extensions,
				};
				if (vk.CreateInstance (&createInfo, null, out var value) != Result.Success)
					throw new PlatformNotSupportedException ("Unable to create a Vulkan instance.");
				return value;
			} finally {
				SilkMarshal.Free (extensions);
			}
		}

		private SurfaceKHR CreateSurface (AndroidSurface androidSurface)
		{
			var nativeWindow = ANativeWindow_fromSurface (JNIEnv.Handle, androidSurface.Handle);
			if (nativeWindow == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to acquire the Android native window.");

			try {
				var createInfo = new AndroidSurfaceCreateInfoKHR {
					SType = StructureType.AndroidSurfaceCreateInfoKhr,
					Window = (nint*)nativeWindow,
				};
				SurfaceKHR value;
				if (createAndroidSurface (
					instance, &createInfo, null, &value) != Result.Success)
					throw new InvalidOperationException ("Unable to create a Vulkan Android surface.");
				return value;
			} finally {
				ANativeWindow_release (nativeWindow);
			}
		}

		private (PhysicalDevice Device, uint QueueFamily) SelectPhysicalDevice ()
		{
			uint deviceCount = 0;
			vk.EnumeratePhysicalDevices (instance, &deviceCount, null);
			if (deviceCount == 0)
				throw new PlatformNotSupportedException ("No Vulkan physical device is available.");

			var devices = stackalloc PhysicalDevice[(int)deviceCount];
			vk.EnumeratePhysicalDevices (instance, &deviceCount, devices);
			for (var deviceIndex = 0; deviceIndex < deviceCount; deviceIndex++) {
				var candidate = devices[deviceIndex];
				PhysicalDeviceProperties properties;
				vk.GetPhysicalDeviceProperties (candidate, &properties);
				if (properties.ApiVersion < VulkanApiVersion)
					continue;

				uint familyCount = 0;
				vk.GetPhysicalDeviceQueueFamilyProperties (candidate, &familyCount, null);
				var families = new QueueFamilyProperties[familyCount];
				fixed (QueueFamilyProperties* familyPtr = families) {
					vk.GetPhysicalDeviceQueueFamilyProperties (
						candidate, &familyCount, familyPtr);

					for (uint familyIndex = 0; familyIndex < familyCount; familyIndex++) {
						if ((families[familyIndex].QueueFlags & QueueFlags.GraphicsBit) == 0)
							continue;

						Bool32 supported;
						var result = getPhysicalDeviceSurfaceSupport (
							candidate, familyIndex, surface, &supported);
						if (result == Result.Success && supported)
							return (candidate, familyIndex);
					}
				}
			}

			throw new PlatformNotSupportedException (
				"No Vulkan queue family supports graphics and Android surface presentation.");
		}

		private Device CreateDevice ()
		{
			var priority = 1f;
			var queueInfo = new DeviceQueueCreateInfo {
				SType = StructureType.DeviceQueueCreateInfo,
				QueueFamilyIndex = queueFamily,
				QueueCount = 1,
				PQueuePriorities = &priority,
			};

			var extensionNames = new[] { SwapchainExtensionName };
			var extensions = SilkMarshal.StringArrayToPtr (extensionNames);
			try {
				var createInfo = new DeviceCreateInfo {
					SType = StructureType.DeviceCreateInfo,
					QueueCreateInfoCount = 1,
					PQueueCreateInfos = &queueInfo,
					EnabledExtensionCount = 1,
					PpEnabledExtensionNames = (byte**)extensions,
				};
				if (TrackDeviceResult (
					vk.CreateDevice (physicalDevice, &createInfo, null, out var value)) != Result.Success)
					throw new PlatformNotSupportedException ("Unable to create a Vulkan device.");
				return value;
			} finally {
				SilkMarshal.Free (extensions);
			}
		}

		private void CreateSwapchain (int requestedWidth, int requestedHeight)
		{
			if (requestedWidth <= 0 || requestedHeight <= 0)
				return;

			if (device.Handle != 0)
				TrackDeviceResult (vk.DeviceWaitIdle (device));
			DisposeSwapchainImages ();
			if (swapchain.Handle != 0) {
				destroySwapchain (device, swapchain, null);
				swapchain = default;
			}

			SurfaceCapabilitiesKHR capabilities;
			getPhysicalDeviceSurfaceCapabilities (
				physicalDevice, surface, &capabilities).ThrowOnError ();

			uint formatCount = 0;
			getPhysicalDeviceSurfaceFormats (
				physicalDevice, surface, &formatCount, null).ThrowOnError ();
			var formats = new SurfaceFormatKHR[formatCount];
			fixed (SurfaceFormatKHR* formatPtr = formats)
				getPhysicalDeviceSurfaceFormats (
					physicalDevice, surface, &formatCount, formatPtr).ThrowOnError ();
			var selectedFormat = SelectSurfaceFormat (formats);
			format = selectedFormat.Format;
			colorType = format == Format.B8G8R8A8Unorm
				? SKColorType.Bgra8888
				: SKColorType.Rgba8888;

			uint modeCount = 0;
			getPhysicalDeviceSurfacePresentModes (
				physicalDevice, surface, &modeCount, null).ThrowOnError ();
			var modes = new PresentModeKHR[modeCount];
			fixed (PresentModeKHR* modePtr = modes)
				getPhysicalDeviceSurfacePresentModes (
					physicalDevice, surface, &modeCount, modePtr).ThrowOnError ();
			var presentMode = Array.IndexOf (modes, PresentModeKHR.MailboxKhr) >= 0
				? PresentModeKHR.MailboxKhr
				: PresentModeKHR.FifoKhr;

			var extent = capabilities.CurrentExtent;
			if (extent.Width == uint.MaxValue) {
				extent.Width = Math.Clamp (
					(uint)requestedWidth,
					capabilities.MinImageExtent.Width,
					capabilities.MaxImageExtent.Width);
				extent.Height = Math.Clamp (
					(uint)requestedHeight,
					capabilities.MinImageExtent.Height,
					capabilities.MaxImageExtent.Height);
			}
			Width = (int)extent.Width;
			Height = (int)extent.Height;

			var imageCount = capabilities.MinImageCount + 1;
			if (capabilities.MaxImageCount > 0)
				imageCount = Math.Min (imageCount, capabilities.MaxImageCount);

			imageUsage =
				ImageUsageFlags.ColorAttachmentBit |
				ImageUsageFlags.TransferSrcBit |
				ImageUsageFlags.TransferDstBit;
			if ((capabilities.SupportedUsageFlags & ImageUsageFlags.InputAttachmentBit) != 0)
				imageUsage |= ImageUsageFlags.InputAttachmentBit;
			if ((capabilities.SupportedUsageFlags & ImageUsageFlags.SampledBit) != 0)
				imageUsage |= ImageUsageFlags.SampledBit;
			if ((capabilities.SupportedUsageFlags & imageUsage) != imageUsage)
				throw new PlatformNotSupportedException (
					"The Vulkan surface does not support Graphite's required image usage flags.");

			var compositeAlpha =
				(capabilities.SupportedCompositeAlpha & CompositeAlphaFlagsKHR.InheritBitKhr) != 0
					? CompositeAlphaFlagsKHR.InheritBitKhr
					: CompositeAlphaFlagsKHR.OpaqueBitKhr;
			sharingMode = SharingMode.Exclusive;

			var createInfo = new SwapchainCreateInfoKHR {
				SType = StructureType.SwapchainCreateInfoKhr,
				Surface = surface,
				MinImageCount = imageCount,
				ImageFormat = selectedFormat.Format,
				ImageColorSpace = selectedFormat.ColorSpace,
				ImageExtent = extent,
				ImageArrayLayers = 1,
				ImageUsage = imageUsage,
				ImageSharingMode = sharingMode,
				PreTransform = capabilities.CurrentTransform,
				CompositeAlpha = compositeAlpha,
				PresentMode = presentMode,
				Clipped = true,
			};
			SwapchainKHR newSwapchain;
			TrackDeviceResult (createSwapchain (
				device, &createInfo, null, &newSwapchain)).ThrowOnError ();
			swapchain = newSwapchain;

			uint actualImageCount = 0;
			TrackDeviceResult (getSwapchainImages (
				device, swapchain, &actualImageCount, null)).ThrowOnError ();
			var vkImages = new Image[actualImageCount];
			fixed (Image* imagePtr = vkImages)
				TrackDeviceResult (getSwapchainImages (
					device, swapchain, &actualImageCount, imagePtr)).ThrowOnError ();

			var newImages = new SwapchainImage[actualImageCount];
			try {
				for (var i = 0; i < newImages.Length; i++)
					newImages[i] = CreateSwapchainImage (vkImages[i]);
				images = newImages;
			} catch {
				DisposeSwapchainImages (newImages);
				destroySwapchain (device, swapchain, null);
				swapchain = default;
				throw;
			}
		}

		private SwapchainImage CreateSwapchainImage (Image image)
		{
			var textureInfo = new SKGraphiteVkTextureInfo {
				SampleCount = 1,
				Mipmapped = false,
				Flags = 0,
				Format = (int)format,
				ImageTiling = (int)ImageTiling.Optimal,
				ImageUsageFlags = (uint)imageUsage,
				SharingMode = (int)sharingMode,
				AspectMask = (uint)ImageAspectFlags.ColorBit,
			};
			var backendTexture = SKGraphiteBackendTexture.CreateVulkan (
				Width,
				Height,
				textureInfo,
				(int)ImageLayout.Undefined,
				queueFamily,
				image.Handle)
				?? throw new InvalidOperationException ("Unable to wrap a Vulkan swapchain image.");
			SKSurface? graphiteSurface = null;
			try {
				graphiteSurface = SKSurface.Create (
					recorder!,
					backendTexture,
					colorType)
					?? throw new InvalidOperationException ("Unable to create a Graphite Vulkan surface.");
				return new SwapchainImage (
					backendTexture,
					graphiteSurface,
					CreateSemaphore ());
			} catch {
				graphiteSurface?.Dispose ();
				backendTexture.Dispose ();
				throw;
			}
		}

		private void DisposeSwapchainImages ()
		{
			DisposeSwapchainImages (images);
			images = Array.Empty<SwapchainImage> ();
		}

		private void DisposeSwapchainImages (SwapchainImage[] swapchainImages)
		{
			foreach (var image in swapchainImages)
				image?.Dispose (vk, device);
		}

		private VkSemaphore CreateSemaphore ()
		{
			var createInfo = new SemaphoreCreateInfo {
				SType = StructureType.SemaphoreCreateInfo,
			};
			TrackDeviceResult (
				vk.CreateSemaphore (device, &createInfo, null, out var semaphore))
				.ThrowOnError ();
			return semaphore;
		}

		private void ReleaseAcquireSemaphore (VkSemaphore semaphore)
		{
			lock (pendingSemaphoreLock) {
				if (!pendingAcquireSemaphores.Remove (semaphore.Handle))
					return;
				vk.DestroySemaphore (device, semaphore, null);
			}
		}

		private Result TrackDeviceResult (Result result)
		{
			if (result == Result.ErrorDeviceLost)
				deviceLost = true;
			return result;
		}

		private void ValidateSurfaceSupport ()
		{
			Bool32 supported;
			var result = getPhysicalDeviceSurfaceSupport (
				physicalDevice,
				queueFamily,
				surface,
				&supported);
			if (result != Result.Success || !supported)
				throw new PlatformNotSupportedException (
					"The Vulkan queue cannot present to the Android surface.");
		}

		private VulkanCore CaptureCore () =>
			new VulkanCore {
				Instance = instance,
				PhysicalDevice = physicalDevice,
				Device = device,
				Queue = queue,
				QueueFamily = queueFamily,
				DeviceLost = deviceLost,
				Context = context!,
				CreateAndroidSurface = createAndroidSurface,
				DestroySurface = destroySurface,
				GetPhysicalDeviceSurfaceSupport = getPhysicalDeviceSurfaceSupport,
				GetPhysicalDeviceSurfaceCapabilities = getPhysicalDeviceSurfaceCapabilities,
				GetPhysicalDeviceSurfaceFormats = getPhysicalDeviceSurfaceFormats,
				GetPhysicalDeviceSurfacePresentModes = getPhysicalDeviceSurfacePresentModes,
				CreateSwapchain = createSwapchain,
				DestroySwapchain = destroySwapchain,
				GetSwapchainImages = getSwapchainImages,
				AcquireNextImage = acquireNextImage,
				QueuePresent = queuePresent,
			};

		private void RestoreCore (VulkanCore core)
		{
			instance = core.Instance;
			physicalDevice = core.PhysicalDevice;
			device = core.Device;
			queue = core.Queue;
			queueFamily = core.QueueFamily;
			deviceLost = core.DeviceLost;
			context = core.Context;
			createAndroidSurface = core.CreateAndroidSurface;
			destroySurface = core.DestroySurface;
			getPhysicalDeviceSurfaceSupport = core.GetPhysicalDeviceSurfaceSupport;
			getPhysicalDeviceSurfaceCapabilities = core.GetPhysicalDeviceSurfaceCapabilities;
			getPhysicalDeviceSurfaceFormats = core.GetPhysicalDeviceSurfaceFormats;
			getPhysicalDeviceSurfacePresentModes = core.GetPhysicalDeviceSurfacePresentModes;
			createSwapchain = core.CreateSwapchain;
			destroySwapchain = core.DestroySwapchain;
			getSwapchainImages = core.GetSwapchainImages;
			acquireNextImage = core.AcquireNextImage;
			queuePresent = core.QueuePresent;
		}

		private static bool TryTakeCore (out VulkanCore core)
		{
			while (true) {
				lock (CorePoolLock) {
					if (CorePool.Count == 0) {
						core = null!;
						return false;
					}
					core = CorePool.Pop ();
				}

				if (!core.DeviceLost && !core.Context.IsDeviceLost)
					return true;
				core.Dispose ();
			}
		}

		private static void ReturnCore (VulkanCore core)
		{
			var dispose = core.DeviceLost || core.Context.IsDeviceLost;
			if (!dispose) {
				lock (CorePoolLock) {
					if (CorePool.Count < MaxPooledCoreCount)
						CorePool.Push (core);
					else
						dispose = true;
				}
			}

			if (dispose)
				core.Dispose ();
		}

		private static void DisposeCorePool ()
		{
			VulkanCore[] cores;
			lock (CorePoolLock) {
				cores = CorePool.ToArray ();
				CorePool.Clear ();
			}
			foreach (var core in cores)
				core.Dispose ();
		}

		private static IntPtr GetProcedureAddress (
			string name,
			IntPtr instanceHandle,
			IntPtr deviceHandle)
		{
			var bytes = Encoding.ASCII.GetBytes (name + "\0");
			fixed (byte* pName = bytes) {
				if (deviceHandle != IntPtr.Zero)
					return SharedVk.GetDeviceProcAddr (new Device (deviceHandle), pName);
				return SharedVk.GetInstanceProcAddr (new Instance (instanceHandle), pName);
			}
		}

		private static SurfaceFormatKHR SelectSurfaceFormat (SurfaceFormatKHR[] formats)
		{
			foreach (var candidate in formats) {
				if (candidate.Format == Format.R8G8B8A8Unorm ||
					candidate.Format == Format.B8G8R8A8Unorm)
					return candidate;
			}

			throw new PlatformNotSupportedException (
				"The Vulkan surface exposes no Graphite-compatible RGBA8 or BGRA8 format.");
		}

		private T LoadInstance<T> (string name)
			where T : Delegate
		{
			var pointer = GetProcedureAddress (name, instance.Handle, IntPtr.Zero);
			if (pointer == IntPtr.Zero)
				throw new PlatformNotSupportedException ($"{name} is unavailable.");
			return Marshal.GetDelegateForFunctionPointer<T> (pointer);
		}

		private T LoadDevice<T> (string name)
			where T : Delegate
		{
			var pointer = GetProcedureAddress (name, instance.Handle, device.Handle);
			if (pointer == IntPtr.Zero)
				throw new PlatformNotSupportedException ($"{name} is unavailable.");
			return Marshal.GetDelegateForFunctionPointer<T> (pointer);
		}

		[UnmanagedFunctionPointer (CallingConvention.Winapi)]
		private delegate Result CreateAndroidSurfaceDelegate (
			Instance instance,
			AndroidSurfaceCreateInfoKHR* createInfo,
			AllocationCallbacks* allocator,
			SurfaceKHR* surface);

		[UnmanagedFunctionPointer (CallingConvention.Winapi)]
		private delegate void DestroySurfaceDelegate (
			Instance instance,
			SurfaceKHR surface,
			AllocationCallbacks* allocator);

		[UnmanagedFunctionPointer (CallingConvention.Winapi)]
		private delegate Result GetPhysicalDeviceSurfaceSupportDelegate (
			PhysicalDevice physicalDevice,
			uint queueFamilyIndex,
			SurfaceKHR surface,
			Bool32* supported);

		[UnmanagedFunctionPointer (CallingConvention.Winapi)]
		private delegate Result GetPhysicalDeviceSurfaceCapabilitiesDelegate (
			PhysicalDevice physicalDevice,
			SurfaceKHR surface,
			SurfaceCapabilitiesKHR* capabilities);

		[UnmanagedFunctionPointer (CallingConvention.Winapi)]
		private delegate Result GetPhysicalDeviceSurfaceFormatsDelegate (
			PhysicalDevice physicalDevice,
			SurfaceKHR surface,
			uint* count,
			SurfaceFormatKHR* formats);

		[UnmanagedFunctionPointer (CallingConvention.Winapi)]
		private delegate Result GetPhysicalDeviceSurfacePresentModesDelegate (
			PhysicalDevice physicalDevice,
			SurfaceKHR surface,
			uint* count,
			PresentModeKHR* modes);

		[UnmanagedFunctionPointer (CallingConvention.Winapi)]
		private delegate Result CreateSwapchainDelegate (
			Device device,
			SwapchainCreateInfoKHR* createInfo,
			AllocationCallbacks* allocator,
			SwapchainKHR* swapchain);

		[UnmanagedFunctionPointer (CallingConvention.Winapi)]
		private delegate void DestroySwapchainDelegate (
			Device device,
			SwapchainKHR swapchain,
			AllocationCallbacks* allocator);

		[UnmanagedFunctionPointer (CallingConvention.Winapi)]
		private delegate Result GetSwapchainImagesDelegate (
			Device device,
			SwapchainKHR swapchain,
			uint* count,
			Image* images);

		[UnmanagedFunctionPointer (CallingConvention.Winapi)]
		private delegate Result AcquireNextImageDelegate (
			Device device,
			SwapchainKHR swapchain,
			ulong timeout,
			VkSemaphore semaphore,
			Fence fence,
			uint* imageIndex);

		[UnmanagedFunctionPointer (CallingConvention.Winapi)]
		private delegate Result QueuePresentDelegate (
			Queue queue,
			PresentInfoKHR* presentInfo);

		[DllImport ("android")]
		private static extern IntPtr ANativeWindow_fromSurface (IntPtr environment, IntPtr surface);

		[DllImport ("android")]
		private static extern void ANativeWindow_release (IntPtr window);

		private sealed class SwapchainImage
		{
			public SwapchainImage (
				SKGraphiteBackendTexture backendTexture,
				SKSurface surface,
				VkSemaphore renderFinished)
			{
				BackendTexture = backendTexture;
				Surface = surface;
				RenderFinished = renderFinished;
			}

			public SKGraphiteBackendTexture BackendTexture { get; }

			public SKSurface Surface { get; }

			public VkSemaphore RenderFinished { get; }

			public void Dispose (Vk vk, Device device)
			{
				Surface.Dispose ();
				BackendTexture.Dispose ();
				if (RenderFinished.Handle != 0)
					vk.DestroySemaphore (device, RenderFinished, null);
			}
		}

		private sealed class VulkanCore
		{
			public Instance Instance;
			public PhysicalDevice PhysicalDevice;
			public Device Device;
			public Queue Queue;
			public uint QueueFamily;
			public bool DeviceLost;
			public SKGraphiteContext Context = null!;
			public CreateAndroidSurfaceDelegate CreateAndroidSurface = null!;
			public DestroySurfaceDelegate DestroySurface = null!;
			public GetPhysicalDeviceSurfaceSupportDelegate GetPhysicalDeviceSurfaceSupport = null!;
			public GetPhysicalDeviceSurfaceCapabilitiesDelegate GetPhysicalDeviceSurfaceCapabilities = null!;
			public GetPhysicalDeviceSurfaceFormatsDelegate GetPhysicalDeviceSurfaceFormats = null!;
			public GetPhysicalDeviceSurfacePresentModesDelegate GetPhysicalDeviceSurfacePresentModes = null!;
			public CreateSwapchainDelegate CreateSwapchain = null!;
			public DestroySwapchainDelegate DestroySwapchain = null!;
			public GetSwapchainImagesDelegate GetSwapchainImages = null!;
			public AcquireNextImageDelegate AcquireNextImage = null!;
			public QueuePresentDelegate QueuePresent = null!;

			public void Dispose ()
			{
				Context.Dispose ();
				if (Device.Handle != 0)
					SharedVk.DestroyDevice (Device, null);
				if (Instance.Handle != 0)
					SharedVk.DestroyInstance (Instance, null);
			}
		}
	}

	internal static class VulkanResultExtensions
	{
		public static void ThrowOnError (this Result result)
		{
			if (result != Result.Success)
				throw new InvalidOperationException ($"Vulkan operation failed: {result}.");
		}
	}
}
