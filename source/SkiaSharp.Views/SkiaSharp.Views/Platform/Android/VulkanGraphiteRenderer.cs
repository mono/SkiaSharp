#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Android.Runtime;
using AndroidSurface = Android.Views.Surface;

namespace SkiaSharp.Views.Android
{
	internal sealed unsafe class VulkanGraphiteRenderer : IDisposable
	{
		private const int MaxFramesInFlight = 3;
		private const long RecorderBudgetBytes = 64 * 1024 * 1024;

		private IntPtr instance;
		private IntPtr physicalDevice;
		private IntPtr device;
		private IntPtr queue;
		private uint queueFamily;
		private ulong surface;
		private IntPtr nativeWindow;
		private ulong swapchain;
		private SwapchainImage[] images = Array.Empty<SwapchainImage>();
		private readonly List<FrameSlot> pendingFrames = new();
		private VkFormat format;
		private SKColorType colorType;
		private VkImageUsageFlags usage;
		private ulong frameId;
		private bool disposed;

		private PFN_vkCreateAndroidSurfaceKHR createSurface = null!;
		private PFN_vkDestroySurfaceKHR destroySurface = null!;
		private PFN_vkGetPhysicalDeviceSurfaceSupportKHR surfaceSupport = null!;
		private PFN_vkGetPhysicalDeviceSurfaceCapabilitiesKHR surfaceCapabilities = null!;
		private PFN_vkGetPhysicalDeviceSurfaceFormatsKHR surfaceFormats = null!;
		private PFN_vkCreateSwapchainKHR createSwapchain = null!;
		private PFN_vkDestroySwapchainKHR destroySwapchain = null!;
		private PFN_vkGetSwapchainImagesKHR getImages = null!;
		private PFN_vkAcquireNextImageKHR acquire = null!;
		private PFN_vkQueuePresentKHR present = null!;

		public VulkanGraphiteRenderer(AndroidSurface window, int width, int height)
		{
			try
			{
				CreateInstance();

				createSurface = Load<PFN_vkCreateAndroidSurfaceKHR>("vkCreateAndroidSurfaceKHR", false);
				destroySurface = Load<PFN_vkDestroySurfaceKHR>("vkDestroySurfaceKHR", false);
				surfaceSupport = Load<PFN_vkGetPhysicalDeviceSurfaceSupportKHR>("vkGetPhysicalDeviceSurfaceSupportKHR", false);
				surfaceCapabilities = Load<PFN_vkGetPhysicalDeviceSurfaceCapabilitiesKHR>("vkGetPhysicalDeviceSurfaceCapabilitiesKHR", false);
				surfaceFormats = Load<PFN_vkGetPhysicalDeviceSurfaceFormatsKHR>("vkGetPhysicalDeviceSurfaceFormatsKHR", false);

				nativeWindow = VulkanInterop.ANativeWindow_fromSurface(JNIEnv.Handle, window.Handle);
				if (nativeWindow == IntPtr.Zero)
				{
					throw new InvalidOperationException("Android native window acquisition failed.");
				}

				var surfaceInfo = new VkAndroidSurfaceCreateInfoKHR
				{
					SType = VkStructureType.AndroidSurfaceCreateInfoKhr,
					Window = nativeWindow,
				};

				ulong result;
				Check(createSurface(instance, &surfaceInfo, null, &result));
				surface = result;

				SelectDevice();
				CreateDevice();

				createSwapchain = Load<PFN_vkCreateSwapchainKHR>("vkCreateSwapchainKHR", true);
				destroySwapchain = Load<PFN_vkDestroySwapchainKHR>("vkDestroySwapchainKHR", true);
				getImages = Load<PFN_vkGetSwapchainImagesKHR>("vkGetSwapchainImagesKHR", true);
				acquire = Load<PFN_vkAcquireNextImageKHR>("vkAcquireNextImageKHR", true);
				present = Load<PFN_vkQueuePresentKHR>("vkQueuePresentKHR", true);

				using var backend = new SKGraphiteVkBackendContext
				{
					VkInstance = instance,
					VkPhysicalDevice = physicalDevice,
					VkDevice = device,
					VkQueue = queue,
					GraphicsQueueIndex = queueFamily,
					MaxApiVersion = VulkanInterop.Version11,
					GetProcedureAddress = GetProcedureAddress,
				};

				Context = SKGraphiteContext.CreateVulkan(backend)
					?? throw new PlatformNotSupportedException("This device cannot create a Graphite Vulkan context.");

				EnsureRecorder();
				Resize(width, height);
			}
			catch
			{
				Dispose();
				throw;
			}
		}

