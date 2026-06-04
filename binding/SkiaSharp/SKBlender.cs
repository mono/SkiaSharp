using System;
using System.Collections.Generic;

namespace SkiaSharp;

public unsafe class SKBlender : SKObject, ISKReferenceCounted
{
	// Process-global blend-mode blender singletons. Populated eagerly by
	// SkiaSharpStatics.EnsureInitialized and rooted here so the GC never collects the immortal wrappers.
	// See SkiaSharpStatics (#3817).
	private static Dictionary<SKBlendMode, SKBlender>? blendModeBlenders;
	// Completion latch kept separate from the dictionary reference: the dictionary is rooted up-front
	// (before it is filled) so that every immortal wrapper created below is strongly held even if a
	// later iteration throws. Using the dictionary's non-null-ness as the "done" signal would early-out
	// on a retry with a half-filled dictionary, so a dedicated flag marks full population instead.
	private static bool blendModeBlendersInitialized;

	internal static void InitializeStatics ()
	{
		// Idempotent: a retry after a partial-init failure must neither replace the dictionary (dropping
		// the only strong roots for already-created immortal wrappers) nor re-create wrappers it already
		// holds. The dictionary is rooted before the loop and we fill in only the still-missing modes.
		if (blendModeBlendersInitialized)
			return;

		// Explicitly list all enum values to avoid reflection (AoT compatibility).
		var modes = new SKBlendMode[] {
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

		// Root the dictionary up-front so each immortal wrapper is held even if a later mode throws.
		blendModeBlenders ??= new Dictionary<SKBlendMode, SKBlender> (modes.Length);
		foreach (SKBlendMode mode in modes) {
			if (!blendModeBlenders.ContainsKey (mode))
				blendModeBlenders[mode] = GetDisposeProtectedObject (SkiaApi.sk_blender_new_mode (mode));
		}

		blendModeBlendersInitialized = true;
	}

	internal SKBlender(IntPtr handle, bool owns)
		: base (handle, owns)
	{
	}

	protected override void Dispose (bool disposing) =>
		base.Dispose (disposing);

	public static SKBlender CreateBlendMode (SKBlendMode mode)
	{
		SkiaSharpStatics.EnsureInitialized ();
		if (!blendModeBlenders!.TryGetValue (mode, out var value))
			throw new ArgumentOutOfRangeException (nameof (mode));
		return value;
	}

	public static SKBlender CreateArithmetic (float k1, float k2, float k3, float k4, bool enforcePMColor) =>
		GetObject (SkiaApi.sk_blender_new_arithmetic (k1, k2, k3, k4, enforcePMColor));

	internal static SKBlender GetObject (IntPtr handle) =>
		GetOrAddObject (handle, (h, o) => new SKBlender (h, o));

	// Used only by the process-global blend-mode cache: latch each shared native blender immortal so it
	// is never freed by this wrapper's finalizer or DisposeInternal.
	internal static SKBlender GetDisposeProtectedObject (IntPtr handle) =>
		GetOrAddImmortalSingletonObject (handle, owns: true, unrefExisting: true, (h, o) => new SKBlender (h, o));
}
