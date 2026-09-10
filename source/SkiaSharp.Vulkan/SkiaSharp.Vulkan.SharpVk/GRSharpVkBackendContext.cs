using System;
using System.Runtime.InteropServices;
using SharpVk;

using PhysicalDeviceFeaturesNative = SharpVk.Interop.PhysicalDeviceFeatures;

namespace SkiaSharp
{
	/// <param name="name">The name of the Vulkan procedure to retrieve.</param>
	/// <param name="instance">The SharpVk Vulkan instance, or <see langword="null" /> for global functions.</param>
	/// <param name="device">The SharpVk Vulkan device, or <see langword="null" /> for instance-level functions.</param>
	/// <summary>Represents a method that retrieves Vulkan procedure addresses using SharpVk types.</summary>
	/// <returns>A pointer to the requested Vulkan procedure, or <see cref="F:System.IntPtr.Zero" /> if not found.</returns>
	/// <remarks />
	public delegate IntPtr GRSharpVkGetProcedureAddressDelegate(string name, Instance instance, Device device);

	/// <summary>A Vulkan backend context that uses SharpVk types for Vulkan interoperability.</summary>
	/// <remarks />
	public unsafe class GRSharpVkBackendContext : GRVkBackendContext
	{
		private Instance vkInstance;
		private PhysicalDevice vkPhysicalDevice;
		private Device vkDevice;
		private Queue vkQueue;
		private PhysicalDeviceFeatures? vkPhysicalDeviceFeatures;
		private GRSharpVkGetProcedureAddressDelegate getProc;

		private PhysicalDeviceFeaturesNative devFeatures;
		private GCHandle devFeaturesHandle;

		/// <param name="disposing">
		///           <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <summary>Releases the unmanaged resources used by the object and optionally releases the managed resources.</summary>
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

		/// <summary>Gets or sets the Vulkan instance.</summary>
		/// <value>The SharpVk instance.</value>
		/// <remarks />
		public new Instance VkInstance
		{
			get => vkInstance;
			set
			{
				vkInstance = value;
				base.VkInstance = (IntPtr)vkInstance?.RawHandle.ToUInt64();
			}
		}

		/// <summary>Gets or sets the Vulkan physical device.</summary>
		/// <value>The SharpVk physical device.</value>
		/// <remarks />
		public new PhysicalDevice VkPhysicalDevice
		{
			get => vkPhysicalDevice;
			set
			{
				vkPhysicalDevice = value;
				base.VkPhysicalDevice = (IntPtr)vkPhysicalDevice?.RawHandle.ToUInt64();
			}
		}

		/// <summary>Gets or sets the Vulkan logical device.</summary>
		/// <value>The SharpVk device.</value>
		/// <remarks />
		public new Device VkDevice
		{
			get => vkDevice;
			set
			{
				vkDevice = value;
				base.VkDevice = (IntPtr)vkDevice?.RawHandle.ToUInt64();
			}
		}

		/// <summary>Gets or sets the Vulkan queue.</summary>
		/// <value>The SharpVk queue.</value>
		/// <remarks />
		public new Queue VkQueue
		{
			get => vkQueue;
			set
			{
				vkQueue = value;
				base.VkQueue = (IntPtr)vkQueue?.RawHandle.ToUInt64();
			}
		}

		/// <summary>Gets or sets the optional Vulkan physical device features.</summary>
		/// <value>The SharpVk physical device features, or <see langword="null" />.</value>
		/// <remarks />
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

		/// <summary>Gets or sets the delegate for resolving Vulkan function addresses.</summary>
		/// <value>The delegate for resolving Vulkan function addresses.</value>
		/// <remarks />
		public new GRSharpVkGetProcedureAddressDelegate GetProcedureAddress
		{
			get => getProc;
			set
			{
				getProc = value;

				base.GetProcedureAddress = null;

				if (value is GRSharpVkGetProcedureAddressDelegate del)
				{
					base.GetProcedureAddress = (name, instance, device) =>
					{
						if (instance != IntPtr.Zero && vkInstance?.RawHandle.ToUInt64() != (ulong)instance.ToInt64())
							throw new InvalidOperationException("Incorrect object for VkInstance.");
						if (device != IntPtr.Zero && vkDevice?.RawHandle.ToUInt64() != (ulong)device.ToInt64())
							throw new InvalidOperationException("Incorrect object for VkDevice.");

						var i = instance != IntPtr.Zero ? vkInstance : null;
						var d = device != IntPtr.Zero ? vkDevice : null;

						return del?.Invoke(name, i, d) ?? IntPtr.Zero;
					};
				}
			}
		}
	}
}
