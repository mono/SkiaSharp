#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SkiaSharp
{
	// Centralized acquisition of every process-global singleton's RAW NATIVE HANDLE (the sRGB color
	// spaces, the empty SKData, the gamma color filters, the blend-mode blenders, the preset font
	// styles, the default font manager, and the default/empty typefaces).
	//
	// WHY THIS TYPE EXISTS — IT BREAKS THE #3817 STATIC-INIT CYCLE
	// -----------------------------------------------------------
	// The singleton wrappers are built lazily-but-eagerly by each wrapper type's own static constructor
	// (e.g. SKColorSpace..cctor builds the sRGB wrappers). Static constructors are the right tool: the
	// CLR runs each exactly once, in a thread-safe way, with no locks of our own, and the resulting
	// fields can be 'readonly'.
	//
	// The hazard with per-type static constructors is a CYCLE between them. The default typeface depends
	// on the default font manager and the "normal" font style:
	//
	//     SKTypeface..cctor -> reads SKFontManager.Default + SKFontStyle.Normal
	//
	// If SKTypeface..cctor read those WRAPPER properties, it would trigger SKFontManager..cctor and
	// SKFontStyle..cctor from inside its own initialization. Should one of THOSE (directly or via the
	// old SKObject..cctor chaining) touch SKTypeface again on the same thread, the CLR returns the
	// half-built SKTypeface type with null fields -> NullReferenceException — exactly the regression in
	// #3817. (CLR type-initializer re-entrancy returns the partially-initialized type instead of
	// blocking, and a throw is cached for the lifetime of the process, permanently poisoning the type.)
	//
	// This type dissolves the cycle by resolving the ONE cross-type dependency at the raw-handle level,
	// touching NO managed SKObject type. Its static constructor calls only SkiaApi.sk_* functions and
	// produces plain IntPtr handles. The default typeface is computed here from the font manager and
	// normal-style HANDLES (not their wrappers), so no wrapper..cctor ever reads another wrapper type:
	//
	//     SKTypeface..cctor   -> reads SkiaSharpStatics.DefaultTypeface (an IntPtr) — NOT SKFontManager
	//     SKFontManager..cctor-> reads SkiaSharpStatics.DefaultFontManager (an IntPtr)
	//     SKFontStyle..cctor  -> reads SkiaSharpStatics.NormalFontStyle (an IntPtr)
	//
	// Because SkiaSharpStatics..cctor depends on no wrapper type, and no wrapper..cctor depends on
	// another wrapper type, the static-initializer graph is acyclic and the #3817 re-entrancy is gone.
	//
	// EAGER, ONE-SHOT
	// ---------------
	// The first access to any singleton handle field triggers this static constructor, which acquires
	// ALL singleton handles up front (matching the 3.119.x behavior where touching any SKObject created
	// every singleton). Initialization is intentionally ONE-SHOT: there is no retry. The only realistic
	// failure here is an incompatible/broken native library, in which case the library is unusable and
	// retrying cannot help. A failure surfaces as a (cached) TypeInitializationException — acceptable
	// for an unrecoverable condition, and far simpler than a lock-based retry orchestrator.
	//
	// REFERENCE COUNTING
	// ------------------
	// Each sk_*_new/create call below returns a native object with a +1 reference. That reference is
	// "held" by the IntPtr field and later adopted (owns: true) by the immortal wrapper built in the
	// owning type's static constructor; the immortal wrapper never releases it, so the singleton lives
	// for the process lifetime. The font manager and normal-style handles passed to
	// sk_fontmgr_legacy_create_typeface are BORROWED (not consumed) by that call, so they remain owned
	// by their own wrappers — no double counting.
	internal static class SkiaSharpStatics
	{
		// Color spaces.
		internal static readonly IntPtr Srgb;
		internal static readonly IntPtr SrgbLinear;

		// Gamma color filters.
		internal static readonly IntPtr SrgbToLinearGamma;
		internal static readonly IntPtr LinearToSrgbGamma;

		// Empty data.
		internal static readonly IntPtr EmptyData;

		// Preset font styles.
		internal static readonly IntPtr NormalFontStyle;
		internal static readonly IntPtr BoldFontStyle;
		internal static readonly IntPtr ItalicFontStyle;
		internal static readonly IntPtr BoldItalicFontStyle;

		// Default font manager.
		internal static readonly IntPtr DefaultFontManager;

		// Typefaces. DefaultTypeface falls back to EmptyTypeface (same handle) when the platform's
		// legacy default-typeface lookup returns null; the owning type's static constructor checks for
		// that aliasing so it does not adopt the empty handle twice.
		internal static readonly IntPtr EmptyTypeface;
		internal static readonly IntPtr DefaultTypeface;

		// Blend-mode blenders (one per SKBlendMode). Wrapped in a ReadOnlyDictionary so the shared map
		// is genuinely immutable — callers cannot mutate it even via an in-assembly downcast.
		internal static readonly IReadOnlyDictionary<SKBlendMode, IntPtr> BlendModeBlenders;

		// Acquire every singleton handle in dependency order using ONLY native calls. Runs exactly once
		// (CLR static-constructor guarantee), on the first access to any field above, with no locks.
		static SkiaSharpStatics ()
		{
			// Independent leaves first.
			Srgb = ThrowIfZero (SkiaApi.sk_colorspace_new_srgb (), nameof (Srgb));
			SrgbLinear = ThrowIfZero (SkiaApi.sk_colorspace_new_srgb_linear (), nameof (SrgbLinear));

			SrgbToLinearGamma = ThrowIfZero (SkiaApi.sk_colorfilter_new_srgb_to_linear_gamma (), nameof (SrgbToLinearGamma));
			LinearToSrgbGamma = ThrowIfZero (SkiaApi.sk_colorfilter_new_linear_to_srgb_gamma (), nameof (LinearToSrgbGamma));

			EmptyData = ThrowIfZero (SkiaApi.sk_data_new_empty (), nameof (EmptyData));

			NormalFontStyle = NewFontStyle (SKFontStyleWeight.Normal, SKFontStyleSlant.Upright, nameof (NormalFontStyle));
			BoldFontStyle = NewFontStyle (SKFontStyleWeight.Bold, SKFontStyleSlant.Upright, nameof (BoldFontStyle));
			ItalicFontStyle = NewFontStyle (SKFontStyleWeight.Normal, SKFontStyleSlant.Italic, nameof (ItalicFontStyle));
			BoldItalicFontStyle = NewFontStyle (SKFontStyleWeight.Bold, SKFontStyleSlant.Italic, nameof (BoldItalicFontStyle));

			var blenders = new Dictionary<SKBlendMode, IntPtr> (BlendModes.Length);
			foreach (var mode in BlendModes)
				blenders[mode] = ThrowIfZero (SkiaApi.sk_blender_new_mode (mode), $"Blender({mode})");
			BlendModeBlenders = new ReadOnlyDictionary<SKBlendMode, IntPtr> (blenders);

			// The default font manager and empty typeface are independent.
			DefaultFontManager = ThrowIfZero (SkiaApi.sk_fontmgr_create_default (), nameof (DefaultFontManager));
			EmptyTypeface = ThrowIfZero (SkiaApi.sk_typeface_create_empty (), nameof (EmptyTypeface));

			// The default typeface is the one true cross-type dependency. Resolve it here at the handle
			// level (manager + normal-style handles), NOT via the SKFontManager/SKFontStyle wrappers, so
			// no wrapper static constructor ever reads another wrapper type. legacyMakeTypeface(null) is
			// used (not matchFamilyStyle(null)) because onMatchFamily(null) returns null on
			// Android/NDK/Custom managers; the legacy path uses the platform default style set. The
			// manager and style handles are borrowed by this call, not consumed.
			IntPtr matched = SkiaApi.sk_fontmgr_legacy_create_typeface (DefaultFontManager, IntPtr.Zero, NormalFontStyle);
			DefaultTypeface = matched == IntPtr.Zero ? EmptyTypeface : matched;
		}

		// All SKBlendMode values, listed explicitly to avoid reflection (AoT compatibility).
		private static readonly SKBlendMode[] BlendModes = {
			SKBlendMode.Clear,
			SKBlendMode.Src,
			SKBlendMode.Dst,
			SKBlendMode.SrcOver,
			SKBlendMode.DstOver,
			SKBlendMode.SrcIn,
			SKBlendMode.DstIn,
			SKBlendMode.SrcOut,
			SKBlendMode.DstOut,
			SKBlendMode.SrcATop,
			SKBlendMode.DstATop,
			SKBlendMode.Xor,
			SKBlendMode.Plus,
			SKBlendMode.Modulate,
			SKBlendMode.Screen,
			SKBlendMode.Overlay,
			SKBlendMode.Darken,
			SKBlendMode.Lighten,
			SKBlendMode.ColorDodge,
			SKBlendMode.ColorBurn,
			SKBlendMode.HardLight,
			SKBlendMode.SoftLight,
			SKBlendMode.Difference,
			SKBlendMode.Exclusion,
			SKBlendMode.Multiply,
			SKBlendMode.Hue,
			SKBlendMode.Saturation,
			SKBlendMode.Color,
			SKBlendMode.Luminosity,
		};

		private static IntPtr NewFontStyle (SKFontStyleWeight weight, SKFontStyleSlant slant, string name) =>
			ThrowIfZero (SkiaApi.sk_fontstyle_new ((int)weight, (int)SKFontStyleWidth.Normal, slant), name);

		// A null handle means the native call failed. Because initialization is one-shot, surface it
		// immediately (rather than caching a null field and deferring to a later NullReferenceException).
		private static IntPtr ThrowIfZero (IntPtr handle, string name)
		{
			if (handle == IntPtr.Zero)
				throw new InvalidOperationException (
					$"SkiaSharp failed to initialize the process-global '{name}' singleton (the native library returned a null handle).");
			return handle;
		}
	}
}
