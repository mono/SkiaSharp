using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Storage;
using Windows.Storage.Streams;

using Xamarin.Forms;

// Accessing Pictures library requires "Pictures Library" capability in Package.appxmanifest

[assembly: Dependency(typeof(SpinPaintShared.UWP.SpinPaintDependencyService))]

namespace SpinPaintShared.UWP
{
    public class SpinPaintDependencyService : ISpinPaintDependencyService
    {
        public async Task<bool> SaveBitmap(byte[] bitmapData, string filename)
        {
            StorageFolder picturesFolder = KnownFolders.PicturesLibrary;
            StorageFolder spinPaintFolder = null;

            // Get the folder or create it if necessary
            try
            {
                spinPaintFolder = await picturesFolder.GetFolderAsync("SpinPaint");
            }
            catch
            { }

            if (spinPaintFolder == null)
            {
                try
                {
                    spinPaintFolder = await picturesFolder.CreateFolderAsync("SpinPaint");
                }
                catch
                {
                    return false;
                }
            }

            try
            { 
                // Create the file.
                StorageFile storageFile = await spinPaintFolder.CreateFileAsync(filename);

                // Convert byte[] to Windows buffer and write it out.
                IBuffer buffer = WindowsRuntimeBuffer.Create(bitmapData, 0, bitmapData.Length, bitmapData.Length);
                await FileIO.WriteBufferAsync(storageFile, buffer);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
