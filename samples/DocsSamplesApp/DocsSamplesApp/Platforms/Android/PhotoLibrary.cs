using System.Threading.Tasks;

using Android.Content;
using Android.Media;

using DocsSamplesApp.Droid;

using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Storage;

using AndroidEnvironment = Android.OS.Environment;
using JavaFile = Java.IO.File;
using JavaFileOutputStream = Java.IO.FileOutputStream;

[assembly: Dependency(typeof(PhotoLibrary))]

namespace DocsSamplesApp.Droid
{
    public class PhotoLibrary : IPhotoLibrary
    {
        public async Task<System.IO.Stream> PickPhotoAsync()
        {
            // Use MAUI's MediaPicker for cross-platform photo picking
            var result = await MediaPicker.Default.PickPhotoAsync();
            if (result != null)
            {
                return await result.OpenReadAsync();
            }
            return null;
        }

        // Saving photos requires android.permission.WRITE_EXTERNAL_STORAGE in AndroidManifest.xml
        public async Task<bool> SavePhotoAsync(byte[] data, string folder, string filename)
        {
            try
            {
                JavaFile picturesDirectory = AndroidEnvironment.GetExternalStoragePublicDirectory(AndroidEnvironment.DirectoryPictures);
                JavaFile folderDirectory = picturesDirectory;

                if (!string.IsNullOrEmpty(folder))
                {
                    folderDirectory = new JavaFile(picturesDirectory, folder);
                    folderDirectory.Mkdirs();
                }

                using (JavaFile bitmapFile = new JavaFile(folderDirectory, filename))
                {
                    bitmapFile.CreateNewFile();

                    using (JavaFileOutputStream outputStream = new JavaFileOutputStream(bitmapFile))
                    {
                        await outputStream.WriteAsync(data);
                    }

                    // Make sure it shows up in the Photos gallery promptly.
                    var context = Android.App.Application.Context;
                    MediaScannerConnection.ScanFile(context,
                                                    new string[] { bitmapFile.Path },
                                                    new string[] { "image/png", "image/jpeg" }, null);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}



