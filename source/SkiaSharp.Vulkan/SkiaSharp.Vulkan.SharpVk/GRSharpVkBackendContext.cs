using System;
using System.Runtime.InteropServices;
using SharpVk;

using PhysicalDeviceFeaturesNative = SharpVk.Interop.PhysicalDeviceFeatures;

namespace SkiaSharp
{
	public unsafe class GRSharpVkBackendContext : GRVkBackendContext
	{
		private Instance vkInstance;
		private PhysicalDevice vkPhysicalDevice;
		private Device vkDevice;
		private Queue vkQueue;
		private PhysicalDeviceFeatures? vkPhysicalDeviceFeatures;

		private PhysicalDeviceFeaturesNative devFeatures;
		private GCHandle devFeaturesHandle;

		public GRSharpVkBackendContext()
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

		public new Instance VkInstance
		{
			get => vkInstance;
			set
			{
				vkInstance = value;
				base.VkInstance = (IntPtr)vkInstance?.RawHandle.ToUInt64();
			}
		}

		public new PhysicalDevice VkPhysicalDevice
		{
			get => vkPhysicalDevice;
			set
			{
				vkPhysicalDevice = value;
				base.VkPhysicalDevice = (IntPtr)vkPhysicalDevice?.RawHandle.ToUInt64();
			}
		}

		public new Device VkDevice
		{
			get => vkDevice;
			set
			{
				vkDevice = value;
				base.VkDevice = (IntPtr)vkDevice?.RawHandle.ToUInt64();
			}
		}

		public new Queue VkQueue
		{
			get => vkQueue;
			set
			{
				vkQueue = value;
				base.VkQueue = (IntPtr)vkQueue?.RawHandle.ToUInt64();
			}
		}

		public new PhysicalDeviceFeatures? VkPhysicalDeviceFeatures
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

				if (value is PhysicalDeviceFeatures feat)
				{
					devFeatures = feat.ToNative();
					devFeaturesHandle = GCHandle.Alloc(devFeatures, GCHandleType.Pinned);
					base.VkPhysicalDeviceFeatures = devFeaturesHandle.AddrOfPinnedObject();
				}
			}
		}
	}
}
