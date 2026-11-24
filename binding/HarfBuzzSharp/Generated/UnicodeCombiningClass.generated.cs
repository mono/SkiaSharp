using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_unicode_combining_class_t
	/// <summary>Specifies the Unicode canonical combining class of a character.</summary>
	/// <remarks>Combining classes determine how combining marks are ordered and rendered relative to base characters. Values correspond to the Unicode Canonical Combining Class property.</remarks>
	public enum UnicodeCombiningClass {
		// HB_UNICODE_COMBINING_CLASS_NOT_REORDERED = 0
		/// <summary>Combining class value.</summary>
		NotReordered = 0,
		// HB_UNICODE_COMBINING_CLASS_OVERLAY = 1
		/// <summary>Combining class value.</summary>
		Overlay = 1,
		// HB_UNICODE_COMBINING_CLASS_NUKTA = 7
		/// <summary>Combining class value.</summary>
		Nukta = 7,
		// HB_UNICODE_COMBINING_CLASS_KANA_VOICING = 8
		/// <summary>Combining class value.</summary>
		KanaVoicing = 8,
		// HB_UNICODE_COMBINING_CLASS_VIRAMA = 9
		/// <summary>Combining class value.</summary>
		Virama = 9,
		// HB_UNICODE_COMBINING_CLASS_CCC10 = 10
		/// <summary>Combining class value.</summary>
		CCC10 = 10,
		// HB_UNICODE_COMBINING_CLASS_CCC11 = 11
		/// <summary>Combining class value.</summary>
		CCC11 = 11,
		// HB_UNICODE_COMBINING_CLASS_CCC12 = 12
		/// <summary>Combining class value.</summary>
		CCC12 = 12,
		// HB_UNICODE_COMBINING_CLASS_CCC13 = 13
		/// <summary>Combining class value.</summary>
		CCC13 = 13,
		// HB_UNICODE_COMBINING_CLASS_CCC14 = 14
		/// <summary>Combining class value.</summary>
		CCC14 = 14,
		// HB_UNICODE_COMBINING_CLASS_CCC15 = 15
		/// <summary>Combining class value.</summary>
		CCC15 = 15,
		// HB_UNICODE_COMBINING_CLASS_CCC16 = 16
		/// <summary>Combining class value.</summary>
		CCC16 = 16,
		// HB_UNICODE_COMBINING_CLASS_CCC17 = 17
		/// <summary>Combining class value.</summary>
		CCC17 = 17,
		// HB_UNICODE_COMBINING_CLASS_CCC18 = 18
		/// <summary>Combining class value.</summary>
		CCC18 = 18,
		// HB_UNICODE_COMBINING_CLASS_CCC19 = 19
		/// <summary>Combining class value.</summary>
		CCC19 = 19,
		// HB_UNICODE_COMBINING_CLASS_CCC20 = 20
		/// <summary>Combining class value.</summary>
		CCC20 = 20,
		// HB_UNICODE_COMBINING_CLASS_CCC21 = 21
		/// <summary>Combining class value.</summary>
		CCC21 = 21,
		// HB_UNICODE_COMBINING_CLASS_CCC22 = 22
		/// <summary>Combining class value.</summary>
		CCC22 = 22,
		// HB_UNICODE_COMBINING_CLASS_CCC23 = 23
		/// <summary>Combining class value.</summary>
		CCC23 = 23,
		// HB_UNICODE_COMBINING_CLASS_CCC24 = 24
		/// <summary>Combining class value.</summary>
		CCC24 = 24,
		// HB_UNICODE_COMBINING_CLASS_CCC25 = 25
		/// <summary>Combining class value.</summary>
		CCC25 = 25,
		// HB_UNICODE_COMBINING_CLASS_CCC26 = 26
		/// <summary>Combining class value.</summary>
		CCC26 = 26,
		// HB_UNICODE_COMBINING_CLASS_CCC27 = 27
		/// <summary>Combining class value.</summary>
		CCC27 = 27,
		// HB_UNICODE_COMBINING_CLASS_CCC28 = 28
		/// <summary>Combining class value.</summary>
		CCC28 = 28,
		// HB_UNICODE_COMBINING_CLASS_CCC29 = 29
		/// <summary>Combining class value.</summary>
		CCC29 = 29,
		// HB_UNICODE_COMBINING_CLASS_CCC30 = 30
		/// <summary>Combining class value.</summary>
		CCC30 = 30,
		// HB_UNICODE_COMBINING_CLASS_CCC31 = 31
		/// <summary>Combining class value.</summary>
		CCC31 = 31,
		// HB_UNICODE_COMBINING_CLASS_CCC32 = 32
		/// <summary>Combining class value.</summary>
		CCC32 = 32,
		// HB_UNICODE_COMBINING_CLASS_CCC33 = 33
		/// <summary>Combining class value.</summary>
		CCC33 = 33,
		// HB_UNICODE_COMBINING_CLASS_CCC34 = 34
		/// <summary>Combining class value.</summary>
		CCC34 = 34,
		// HB_UNICODE_COMBINING_CLASS_CCC35 = 35
		/// <summary>Combining class value.</summary>
		CCC35 = 35,
		// HB_UNICODE_COMBINING_CLASS_CCC36 = 36
		/// <summary>Combining class value.</summary>
		CCC36 = 36,
		// HB_UNICODE_COMBINING_CLASS_CCC84 = 84
		/// <summary>Combining class value.</summary>
		CCC84 = 84,
		// HB_UNICODE_COMBINING_CLASS_CCC91 = 91
		/// <summary>Combining class value.</summary>
		CCC91 = 91,
		// HB_UNICODE_COMBINING_CLASS_CCC103 = 103
		/// <summary>Combining class value.</summary>
		CCC103 = 103,
		// HB_UNICODE_COMBINING_CLASS_CCC107 = 107
		/// <summary>Combining class value.</summary>
		CCC107 = 107,
		// HB_UNICODE_COMBINING_CLASS_CCC118 = 118
		/// <summary>Combining class value.</summary>
		CCC118 = 118,
		// HB_UNICODE_COMBINING_CLASS_CCC122 = 122
		/// <summary>Combining class value.</summary>
		CCC122 = 122,
		// HB_UNICODE_COMBINING_CLASS_CCC129 = 129
		/// <summary>Combining class value.</summary>
		CCC129 = 129,
		// HB_UNICODE_COMBINING_CLASS_CCC130 = 130
		/// <summary>Combining class value.</summary>
		CCC130 = 130,
		// HB_UNICODE_COMBINING_CLASS_CCC132 = 132
		/// <summary>Canonical combining class 132.</summary>
		CCC132 = 132,
		// HB_UNICODE_COMBINING_CLASS_ATTACHED_BELOW_LEFT = 200
		/// <summary>Combining class value.</summary>
		AttachedBelowLeft = 200,
		// HB_UNICODE_COMBINING_CLASS_ATTACHED_BELOW = 202
		/// <summary>Combining class value.</summary>
		AttachedBelow = 202,
		// HB_UNICODE_COMBINING_CLASS_ATTACHED_ABOVE = 214
		/// <summary>Combining class value.</summary>
		AttachedAbove = 214,
		// HB_UNICODE_COMBINING_CLASS_ATTACHED_ABOVE_RIGHT = 216
		/// <summary>Combining class value.</summary>
		AttachedAboveRight = 216,
		// HB_UNICODE_COMBINING_CLASS_BELOW_LEFT = 218
		/// <summary>Combining class value.</summary>
		BelowLeft = 218,
		// HB_UNICODE_COMBINING_CLASS_BELOW = 220
		/// <summary>Combining class value.</summary>
		Below = 220,
		// HB_UNICODE_COMBINING_CLASS_BELOW_RIGHT = 222
		/// <summary>Combining class value.</summary>
		BelowRight = 222,
		// HB_UNICODE_COMBINING_CLASS_LEFT = 224
		/// <summary>Combining class value.</summary>
		Left = 224,
		// HB_UNICODE_COMBINING_CLASS_RIGHT = 226
		/// <summary>Combining class value.</summary>
		Right = 226,
		// HB_UNICODE_COMBINING_CLASS_ABOVE_LEFT = 228
		/// <summary>Combining class value.</summary>
		AboveLeft = 228,
		// HB_UNICODE_COMBINING_CLASS_ABOVE = 230
		/// <summary>Combining class value.</summary>
		Above = 230,
		// HB_UNICODE_COMBINING_CLASS_ABOVE_RIGHT = 232
		/// <summary>Combining class value.</summary>
		AboveRight = 232,
		// HB_UNICODE_COMBINING_CLASS_DOUBLE_BELOW = 233
		/// <summary>Combining class value.</summary>
		DoubleBelow = 233,
		// HB_UNICODE_COMBINING_CLASS_DOUBLE_ABOVE = 234
		/// <summary>Combining class value.</summary>
		DoubleAbove = 234,
		// HB_UNICODE_COMBINING_CLASS_IOTA_SUBSCRIPT = 240
		/// <summary>Combining class value.</summary>
		IotaSubscript = 240,
		// HB_UNICODE_COMBINING_CLASS_INVALID = 255
		/// <summary>Combining class value.</summary>
		Invalid = 255,
	}
}
