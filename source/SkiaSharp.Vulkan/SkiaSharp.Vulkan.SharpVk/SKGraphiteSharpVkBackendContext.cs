using System;
using SharpVk;

namespace SkiaSharp
{
	/// <summary>
	/// Strongly-typed <see cref="SKGraphiteVkBackendContext"/> that accepts SharpVk handle
	/// objects (<see cref="Instance"/>, <see cref="PhysicalDevice"/>, <see cref="Device"/>,
	/// <see cref="Queue"/>) instead of raw <see cref="IntPtr"/>s. Setting a typed handle
	/// writes the corresponding raw handle through to the base class, so the resulting
	/// context can be built via <see cref="SKGraphiteContext.CreateVulkan(SKGraphiteVkBackendContext)"/>.
	/// This mirrors <see cref="GRSharpVkBackendContext"/> for the Graphite backend.
	/// </summary>
	public unsafe class SKGraphiteSharpVkBackendContext : SKGraphiteVkBackendContext
	{
		private Instance vkInstance;
		private PhysicalDevice vkPhysicalDevice;
		private Device vkDevice;
		private Queue vkQueue;
		private GRSharpVkGetProcedureAddressDelegate getProc;

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
