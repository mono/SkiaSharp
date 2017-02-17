//
// Bindings for SKColorTable
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2016 Xamarin Inc
//

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	public class SKColorTable : SKObject
	{
		public const int MaxLength = 256;

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_colortable_unref (Handle);
			}

			base.Dispose (disposing);
		}

		[Preserve]
		internal SKColorTable (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		public SKColorTable ()
			: this (new SKColor [MaxLength])
		{
		}

		public SKColorTable (int count)
			: this (new SKColor [count])
		{
		}

		public SKColorTable (SKColor[] colors)
			: this (colors, colors.Length)
		{
		}

		public SKColorTable (SKColor[] colors, int count)
			: this (SkiaApi.sk_colortable_new (colors, count), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKColorTable instance.");
			}
		}

		public int Count => SkiaApi.sk_colortable_count (Handle);

		public SKColor[] Colors
		{
			get
			{
				var count = Count;
				var pointer = ReadColors ();

				if (count == 0 || pointer == IntPtr.Zero) {
					return new SKColor[0];
				}

				return PtrToStructureArray <SKColor> (pointer, count);
			}
		}

		public SKColor this [int index]
		{
			get
			{
				var count = Count;
				var pointer = ReadColors ();

				if (index < 0 || index >= count || pointer == IntPtr.Zero) {
					throw new ArgumentOutOfRangeException (nameof (index));
				}

				return PtrToStructure <SKColor> (pointer, index);
			}
		}

		public IntPtr ReadColors ()
		{
			IntPtr colors;
			SkiaApi.sk_colortable_read_colors (Handle, out colors);
			return colors;
		}
	}
}

