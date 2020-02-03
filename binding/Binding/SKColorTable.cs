using System;
using System.ComponentModel;

namespace SkiaSharp
{
	[EditorBrowsable (EditorBrowsableState.Never)]
	[Obsolete ("The Index8 color type and color table is no longer supported.")]
	public unsafe class SKColorTable : SKObject
	{
		public const int MaxLength = 256;

		[Preserve]
		internal SKColorTable (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		public SKColorTable ()
			: this (new SKPMColor[MaxLength])
		{
		}

		public SKColorTable (int count)
			: this (new SKPMColor[count])
		{
		}

		public SKColorTable (SKColor[] colors)
			: this (colors, colors.Length)
		{
		}

		public SKColorTable (SKColor[] colors, int count)
			: this (SKPMColor.PreMultiply (colors), count)
		{
		}

		public SKColorTable (SKPMColor[] colors)
			: this (colors, colors.Length)
		{
		}

		public SKColorTable (SKPMColor[] colors, int count)
			: this (CreateNew (colors, count), true)
		{
			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKColorTable instance.");
		}

		private static IntPtr CreateNew (SKPMColor[] colors, int count)
		{
			fixed (SKPMColor* c = colors) {
				return SkiaApi.sk_colortable_new ((uint*)c, count);
			}
		}

		public int Count =>
			SkiaApi.sk_colortable_count (Handle);

		public SKPMColor[] Colors {
			get {
				var count = SkiaApi.sk_colortable_count (Handle);
				if (count == 0)
					return new SKPMColor[0];

				uint* colors;
				SkiaApi.sk_colortable_read_colors (Handle, &colors);
				if (colors == null)
					return new SKPMColor[0];

				return new ReadOnlySpan<SKPMColor> (colors, count).ToArray ();
			}
		}

		public SKColor[] UnPreMultipledColors =>
			SKPMColor.UnPreMultiply (Colors);

		public SKPMColor this[int index] {
			get {
				var count = SkiaApi.sk_colortable_count (Handle);
				if (index < 0 || index >= count)
					throw new ArgumentOutOfRangeException (nameof (index));

				uint* colors;
				SkiaApi.sk_colortable_read_colors (Handle, &colors);
				var span = new ReadOnlySpan<uint> (colors, count);

				return span[index];
			}
		}

		public SKColor GetUnPreMultipliedColor (int index) =>
			SKPMColor.UnPreMultiply (this[index]);

		public IntPtr ReadColors ()
		{
			uint* colors;
			SkiaApi.sk_colortable_read_colors (Handle, &colors);
			return (IntPtr)colors;
		}
	}
}
