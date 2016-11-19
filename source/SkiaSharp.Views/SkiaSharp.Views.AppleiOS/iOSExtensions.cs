using UIKit;

#if __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __IOS__
namespace SkiaSharp.Views.iOS
#endif
{
	public static class iOSExtensions
	{
		// UIColor

		public static SKColor ToSKColor(this UIColor color)
		{
			System.nfloat r, g, b, a;
			color.GetRGBA(out r, out g, out b, out a);
			return new SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255));
		}

		public static UIColor ToUIColor(this SKColor color)
		{
			return UIColor.FromRGBA(color.Red, color.Green, color.Blue, color.Alpha);
		}
	}
}
