using System;
using System.Collections.Generic;

namespace SkiaSharp;

public unsafe class SKBlender : SKObject, ISKReferenceCounted
{
	private static readonly Dictionary<SKBlendMode, SKBlender> blendModeBlenders;

	static SKBlender ()
	{
		// TODO: This is not the best way to do this as it will create a lot of objects that
		//       might not be needed, but it is the only way to ensure that the static
		//       instances are created before any access is made to them.
		//       See more info: SKObject.EnsureStaticInstanceAreInitialized()

		// Explicitly list all enum values to avoid reflection (AoT compatibility)
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
		blendModeBlenders = new Dictionary<SKBlendMode, SKBlender> (modes.Length);
		foreach (SKBlendMode mode in modes)
		{
			blendModeBlenders [mode] = new SKBlenderStatic (SkiaApi.sk_blender_new_mode (mode));
		}
	}

	internal static void EnsureStaticInstanceAreInitialized ()
	{
		// IMPORTANT: do not remove to ensure that the static instances
		//            are initialized before any access is made to them
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

	//

	private sealed class SKBlenderStatic : SKBlender
	{
		internal SKBlenderStatic (IntPtr x)
			: base (x, false)
		{
		}

		protected override void Dispose (bool disposing) { }
	}
}
