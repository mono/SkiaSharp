using System;

namespace SkiaSharpSample
{
	[Flags]
	public enum SampleCategories
	{
		Showcases = 1 << 0,

		General = 1 << 1,
		BitmapDecoding = 1 << 2,
		Text = 1 << 3,
		Paths = 1 << 4,
		Shaders = 1 << 5,
		Fonts = 1 << 6,
		MaskFilters = 1 << 7,
		ColorFilters = 1 << 8,
		ImageFilters = 1 << 9,
		PathEffects = 1 << 10,
		SVG = 1 << 11,
		Documents = 1 << 12,
	}
}
