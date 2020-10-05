using System.Windows.Media;

namespace SkiaSharp.Views.WPF.OutputImage
{
	internal interface IOutputImage
	{
		ImageSource Source { get; }

		public SizeWithDpi Size { get; }

		void TryResize(SizeWithDpi size);

		bool TryLock();
		void Unlock();

		SKSurface CreateSurface(WaterfallContext context);
	}
}
