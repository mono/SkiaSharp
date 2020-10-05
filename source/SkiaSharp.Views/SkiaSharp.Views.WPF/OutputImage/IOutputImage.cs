using System;
using System.Windows.Media;
using SkiaSharp.Views.WPF.OutputImage;

namespace SkiaSharp.Views.WPF
{
	internal interface IOutputImage
	{
		ImageSource Source { get; }

		public SizeWithDpi Size { get; }

		void TryResize(SizeWithDpi size);

		bool TryLock();
		void Unlock();

		SKSurface CreateSurface(FallbackContext context);
	}
}
