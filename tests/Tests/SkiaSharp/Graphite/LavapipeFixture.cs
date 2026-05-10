using System;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests.Graphite
{
	/// <summary>
	/// Brings up a minimal Vulkan instance + logical device against Mesa Lavapipe (the
	/// CPU-only software ICD shipped by mesa-vulkan-drivers) and packages the handles
	/// into an <see cref="SKGraphiteVkBackendContext"/> ready to feed to
	/// <see cref="SKGraphiteContext.CreateVulkan(SKGraphiteVkBackendContext)"/>.
	///
	/// Used by every Graphite/Vulkan test in this project. Linux / WSL2 / CI only.
	/// </summary>
	internal sealed unsafe class LavapipeFixture : IDisposable
	{
		private const string libvulkan = "vulkan";  // resolves to libvulkan.so.1 on Linux

		private IntPtr instance;
		private IntPtr device;
		private IntPtr queue;
		private uint queueFamilyIndex;
		private SKGraphiteVkBackendContext backendContext;

		public bool IsAvailable { get; private set; }
		public string FailureReason { get; private set; }
		public SKGraphiteVkBackendContext BackendContext => backendContext;

		/// <summary>
		/// Used by the disposal-order regression test for Variant A. After
		/// SKGraphiteContext.CreateVulkan transfers GCHandle ownership, calling this
		/// disposes the SKGraphiteVkBackendContext while leaving the rest of the fixture
		/// (instance/device/queue) intact for further use against the still-live SKGraphiteContext.
		/// </summary>
		public void DisposeBackendContextOnly ()
		{
			backendContext?.Dispose ();
			backendContext = null;
		}

		public static LavapipeFixture TryCreate ()
		{
			var f = new LavapipeFixture ();
			f.Initialize ();
			return f;
		}

		private void Initialize ()
		{
			try {
				if (!CreateInstance (out var failure)) {
					FailureReason = failure;
					return;
				}
				if (!PickPhysicalDeviceAndCreateDevice (out failure)) {
					FailureReason = failure;
					return;
				}

				backendContext = new SKGraphiteVkBackendContext {
					VkInstance        = instance,
					VkPhysicalDevice  = physicalDevice,
					VkDevice          = device,
					VkQueue           = queue,
					GraphicsQueueIndex = queueFamilyIndex,
					MaxApiVersion     = VK_API_VERSION_1_3,
					ProtectedContext  = false,
					GetProcedureAddress = GetProc,
				};

				IsAvailable = true;
			} catch (DllNotFoundException ex) {
				FailureReason = $"libvulkan.so not loadable: {ex.Message}";
			} catch (Exception ex) {
				FailureReason = $"Vulkan init failed: {ex.GetType ().Name}: {ex.Message}";
			}
		}

		private IntPtr physicalDevice;

		private bool CreateInstance (out string failure)
		{
			failure = null;
			var appInfo = new VkApplicationInfo {
				sType = VK_STRUCTURE_TYPE_APPLICATION_INFO,
				apiVersion = VK_API_VERSION_1_3,
			};
			var ici = new VkInstanceCreateInfo {
				sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO,
				pApplicationInfo = &appInfo,
			};
			var rc = vkCreateInstance (&ici, IntPtr.Zero, out instance);
			if (rc != 0) {
				failure = $"vkCreateInstance returned {rc}";
				return false;
			}
			return true;
		}

		private bool PickPhysicalDeviceAndCreateDevice (out string failure)
		{
			failure = null;
			uint count = 0;
			vkEnumeratePhysicalDevices (instance, ref count, null);
			if (count == 0) {
				failure = "no Vulkan physical devices found";
				return false;
			}
			var devices = new IntPtr[count];
			fixed (IntPtr* pd = devices) {
				vkEnumeratePhysicalDevices (instance, ref count, pd);
			}

			// Prefer Lavapipe (CPU device type 4) if present.
			physicalDevice = devices[0];
			for (var i = 0; i < count; i++) {
				var props = new VkPhysicalDeviceProperties ();
				vkGetPhysicalDeviceProperties (devices[i], &props);
				if (props.deviceType == 4 /* VK_PHYSICAL_DEVICE_TYPE_CPU */) {
					physicalDevice = devices[i];
					break;
				}
			}

			// Find a graphics queue family.
			uint qfCount = 0;
			vkGetPhysicalDeviceQueueFamilyProperties (physicalDevice, ref qfCount, null);
			if (qfCount == 0) {
				failure = "no Vulkan queue families";
				return false;
			}
			var qfs = new VkQueueFamilyProperties[qfCount];
			fixed (VkQueueFamilyProperties* pqfs = qfs) {
				vkGetPhysicalDeviceQueueFamilyProperties (physicalDevice, ref qfCount, pqfs);
			}
			queueFamilyIndex = uint.MaxValue;
			for (uint i = 0; i < qfCount; i++) {
				if ((qfs[i].queueFlags & 1u /* VK_QUEUE_GRAPHICS_BIT */) != 0) {
					queueFamilyIndex = i;
					break;
				}
			}
			if (queueFamilyIndex == uint.MaxValue) {
				failure = "no graphics-capable queue family";
				return false;
			}

			float prio = 1.0f;
			var qci = new VkDeviceQueueCreateInfo {
				sType = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO,
				queueFamilyIndex = queueFamilyIndex,
				queueCount = 1,
				pQueuePriorities = &prio,
			};
			var dci = new VkDeviceCreateInfo {
				sType = VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO,
				queueCreateInfoCount = 1,
				pQueueCreateInfos = &qci,
			};
			var rc = vkCreateDevice (physicalDevice, &dci, IntPtr.Zero, out device);
			if (rc != 0) {
				failure = $"vkCreateDevice returned {rc}";
				return false;
			}
			vkGetDeviceQueue (device, queueFamilyIndex, 0, out queue);
			return true;
		}

		private IntPtr GetProc (string name, IntPtr instance, IntPtr device)
		{
			if (device != IntPtr.Zero)
				return vkGetDeviceProcAddr (device, name);
			if (instance != IntPtr.Zero)
				return vkGetInstanceProcAddr (instance, name);
			return vkGetInstanceProcAddr (IntPtr.Zero, name);
		}

		public void Dispose ()
		{
			backendContext?.Dispose ();
			backendContext = null;

			if (device != IntPtr.Zero) {
				vkDestroyDevice (device, IntPtr.Zero);
				device = IntPtr.Zero;
			}
			if (instance != IntPtr.Zero) {
				vkDestroyInstance (instance, IntPtr.Zero);
				instance = IntPtr.Zero;
			}
		}

		// Minimal Vulkan 1.3 P/Invoke surface — just the bring-up calls we need.

		private const uint VK_STRUCTURE_TYPE_APPLICATION_INFO        = 0;
		private const uint VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO    = 1;
		private const uint VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO = 2;
		private const uint VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO      = 3;
		private const uint VK_API_VERSION_1_3                         = (1u << 22) | (3u << 12);

		[StructLayout (LayoutKind.Sequential)]
		private struct VkApplicationInfo
		{
			public uint sType;
			public IntPtr pNext;
			public IntPtr pApplicationName;
			public uint applicationVersion;
			public IntPtr pEngineName;
			public uint engineVersion;
			public uint apiVersion;
		}

		[StructLayout (LayoutKind.Sequential)]
		private struct VkInstanceCreateInfo
		{
			public uint sType;
			public IntPtr pNext;
			public uint flags;
			public VkApplicationInfo* pApplicationInfo;
			public uint enabledLayerCount;
			public IntPtr ppEnabledLayerNames;
			public uint enabledExtensionCount;
			public IntPtr ppEnabledExtensionNames;
		}

		[StructLayout (LayoutKind.Sequential, Size = 824)]
		private struct VkPhysicalDeviceProperties
		{
			public uint apiVersion;
			public uint driverVersion;
			public uint vendorID;
			public uint deviceID;
			public int deviceType;
			// remaining fields padded by Size = 824
		}

		[StructLayout (LayoutKind.Sequential)]
		private struct VkQueueFamilyProperties
		{
			public uint queueFlags;
			public uint queueCount;
			public uint timestampValidBits;
			public VkExtent3D minImageTransferGranularity;
		}

		[StructLayout (LayoutKind.Sequential)]
		private struct VkExtent3D
		{
			public uint width, height, depth;
		}

		[StructLayout (LayoutKind.Sequential)]
		private struct VkDeviceQueueCreateInfo
		{
			public uint sType;
			public IntPtr pNext;
			public uint flags;
			public uint queueFamilyIndex;
			public uint queueCount;
			public float* pQueuePriorities;
		}

		[StructLayout (LayoutKind.Sequential)]
		private struct VkDeviceCreateInfo
		{
			public uint sType;
			public IntPtr pNext;
			public uint flags;
			public uint queueCreateInfoCount;
			public VkDeviceQueueCreateInfo* pQueueCreateInfos;
			public uint enabledLayerCount;
			public IntPtr ppEnabledLayerNames;
			public uint enabledExtensionCount;
			public IntPtr ppEnabledExtensionNames;
			public IntPtr pEnabledFeatures;
		}

		[DllImport (libvulkan)]
		private static extern int vkCreateInstance (VkInstanceCreateInfo* createInfo, IntPtr allocator, out IntPtr instance);

		[DllImport (libvulkan)]
		private static extern void vkDestroyInstance (IntPtr instance, IntPtr allocator);

		[DllImport (libvulkan)]
		private static extern int vkEnumeratePhysicalDevices (IntPtr instance, ref uint count, IntPtr* devices);

		[DllImport (libvulkan)]
		private static extern void vkGetPhysicalDeviceProperties (IntPtr physicalDevice, VkPhysicalDeviceProperties* props);

		[DllImport (libvulkan)]
		private static extern void vkGetPhysicalDeviceQueueFamilyProperties (IntPtr physicalDevice, ref uint count, VkQueueFamilyProperties* props);

		[DllImport (libvulkan)]
		private static extern int vkCreateDevice (IntPtr physicalDevice, VkDeviceCreateInfo* createInfo, IntPtr allocator, out IntPtr device);

		[DllImport (libvulkan)]
		private static extern void vkDestroyDevice (IntPtr device, IntPtr allocator);

		[DllImport (libvulkan)]
		private static extern void vkGetDeviceQueue (IntPtr device, uint queueFamilyIndex, uint queueIndex, out IntPtr queue);

		[DllImport (libvulkan, CharSet = CharSet.Ansi)]
		private static extern IntPtr vkGetInstanceProcAddr (IntPtr instance, string name);

		[DllImport (libvulkan, CharSet = CharSet.Ansi)]
		private static extern IntPtr vkGetDeviceProcAddr (IntPtr device, string name);
	}
}
