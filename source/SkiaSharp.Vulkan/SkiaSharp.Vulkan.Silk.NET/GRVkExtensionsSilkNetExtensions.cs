using System;
using Silk.NET.Vulkan;

namespace SkiaSharp
{
	/// <summary>Provides extension methods for <see cref="T:SkiaSharp.GRVkExtensions" /> to support Silk.NET Vulkan types.</summary>
	/// <remarks />
	public static class GRVkExtensionsSilkNetExtensions
	{
		/// <summary>Initializes the extensions object with the specified Silk.NET Vulkan instance and physical device.</summary>
		/// <param name="extensions">The <see cref="T:SkiaSharp.GRVkExtensions" /> instance to initialize.</param>
		/// <param name="getProc">The delegate used to retrieve Vulkan procedure addresses.</param>
		/// <param name="instance">The Silk.NET Vulkan instance.</param>
		/// <param name="physicalDevice">The Silk.NET Vulkan physical device.</param>
		/// <remarks />
		public static void Initialize(this GRVkExtensions extensions, GRSilkNetGetProcedureAddressDelegate getProc, Instance instance, PhysicalDevice physicalDevice) =>
			extensions.Initialize(getProc, instance, physicalDevice, null, null);

		/// <summary>Initializes the extensions object with the specified Silk.NET Vulkan objects and extensions.</summary>
		/// <param name="extensions">The <see cref="T:SkiaSharp.GRVkExtensions" /> instance to initialize.</param>
		/// <param name="getProc">The delegate used to retrieve Vulkan procedure addresses.</param>
		/// <param name="instance">The Silk.NET Vulkan instance.</param>
		/// <param name="physicalDevice">The Silk.NET Vulkan physical device.</param>
		/// <param name="instanceExtensions">The array of enabled Vulkan instance extension names.</param>
		/// <param name="deviceExtensions">The array of enabled Vulkan device extension names.</param>
		/// <remarks />
		public static void Initialize(this GRVkExtensions extensions, GRSilkNetGetProcedureAddressDelegate getProc, Instance instance, PhysicalDevice physicalDevice, string[] instanceExtensions, string[] deviceExtensions) =>
			extensions.Initialize(ToBaseProc(getProc, instance), instance.Handle, physicalDevice.Handle, instanceExtensions, deviceExtensions);

		private static GRVkGetProcedureAddressDelegate ToBaseProc(GRSilkNetGetProcedureAddressDelegate getProc, Instance instance) =>
			(name, inst, _) =>
			{
				if (inst != IntPtr.Zero && instance.Handle != inst)
					throw new InvalidOperationException("Incorrect object for VkInstance.");

				return getProc?.Invoke(name, inst != IntPtr.Zero ? instance : default, default) ?? IntPtr.Zero;
			};
	}
}
