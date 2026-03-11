using System;

namespace SkiaSharp.Resources
{
	public abstract unsafe class ResourceProvider : SKObject, ISKReferenceCounted, ISKSkipObjectRegistration
	{
		internal ResourceProvider (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

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
		[Obsolete ("Use the overload that takes an ImageDecodeStrategy instead.")]
		public DataUriResourceProvider (bool preDecode)
			: this (null, preDecode ? ImageDecodeStrategy.PreDecode : ImageDecodeStrategy.LazyDecode)
		{
		}

		[Obsolete ("Use the overload that takes an ImageDecodeStrategy instead.")]
		public DataUriResourceProvider (ResourceProvider? fallbackProvider, bool preDecode)
			: this (fallbackProvider, preDecode ? ImageDecodeStrategy.PreDecode : ImageDecodeStrategy.LazyDecode)
		{
		}

		public DataUriResourceProvider (ImageDecodeStrategy strategy = ImageDecodeStrategy.LazyDecode)
			: this (null, strategy)
		{
		}

		public DataUriResourceProvider (ResourceProvider? fallbackProvider, ImageDecodeStrategy strategy = ImageDecodeStrategy.LazyDecode)
			: base (Create (fallbackProvider, strategy), true)
		{
			Referenced (this, fallbackProvider);
		}

		private static IntPtr Create (ResourceProvider? fallbackProvider, ImageDecodeStrategy strategy) =>
			ResourcesApi.skresources_data_uri_resource_provider_proxy_make2 (fallbackProvider?.Handle ?? IntPtr.Zero, strategy);
	}

	public sealed class FileResourceProvider : ResourceProvider
	{
		[Obsolete ("Use the overload that takes an ImageDecodeStrategy instead.")]
		public FileResourceProvider (string baseDirectory, bool preDecode)
			: this (baseDirectory, preDecode ? ImageDecodeStrategy.PreDecode : ImageDecodeStrategy.LazyDecode)
		{
		}

		public FileResourceProvider (string baseDirectory, ImageDecodeStrategy strategy = ImageDecodeStrategy.LazyDecode)
			: base (Create (baseDirectory, strategy), true)
		{
		}

		private static IntPtr Create (string baseDirectory, ImageDecodeStrategy strategy)
		{
			using var baseDir = new SKString(baseDirectory ?? throw new ArgumentNullException (nameof (baseDirectory)));
			return ResourcesApi.skresources_file_resource_provider_make2 (baseDir.Handle, strategy);
		}
	}
}