		public SKGraphiteContext? Context { get; private set; }

		public SKGraphiteRecorder? Recorder { get; private set; }

		public int Width { get; private set; }

		public int Height { get; private set; }

		public void CheckAsyncWorkCompletion()
		{
			Context?.CheckAsyncWorkCompletion();
			RetireCompletedFrames();
		}

		private void EnsureRecorder()
		{
			if (Recorder is not null || Context is null)
				return;

			var cache = new SKGraphiteImageCache();
			try
			{
				Recorder = Context.CreateRecorder(RecorderBudgetBytes, cache.FindOrCreate, cache.Dispose);
				if (Recorder is null)
				{
					cache.Dispose();
					throw new InvalidOperationException("Graphite recorder creation failed.");
				}
			}
			catch
			{
				cache.Dispose();
				throw;
			}
		}

		private void CreateInstance()
		{
			var surfaceExtension = Marshal.StringToHGlobalAnsi("VK_KHR_surface");
			var androidSurfaceExtension = Marshal.StringToHGlobalAnsi("VK_KHR_android_surface");

			try
			{
				var extensions = stackalloc byte*[2];
				extensions[0] = (byte*)surfaceExtension;
				extensions[1] = (byte*)androidSurfaceExtension;

				var app = new VkApplicationInfo
				{
					SType = VkStructureType.ApplicationInfo,
					ApiVersion = VulkanInterop.Version11,
				};

				var info = new VkInstanceCreateInfo
				{
					SType = VkStructureType.InstanceCreateInfo,
					PApplicationInfo = &app,
					EnabledExtensionCount = 2,
					PpEnabledExtensionNames = extensions,
				};

				IntPtr createdInstance;
				Check(VulkanInterop.vkCreateInstance(&info, null, &createdInstance));
				instance = createdInstance;
			}
			finally
			{
				Marshal.FreeHGlobal(surfaceExtension);
				Marshal.FreeHGlobal(androidSurfaceExtension);
			}
		}

		private void SelectDevice()
		{
			uint count = 0;
			Check(VulkanInterop.vkEnumeratePhysicalDevices(instance, &count, null));
			if (count == 0)
			{
				throw new PlatformNotSupportedException("No Vulkan physical devices are available.");
			}

			var devices = new IntPtr[count];
			fixed (IntPtr* ptr = devices)
			{
				Check(VulkanInterop.vkEnumeratePhysicalDevices(instance, &count, ptr));
			}

			// VkPhysicalDeviceProperties contains 64-bit aligned VkDeviceSize fields and is ~824 bytes;
			// apiVersion is the first uint32_t at offset 0.
			var propertiesBuffer = stackalloc ulong[128];

			foreach (var candidate in devices)
			{
				VulkanInterop.vkGetPhysicalDeviceProperties(candidate, propertiesBuffer);
				var apiVersion = *(uint*)propertiesBuffer;
				if (apiVersion < VulkanInterop.Version11)
					continue;

				uint familiesCount = 0;
				VulkanInterop.vkGetPhysicalDeviceQueueFamilyProperties(candidate, &familiesCount, null);
				if (familiesCount == 0)
					continue;

				var families = new VkQueueFamilyProperties[familiesCount];
				fixed (VkQueueFamilyProperties* ptr = families)
				{
					VulkanInterop.vkGetPhysicalDeviceQueueFamilyProperties(candidate, &familiesCount, ptr);
				}

				for (uint i = 0; i < families.Length; i++)
				{
					uint supported = VulkanInterop.False;
					if ((families[i].QueueFlags & VkQueueFlags.GraphicsBit) != 0 &&
						surfaceSupport(candidate, i, surface, &supported) == VkResult.Success &&
						supported != VulkanInterop.False)
					{
						physicalDevice = candidate;
						queueFamily = i;
						return;
					}
				}
			}

			throw new PlatformNotSupportedException("No Vulkan 1.1 graphics queue can present to this Android window.");
		}

