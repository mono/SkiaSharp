using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_unicode_general_category_t
	/// <summary>Specifies the Unicode general category of a character.</summary>
	/// <remarks />
	public enum UnicodeGeneralCategory {
		// HB_UNICODE_GENERAL_CATEGORY_CONTROL = 0
		/// <summary>Control character (Cc).</summary>
		Control = 0,
		// HB_UNICODE_GENERAL_CATEGORY_FORMAT = 1
		/// <summary>Format character (Cf).</summary>
		Format = 1,
		// HB_UNICODE_GENERAL_CATEGORY_UNASSIGNED = 2
		/// <summary>Unassigned code point (Cn).</summary>
		Unassigned = 2,
		// HB_UNICODE_GENERAL_CATEGORY_PRIVATE_USE = 3
		/// <summary>Private-use character (Co).</summary>
		PrivateUse = 3,
		// HB_UNICODE_GENERAL_CATEGORY_SURROGATE = 4
		/// <summary>Surrogate code point (Cs).</summary>
		Surrogate = 4,
		// HB_UNICODE_GENERAL_CATEGORY_LOWERCASE_LETTER = 5
		/// <summary>Lowercase letter (Ll).</summary>
		LowercaseLetter = 5,
		// HB_UNICODE_GENERAL_CATEGORY_MODIFIER_LETTER = 6
		/// <summary>Modifier letter (Lm).</summary>
		ModifierLetter = 6,
		// HB_UNICODE_GENERAL_CATEGORY_OTHER_LETTER = 7
		/// <summary>Other letter (Lo).</summary>
		OtherLetter = 7,
		// HB_UNICODE_GENERAL_CATEGORY_TITLECASE_LETTER = 8
		/// <summary>Titlecase letter (Lt).</summary>
		TitlecaseLetter = 8,
		// HB_UNICODE_GENERAL_CATEGORY_UPPERCASE_LETTER = 9
		/// <summary>Uppercase letter (Lu).</summary>
		UppercaseLetter = 9,
		// HB_UNICODE_GENERAL_CATEGORY_SPACING_MARK = 10
		/// <summary>Spacing combining mark (Mc).</summary>
		SpacingMark = 10,
		// HB_UNICODE_GENERAL_CATEGORY_ENCLOSING_MARK = 11
		/// <summary>Enclosing mark (Me).</summary>
		EnclosingMark = 11,
		// HB_UNICODE_GENERAL_CATEGORY_NON_SPACING_MARK = 12
		/// <summary>Non-spacing mark (Mn).</summary>
		NonSpacingMark = 12,
		// HB_UNICODE_GENERAL_CATEGORY_DECIMAL_NUMBER = 13
		/// <summary>Decimal digit number (Nd).</summary>
		DecimalNumber = 13,
		// HB_UNICODE_GENERAL_CATEGORY_LETTER_NUMBER = 14
		/// <summary>Letter number (Nl).</summary>
		LetterNumber = 14,
		// HB_UNICODE_GENERAL_CATEGORY_OTHER_NUMBER = 15
		/// <summary>Other number (No).</summary>
		OtherNumber = 15,
		// HB_UNICODE_GENERAL_CATEGORY_CONNECT_PUNCTUATION = 16
		/// <summary>Connector punctuation (Pc).</summary>
		ConnectPunctuation = 16,
		// HB_UNICODE_GENERAL_CATEGORY_DASH_PUNCTUATION = 17
		/// <summary>Dash punctuation (Pd).</summary>
		DashPunctuation = 17,
		// HB_UNICODE_GENERAL_CATEGORY_CLOSE_PUNCTUATION = 18
		/// <summary>Close punctuation (Pe).</summary>
		ClosePunctuation = 18,
		// HB_UNICODE_GENERAL_CATEGORY_FINAL_PUNCTUATION = 19
		/// <summary>Final punctuation (Pf).</summary>
		FinalPunctuation = 19,
		// HB_UNICODE_GENERAL_CATEGORY_INITIAL_PUNCTUATION = 20
		/// <summary>Initial punctuation (Pi).</summary>
		InitialPunctuation = 20,
		// HB_UNICODE_GENERAL_CATEGORY_OTHER_PUNCTUATION = 21
		/// <summary>Other punctuation (Po).</summary>
		OtherPunctuation = 21,
		// HB_UNICODE_GENERAL_CATEGORY_OPEN_PUNCTUATION = 22
		/// <summary>Open punctuation (Ps).</summary>
		OpenPunctuation = 22,
		// HB_UNICODE_GENERAL_CATEGORY_CURRENCY_SYMBOL = 23
		/// <summary>Currency symbol (Sc).</summary>
		CurrencySymbol = 23,
		// HB_UNICODE_GENERAL_CATEGORY_MODIFIER_SYMBOL = 24
		/// <summary>Modifier symbol (Sk).</summary>
		ModifierSymbol = 24,
		// HB_UNICODE_GENERAL_CATEGORY_MATH_SYMBOL = 25
		/// <summary>Math symbol (Sm).</summary>
		MathSymbol = 25,
		// HB_UNICODE_GENERAL_CATEGORY_OTHER_SYMBOL = 26
		/// <summary>Other symbol (So).</summary>
		OtherSymbol = 26,
		// HB_UNICODE_GENERAL_CATEGORY_LINE_SEPARATOR = 27
		/// <summary>Line separator (Zl).</summary>
		LineSeparator = 27,
		// HB_UNICODE_GENERAL_CATEGORY_PARAGRAPH_SEPARATOR = 28
		/// <summary>Paragraph separator (Zp).</summary>
		ParagraphSeparator = 28,
		// HB_UNICODE_GENERAL_CATEGORY_SPACE_SEPARATOR = 29
		/// <summary>Space separator (Zs).</summary>
		SpaceSeparator = 29,
	}
}
