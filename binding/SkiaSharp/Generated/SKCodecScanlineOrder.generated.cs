using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_codec_scanline_order_t
	/// <summary>The orders in which scanlines can be returned.</summary>
	/// <remarks>These values are obtained through the <see cref="P:SkiaSharp.SKCodec.ScanlineOrder" /> property.</remarks>
	public enum SKCodecScanlineOrder {
		// TOP_DOWN_SK_CODEC_SCANLINE_ORDER = 0
		/// <summary>Indicates that the image can be decoded reliably using the scanline decoder, and that rows will be output in the logical order.</summary>
		TopDown = 0,
		// BOTTOM_UP_SK_CODEC_SCANLINE_ORDER = 1
		/// <summary>Indicates that the scanline decoder reliably outputs rows, but they will be returned in reverse order. The <see cref="P:SkiaSharp.SKCodec.NextScanline" /> property can be used to determine the actual y-coordinate of the next output row.</summary>
		BottomUp = 1,
	}
}
