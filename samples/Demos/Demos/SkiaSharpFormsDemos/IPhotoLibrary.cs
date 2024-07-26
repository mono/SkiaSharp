using System;
using System.IO;
using System.Threading.Tasks;

namespace SkiaSharpFormsDemos
{
    public interface IPhotoLibrary
    {
        Task<Stream> PickPhotoAsync();

        Task<bool> SavePhotoAsync(byte[] data, string folder, string filename);
    }
}
