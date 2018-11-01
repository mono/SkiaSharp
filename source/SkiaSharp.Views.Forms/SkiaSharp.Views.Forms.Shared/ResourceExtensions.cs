using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

#if __ANDROID__
using Xamarin.Forms.Platform.Android;
using Application = global::Android.App.Application;
#elif __IOS__ || __MACOS__
using Foundation;
#elif __TIZEN__
using Xamarin.Forms.Platform.Tizen;
#elif WINDOWS_UWP
using Windows.ApplicationModel;
#endif

namespace SkiaSharp.Views.Forms
{
	public static class ResourceExtensions
	{
		public static async Task<SKData> LoadImageDataAsync(string resource)
		{
			if (string.IsNullOrEmpty(resource))
				throw new ArgumentNullException(nameof(resource));

			resource = NormalizePath(resource);

			if (File.Exists(resource))
			{
				return SKData.Create(resource);
			}
			else
			{
#if __ANDROID__
				if ((await Application.Context.Assets.ListAsync(resource)).Length > 0)
				{
					using (var fd = Application.Context.Assets.OpenFd(resource))
					{
						if (fd != null)
						{
							using (var stream = fd.CreateInputStream())
							{
								return SKData.Create(stream, fd.Length);
							}
						}
					}
				}

				var id = ResourceManager.GetDrawableByName(resource);
				if (id != 0)
				{
					using (var fd = Application.Context.Resources.OpenRawResourceFd(id))
					{
						if (fd != null)
						{
							using (var stream = fd.CreateInputStream())
							{
								return SKData.Create(stream, fd.Length);
							}
						}
					}
				}
#elif __IOS__ || __MACOS__
				resource = NSBundle.MainBundle.PathForResource(resource, null);
				if (string.IsNullOrEmpty(resource))
					return null;

				return SKData.Create(resource);
#elif __TIZEN__
				resource = ResourcePath.GetPath(resource);
				if (string.IsNullOrEmpty(resource))
					return null;

				return SKData.Create(resource);
#elif WINDOWS_UWP
				using (var stream = await Package.Current.InstalledLocation.OpenStreamForReadAsync(resource))
				{
					return SKData.Create(stream);
				}
#elif NETSTANDARD
#else
#error Missing platform logic
#endif

				return null;
			}
		}

		public static async Task<SKImage> DecodeImageAsync(string resource)
		{
			using (var data = await LoadImageDataAsync(resource))
			{
				if (data == null)
					return null;

				return SKImage.FromEncodedData(data);
			}
		}

		public static async Task<SKBitmap> DecodeBitmapAsync(string resource)
		{
			if (string.IsNullOrEmpty(resource))
				throw new ArgumentNullException(nameof(resource));

			resource = NormalizePath(resource);

			if (File.Exists(resource))
			{
				return SKBitmap.Decode(resource);
			}
			else
			{
#if __ANDROID__
				if ((await Application.Context.Assets.ListAsync(resource)).Length > 0)
				{
					using (var stream = Application.Context.Assets.Open(resource))
					{
						if (stream != null)
						{
							return SKBitmap.Decode(stream);
						}
					}
				}

				var id = ResourceManager.GetDrawableByName(resource);
				if (id != 0)
				{
					using (var stream = Application.Context.Resources.OpenRawResource(id))
					{
						return SKBitmap.Decode(stream);
					}
				}
#elif __IOS__ || __MACOS__
				resource = NSBundle.MainBundle.PathForResource(resource, null);
				if (string.IsNullOrEmpty(resource))
					return null;

				return SKBitmap.Decode(resource);
#elif __TIZEN__
				resource = ResourcePath.GetPath(resource);
				if (string.IsNullOrEmpty(resource))
					return null;

				return SKBitmap.Decode(resource);
#elif WINDOWS_UWP
				resource = resource.Replace('/', Path.DirectorySeparatorChar);
				using (var stream = await Package.Current.InstalledLocation.OpenStreamForReadAsync(resource))
				{
					return SKBitmap.Decode(stream);
				}
#elif NETSTANDARD
#else
#error Missing platform logic
#endif

				return null;
			}
		}

		private static string NormalizePath(string path) =>
#if WINDOWS_UWP
			path.Replace('/', Path.DirectorySeparatorChar);
#else
			path.Replace('\\', Path.DirectorySeparatorChar);
#endif
	}
}
