using System;
using System.ComponentModel;

namespace SkiaSharp
{
	[EditorBrowsable (EditorBrowsableState.Never)]
	[Obsolete ("The Index8 color type and color table is no longer supported.")]
	public unsafe class SKColorTable : SKObject, ISKReferenceCounted, ISKSkipObjectRegistration
	{
		public const int MaxLength = 256;

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
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKColorTable instance.");
			}
		}

		private static IntPtr CreateNew (SKPMColor[] colors, int count)
		{
			fixed (SKPMColor* c = colors) {
				return SkiaApi.sk_colortable_new ((uint*)c, count);
			}
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		public int Count => SkiaApi.sk_colortable_count (Handle);

		public SKPMColor[] Colors {
			get {
				var count = Count;
				var pointer = ReadColors ();

				if (count == 0 || pointer == IntPtr.Zero) {
					return new SKPMColor[0];
				}

				return PtrToStructureArray<SKPMColor> (pointer, count);
			}
		}

		public SKColor[] UnPreMultipledColors => SKPMColor.UnPreMultiply (Colors);

		public SKPMColor this[int index] {
			get {
				var count = Count;
				var pointer = ReadColors ();

				if (index < 0 || index >= count || pointer == IntPtr.Zero) {
					throw new ArgumentOutOfRangeException (nameof (index));
				}

				return PtrToStructure<SKPMColor> (pointer, index);
			}
		}

		public SKColor GetUnPreMultipliedColor (int index) => SKPMColor.UnPreMultiply (this[index]);

		public IntPtr ReadColors ()
		{
			uint* colors;
			SkiaApi.sk_colortable_read_colors (Handle, &colors);
			return (IntPtr)colors;
		}
	}
}

