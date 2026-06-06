using System;
using System.Collections.Generic;

namespace SkiaSharp;

public unsafe class SKBlender : SKObject, ISKReferenceCounted
{
	// Process-global immortal blend-mode blender singletons, built once by this type's static
	// constructor from the raw handles SkiaSharpStatics acquired, and rooted here so the GC never
	// collects them. The explicit static constructor makes the type NOT 'beforefieldinit', so the
	// wrappers are registered as immortal before any static method body (including GetObject) runs.
	// See SkiaSharpStatics (#3817).
	private static readonly Dictionary<SKBlendMode, SKBlender> blendModeBlenders;

	static SKBlender ()
	{
		var source = SkiaSharpStatics.BlendModeBlenders;
		blendModeBlenders = new Dictionary<SKBlendMode, SKBlender> (source.Count);
		foreach (var pair in source)
			blendModeBlenders[pair.Key] = GetImmortalSingletonObject (pair.Value);
	}

	internal SKBlender(IntPtr handle, bool owns)
		: base (handle, owns)
	{
	}

	protected override void Dispose (bool disposing) =>
		base.Dispose (disposing);

	public static SKBlender CreateBlendMode (SKBlendMode mode)
	{
		if (!blendModeBlenders.TryGetValue (mode, out var value))
			throw new ArgumentOutOfRangeException (nameof (mode));
		return value;
	}

	public static SKBlender CreateArithmetic (float k1, float k2, float k3, float k4, bool enforcePMColor) =>
		GetObject (SkiaApi.sk_blender_new_arithmetic (k1, k2, k3, k4, enforcePMColor));

	internal static SKBlender GetObject (IntPtr handle) =>
		GetOrAddObject (handle, (h, o) => new SKBlender (h, o));

	// Used only by the process-global blend-mode cache: latch each shared native blender immortal so it
	// is never freed by this wrapper's finalizer or DisposeInternal.
	internal static SKBlender GetImmortalSingletonObject (IntPtr handle) =>
		GetOrAddImmortalSingletonObject (handle, owns: true, unrefExisting: true, (h, o) => new SKBlender (h, o));
}
