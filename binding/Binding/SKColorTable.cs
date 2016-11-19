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
				var colors = new SKColor[count];
				var type = typeof(SKColor);
				var size = Marshal.SizeOf (type);
				var pointer = ReadColors ();
				for (var i = 0; i < count; i++) {
					colors[i] = (SKColor)Marshal.PtrToStructure (pointer + (i * size), type);
				}
				return colors;
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

				var type = typeof(SKColor);
				var size = Marshal.SizeOf (type);
				return (SKColor)Marshal.PtrToStructure (pointer + (index * size), type);
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

