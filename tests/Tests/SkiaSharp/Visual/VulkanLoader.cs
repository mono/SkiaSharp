using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Process-singleton Vulkan instance + logical device, used to feed both
	/// Ganesh (<c>GRContext.CreateVulkan</c>) and Graphite
	/// (<c>SKGraphiteContext.CreateVulkan</c>) for visual tests.
	///
	/// Targets Mesa Lavapipe (the CPU-only software ICD shipped by
	/// mesa-vulkan-drivers) so tests run in headless CI / WSL2 without a GPU.
	/// Prefers <c>VK_PHYSICAL_DEVICE_TYPE_CPU</c> when present so we get
	/// deterministic software rendering even on machines that ALSO have a real
	/// GPU (deterministic = stable goldens).
	///
	/// Lifetime is process-wide. The first test that touches
	/// <see cref="Shared"/> brings up Vulkan; everything else reuses it. The
	/// runtime tears down on process exit.
	/// </summary>
	internal sealed unsafe class VulkanLoader
	{
		private const string libvulkan = "vulkan";   // resolves to libvulkan.so.1 on Linux

		private static VulkanLoader instance;
		private static readonly object instanceLock = new object ();

		public IntPtr Instance         { get; private set; }
		public IntPtr PhysicalDevice   { get; private set; }
		public IntPtr Device           { get; private set; }
		public IntPtr Queue            { get; private set; }
		public uint   QueueFamilyIndex { get; private set; }
		public bool   IsAvailable      { get; private set; }
		public string FailureReason    { get; private set; }

		/// <summary>Process-singleton accessor. First call brings Vulkan up.</summary>
		public static VulkanLoader Shared {
			get {
				if (instance == null) {
					lock (instanceLock) {
						if (instance == null) {
							instance = new VulkanLoader ();
							instance.Initialize ();
						}
					}
				}
				return instance;
			}
		}

		/// <summary>Procedure-address adapter compatible with both Ganesh and Graphite GetProc shapes.</summary>
		public IntPtr GetProc (string name, IntPtr instance, IntPtr device)
		{
			if (device != IntPtr.Zero)
				return vkGetDeviceProcAddr (device, name);
			if (instance != IntPtr.Zero)
				return vkGetInstanceProcAddr (instance, name);
			return vkGetInstanceProcAddr (IntPtr.Zero, name);
		}

		private void Initialize ()
		{
			try {
				if (!CreateInstance (out var failure)) { FailureReason = failure; return; }
				if (!PickPhysicalDeviceAndCreateDevice (out failure)) { FailureReason = failure; return; }
				IsAvailable = true;
			} catch (DllNotFoundException ex) {
				FailureReason = $"libvulkan.so not loadable: {ex.Message}";
			} catch (Exception ex) {
				FailureReason = $"Vulkan init failed: {ex.GetType ().Name}: {ex.Message}";
			}
		}

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
			var rc = vkCreateInstance (&ici, IntPtr.Zero, out var inst);
			if (rc != 0) { failure = $"vkCreateInstance returned {rc}"; return false; }
			Instance = inst;
			return true;
		}

		private bool PickPhysicalDeviceAndCreateDevice (out string failure)
		{
			failure = null;
			uint count = 0;
			vkEnumeratePhysicalDevices (Instance, ref count, null);
			if (count == 0) { failure = "no Vulkan physical devices found"; return false; }
			var devices = new IntPtr[count];
			fixed (IntPtr* pd = devices) {
				vkEnumeratePhysicalDevices (Instance, ref count, pd);
			}

			PhysicalDevice = devices[0];
			for (var i = 0; i < count; i++) {
				var props = new VkPhysicalDeviceProperties ();
				vkGetPhysicalDeviceProperties (devices[i], &props);
				if (props.deviceType == 4 /* VK_PHYSICAL_DEVICE_TYPE_CPU */) {
					PhysicalDevice = devices[i];
					break;
				}
			}

			uint qfCount = 0;
			vkGetPhysicalDeviceQueueFamilyProperties (PhysicalDevice, ref qfCount, null);
			if (qfCount == 0) { failure = "no Vulkan queue families"; return false; }
			var qfs = new VkQueueFamilyProperties[qfCount];
			fixed (VkQueueFamilyProperties* pqfs = qfs) {
				vkGetPhysicalDeviceQueueFamilyProperties (PhysicalDevice, ref qfCount, pqfs);
			}
			var qfIdx = uint.MaxValue;
			for (uint i = 0; i < qfCount; i++) {
				if ((qfs[i].queueFlags & 1u /* VK_QUEUE_GRAPHICS_BIT */) != 0) {
					qfIdx = i;
					break;
				}
			}
			if (qfIdx == uint.MaxValue) { failure = "no graphics-capable queue family"; return false; }
			QueueFamilyIndex = qfIdx;

			float prio = 1.0f;
			var qci = new VkDeviceQueueCreateInfo {
				sType = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO,
				queueFamilyIndex = QueueFamilyIndex,
				queueCount = 1,
				pQueuePriorities = &prio,
			};
			var dci = new VkDeviceCreateInfo {
				sType = VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO,
				queueCreateInfoCount = 1,
				pQueueCreateInfos = &qci,
			};
			var rc = vkCreateDevice (PhysicalDevice, &dci, IntPtr.Zero, out var device);
			if (rc != 0) { failure = $"vkCreateDevice returned {rc}"; return false; }
			Device = device;
			vkGetDeviceQueue (device, QueueFamilyIndex, 0, out var queue);
			Queue = queue;
			return true;
		}

		// Minimal Vulkan 1.3 P/Invoke surface — same shape as LavapipeFixture had.

		public  const uint VK_API_VERSION_1_3                          = (1u << 22) | (3u << 12);
		private const uint VK_STRUCTURE_TYPE_APPLICATION_INFO          = 0;
		private const uint VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO      = 1;
		private const uint VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO  = 2;
		private const uint VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO        = 3;

		[StructLayout (LayoutKind.Sequential)]
		private struct VkApplicationInfo {
			public uint sType;
			public IntPtr pNext;
			public IntPtr pApplicationName;
			public uint applicationVersion;
			public IntPtr pEngineName;
			public uint engineVersion;
			public uint apiVersion;
		}

		[StructLayout (LayoutKind.Sequential)]
		private struct VkInstanceCreateInfo {
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
		private struct VkPhysicalDeviceProperties {
			public uint apiVersion, driverVersion, vendorID, deviceID;
			public int  deviceType;
			// remaining fields padded
		}

		[StructLayout (LayoutKind.Sequential)]
		private struct VkQueueFamilyProperties {
			public uint queueFlags, queueCount, timestampValidBits;
			public VkExtent3D minImageTransferGranularity;
		}

		[StructLayout (LayoutKind.Sequential)]
		private struct VkExtent3D {
			public uint width, height, depth;
		}

		[StructLayout (LayoutKind.Sequential)]
		private struct VkDeviceQueueCreateInfo {
			public uint sType;
			public IntPtr pNext;
			public uint flags;
			public uint queueFamilyIndex;
			public uint queueCount;
			public float* pQueuePriorities;
		}

		[StructLayout (LayoutKind.Sequential)]
		private struct VkDeviceCreateInfo {
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

		[DllImport (libvulkan)] private static extern int vkCreateInstance (VkInstanceCreateInfo* createInfo, IntPtr allocator, out IntPtr instance);
		[DllImport (libvulkan)] private static extern int vkEnumeratePhysicalDevices (IntPtr instance, ref uint count, IntPtr* devices);
		[DllImport (libvulkan)] private static extern void vkGetPhysicalDeviceProperties (IntPtr physicalDevice, VkPhysicalDeviceProperties* props);
		[DllImport (libvulkan)] private static extern void vkGetPhysicalDeviceQueueFamilyProperties (IntPtr physicalDevice, ref uint count, VkQueueFamilyProperties* props);
		[DllImport (libvulkan)] private static extern int vkCreateDevice (IntPtr physicalDevice, VkDeviceCreateInfo* createInfo, IntPtr allocator, out IntPtr device);
		[DllImport (libvulkan)] private static extern void vkGetDeviceQueue (IntPtr device, uint queueFamilyIndex, uint queueIndex, out IntPtr queue);
		[DllImport (libvulkan, CharSet = CharSet.Ansi)] private static extern IntPtr vkGetInstanceProcAddr (IntPtr instance, string name);
		[DllImport (libvulkan, CharSet = CharSet.Ansi)] private static extern IntPtr vkGetDeviceProcAddr (IntPtr device, string name);
	}
}
