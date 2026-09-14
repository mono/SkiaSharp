using System;

namespace SkiaSharp.Resources
{
	/// <summary>An interface that lets rich-content modules defer loading of external resources (images, fonts, etc.) to embedding clients.</summary>
	/// <remarks><para>ResourceProvider is used by <see cref="T:SkiaSharp.Skottie.AnimationBuilder" /> to load external assets referenced by Lottie animations, such as images, fonts, and nested animations.</para><para>Several built-in implementations are available: <see cref="T:SkiaSharp.Resources.FileResourceProvider" /> for loading from the file system, <see cref="T:SkiaSharp.Resources.CachingResourceProvider" /> for caching loaded assets, and <see cref="T:SkiaSharp.Resources.DataUriResourceProvider" /> for loading embedded data URIs.</para></remarks>
	public abstract unsafe class ResourceProvider : SKObject, ISKReferenceCounted, ISKSkipObjectRegistration
	{
		internal ResourceProvider (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Loads a generic resource by name and returns it as data.</summary>
		/// <param name="resourceName">The name of the resource to load.</param>
		/// <returns>The resource data, or <see langword="null" /> if the resource could not be loaded.</returns>
		/// <remarks>This is equivalent to calling <see cref="M:SkiaSharp.Resources.ResourceProvider.Load(System.String,System.String)" /> with an empty path.</remarks>
		public SKData? Load (string resourceName) =>
			Load ("", resourceName);

		/// <summary>Loads a generic resource specified by path and name, and returns it as data.</summary>
		/// <param name="resourcePath">The path to the resource directory.</param>
		/// <param name="resourceName">The name of the resource to load.</param>
		/// <returns>The resource data, or <see langword="null" /> if the resource could not be loaded.</returns>
		/// <remarks>This method is typically used to load nested animations or other generic data assets.</remarks>
		public SKData? Load (string resourcePath, string resourceName)
		{
			var r = SKData.GetObject (ResourcesApi.skresources_resource_provider_load (Handle, resourcePath, resourceName));
			GC.KeepAlive (this);
			return r;
		}
	}

	/// <summary>A resource provider proxy that caches loaded image assets.</summary>
	/// <remarks><para>This provider wraps another <see cref="T:SkiaSharp.Resources.ResourceProvider" /> and caches loaded image assets to avoid repeated loading of the same resources.</para><para>Use this provider to improve performance when an animation references the same image assets multiple times.</para></remarks>
	public sealed class CachingResourceProvider : ResourceProvider
	{
		/// <summary>Creates a caching proxy around the specified resource provider.</summary>
		/// <param name="resourceProvider">The underlying resource provider to wrap with caching.</param>
		/// <remarks>Image assets loaded through this provider will be cached and reused on subsequent requests for the same resource.</remarks>
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

	/// <summary>A resource provider that can load resources from data URIs embedded in Lottie JSON files.</summary>
	/// <remarks><para>Data URIs allow embedding image and font data directly in the Lottie JSON file using base64 encoding. This provider parses and decodes such embedded resources.</para><para>Use the constructor with a fallback provider to handle both embedded data URIs and external resources.</para></remarks>
	public sealed class DataUriResourceProvider : ResourceProvider
	{
		/// <summary>Creates a data URI resource provider without a fallback provider.</summary>
		/// <param name="preDecode">If <see langword="true" />, images are decoded upfront at load time; if <see langword="false" />, images are decoded on-the-fly at rasterization time.</param>
		/// <remarks><para>This constructor creates a provider that only handles data URIs. Resources that are not embedded as data URIs will not be loaded.</para><para>Use the constructor with a fallback provider if you need to load both embedded and external resources.</para></remarks>
		public DataUriResourceProvider (bool preDecode = false)
			: this (null, preDecode)
		{
		}

		/// <summary>Creates a data URI resource provider with an optional fallback provider.</summary>
		/// <param name="fallbackProvider">A resource provider to use for resources that are not embedded as data URIs. Can be <see langword="null" />.</param>
		/// <param name="preDecode">If <see langword="true" />, images are decoded upfront at load time; if <see langword="false" />, images are decoded on-the-fly at rasterization time.</param>
		/// <remarks><para>Resources that are embedded as data URIs will be decoded directly. Resources that are not data URIs will be delegated to the fallback provider.</para><para>A common pattern is to use a <see cref="T:SkiaSharp.Resources.FileResourceProvider" /> as the fallback to load non-embedded resources from the file system.</para></remarks>
		public DataUriResourceProvider (ResourceProvider? fallbackProvider, bool preDecode = false)
			: base (Create (fallbackProvider, preDecode), true)
		{
			Referenced (this, fallbackProvider);
		}

		private static IntPtr Create (ResourceProvider? fallbackProvider, bool preDecode = false) =>
			ResourcesApi.skresources_data_uri_resource_provider_proxy_make (fallbackProvider?.Handle ?? IntPtr.Zero, preDecode);
	}

	/// <summary>A resource provider that loads resources from a directory on the file system.</summary>
	/// <remarks>Use this provider to load Lottie animation assets (images, fonts, nested animations) from a local directory.</remarks>
	public sealed class FileResourceProvider : ResourceProvider
	{
		/// <summary>Creates a new file resource provider with the specified base directory.</summary>
		/// <param name="baseDirectory">The base directory path from which to load resources.</param>
		/// <param name="preDecode">If <see langword="true" />, images are decoded upfront at load time; if <see langword="false" />, images are decoded on-the-fly at rasterization time.</param>
		/// <remarks><para>By default, images are decoded on-the-fly at rasterization time. Large images may cause jank as decoding is expensive and can thrash internal caches.</para><para>Set <paramref name="preDecode" /> to <see langword="true" /> to force-decode all images upfront, at the cost of potentially more RAM and slower animation build times.</para></remarks>
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
