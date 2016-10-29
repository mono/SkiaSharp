using AppKit;

namespace SkiaSharp.Views.Mac
{
	public static class MacExtensions
	{
		// NSColor

		public static SKColor ToSKColor(this NSColor color)
		{
			System.nfloat r, g, b, a;
			color.GetRgba(out r, out g, out b, out a);
			return new SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255));
		}

		public static NSColor ToNSColor(this SKColor color)
		{
			return NSColor.FromRgba(color.Red, color.Green, color.Blue, color.Alpha);
		}
	}
}
