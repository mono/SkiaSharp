using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_colorspace_transfer_fn_cicp_t
	/// <summary>Identifies transfer functions (opto-electronic transfer characteristics) using ITU-T H.273 / ISO 23001-8 Coding-Independent Code Points (CICP) values.</summary>
	/// <remarks><![CDATA[
	/// ## Remarks
	///
	/// `SKColorspaceTransferFnCicp` enumerates the standard transfer functions defined by the ITU-T H.273 and ISO/IEC 23001-8 specifications. These values are used as parameters when calling <xref:SkiaSharp.SKColorSpace.CreateCicp*> to create a CICP-specified color space.
	///
	/// Each member's numeric value corresponds directly to the "Transfer characteristics" code point in Table 3 of ITU-T H.273.
	///
	/// ## Examples
	///
	/// Creating an HDR PQ color space with BT.2020 primaries:
	///
	/// ```csharp
	/// using var colorSpace = SKColorSpace.CreateCicp(
	///     SKColorspacePrimariesCicp.Rec2020,
	///     SKColorspaceTransferFnCicp.Pq,
	///     SKColorspaceMatrixCoefficients.Identity,
	///     SKColorspaceRange.Full);
	/// ```
	/// ]]></remarks>
	public enum SKColorspaceTransferFnCicp {
		// UNKNOWN_SK_COLORSPACE_TRANSFER_FN_CICP = 0
		/// <summary>Unknown or unspecified transfer characteristics (code point 0).</summary>
		Unknown = 0,
		// REC709_SK_COLORSPACE_TRANSFER_FN_CICP = 1
		/// <summary>ITU-R BT.709 transfer characteristics (code point 1), used for HDTV; also matches sRGB in practice.</summary>
		Rec709 = 1,
		// REC470_SYSTEM_M_SK_COLORSPACE_TRANSFER_FN_CICP = 4
		/// <summary>ITU-R BT.470 System M transfer characteristics (code point 4), assuming a display gamma of 2.2.</summary>
		Rec470SystemM = 4,
		// REC470_SYSTEM_BG_SK_COLORSPACE_TRANSFER_FN_CICP = 5
		/// <summary>ITU-R BT.470 System B/G transfer characteristics (code point 5), assuming a display gamma of 2.8.</summary>
		Rec470SystemBg = 5,
		// REC601_SK_COLORSPACE_TRANSFER_FN_CICP = 6
		/// <summary>ITU-R BT.601 transfer characteristics (code point 6), used for standard-definition television.</summary>
		Rec601 = 6,
		// SMPTE_ST240_SK_COLORSPACE_TRANSFER_FN_CICP = 7
		/// <summary>SMPTE ST 240 transfer characteristics (code point 7), used for early HDTV production.</summary>
		SmpteSt240 = 7,
		// LINEAR_SK_COLORSPACE_TRANSFER_FN_CICP = 8
		/// <summary>Linear transfer characteristics with no gamma encoding (code point 8).</summary>
		Linear = 8,
		// IEC61966_2_4_SK_COLORSPACE_TRANSFER_FN_CICP = 11
		/// <summary>IEC 61966-2-4 transfer characteristics (code point 11), used for xvYCC extended-gamut video.</summary>
		Iec6196624 = 11,
		// IEC61966_2_1_SK_COLORSPACE_TRANSFER_FN_CICP = 13
		/// <summary>IEC 61966-2-1 transfer characteristics (code point 13), defining the sRGB and sYCC transfer functions.</summary>
		Iec6196621 = 13,
		// REC2020_10BIT_SK_COLORSPACE_TRANSFER_FN_CICP = 14
		/// <summary>ITU-R BT.2020 10-bit transfer characteristics (code point 14), used for 10-bit UHD content.</summary>
		Rec202010bit = 14,
		// REC2020_12BIT_SK_COLORSPACE_TRANSFER_FN_CICP = 15
		/// <summary>ITU-R BT.2020 12-bit transfer characteristics (code point 15), used for 12-bit UHD content.</summary>
		Rec202012bit = 15,
		// PQ_SK_COLORSPACE_TRANSFER_FN_CICP = 16
		/// <summary>SMPTE ST 2084 Perceptual Quantizer (PQ) transfer characteristics (code point 16), used for HDR10 content.</summary>
		Pq = 16,
		// SMPTE_ST428_1_SK_COLORSPACE_TRANSFER_FN_CICP = 17
		/// <summary>SMPTE ST 428-1 transfer characteristics (code point 17), defining the D-Cinema transfer function with a power-law gamma of approximately 2.6.</summary>
		SmpteSt4281 = 17,
		// HLG_SK_COLORSPACE_TRANSFER_FN_CICP = 18
		/// <summary>ARIB STD-B67 Hybrid Log-Gamma (HLG) transfer characteristics (code point 18), used for HDR broadcast per ITU-R BT.2100.</summary>
		Hlg = 18,
	}
}
