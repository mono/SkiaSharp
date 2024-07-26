using System;
using System.IO;
using System.Threading.Tasks;

namespace SpinPaint
{
    public interface ISpinPaintDependencyService
    {
        Task<bool> SaveBitmap(byte[] bitmapData, string filename);
    }
}
