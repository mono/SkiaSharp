using System;
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;

namespace SkiaSharp
{
	public delegate IntPtr GRSilkNetGetProcedureAddressDelegate(string name, Instance instance, Device device);

	public class GRSilkNetBackendContext : GRVkBackendContext
	{
		private Instance vkInstance;
		private PhysicalDevice vkPhysicalDevice;
		private Device vkDevice;
		private Queue vkQueue;
		private PhysicalDeviceFeatures? vkPhysicalDeviceFeatures;
		private GRSilkNetGetProcedureAddressDelegate getProc;

		private GCHandle devFeaturesHandle;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (devFeaturesHandle.IsAllocated)
				{
					devFeaturesHandle.Free();
					devFeaturesHandle = default;
				}
			}
		}

		public new Instance VkInstance
		{
			get => vkInstance;
			set
			{
				vkInstance = value;
				base.VkInstance = value.Handle;
			}
		}

		public new PhysicalDevice VkPhysicalDevice
		{
			get => vkPhysicalDevice;
			set
			{
				vkPhysicalDevice = value;
				base.VkPhysicalDevice = value.Handle;
			}
		}

		public new Device VkDevice
		{
			get => vkDevice;
			set
			{
				vkDevice = value;
				base.VkDevice = value.Handle;
			}
		}

		public new Queue VkQueue
		{
			get => vkQueue;
			set
			{
				vkQueue = value;
				base.VkQueue = value.Handle;
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

				devFeaturesHandle = default;
				base.VkPhysicalDeviceFeatures = IntPtr.Zero;

				if (value is PhysicalDeviceFeatures feat)
				{
					// Silk.NET's PhysicalDeviceFeatures is already the native (blittable)
					// layout, so it can be pinned and handed straight to Skia.
					devFeaturesHandle = GCHandle.Alloc(feat, GCHandleType.Pinned);
					base.VkPhysicalDeviceFeatures = devFeaturesHandle.AddrOfPinnedObject();
				}
			}
		}

		public new GRSilkNetGetProcedureAddressDelegate GetProcedureAddress
		{
			get => getProc;
			set
			{
				getProc = value;

				base.GetProcedureAddress = null;

				if (value is GRSilkNetGetProcedureAddressDelegate del)
				{
					base.GetProcedureAddress = (name, instance, device) =>
					{
						if (instance != IntPtr.Zero && vkInstance.Handle != instance)
							throw new InvalidOperationException("Incorrect object for VkInstance.");
						if (device != IntPtr.Zero && vkDevice.Handle != device)
							throw new InvalidOperationException("Incorrect object for VkDevice.");

						var i = instance != IntPtr.Zero ? vkInstance : default;
						var d = device != IntPtr.Zero ? vkDevice : default;

						return del(name, i, d);
					};
				}
			}
		}
	}
}
