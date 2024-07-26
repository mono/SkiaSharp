using System;
using System.Threading.Tasks;
using Foundation;
using UIKit;

using Xamarin.Forms;

// Requires NSPhotoLibraryUsageDescription in Info.plist

[assembly: Dependency(typeof(SpinPaint.iOS.SpinPaintDependencyService))]

namespace SpinPaint.iOS
{
    public class SpinPaintDependencyService : ISpinPaintDependencyService
    {
        public Task<bool> SaveBitmap(byte[] bitmapData, string filename)
        {
            NSData data = NSData.FromArray(bitmapData);
            UIImage image = new UIImage(data);
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            image.SaveToPhotosAlbum((UIImage img, NSError error) =>
            {
                taskCompletionSource.SetResult(error == null);
            });

            return taskCompletionSource.Task; 
        }
    }
}