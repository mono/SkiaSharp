using CoreGraphics;
using CoreImage;

#if __MACOS__
using UIColor = AppKit.NSColor;
#else
using UIKit;
#endif

#if __IOS__
namespace SkiaSharp.Views.iOS
#elif __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __MACOS__
namespace SkiaSharp.Views.Mac
#endif
{
	public static class AppleExtensions
	{
		// CGPoint

		public static SKPoint ToSKPoint(this CGPoint point)
		{
			return new SKPoint((float)point.X, (float)point.Y);
		}

		public static CGPoint ToPoint(this SKPoint point)
		{
			return new CGPoint(point.X, point.Y);
		}

		// CGRect

		public static SKRect ToSKRect(this CGRect rect)
		{
			return new SKRect((float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom);
		}

		public static CGRect ToRect(this SKRect rect)
		{
			return new CGRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		// CGSize

		public static SKSize ToSKSize(this CGSize size)
		{
			return new SKSize((float)size.Width, (float)size.Height);
		}

		public static CGSize ToSize(this SKSize size)
		{
			return new CGSize(size.Width, size.Height);
		}

		// CGColor

		public static SKColor ToSKColor(this CGColor color)
		{
			return UIColor.FromCGColor(color).ToSKColor();
		}

		public static CGColor ToCGColor(this SKColor color)
		{
#if __TVOS__
			// see https://bugzilla.xamarin.com/show_bug.cgi?id=44507
			return UIColor.FromRGBA(color.Red, color.Green, color.Blue, color.Alpha).CGColor;
#else
			return new CGColor(color.Red / 255f, color.Green / 255f, color.Blue / 255f, color.Alpha / 255f);
#endif
		}

		// CIColor

		public static SKColor ToSKColor(this CIColor color)
		{
			return UIColor.FromCIColor(color).ToSKColor();
		}

		public static CIColor ToCIColor(this SKColor color)
		{
			return new CIColor(color.Red / 255f, color.Green / 255f, color.Blue / 255f, color.Alpha / 255f);
		}
	}
}