		private void CreateDevice()
		{
			var swapchainExtension = Marshal.StringToHGlobalAnsi("VK_KHR_swapchain");

			try
			{
				var extensions = stackalloc byte*[1];
				extensions[0] = (byte*)swapchainExtension;

				var priority = 1f;
				var queueInfo = new VkDeviceQueueCreateInfo
				{
					SType = VkStructureType.DeviceQueueCreateInfo,
					QueueFamilyIndex = queueFamily,
					QueueCount = 1,
					PQueuePriorities = &priority,
				};

				var info = new VkDeviceCreateInfo
				{
					SType = VkStructureType.DeviceCreateInfo,
					QueueCreateInfoCount = 1,
					PQueueCreateInfos = &queueInfo,
					EnabledExtensionCount = 1,
					PpEnabledExtensionNames = extensions,
				};

				IntPtr createdDevice;
				Check(VulkanInterop.vkCreateDevice(physicalDevice, &info, null, &createdDevice));
				device = createdDevice;

				IntPtr createdQueue;
				VulkanInterop.vkGetDeviceQueue(device, queueFamily, 0, &createdQueue);
				queue = createdQueue;
			}
			finally
			{
				Marshal.FreeHGlobal(swapchainExtension);
			}
		}

		public void Resize(int width, int height)
		{
			if (Context is null)
				return;

			if (width <= 0 || height <= 0)
			{
				Width = 0;
				Height = 0;
				return;
			}

			Check(VulkanInterop.vkDeviceWaitIdle(device));
			Context.CheckAsyncWorkCompletion();
			ReleaseFrameSlots();
			ReleaseSwapchain();
			EnsureRecorder();

			var geometryResult = VulkanInterop.ANativeWindow_setBuffersGeometry(nativeWindow, width, height, 0);
			if (geometryResult != 0)
			{
				throw new InvalidOperationException($"Android buffer resize failed: {geometryResult}.");
			}

			VkSurfaceCapabilitiesKHR caps;
			Check(surfaceCapabilities(physicalDevice, surface, &caps));

			uint count = 0;
			Check(surfaceFormats(physicalDevice, surface, &count, null));
			if (count == 0)
			{
				throw new PlatformNotSupportedException("No Vulkan surface formats are available.");
			}

			var formats = new VkSurfaceFormatKHR[count];
			fixed (VkSurfaceFormatKHR* ptr = formats)
			{
				Check(surfaceFormats(physicalDevice, surface, &count, ptr));
			}

			VkSurfaceFormatKHR? selected = null;
			foreach (var candidate in formats)
			{
				if (candidate.Format == VkFormat.R8G8B8A8Unorm || candidate.Format == VkFormat.B8G8R8A8Unorm)
				{
					selected = candidate;
					break;
				}
			}

			if (selected is not { } selectedFormat)
			{
				throw new PlatformNotSupportedException("No Graphite-compatible RGBA8 presentation format is available.");
			}

			format = selectedFormat.Format;
			colorType = format == VkFormat.B8G8R8A8Unorm ? SKColorType.Bgra8888 : SKColorType.Rgba8888;
			usage = VkImageUsageFlags.ColorAttachmentBit | VkImageUsageFlags.TransferSrcBit | VkImageUsageFlags.TransferDstBit;
			if ((caps.SupportedUsageFlags & usage) != usage)
			{
				throw new PlatformNotSupportedException("The window does not support Graphite's required texture usage.");
			}

			if ((caps.SupportedUsageFlags & VkImageUsageFlags.InputAttachmentBit) != 0)
			{
				usage |= VkImageUsageFlags.InputAttachmentBit;
			}

			var extent = caps.CurrentExtent;
			if (extent.Width == uint.MaxValue)
			{
				extent.Width = Math.Clamp((uint)width, caps.MinImageExtent.Width, caps.MaxImageExtent.Width);
				extent.Height = Math.Clamp((uint)height, caps.MinImageExtent.Height, caps.MaxImageExtent.Height);
			}

			Width = (int)extent.Width;
			Height = (int)extent.Height;

			var imageCount = caps.MinImageCount + 1;
			if (caps.MaxImageCount > 0)
			{
				imageCount = Math.Min(imageCount, caps.MaxImageCount);
			}

			var alpha = (caps.SupportedCompositeAlpha & VkCompositeAlphaFlagsKHR.PreMultipliedBitKhr) != 0
				? VkCompositeAlphaFlagsKHR.PreMultipliedBitKhr
				: VkCompositeAlphaFlagsKHR.InheritBitKhr;
			if ((caps.SupportedCompositeAlpha & alpha) == 0)
			{
				throw new PlatformNotSupportedException("The Android window cannot present premultiplied transparent content.");
			}

			var info = new VkSwapchainCreateInfoKHR
			{
				SType = VkStructureType.SwapchainCreateInfoKhr,
				Surface = surface,
				MinImageCount = imageCount,
				ImageFormat = format,
				ImageColorSpace = selectedFormat.ColorSpace,
				ImageExtent = extent,
				ImageArrayLayers = 1,
				ImageUsage = usage,
				ImageSharingMode = VkSharingMode.Exclusive,
				PreTransform = caps.CurrentTransform,
				CompositeAlpha = alpha,
				PresentMode = VkPresentModeKHR.FifoKhr,
				Clipped = VulkanInterop.True,
			};

			ulong newSwapchain;
			Check(createSwapchain(device, &info, null, &newSwapchain));
			swapchain = newSwapchain;

			uint actualCount = 0;
			Check(getImages(device, swapchain, &actualCount, null));

			var nativeImages = new ulong[actualCount];
			fixed (ulong* ptr = nativeImages)
			{
				Check(getImages(device, swapchain, &actualCount, ptr));
			}

			images = new SwapchainImage[actualCount];
			for (var i = 0; i < images.Length; i++)
			{
				images[i] = CreateImage(nativeImages[i]);
			}
		}

