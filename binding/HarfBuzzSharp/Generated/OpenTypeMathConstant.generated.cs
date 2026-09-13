using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_math_constant_t
	/// <summary>Specifies constants from the OpenType MATH table for mathematical typesetting.</summary>
	/// <remarks />
	public enum OpenTypeMathConstant {
		// HB_OT_MATH_CONSTANT_SCRIPT_PERCENT_SCALE_DOWN = 0
		/// <summary>Math layout constant.</summary>
		ScriptPercentScaleDown = 0,
		// HB_OT_MATH_CONSTANT_SCRIPT_SCRIPT_PERCENT_SCALE_DOWN = 1
		/// <summary>Math layout constant.</summary>
		ScriptScriptPercentScaleDown = 1,
		// HB_OT_MATH_CONSTANT_DELIMITED_SUB_FORMULA_MIN_HEIGHT = 2
		/// <summary>Math layout constant.</summary>
		DelimitedSubFormulaMinHeight = 2,
		// HB_OT_MATH_CONSTANT_DISPLAY_OPERATOR_MIN_HEIGHT = 3
		/// <summary>Math layout constant.</summary>
		DisplayOperatorMinHeight = 3,
		// HB_OT_MATH_CONSTANT_MATH_LEADING = 4
		/// <summary>Math layout constant.</summary>
		MathLeading = 4,
		// HB_OT_MATH_CONSTANT_AXIS_HEIGHT = 5
		/// <summary>Math layout constant.</summary>
		AxisHeight = 5,
		// HB_OT_MATH_CONSTANT_ACCENT_BASE_HEIGHT = 6
		/// <summary>Math layout constant.</summary>
		AccentBaseHeight = 6,
		// HB_OT_MATH_CONSTANT_FLATTENED_ACCENT_BASE_HEIGHT = 7
		/// <summary>Math layout constant.</summary>
		FlattenedAccentBaseHeight = 7,
		// HB_OT_MATH_CONSTANT_SUBSCRIPT_SHIFT_DOWN = 8
		/// <summary>Math layout constant.</summary>
		SubscriptShiftDown = 8,
		// HB_OT_MATH_CONSTANT_SUBSCRIPT_TOP_MAX = 9
		/// <summary>Math layout constant.</summary>
		SubscriptTopMax = 9,
		// HB_OT_MATH_CONSTANT_SUBSCRIPT_BASELINE_DROP_MIN = 10
		/// <summary>Math layout constant.</summary>
		SubscriptBaselineDropMin = 10,
		// HB_OT_MATH_CONSTANT_SUPERSCRIPT_SHIFT_UP = 11
		/// <summary>Math layout constant.</summary>
		SuperscriptShiftUp = 11,
		// HB_OT_MATH_CONSTANT_SUPERSCRIPT_SHIFT_UP_CRAMPED = 12
		/// <summary>Math layout constant.</summary>
		SuperscriptShiftUpCramped = 12,
		// HB_OT_MATH_CONSTANT_SUPERSCRIPT_BOTTOM_MIN = 13
		/// <summary>Math layout constant.</summary>
		SuperscriptBottomMin = 13,
		// HB_OT_MATH_CONSTANT_SUPERSCRIPT_BASELINE_DROP_MAX = 14
		/// <summary>Math layout constant.</summary>
		SuperscriptBaselineDropMax = 14,
		// HB_OT_MATH_CONSTANT_SUB_SUPERSCRIPT_GAP_MIN = 15
		/// <summary>Math layout constant.</summary>
		SubSuperscriptGapMin = 15,
		// HB_OT_MATH_CONSTANT_SUPERSCRIPT_BOTTOM_MAX_WITH_SUBSCRIPT = 16
		/// <summary>Math layout constant.</summary>
		SuperscriptBottomMaxWithSubscript = 16,
		// HB_OT_MATH_CONSTANT_SPACE_AFTER_SCRIPT = 17
		/// <summary>Math layout constant.</summary>
		SpaceAfterScript = 17,
		// HB_OT_MATH_CONSTANT_UPPER_LIMIT_GAP_MIN = 18
		/// <summary>Math layout constant.</summary>
		UpperLimitGapMin = 18,
		// HB_OT_MATH_CONSTANT_UPPER_LIMIT_BASELINE_RISE_MIN = 19
		/// <summary>Math layout constant.</summary>
		UpperLimitBaselineRiseMin = 19,
		// HB_OT_MATH_CONSTANT_LOWER_LIMIT_GAP_MIN = 20
		/// <summary>Math layout constant.</summary>
		LowerLimitGapMin = 20,
		// HB_OT_MATH_CONSTANT_LOWER_LIMIT_BASELINE_DROP_MIN = 21
		/// <summary>Math layout constant.</summary>
		LowerLimitBaselineDropMin = 21,
		// HB_OT_MATH_CONSTANT_STACK_TOP_SHIFT_UP = 22
		/// <summary>Math layout constant.</summary>
		StackTopShiftUp = 22,
		// HB_OT_MATH_CONSTANT_STACK_TOP_DISPLAY_STYLE_SHIFT_UP = 23
		/// <summary>Math layout constant.</summary>
		StackTopDisplayStyleShiftUp = 23,
		// HB_OT_MATH_CONSTANT_STACK_BOTTOM_SHIFT_DOWN = 24
		/// <summary>Math layout constant.</summary>
		StackBottomShiftDown = 24,
		// HB_OT_MATH_CONSTANT_STACK_BOTTOM_DISPLAY_STYLE_SHIFT_DOWN = 25
		/// <summary>Math layout constant.</summary>
		StackBottomDisplayStyleShiftDown = 25,
		// HB_OT_MATH_CONSTANT_STACK_GAP_MIN = 26
		/// <summary>Math layout constant.</summary>
		StackGapMin = 26,
		// HB_OT_MATH_CONSTANT_STACK_DISPLAY_STYLE_GAP_MIN = 27
		/// <summary>Math layout constant.</summary>
		StackDisplayStyleGapMin = 27,
		// HB_OT_MATH_CONSTANT_STRETCH_STACK_TOP_SHIFT_UP = 28
		/// <summary>Math layout constant.</summary>
		StretchStackTopShiftUp = 28,
		// HB_OT_MATH_CONSTANT_STRETCH_STACK_BOTTOM_SHIFT_DOWN = 29
		/// <summary>Math layout constant.</summary>
		StretchStackBottomShiftDown = 29,
		// HB_OT_MATH_CONSTANT_STRETCH_STACK_GAP_ABOVE_MIN = 30
		/// <summary>Math layout constant.</summary>
		StretchStackGapAboveMin = 30,
		// HB_OT_MATH_CONSTANT_STRETCH_STACK_GAP_BELOW_MIN = 31
		/// <summary>Math layout constant.</summary>
		StretchStackGapBelowMin = 31,
		// HB_OT_MATH_CONSTANT_FRACTION_NUMERATOR_SHIFT_UP = 32
		/// <summary>Math layout constant.</summary>
		FractionNumeratorShiftUp = 32,
		// HB_OT_MATH_CONSTANT_FRACTION_NUMERATOR_DISPLAY_STYLE_SHIFT_UP = 33
		/// <summary>Math layout constant.</summary>
		FractionNumeratorDisplayStyleShiftUp = 33,
		// HB_OT_MATH_CONSTANT_FRACTION_DENOMINATOR_SHIFT_DOWN = 34
		/// <summary>Math layout constant.</summary>
		FractionDenominatorShiftDown = 34,
		// HB_OT_MATH_CONSTANT_FRACTION_DENOMINATOR_DISPLAY_STYLE_SHIFT_DOWN = 35
		/// <summary>Math layout constant.</summary>
		FractionDenominatorDisplayStyleShiftDown = 35,
		// HB_OT_MATH_CONSTANT_FRACTION_NUMERATOR_GAP_MIN = 36
		/// <summary>Math layout constant.</summary>
		FractionNumeratorGapMin = 36,
		// HB_OT_MATH_CONSTANT_FRACTION_NUM_DISPLAY_STYLE_GAP_MIN = 37
		/// <summary>Math layout constant.</summary>
		FractionNumDisplayStyleGapMin = 37,
		// HB_OT_MATH_CONSTANT_FRACTION_RULE_THICKNESS = 38
		/// <summary>Math layout constant.</summary>
		FractionRuleThickness = 38,
		// HB_OT_MATH_CONSTANT_FRACTION_DENOMINATOR_GAP_MIN = 39
		/// <summary>Math layout constant.</summary>
		FractionDenominatorGapMin = 39,
		// HB_OT_MATH_CONSTANT_FRACTION_DENOM_DISPLAY_STYLE_GAP_MIN = 40
		/// <summary>Math layout constant.</summary>
		FractionDenomDisplayStyleGapMin = 40,
		// HB_OT_MATH_CONSTANT_SKEWED_FRACTION_HORIZONTAL_GAP = 41
		/// <summary>Math layout constant.</summary>
		SkewedFractionHorizontalGap = 41,
		// HB_OT_MATH_CONSTANT_SKEWED_FRACTION_VERTICAL_GAP = 42
		/// <summary>Math layout constant.</summary>
		SkewedFractionVerticalGap = 42,
		// HB_OT_MATH_CONSTANT_OVERBAR_VERTICAL_GAP = 43
		/// <summary>Math layout constant.</summary>
		OverbarVerticalGap = 43,
		// HB_OT_MATH_CONSTANT_OVERBAR_RULE_THICKNESS = 44
		/// <summary>Math layout constant.</summary>
		OverbarRuleThickness = 44,
		// HB_OT_MATH_CONSTANT_OVERBAR_EXTRA_ASCENDER = 45
		/// <summary>Math layout constant.</summary>
		OverbarExtraAscender = 45,
		// HB_OT_MATH_CONSTANT_UNDERBAR_VERTICAL_GAP = 46
		/// <summary>Math layout constant.</summary>
		UnderbarVerticalGap = 46,
		// HB_OT_MATH_CONSTANT_UNDERBAR_RULE_THICKNESS = 47
		/// <summary>Math layout constant.</summary>
		UnderbarRuleThickness = 47,
		// HB_OT_MATH_CONSTANT_UNDERBAR_EXTRA_DESCENDER = 48
		/// <summary>Math layout constant.</summary>
		UnderbarExtraDescender = 48,
		// HB_OT_MATH_CONSTANT_RADICAL_VERTICAL_GAP = 49
		/// <summary>Math layout constant.</summary>
		RadicalVerticalGap = 49,
		// HB_OT_MATH_CONSTANT_RADICAL_DISPLAY_STYLE_VERTICAL_GAP = 50
		/// <summary>Math layout constant.</summary>
		RadicalDisplayStyleVerticalGap = 50,
		// HB_OT_MATH_CONSTANT_RADICAL_RULE_THICKNESS = 51
		/// <summary>Math layout constant.</summary>
		RadicalRuleThickness = 51,
		// HB_OT_MATH_CONSTANT_RADICAL_EXTRA_ASCENDER = 52
		/// <summary>Math layout constant.</summary>
		RadicalExtraAscender = 52,
		// HB_OT_MATH_CONSTANT_RADICAL_KERN_BEFORE_DEGREE = 53
		/// <summary>Math layout constant.</summary>
		RadicalKernBeforeDegree = 53,
		// HB_OT_MATH_CONSTANT_RADICAL_KERN_AFTER_DEGREE = 54
		/// <summary>Math layout constant.</summary>
		RadicalKernAfterDegree = 54,
		// HB_OT_MATH_CONSTANT_RADICAL_DEGREE_BOTTOM_RAISE_PERCENT = 55
		/// <summary>Math layout constant.</summary>
		RadicalDegreeBottomRaisePercent = 55,
	}
}
