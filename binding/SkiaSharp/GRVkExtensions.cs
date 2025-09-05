#nullable disable

using System;

namespace SkiaSharp
{
	public unsafe class GRVkExtensions : SKObject, ISKSkipObjectRegistration
	{
		internal GRVkExtensions (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		private GRVkExtensions ()
			: this (SkiaApi.gr_vk_extensions_new (), true)
		{
		}

		protected override void DisposeNative () =>
			SkiaApi.gr_vk_extensions_delete (Handle);

		/// <param name="extension"></param>
		/// <param name="minVersion"></param>
		public void HasExtension (string extension, int minVersion) =>
			SkiaApi.gr_vk_extensions_has_extension (Handle, extension, (uint)minVersion);

		/// <param name="getProc"></param>
		/// <param name="vkInstance"></param>
		/// <param name="vkPhysicalDevice"></param>
		public void Initialize (GRVkGetProcedureAddressDelegate getProc, IntPtr vkInstance, IntPtr vkPhysicalDevice) =>
			Initialize (getProc, vkInstance, vkPhysicalDevice, null, null);

		/// <param name="getProc"></param>
		/// <param name="vkInstance"></param>
		/// <param name="vkPhysicalDevice"></param>
		/// <param name="instanceExtensions"></param>
		/// <param name="deviceExtensions"></param>
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

		/// <param name="getProc"></param>
		/// <param name="vkInstance"></param>
		/// <param name="vkPhysicalDevice"></param>
		/// <param name="instanceExtensions"></param>
		/// <param name="deviceExtensions"></param>
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
