using System;
using System.IO;

namespace SkiaSharp.Resources
{
	public unsafe class ResourceProvider : SKObject, ISKSkipObjectRegistration
	{
		internal ResourceProvider (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void DisposeNative ()
			=> ResourcesApi.skresources_resource_provider_delete (Handle);

		public SKData? Load (string resourceName) =>
			Load ("", resourceName);

		public SKData? Load (string resourcePath, string resourceName) =>
			SKData.GetObject (ResourcesApi.skresources_resource_provider_load (Handle, resourcePath, resourceName));

		public static ResourceProvider CreateCaching (ResourceProvider resourceProvider)
		{
			_ = resourceProvider ?? throw new ArgumentNullException (nameof (resourceProvider));
			return GetObject (ResourcesApi.skresources_caching_resource_provider_proxy_make (resourceProvider.Handle))!;
		}

		public static ResourceProvider CreateDataUri (ResourceProvider fallbackProvider, bool preDecode = false)
		{
			_ = fallbackProvider ?? throw new ArgumentNullException (nameof (fallbackProvider));
			return GetObject (ResourcesApi.skresources_data_uri_resource_provider_proxy_make (fallbackProvider.Handle, preDecode))!;
		}

		public static ResourceProvider CreateFile (string baseDirectory, bool preDecode = false)
		{
			using var baseDir = new SKString(baseDirectory ?? throw new ArgumentNullException (nameof (baseDirectory)));
			return GetObject (ResourcesApi.skresources_file_resource_provider_make (baseDir.Handle, preDecode))!;
		}
	
		internal static ResourceProvider? GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new ResourceProvider (handle, true);
	}
}
