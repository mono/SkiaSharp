using System;

namespace SkiaSharp.Resources
{
	public abstract unsafe class ResourceProvider : SKObject, ISKSkipObjectRegistration
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
	}

	public sealed class CachingResourceProvider : ResourceProvider
	{
		public CachingResourceProvider (ResourceProvider resourceProvider)
			: base (Create (resourceProvider), true)
		{
			Referenced(this, resourceProvider);
		}

		private static IntPtr Create (ResourceProvider resourceProvider)
		{
			_ = resourceProvider ?? throw new ArgumentNullException (nameof (resourceProvider));
			return ResourcesApi.skresources_caching_resource_provider_proxy_make (resourceProvider.Handle);
		}
	}

	public sealed class DataUriResourceProvider : ResourceProvider
	{
		public DataUriResourceProvider (bool preDecode = false)
			: this (null, preDecode)
		{
		}

		public DataUriResourceProvider (ResourceProvider? fallbackProvider, bool preDecode = false)
			: base (Create (fallbackProvider, preDecode), true)
		{
			Referenced (this, fallbackProvider);
		}

		private static IntPtr Create (ResourceProvider? fallbackProvider, bool preDecode = false) =>
			ResourcesApi.skresources_data_uri_resource_provider_proxy_make (fallbackProvider?.Handle ?? IntPtr.Zero, preDecode);
	}

	public sealed class FileResourceProvider : ResourceProvider
	{
		public FileResourceProvider (string baseDirectory, bool preDecode = false)
			: base (Create (baseDirectory, preDecode), true)
		{
		}

		private static IntPtr Create (string baseDirectory, bool preDecode)
		{
			using var baseDir = new SKString(baseDirectory ?? throw new ArgumentNullException (nameof (baseDirectory)));
			return ResourcesApi.skresources_file_resource_provider_make (baseDir.Handle, preDecode);
		}
	}
}
