using System;
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;

namespace SkiaSharp
{
	/// <summary>Represents a method that retrieves Vulkan procedure addresses using Silk.NET types.</summary>
	/// <param name="name">The name of the Vulkan procedure to retrieve.</param>
	/// <param name="instance">The Silk.NET Vulkan instance, or its default value for global functions.</param>
	/// <param name="device">The Silk.NET Vulkan device, or its default value for instance-level functions.</param>
	/// <returns>A pointer to the requested Vulkan procedure, or <see cref="F:System.IntPtr.Zero" /> if it was not found.</returns>
	/// <remarks />
	public delegate IntPtr GRSilkNetGetProcedureAddressDelegate(string name, Instance instance, Device device);

	/// <summary>A Vulkan backend context that uses Silk.NET types for Vulkan interoperability.</summary>
	/// <remarks><format type="text/markdown"><![CDATA[
	/// ## Remarks
	///
	/// Use this type to pass Vulkan objects created with Silk.NET to <xref:SkiaSharp.GRContext.CreateVulkan(SkiaSharp.GRVkBackendContext)>. The typed properties update the corresponding native handles on the base Vulkan backend context. The context does not own the Vulkan instance, physical device, logical device, or queue.
	///
	/// ## Examples
	///
	/// ```csharp
	/// static GRContext CreateContext(
	///     Silk.NET.Vulkan.Instance instance,
	///     Silk.NET.Vulkan.PhysicalDevice physicalDevice,
	///     Silk.NET.Vulkan.Device device,
	///     Silk.NET.Vulkan.Queue queue,
	///     uint graphicsQueueIndex,
	///     Silk.NET.Vulkan.PhysicalDeviceFeatures features,
	///     GRSilkNetGetProcedureAddressDelegate getProc)
	/// {
	///     using var extensions = new GRVkExtensions();
	///     extensions.Initialize(getProc, instance, physicalDevice);
	///
	///     using var backendContext = new GRSilkNetBackendContext
	///     {
	///         VkInstance = instance,
	///         VkPhysicalDevice = physicalDevice,
	///         VkDevice = device,
	///         VkQueue = queue,
	///         GraphicsQueueIndex = graphicsQueueIndex,
	///         Extensions = extensions,
	///         GetProcedureAddress = getProc,
	///         VkPhysicalDeviceFeatures = features,
	///     };
	///
	///     return GRContext.CreateVulkan(backendContext)
	///         ?? throw new System.InvalidOperationException("Unable to create the Vulkan context.");
	/// }
	/// ```
	/// ]]></format></remarks>
	public class GRSilkNetBackendContext : GRVkBackendContext
	{
		private Instance vkInstance;
		private PhysicalDevice vkPhysicalDevice;
		private Device vkDevice;
		private Queue vkQueue;
		private PhysicalDeviceFeatures? vkPhysicalDeviceFeatures;
		private GRSilkNetGetProcedureAddressDelegate getProc;

		private GCHandle devFeaturesHandle;

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.GRSilkNetBackendContext" /> class.</summary>
		/// <remarks />
		public GRSilkNetBackendContext()
		{
		}

		/// <summary>Releases the unmanaged resources used by the object and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
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
		/// <value>The Silk.NET Vulkan instance.</value>
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

		/// <summary>Gets or sets the Vulkan physical device.</summary>
		/// <value>The Silk.NET Vulkan physical device.</value>
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

		/// <summary>Gets or sets the Vulkan logical device.</summary>
		/// <value>The Silk.NET Vulkan logical device.</value>
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

		/// <summary>Gets or sets the Vulkan queue.</summary>
		/// <value>The Silk.NET Vulkan queue.</value>
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

		/// <summary>Gets or sets the optional Vulkan physical device features.</summary>
		/// <value>The Silk.NET Vulkan physical device features, or <see langword="null" />.</value>
		/// <remarks>The assigned value is pinned until it is replaced or the backend context is disposed.</remarks>
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

		/// <summary>Gets or sets the delegate for resolving Vulkan function addresses.</summary>
		/// <value>The delegate for resolving Vulkan function addresses.</value>
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
