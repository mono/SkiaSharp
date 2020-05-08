using System;

namespace SkiaSharp
{
	public unsafe class GRVkExtensions : SKObject
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

		public void HasExtension (string extension, int minVersion) =>
			SkiaApi.gr_vk_extensions_has_extension (Handle, extension, (uint)minVersion);

		private void Init (GRVkGetProcDelegate getProc, IntPtr vkInstance, IntPtr vkPhysicalDevice, string[] instanceExtensions, string[] deviceExtensions)
		{
			var proxy = DelegateProxies.Create (getProc, DelegateProxies.GRVkGetProcDelegateProxy, out var gch, out var ctx);
			try {
				var ie = instanceExtensions;
				var de = deviceExtensions;
				SkiaApi.gr_vk_extensions_init (Handle, proxy, (void*)ctx, vkInstance, vkPhysicalDevice, (uint)(ie?.Length ?? 0), ie, (uint)(de?.Length ?? 0), de);
			} finally {
				gch.Free ();
			}
		}

		public static GRVkExtensions Create (GRVkGetProcDelegate getProc, IntPtr vkInstance, IntPtr vkPhysicalDevice, string[] instanceExtensions, string[] deviceExtensions)
		{
			var extensions = new GRVkExtensions ();
			extensions.Init (getProc, vkInstance, vkPhysicalDevice, instanceExtensions, deviceExtensions);
			return extensions;
		}

		internal static GRVkExtensions GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new GRVkExtensions (h, o));
	}
}
