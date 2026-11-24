using System;
using System.Linq;
using SharpVk;
using SharpVk.Khronos;

namespace SkiaSharp.Tests
{
	public sealed class Win32VkContext : VkContext
	{
		// Skia requires Vulkan 1.1 and acquires the core 1.1 device procs
		// unconditionally. vkGetDeviceProcAddr gates those on the instance's
		// apiVersion, so an instance created without an ApplicationInfo is 1.0 and
		// its device returns null for every one of them -- GRContext.CreateVulkan
		// is then null no matter what MaxAPIVersion says.
		private static readonly SharpVk.Version apiVersion = new SharpVk.Version(1, 1, 0);

		private static readonly Win32Window window = new Win32Window("Win32VkContext");

		public Win32VkContext()
		{
			ApiVersion = (uint)apiVersion;

			InstanceExtensions = new[] { "VK_KHR_surface", "VK_KHR_win32_surface" };
			Instance = Instance.Create(
				null,
				InstanceExtensions,
				applicationInfo: new ApplicationInfo { ApiVersion = apiVersion });

			PhysicalDevice = Instance.EnumeratePhysicalDevices().First();

			Surface = Instance.CreateWin32Surface(Kernel32.CurrentModuleHandle, window.WindowHandle);

			(GraphicsFamily, PresentFamily) = FindQueueFamilies();

			var queueInfos = new[]
			{
				new DeviceQueueCreateInfo { QueueFamilyIndex = GraphicsFamily, QueuePriorities = new[] { 1f } },
				new DeviceQueueCreateInfo { QueueFamilyIndex = PresentFamily, QueuePriorities = new[] { 1f } },
			};

			// Enabled on the device and handed to Skia below. The two have to agree:
			// Skia trusts this list rather than re-querying, so claiming a feature the
			// device never enabled would have it emit commands the driver rejects.
			Features = PhysicalDevice.GetFeatures();
			DeviceExtensions = Array.Empty<string>();

			Device = PhysicalDevice.CreateDevice(queueInfos, null, DeviceExtensions, null, Features);

			GraphicsQueue = Device.GetQueue(GraphicsFamily, 0);

			PresentQueue = Device.GetQueue(PresentFamily, 0);

			GetProc = (name, instanceHandle, deviceHandle) =>
			{
				if (deviceHandle != IntPtr.Zero)
					return Device.GetProcedureAddress(name);

				return Instance.GetProcedureAddress(name);
			};

			SharpVkGetProc = (name, instance, device) =>
			{
				if (device != null)
					return device.GetProcedureAddress(name);
				if (instance != null)
					return instance.GetProcedureAddress(name);

				// SharpVk includes the static functions on Instance, but this is not actually correct
				// since the functions are static, they are not tied to an instance. For example,
				// VkCreateInstance is not found on an instance, it is creating said instance.
				// Other libraries, such as VulkanCore, use another type to do this.
				return Instance.GetProcedureAddress(name);
			};
		}

		private (uint, uint) FindQueueFamilies()
		{
			var queueFamilyProperties = PhysicalDevice.GetQueueFamilyProperties();

			var graphicsFamily = queueFamilyProperties
				.Select((properties, index) => new { properties, index })
				.SkipWhile(pair => !pair.properties.QueueFlags.HasFlag(QueueFlags.Graphics))
				.FirstOrDefault();

			if (graphicsFamily == null)
				throw new Exception("Unable to find graphics queue");

			uint? presentFamily = default;

			for (uint i = 0; i < queueFamilyProperties.Length; ++i)
			{
				if (PhysicalDevice.GetSurfaceSupport(i, Surface))
					presentFamily = i;
			}

			if (!presentFamily.HasValue)
				throw new Exception("Unable to find present queue");

			return ((uint)graphicsFamily.index, presentFamily.Value);
		}
	}
}
