using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_colorspace_primaries_cicp_t
	/// <summary>Identifies color primaries using ITU-T H.273 / ISO 23001-8 Coding-Independent Code Points (CICP) values.</summary>
	/// <remarks><![CDATA[
	/// ## Remarks
	///
	/// `SKColorspacePrimariesCicp` enumerates the standard color primaries defined by the ITU-T H.273 and ISO/IEC 23001-8 specifications. These values are used as parameters when calling <xref:SkiaSharp.SKColorSpace.CreateCicp*> to create a CICP-specified color space.
	///
	/// Each member's numeric value corresponds directly to the color primaries code point in Table 2 of ITU-T H.273.
	///
	/// ## Examples
	///
	/// Creating a BT.2020 wide-gamut color space:
	///
	/// ```csharp
	/// using var colorSpace = SKColorSpace.CreateCicp(
	///     SKColorspacePrimariesCicp.Rec2020,
	///     SKColorspaceTransferFnCicp.Pq,
	///     SKColorspaceMatrixCoefficients.Identity,
	///     SKColorspaceRange.Full);
	/// ```
	/// ]]></remarks>
	public enum SKColorspacePrimariesCicp {
		// UNKNOWN_SK_COLORSPACE_PRIMARIES_CICP = 0
		/// <summary>Unknown or unspecified color primaries (code point 0).</summary>
		Unknown = 0,
		// REC709_SK_COLORSPACE_PRIMARIES_CICP = 1
		/// <summary>ITU-R BT.709 color primaries (code point 1), used for HDTV and sRGB.</summary>
		Rec709 = 1,
		// REC470_SYSTEM_M_SK_COLORSPACE_PRIMARIES_CICP = 4
		/// <summary>ITU-R BT.470 System M color primaries (code point 4), used for NTSC analog television.</summary>
		Rec470SystemM = 4,
		// REC470_SYSTEM_BG_SK_COLORSPACE_PRIMARIES_CICP = 5
		/// <summary>ITU-R BT.470 System B/G color primaries (code point 5), used for PAL and SECAM analog television.</summary>
		Rec470SystemBg = 5,
		// REC601_SK_COLORSPACE_PRIMARIES_CICP = 6
		/// <summary>ITU-R BT.601 color primaries (code point 6), used for standard-definition television.</summary>
		Rec601 = 6,
		// SMPTE_ST240_SK_COLORSPACE_PRIMARIES_CICP = 7
		/// <summary>SMPTE ST 240 color primaries (code point 7), used for early HDTV production.</summary>
		SmpteSt240 = 7,
		// GENERIC_FILM_SK_COLORSPACE_PRIMARIES_CICP = 8
		/// <summary>Generic film color primaries (code point 8), based on D65 white point and C-illuminant.</summary>
		GenericFilm = 8,
		// REC2020_SK_COLORSPACE_PRIMARIES_CICP = 9
		/// <summary>ITU-R BT.2020 color primaries (code point 9), used for ultra-high-definition television with a wide color gamut.</summary>
		Rec2020 = 9,
		// SMPTE_ST428_1_SK_COLORSPACE_PRIMARIES_CICP = 10
		/// <summary>SMPTE ST 428-1 color primaries (code point 10), using CIE XYZ with a D50 white point.</summary>
		SmpteSt4281 = 10,
		// SMPTE_RP431_2_SK_COLORSPACE_PRIMARIES_CICP = 11
		/// <summary>SMPTE RP 431-2 color primaries (code point 11), defining the DCI-P3 gamut used in digital cinema.</summary>
		SmpteRp4312 = 11,
		// SMPTE_EG432_1_SK_COLORSPACE_PRIMARIES_CICP = 12
		/// <summary>SMPTE EG 432-1 color primaries (code point 12), defining the Display P3 (P3-D65) gamut used on Apple displays.</summary>
		SmpteEg4321 = 12,
		// ITU_T_H273_VALUE22_SK_COLORSPACE_PRIMARIES_CICP = 22
		/// <summary>ITU-T H.273 value 22 color primaries (code point 22), intended for EBU Tech 3213-E and related consumer electronics standards.</summary>
		ItuTH273Value22 = 22,
	}
}
