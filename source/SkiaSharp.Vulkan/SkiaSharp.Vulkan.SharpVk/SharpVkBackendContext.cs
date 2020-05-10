using System;
using System.Runtime.InteropServices;

using PhysicalDeviceFeaturesNative = SharpVk.Interop.PhysicalDeviceFeatures;

namespace SkiaSharp.Vulkan.SharpVk
{
	public unsafe class SharpVkBackendContext : GRVkBackendContext
	{
		private global::SharpVk.Instance vkInstance;
		private global::SharpVk.PhysicalDevice vkPhysicalDevice;
		private global::SharpVk.Device vkDevice;
		private global::SharpVk.Queue vkQueue;
		private global::SharpVk.PhysicalDeviceFeatures? vkPhysicalDeviceFeatures;

		private PhysicalDeviceFeaturesNative devFeatures;
		private GCHandle devFeaturesHandle;

		public SharpVkBackendContext()
		{
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (devFeaturesHandle.IsAllocated)
					devFeaturesHandle.Free();
			}
		}

		public new global::SharpVk.Instance VkInstance
		{
			get => vkInstance;
			set
			{
				vkInstance = value;
				base.VkInstance = (IntPtr)vkInstance?.RawHandle.ToUInt64();
			}
		}

		public new global::SharpVk.PhysicalDevice VkPhysicalDevice
		{
			get => vkPhysicalDevice;
			set
			{
				vkPhysicalDevice = value;
				base.VkPhysicalDevice = (IntPtr)vkPhysicalDevice?.RawHandle.ToUInt64();
			}
		}

		public new global::SharpVk.Device VkDevice
		{
			get => vkDevice;
			set
			{
				vkDevice = value;
				base.VkDevice = (IntPtr)vkDevice?.RawHandle.ToUInt64();
			}
		}

		public new global::SharpVk.Queue VkQueue
		{
			get => vkQueue;
			set
			{
				vkQueue = value;
				base.VkQueue = (IntPtr)vkQueue?.RawHandle.ToUInt64();
			}
		}

		public new global::SharpVk.PhysicalDeviceFeatures? VkPhysicalDeviceFeatures
		{
			get => vkPhysicalDeviceFeatures;
			set
			{
				vkPhysicalDeviceFeatures = value;

				if (devFeaturesHandle.IsAllocated)
					devFeaturesHandle.Free();

				devFeatures = default;
				devFeaturesHandle = default;
				base.VkPhysicalDeviceFeatures = IntPtr.Zero;

				if (value is global::SharpVk.PhysicalDeviceFeatures feat)
				{
					devFeatures = feat.ToNative();
					devFeaturesHandle = GCHandle.Alloc(devFeatures, GCHandleType.Pinned);
					base.VkPhysicalDeviceFeatures = devFeaturesHandle.AddrOfPinnedObject();
				}
			}
		}
	}
}