		private SwapchainImage CreateImage(ulong image)
		{
			var textureInfo = new SKGraphiteVkTextureInfo
			{
				SampleCount = 1,
				Mipmapped = false,
				Format = (int)format,
				ImageTiling = (int)VkImageTiling.Optimal,
				ImageUsageFlags = (uint)usage,
				SharingMode = (int)VkSharingMode.Exclusive,
				AspectMask = (uint)VkImageAspectFlags.ColorBit,
			};

			var texture = SKGraphiteBackendTexture.CreateVulkan(
				new SKSizeI(Width, Height),
				textureInfo,
				(int)VkImageLayout.Undefined,
				queueFamily,
				image)
				?? throw new InvalidOperationException("Vulkan texture wrapping failed.");

			SKSurface? target = null;
			try
			{
				target = SKSurface.Create(Recorder!, texture, colorType)
					?? throw new InvalidOperationException("Graphite Vulkan surface creation failed.");

				return new SwapchainImage
				{
					Texture = texture,
					Surface = target,
					Finished = CreateSemaphore(),
				};
			}
			catch
			{
				target?.Dispose();
				texture.Dispose();
				throw;
			}
		}

		public bool Render(SKGraphiteView owner)
		{
			if (Context is null || Recorder is null || Width <= 0 || Height <= 0 || images.Length == 0)
				return true;

			if (Context.IsDeviceLost)
			{
				throw new InvalidOperationException("The Graphite Vulkan device was lost.");
			}

			CheckAsyncWorkCompletion();
			if (pendingFrames.Count >= Math.Min(MaxFramesInFlight, images.Length))
				return false;

			var slot = new FrameSlot
			{
				Acquired = CreateSemaphore(),
			};

			var presentedFrame = false;
			try
			{
				var fenceInfo = new VkFenceCreateInfo
				{
					SType = VkStructureType.FenceCreateInfo,
				};

				ulong acquireFence;
				Check(VulkanInterop.vkCreateFence(device, &fenceInfo, null, &acquireFence));
				slot.AcquireFence = acquireFence;

				uint index = 0;
				var result = acquire(device, swapchain, 0, slot.Acquired, slot.AcquireFence, &index);
				if (result == VkResult.Timeout || result == VkResult.NotReady)
					return false;

				if (result == VkResult.ErrorOutOfDateKhr)
				{
					Resize(Width, Height);
					return false;
				}

				if (result != VkResult.Success && result != VkResult.SuboptimalKhr)
				{
					Check(result);
				}

				slot.ImageAcquired = true;
				pendingFrames.Add(slot);

				var image = images[index];

				var density = owner.Resources?.DisplayMetrics?.Density ?? 1f;
				if (density <= 0f)
					density = 1f;

				var raw = new SKImageInfo(Width, Height, colorType, SKAlphaType.Premul);
				var info = owner.IgnorePixelScaling
					? raw.WithSize((int)(Width / density), (int)(Height / density))
					: raw;

				owner.CanvasSize = info.Size;

				using (new SKAutoCanvasRestore(image.Surface.Canvas, true))
				{
					image.Surface.Canvas.Clear(SKColors.Transparent);

					if (owner.IgnorePixelScaling)
					{
						image.Surface.Canvas.Scale(density);
					}

					var e = new SKPaintGraphiteSurfaceEventArgs(image.Surface, Context, Recorder, image.Texture, info, raw);
					owner.RaisePaintSurface(e);
				}

				using var recording = Recorder.Snap()
					?? throw new InvalidOperationException("Graphite snap failed.");
				using var wait = SKGraphiteBackendSemaphore.CreateVulkan(slot.Acquired)
					?? throw new InvalidOperationException("Vulkan acquisition semaphore wrapping failed.");
				using var signal = SKGraphiteBackendSemaphore.CreateVulkan(image.Finished)
					?? throw new InvalidOperationException("Vulkan presentation semaphore wrapping failed.");
				using var state = SKGraphiteMutableTextureState.CreateVulkan((int)VkImageLayout.PresentSrcKhr, queueFamily)
					?? throw new InvalidOperationException("Vulkan presentation state creation failed.");

				SKGraphiteFinishedDelegate finished = success =>
				{
					slot.Succeeded = success;
					slot.Completed = true;
				};

				var options = new SKGraphiteInsertRecordingOptions
				{
					TargetSurface = image.Surface,
					TargetTextureState = state,
					WaitSemaphores = new[] { wait },
					SignalSemaphores = new[] { signal },
					Finished = finished,
				};

				var status = Context.InsertRecording(recording, options);
				if (status != SKGraphiteInsertStatus.Success)
				{
					throw new InvalidOperationException($"Graphite Vulkan insertion failed: {status}.");
				}

				if (!Context.Submit(new SKGraphiteSubmitInfo { Sync = false, MarkBoundary = true, FrameID = ++frameId }))
				{
					throw new InvalidOperationException("Graphite Vulkan submission failed.");
				}

				var signalSemaphore = image.Finished;
				var currentSwapchain = swapchain;
				var presentInfo = new VkPresentInfoKHR
				{
					SType = VkStructureType.PresentInfoKhr,
					WaitSemaphoreCount = 1,
					PWaitSemaphores = &signalSemaphore,
					SwapchainCount = 1,
					PSwapchains = &currentSwapchain,
					PImageIndices = &index,
				};

				var presented = present(queue, &presentInfo);
				if (presented == VkResult.ErrorOutOfDateKhr || presented == VkResult.SuboptimalKhr || result == VkResult.SuboptimalKhr)
				{
					presentedFrame = true;
					Resize(Width, Height);
					return false;
				}

				Check(presented);
				presentedFrame = true;
				return true;
			}
			finally
			{
				if (!presentedFrame)
				{
					if (slot.ImageAcquired)
					{
						var fence = slot.AcquireFence;
						CheckCleanup(VulkanInterop.vkWaitForFences(device, 1, &fence, VulkanInterop.True, ulong.MaxValue));
						CheckCleanup(VulkanInterop.vkDeviceWaitIdle(device));
						Context.CheckAsyncWorkCompletion();
					}

					pendingFrames.Remove(slot);
					ReleaseFrameSlot(slot);
				}
			}
		}

