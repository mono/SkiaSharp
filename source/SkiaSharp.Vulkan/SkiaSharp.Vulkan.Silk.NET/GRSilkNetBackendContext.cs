using System;
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;

namespace SkiaSharp
{
	/// <summary>Represents a callback that resolves Vulkan procedure addresses from Silk.NET handles.</summary>
	/// <param name="name">The Vulkan procedure name.</param>
	/// <param name="instance">The Vulkan instance.</param>
	/// <param name="device">The Vulkan device.</param>
	/// <returns>The address of the Vulkan procedure.</returns>
	public delegate IntPtr GRSilkNetGetProcedureAddressDelegate(string name, Instance instance, Device device);

	/// <summary>Represents a Vulkan backend context that uses Silk.NET Vulkan handles.</summary>
	/// <remarks />
	public class GRSilkNetBackendContext : GRVkBackendContext
	{
		private Instance vkInstance;
		private PhysicalDevice vkPhysicalDevice;
		private Device vkDevice;
		private Queue vkQueue;
		private PhysicalDeviceFeatures? vkPhysicalDeviceFeatures;
		private GRSilkNetGetProcedureAddressDelegate getProc;

		private GCHandle devFeaturesHandle;

		/// <summary>Releases the resources used by this backend context.</summary>
		/// <param name="disposing"><see langword="true" /> to release managed resources; otherwise, <see langword="false" />.</param>
		/// <remarks />
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

		/// <summary>Gets or sets the Silk.NET Vulkan instance.</summary>
		/// <value>The Vulkan instance.</value>
		/// <remarks />
		public new Instance VkInstance
		{
			get => vkInstance;
			set
			{
				vkInstance = value;
				base.VkInstance = value.Handle;
			}
		}

		/// <summary>Gets or sets the Silk.NET Vulkan physical device.</summary>
		/// <value>The Vulkan physical device.</value>
		/// <remarks />
		public new PhysicalDevice VkPhysicalDevice
		{
			get => vkPhysicalDevice;
			set
			{
				vkPhysicalDevice = value;
				base.VkPhysicalDevice = value.Handle;
			}
		}

		/// <summary>Gets or sets the Silk.NET Vulkan logical device.</summary>
		/// <value>The Vulkan logical device.</value>
		/// <remarks />
		public new Device VkDevice
		{
			get => vkDevice;
			set
			{
				vkDevice = value;
				base.VkDevice = value.Handle;
			}
		}

		/// <summary>Gets or sets the Silk.NET Vulkan graphics queue.</summary>
		/// <value>The Vulkan graphics queue.</value>
		/// <remarks />
		public new Queue VkQueue
		{
			get => vkQueue;
			set
			{
				vkQueue = value;
				base.VkQueue = value.Handle;
			}
		}

		/// <summary>Gets or sets the Silk.NET Vulkan physical device features.</summary>
		/// <value>The physical device features, or <see langword="null" />.</value>
		/// <remarks />
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

		/// <summary>Gets or sets the callback that resolves Vulkan procedure addresses.</summary>
		/// <value>The Vulkan procedure address callback.</value>
		/// <remarks />
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
