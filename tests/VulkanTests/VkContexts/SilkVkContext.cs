using System;
using System.Text;
using Silk.NET.Vulkan;

namespace SkiaSharp.Tests
{
	/// <summary>
	/// Headless Silk.NET Vulkan bring-up: <c>Instance</c> → <c>PhysicalDevice</c>
	/// → graphics <c>Queue</c> → <c>Device</c>, with no <c>VK_KHR_surface</c> /
	/// swapchain and no window — exactly the inputs
	/// <see cref="GRContext.CreateVulkan"/> needs to render to an offscreen
	/// <see cref="SKSurface"/>.
	///
	/// <para>
	/// Because Silk.NET loads the OS-provided Vulkan loader itself
	/// (<c>vulkan-1.dll</c> / <c>libvulkan.so</c> / <c>libvulkan.dylib</c> /
	/// <c>libMoltenVK.dylib</c>), the same file runs on the Linux and Windows
	/// desktop hosts and — with a software ICD such as Mesa Lavapipe — inside a
	/// container. It is the Silk.NET analogue of the SharpVk <c>Win32VkContext</c>,
	/// minus the window/surface.
	/// </para>
	/// </summary>
	public sealed unsafe class SilkVkContext : IDisposable
	{
		// Vulkan 1.1 — the API level Skia targets; also handed to GRVkBackendContext.MaxAPIVersion.
		public const uint ApiVersion = (1u << 22) | (1u << 12);

		private readonly Vk vk;

		public Instance Instance { get; }

		public PhysicalDevice PhysicalDevice { get; }

		public Device Device { get; }

		public Queue GraphicsQueue { get; }

		public uint GraphicsFamily { get; }

		public PhysicalDeviceFeatures Features { get; }

		/// <summary>Silk.NET-typed proc lookup, for <see cref="GRSilkNetBackendContext"/>.</summary>
		public GRSilkNetGetProcedureAddressDelegate GetProc { get; }

		/// <summary>Raw-handle proc lookup, for the binding-neutral <see cref="GRVkBackendContext"/>.</summary>
		public GRVkGetProcedureAddressDelegate BaseGetProc { get; }

		public SilkVkContext()
		{
			vk = Vk.GetApi();

			var appInfo = new ApplicationInfo
			{
				SType = StructureType.ApplicationInfo,
				ApiVersion = Vk.Version11,
			};
			var instanceInfo = new InstanceCreateInfo
			{
				SType = StructureType.InstanceCreateInfo,
				PApplicationInfo = &appInfo,
			};
			Instance instance;
			if (vk.CreateInstance(&instanceInfo, null, &instance) != Result.Success)
				throw new InvalidOperationException("vkCreateInstance failed.");
			Instance = instance;

			uint deviceCount = 0;
			vk.EnumeratePhysicalDevices(instance, &deviceCount, null);
			if (deviceCount == 0)
				throw new InvalidOperationException("No Vulkan physical device (no driver or software ICD installed).");
			var physicalDevices = stackalloc PhysicalDevice[(int)deviceCount];
			vk.EnumeratePhysicalDevices(instance, &deviceCount, physicalDevices);
			PhysicalDevice = physicalDevices[0];

			uint familyCount = 0;
			vk.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, &familyCount, null);
			var families = stackalloc QueueFamilyProperties[(int)familyCount];
			vk.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, &familyCount, families);
			var graphicsFamily = uint.MaxValue;
			for (uint i = 0; i < familyCount; i++)
			{
				if ((families[i].QueueFlags & QueueFlags.GraphicsBit) != 0)
				{
					graphicsFamily = i;
					break;
				}
			}
			if (graphicsFamily == uint.MaxValue)
				throw new InvalidOperationException("This Vulkan device exposes no graphics queue family.");
			GraphicsFamily = graphicsFamily;

			PhysicalDeviceFeatures features;
			vk.GetPhysicalDeviceFeatures(PhysicalDevice, &features);
			Features = features;

			var priority = 1f;
			var queueInfo = new DeviceQueueCreateInfo
			{
				SType = StructureType.DeviceQueueCreateInfo,
				QueueFamilyIndex = graphicsFamily,
				QueueCount = 1,
				PQueuePriorities = &priority,
			};
			var deviceInfo = new DeviceCreateInfo
			{
				SType = StructureType.DeviceCreateInfo,
				QueueCreateInfoCount = 1,
				PQueueCreateInfos = &queueInfo,
			};
			Device device;
			if (vk.CreateDevice(PhysicalDevice, &deviceInfo, null, &device) != Result.Success)
				throw new InvalidOperationException("vkCreateDevice failed.");
			Device = device;

			Queue queue;
			vk.GetDeviceQueue(device, graphicsFamily, 0, &queue);
			GraphicsQueue = queue;

			var localVk = vk;
			GetProc = (name, instanceHandle, deviceHandle) =>
			{
				var bytes = Encoding.ASCII.GetBytes(name + "\0");
				fixed (byte* pName = bytes)
				{
					if (deviceHandle.Handle != 0)
						return localVk.GetDeviceProcAddr(deviceHandle, pName);
					return localVk.GetInstanceProcAddr(instanceHandle, pName);
				}
			};

			BaseGetProc = (name, instanceHandle, deviceHandle) =>
			{
				var bytes = Encoding.ASCII.GetBytes(name + "\0");
				fixed (byte* pName = bytes)
				{
					if (deviceHandle != IntPtr.Zero)
						return localVk.GetDeviceProcAddr(new Device(deviceHandle), pName);
					return localVk.GetInstanceProcAddr(new Instance(instanceHandle), pName);
				}
			};
		}

		public void Dispose()
		{
			if (Device.Handle != 0)
				vk.DestroyDevice(Device, null);
			if (Instance.Handle != 0)
				vk.DestroyInstance(Instance, null);
			vk.Dispose();
		}
	}
}
