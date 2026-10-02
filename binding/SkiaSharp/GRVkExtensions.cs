#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>Represents a collection of Vulkan extensions for use with Skia's GPU backend.</summary>
	/// <remarks />
	public unsafe class GRVkExtensions : SKObject, ISKSkipObjectRegistration
	{
		internal GRVkExtensions (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.GRVkExtensions" /> class.</summary>
		/// <remarks />
		public GRVkExtensions ()
			: this (SkiaApi.gr_vk_extensions_new (), true)
		{
		}

		/// <summary>Releases the native resources associated with this object.</summary>
		/// <remarks />
		protected override void DisposeNative () =>
			SkiaApi.gr_vk_extensions_delete (Handle);

		/// <summary>Verifies that the specified Vulkan extension is available with at least the given version.</summary>
		/// <param name="extension">The name of the Vulkan extension to check.</param>
		/// <param name="minVersion">The minimum required version of the extension.</param>
		/// <remarks />
		public void HasExtension (string extension, int minVersion) =>
			SkiaApi.gr_vk_extensions_has_extension (Handle, extension, (uint)minVersion);

		/// <summary>Initializes the extensions object with the specified Vulkan instance and physical device.</summary>
		/// <param name="getProc">The delegate used to retrieve Vulkan procedure addresses.</param>
		/// <param name="vkInstance">The Vulkan instance handle.</param>
		/// <param name="vkPhysicalDevice">The Vulkan physical device handle.</param>
		/// <remarks />
		public void Initialize (GRVkGetProcedureAddressDelegate getProc, IntPtr vkInstance, IntPtr vkPhysicalDevice) =>
			Initialize (getProc, vkInstance, vkPhysicalDevice, null, null);

		/// <summary>Initializes the extensions object with the specified Vulkan objects and extensions.</summary>
		/// <param name="getProc">The delegate used to retrieve Vulkan procedure addresses.</param>
		/// <param name="vkInstance">The Vulkan instance handle.</param>
		/// <param name="vkPhysicalDevice">The Vulkan physical device handle.</param>
		/// <param name="instanceExtensions">The array of enabled Vulkan instance extension names.</param>
		/// <param name="deviceExtensions">The array of enabled Vulkan device extension names.</param>
		/// <remarks />
		public void Initialize (GRVkGetProcedureAddressDelegate getProc, IntPtr vkInstance, IntPtr vkPhysicalDevice, string[] instanceExtensions, string[] deviceExtensions)
		{
			DelegateProxies.Create (getProc, out var gch, out var ctx);
			try {
				var ie = instanceExtensions;
				var de = deviceExtensions;
				var proxy = getProc != null ? DelegateProxies.GRVkGetProcProxy : null;
				SkiaApi.gr_vk_extensions_init (Handle, proxy, (void*)ctx, vkInstance, vkPhysicalDevice, (uint)(ie?.Length ?? 0), ie, (uint)(de?.Length ?? 0), de);
			} finally {
				gch.Free ();
			}
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.GRVkExtensions" /> instance and initializes it with the specified Vulkan objects and extensions.</summary>
		/// <param name="getProc">The delegate used to retrieve Vulkan procedure addresses.</param>
		/// <param name="vkInstance">The Vulkan instance handle.</param>
		/// <param name="vkPhysicalDevice">The Vulkan physical device handle.</param>
		/// <param name="instanceExtensions">The array of enabled Vulkan instance extension names.</param>
		/// <param name="deviceExtensions">The array of enabled Vulkan device extension names.</param>
		/// <returns>A new <see cref="T:SkiaSharp.GRVkExtensions" /> instance.</returns>
		/// <remarks />
		public static GRVkExtensions Create (GRVkGetProcedureAddressDelegate getProc, IntPtr vkInstance, IntPtr vkPhysicalDevice, string[] instanceExtensions, string[] deviceExtensions)
		{
			var extensions = new GRVkExtensions ();
			extensions.Initialize (getProc, vkInstance, vkPhysicalDevice, instanceExtensions, deviceExtensions);
			return extensions;
		}

		internal static GRVkExtensions GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new GRVkExtensions (handle, true);
	}
}
