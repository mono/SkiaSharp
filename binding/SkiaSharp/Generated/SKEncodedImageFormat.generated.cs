using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_encoded_image_format_t
	/// <summary>The various formats used by a <see cref="T:SkiaSharp.SKCodec" />.</summary>
	/// <remarks />
	public enum SKEncodedImageFormat {
		// BMP_SK_ENCODED_FORMAT = 0
		/// <summary>The BMP image format.</summary>
		Bmp = 0,
		// GIF_SK_ENCODED_FORMAT = 1
		/// <summary>The GIF image format.</summary>
		Gif = 1,
		// ICO_SK_ENCODED_FORMAT = 2
		/// <summary>The ICO image format.</summary>
		Ico = 2,
		// JPEG_SK_ENCODED_FORMAT = 3
		/// <summary>The JPEG image format.</summary>
		Jpeg = 3,
		// PNG_SK_ENCODED_FORMAT = 4
		/// <summary>The PNG image format.</summary>
		Png = 4,
		// WBMP_SK_ENCODED_FORMAT = 5
		/// <summary>The WBMP image format.</summary>
		Wbmp = 5,
		// WEBP_SK_ENCODED_FORMAT = 6
		/// <summary>The WEBP image format.</summary>
		Webp = 6,
		// PKM_SK_ENCODED_FORMAT = 7
		/// <summary>The PKM image format.</summary>
		Pkm = 7,
		// KTX_SK_ENCODED_FORMAT = 8
		/// <summary>The KTX image format.</summary>
		Ktx = 8,
		// ASTC_SK_ENCODED_FORMAT = 9
		/// <summary>The ASTC image format.</summary>
		Astc = 9,
		// DNG_SK_ENCODED_FORMAT = 10
		/// <summary>The Adobe DNG image format.</summary>
		Dng = 10,
		// HEIF_SK_ENCODED_FORMAT = 11
		/// <summary>The HEIF or High Efficiency Image File format.</summary>
		Heif = 11,
		// AVIF_SK_ENCODED_FORMAT = 12
		/// <summary>The AVIF (AV1 Image File Format) image format.</summary>
		Avif = 12,
		// JPEGXL_SK_ENCODED_FORMAT = 13
		/// <summary>The JPEG XL image format.</summary>
		Jpegxl = 13,
	}
}