		private void RetireCompletedFrames()
		{
			for (var i = pendingFrames.Count - 1; i >= 0; i--)
			{
				var slot = pendingFrames[i];
				if (!slot.Completed)
					continue;

				pendingFrames.RemoveAt(i);
				ReleaseFrameSlot(slot);

				if (!slot.Succeeded)
				{
					throw new InvalidOperationException("Graphite Vulkan frame execution failed.");
				}
			}
		}

		private void ReleaseFrameSlot(FrameSlot slot)
		{
			if (slot.AcquireFence != 0)
			{
				VulkanInterop.vkDestroyFence(device, slot.AcquireFence, null);
				slot.AcquireFence = 0;
			}

			if (slot.Acquired != 0)
			{
				VulkanInterop.vkDestroySemaphore(device, slot.Acquired, null);
				slot.Acquired = 0;
			}
		}

		private void ReleaseFrameSlots()
		{
			foreach (var slot in pendingFrames)
			{
				ReleaseFrameSlot(slot);
			}

			pendingFrames.Clear();
		}

		private static void CheckCleanup(VkResult result)
		{
			if (result != VkResult.Success && result != VkResult.ErrorDeviceLost)
			{
				Check(result);
			}
		}

		private ulong CreateSemaphore()
		{
			var info = new VkSemaphoreCreateInfo
			{
				SType = VkStructureType.SemaphoreCreateInfo,
			};

			ulong result;
			Check(VulkanInterop.vkCreateSemaphore(device, &info, null, &result));
			return result;
		}

