#nullable enable

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp.Views.Android
{
	internal enum VkResult : int
	{
		Success = 0,
		NotReady = 1,
		Timeout = 2,
		SuboptimalKhr = 1000001003,
		ErrorDeviceLost = -4,
		ErrorOutOfDateKhr = -1000001004,
	}

	internal enum VkStructureType : int
	{
		ApplicationInfo = 0,
		InstanceCreateInfo = 1,
		DeviceQueueCreateInfo = 2,
		DeviceCreateInfo = 3,
		FenceCreateInfo = 8,
		SemaphoreCreateInfo = 9,
		SwapchainCreateInfoKhr = 1000001000,
		PresentInfoKhr = 1000001001,
		AndroidSurfaceCreateInfoKhr = 1000008000,
	}

	internal enum VkFormat : int
	{
		Undefined = 0,
		R8G8B8A8Unorm = 37,
		B8G8R8A8Unorm = 44,
	}

	internal enum VkImageLayout : int
	{
		Undefined = 0,
		PresentSrcKhr = 1000001002,
	}

	internal enum VkImageTiling : int
	{
		Optimal = 0,
	}

	[Flags]
	internal enum VkImageUsageFlags : uint
	{
		TransferSrcBit = 0x00000001,
		TransferDstBit = 0x00000002,
		SampledBit = 0x00000004,
		ColorAttachmentBit = 0x00000010,
		InputAttachmentBit = 0x00000080,
	}

	[Flags]
	internal enum VkImageAspectFlags : uint
	{
		ColorBit = 0x00000001,
	}

	internal enum VkSharingMode : int
	{
		Exclusive = 0,
	}

	[Flags]
	internal enum VkQueueFlags : uint
	{
		GraphicsBit = 0x00000001,
	}

	[Flags]
	internal enum VkCompositeAlphaFlagsKHR : uint
	{
		OpaqueBitKhr = 0x00000001,
		PreMultipliedBitKhr = 0x00000002,
		PostMultipliedBitKhr = 0x00000004,
		InheritBitKhr = 0x00000008,
	}

	internal enum VkPresentModeKHR : int
	{
		FifoKhr = 2,
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct VkExtent2D
	{
		public uint Width;
		public uint Height;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct VkExtent3D
	{
		public uint Width;
		public uint Height;
		public uint Depth;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct VkApplicationInfo
	{
		public VkStructureType SType;
		public void* PNext;
		public byte* PApplicationName;
		public uint ApplicationVersion;
		public byte* PEngineName;
		public uint EngineVersion;
		public uint ApiVersion;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct VkInstanceCreateInfo
	{
		public VkStructureType SType;
		public void* PNext;
		public uint Flags;
		public VkApplicationInfo* PApplicationInfo;
		public uint EnabledLayerCount;
		public byte** PpEnabledLayerNames;
		public uint EnabledExtensionCount;
		public byte** PpEnabledExtensionNames;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct VkQueueFamilyProperties
	{
		public VkQueueFlags QueueFlags;
		public uint QueueCount;
		public uint TimestampValidBits;
		public VkExtent3D MinImageTransferGranularity;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct VkDeviceQueueCreateInfo
	{
		public VkStructureType SType;
		public void* PNext;
		public uint Flags;
		public uint QueueFamilyIndex;
		public uint QueueCount;
		public float* PQueuePriorities;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct VkDeviceCreateInfo
	{
		public VkStructureType SType;
		public void* PNext;
		public uint Flags;
		public uint QueueCreateInfoCount;
		public VkDeviceQueueCreateInfo* PQueueCreateInfos;
		public uint EnabledLayerCount;
		public byte** PpEnabledLayerNames;
		public uint EnabledExtensionCount;
		public byte** PpEnabledExtensionNames;
		public void* PEnabledFeatures;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct VkAndroidSurfaceCreateInfoKHR
	{
		public VkStructureType SType;
		public void* PNext;
		public uint Flags;
		public IntPtr Window;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct VkSurfaceCapabilitiesKHR
	{
		public uint MinImageCount;
		public uint MaxImageCount;
		public VkExtent2D CurrentExtent;
		public VkExtent2D MinImageExtent;
		public VkExtent2D MaxImageExtent;
		public uint MaxImageArrayLayers;
		public uint SupportedTransforms;
		public uint CurrentTransform;
		public VkCompositeAlphaFlagsKHR SupportedCompositeAlpha;
		public VkImageUsageFlags SupportedUsageFlags;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct VkSurfaceFormatKHR
	{
		public VkFormat Format;
		public int ColorSpace;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct VkSwapchainCreateInfoKHR
	{
		public VkStructureType SType;
		public void* PNext;
		public uint Flags;
		public ulong Surface;
		public uint MinImageCount;
		public VkFormat ImageFormat;
		public int ImageColorSpace;
		public VkExtent2D ImageExtent;
		public uint ImageArrayLayers;
		public VkImageUsageFlags ImageUsage;
		public VkSharingMode ImageSharingMode;
		public uint QueueFamilyIndexCount;
		public uint* PQueueFamilyIndices;
		public uint PreTransform;
		public VkCompositeAlphaFlagsKHR CompositeAlpha;
		public VkPresentModeKHR PresentMode;
		public uint Clipped;
		public ulong OldSwapchain;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct VkSemaphoreCreateInfo
	{
		public VkStructureType SType;
		public void* PNext;
		public uint Flags;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct VkFenceCreateInfo
	{
		public VkStructureType SType;
		public void* PNext;
		public uint Flags;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct VkPresentInfoKHR
	{
		public VkStructureType SType;
		public void* PNext;
		public uint WaitSemaphoreCount;
		public ulong* PWaitSemaphores;
		public uint SwapchainCount;
		public ulong* PSwapchains;
		public uint* PImageIndices;
		public VkResult* PResults;
	}

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	internal unsafe delegate VkResult PFN_vkCreateAndroidSurfaceKHR(
		IntPtr instance,
		VkAndroidSurfaceCreateInfoKHR* pCreateInfo,
		void* pAllocator,
		ulong* pSurface);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	internal unsafe delegate void PFN_vkDestroySurfaceKHR(
		IntPtr instance,
		ulong surface,
		void* pAllocator);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	internal unsafe delegate VkResult PFN_vkGetPhysicalDeviceSurfaceSupportKHR(
		IntPtr physicalDevice,
		uint queueFamilyIndex,
		ulong surface,
		uint* pSupported);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	internal unsafe delegate VkResult PFN_vkGetPhysicalDeviceSurfaceCapabilitiesKHR(
		IntPtr physicalDevice,
		ulong surface,
		VkSurfaceCapabilitiesKHR* pSurfaceCapabilities);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	internal unsafe delegate VkResult PFN_vkGetPhysicalDeviceSurfaceFormatsKHR(
		IntPtr physicalDevice,
		ulong surface,
		uint* pSurfaceFormatCount,
		VkSurfaceFormatKHR* pSurfaceFormats);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	internal unsafe delegate VkResult PFN_vkCreateSwapchainKHR(
		IntPtr device,
		VkSwapchainCreateInfoKHR* pCreateInfo,
		void* pAllocator,
		ulong* pSwapchain);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	internal unsafe delegate void PFN_vkDestroySwapchainKHR(
		IntPtr device,
		ulong swapchain,
		void* pAllocator);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	internal unsafe delegate VkResult PFN_vkGetSwapchainImagesKHR(
		IntPtr device,
		ulong swapchain,
		uint* pSwapchainImageCount,
		ulong* pSwapchainImages);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	internal unsafe delegate VkResult PFN_vkAcquireNextImageKHR(
		IntPtr device,
		ulong swapchain,
		ulong timeout,
		ulong semaphore,
		ulong fence,
		uint* pImageIndex);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	internal unsafe delegate VkResult PFN_vkQueuePresentKHR(
		IntPtr queue,
		VkPresentInfoKHR* pPresentInfo);

	internal static unsafe class VulkanInterop
	{
		private const string libVulkan = "vulkan";
		private const string libAndroid = "android";

		public const uint Version11 = (1u << 22) | (1u << 12);
		public const uint True = 1;
		public const uint False = 0;

		[DllImport(libVulkan, CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr vkGetInstanceProcAddr(IntPtr instance, byte* pName);

		[DllImport(libVulkan, CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr vkGetDeviceProcAddr(IntPtr device, byte* pName);

		[DllImport(libVulkan, CallingConvention = CallingConvention.Winapi)]
		public static extern VkResult vkCreateInstance(VkInstanceCreateInfo* pCreateInfo, void* pAllocator, IntPtr* pInstance);

		[DllImport(libVulkan, CallingConvention = CallingConvention.Winapi)]
		public static extern void vkDestroyInstance(IntPtr instance, void* pAllocator);

		[DllImport(libVulkan, CallingConvention = CallingConvention.Winapi)]
		public static extern VkResult vkEnumeratePhysicalDevices(IntPtr instance, uint* pPhysicalDeviceCount, IntPtr* pPhysicalDevices);

		[DllImport(libVulkan, CallingConvention = CallingConvention.Winapi)]
		public static extern void vkGetPhysicalDeviceProperties(IntPtr physicalDevice, void* pProperties);

		[DllImport(libVulkan, CallingConvention = CallingConvention.Winapi)]
		public static extern void vkGetPhysicalDeviceQueueFamilyProperties(IntPtr physicalDevice, uint* pQueueFamilyPropertyCount, VkQueueFamilyProperties* pQueueFamilyProperties);

		[DllImport(libVulkan, CallingConvention = CallingConvention.Winapi)]
		public static extern VkResult vkCreateDevice(IntPtr physicalDevice, VkDeviceCreateInfo* pCreateInfo, void* pAllocator, IntPtr* pDevice);

		[DllImport(libVulkan, CallingConvention = CallingConvention.Winapi)]
		public static extern void vkDestroyDevice(IntPtr device, void* pAllocator);

		[DllImport(libVulkan, CallingConvention = CallingConvention.Winapi)]
		public static extern void vkGetDeviceQueue(IntPtr device, uint queueFamilyIndex, uint queueIndex, IntPtr* pQueue);

		[DllImport(libVulkan, CallingConvention = CallingConvention.Winapi)]
		public static extern VkResult vkDeviceWaitIdle(IntPtr device);

		[DllImport(libVulkan, CallingConvention = CallingConvention.Winapi)]
		public static extern VkResult vkCreateSemaphore(IntPtr device, VkSemaphoreCreateInfo* pCreateInfo, void* pAllocator, ulong* pSemaphore);

		[DllImport(libVulkan, CallingConvention = CallingConvention.Winapi)]
		public static extern void vkDestroySemaphore(IntPtr device, ulong semaphore, void* pAllocator);

		[DllImport(libVulkan, CallingConvention = CallingConvention.Winapi)]
		public static extern VkResult vkCreateFence(IntPtr device, VkFenceCreateInfo* pCreateInfo, void* pAllocator, ulong* pFence);

		[DllImport(libVulkan, CallingConvention = CallingConvention.Winapi)]
		public static extern void vkDestroyFence(IntPtr device, ulong fence, void* pAllocator);

		[DllImport(libVulkan, CallingConvention = CallingConvention.Winapi)]
		public static extern VkResult vkWaitForFences(IntPtr device, uint fenceCount, ulong* pFences, uint waitAll, ulong timeout);

		[DllImport(libAndroid)]
		public static extern IntPtr ANativeWindow_fromSurface(IntPtr env, IntPtr surface);

		[DllImport(libAndroid)]
		public static extern void ANativeWindow_release(IntPtr window);

		[DllImport(libAndroid)]
		public static extern int ANativeWindow_setBuffersGeometry(IntPtr window, int width, int height, int format);
	}
}