		private void ReleaseSwapchain()
		{
			foreach (var image in images)
			{
				if (image is null)
					continue;

				image.Surface.Dispose();
				image.Texture.Dispose();

				if (image.Finished != 0)
				{
					VulkanInterop.vkDestroySemaphore(device, image.Finished, null);
					image.Finished = 0;
				}
			}

			images = Array.Empty<SwapchainImage>();

			Recorder?.Dispose();
			Recorder = null;

			if (swapchain != 0)
			{
				destroySwapchain(device, swapchain, null);
				swapchain = 0;
			}
		}

		public void Dispose()
		{
			if (disposed)
				return;

			disposed = true;

			if (device != IntPtr.Zero)
			{
				VulkanInterop.vkDeviceWaitIdle(device);
			}

			try
			{
				if (Context is not null)
				{
					if (!Context.IsDeviceLost)
					{
						Context.Submit(new SKGraphiteSubmitInfo { Sync = true });
					}

					Context.CheckAsyncWorkCompletion();
				}
			}
			finally
			{
				ReleaseFrameSlots();
				ReleaseSwapchain();

				Recorder?.Dispose();
				Recorder = null;

				Context?.Dispose();
				Context = null;

				if (surface != 0 && instance != IntPtr.Zero)
				{
					destroySurface(instance, surface, null);
					surface = 0;
				}

				if (nativeWindow != IntPtr.Zero)
				{
					VulkanInterop.ANativeWindow_release(nativeWindow);
					nativeWindow = IntPtr.Zero;
				}

				if (device != IntPtr.Zero)
				{
					VulkanInterop.vkDestroyDevice(device, null);
					device = IntPtr.Zero;
				}

				if (instance != IntPtr.Zero)
				{
					VulkanInterop.vkDestroyInstance(instance, null);
					instance = IntPtr.Zero;
				}

				Width = 0;
				Height = 0;
			}
		}

		private IntPtr GetProcedureAddress(string name, IntPtr instanceHandle, IntPtr deviceHandle)
		{
			var count = Encoding.ASCII.GetByteCount(name);
			Span<byte> buffer = count < 256 ? stackalloc byte[count + 1] : new byte[count + 1];
			Encoding.ASCII.GetBytes(name, buffer);
			buffer[count] = 0;

			fixed (byte* ptr = buffer)
			{
				return deviceHandle != IntPtr.Zero
					? VulkanInterop.vkGetDeviceProcAddr(deviceHandle, ptr)
					: VulkanInterop.vkGetInstanceProcAddr(instanceHandle, ptr);
			}
		}

		private T Load<T>(string name, bool deviceProcedure)
			where T : Delegate
		{
			var ptr = GetProcedureAddress(name, instance, deviceProcedure ? device : IntPtr.Zero);
			if (ptr == IntPtr.Zero)
			{
				throw new PlatformNotSupportedException($"{name} is unavailable.");
			}

			return Marshal.GetDelegateForFunctionPointer<T>(ptr);
		}

		private static void Check(VkResult result)
		{
			if (result != VkResult.Success)
			{
				throw new InvalidOperationException($"Vulkan operation failed: {result}.");
			}
		}

		private sealed class SwapchainImage
		{
			public SKGraphiteBackendTexture Texture = null!;
			public SKSurface Surface = null!;
			public ulong Finished;
		}

		private sealed class FrameSlot
		{
			public ulong Acquired;
			public ulong AcquireFence;
			public bool ImageAcquired;
			public bool Completed;
			public bool Succeeded;
		}
	}
}
