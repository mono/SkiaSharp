#nullable disable

using System;
using System.ComponentModel;

namespace SkiaSharp
{
	/// <summary>
	/// Describes properties and constraints of a given <see cref="T:SkiaSharp.SKSurface" />.
	/// </summary>
	/// <remarks>The rendering engine can parse these during drawing, and can sometimes optimize its performance (e.g. disabling an expensive feature).</remarks>
	public class SKSurfaceProperties : SKObject
	{
		internal SKSurfaceProperties (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.SKSurfaceProperties" /> instance.
		/// </summary>
		/// <param name="pixelGeometry">The description of how the LCD strips are arranged for each pixel.</param>
		public SKSurfaceProperties (SKPixelGeometry pixelGeometry)
			: this ((uint)0, pixelGeometry)
		{
		}

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.SKSurfaceProperties" /> instance.
		/// </summary>
		/// <param name="flags">The flags to use when creating the surface.</param>
		/// <param name="pixelGeometry">The LCD geometry of each pixel on the surface.</param>
		public SKSurfaceProperties (uint flags, SKPixelGeometry pixelGeometry)
			: this (SkiaApi.sk_surfaceprops_new (flags, pixelGeometry), true)
		{
		}

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.SKSurfaceProperties" /> instance.
		/// </summary>
		/// <param name="flags">The flags to use when creating the surface.</param>
		/// <param name="pixelGeometry">The LCD geometry of each pixel on the surface.</param>
		public SKSurfaceProperties (SKSurfacePropsFlags flags, SKPixelGeometry pixelGeometry)
			: this (SkiaApi.sk_surfaceprops_new ((uint)flags, pixelGeometry), true)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_surfaceprops_delete (Handle);

		/// <summary>
		/// Gets or sets the flags.
		/// </summary>
		public SKSurfacePropsFlags Flags =>
			(SKSurfacePropsFlags)SkiaApi.sk_surfaceprops_get_flags (Handle);

		/// <summary>
		/// Gets or sets the LCD geometry of each pixel on the surface.
		/// </summary>
		public SKPixelGeometry PixelGeometry =>
			SkiaApi.sk_surfaceprops_get_pixel_geometry (Handle);

		/// <summary>
		/// Gets a value indicating whether the surface should use device independent fonts.
		/// </summary>
		public bool IsUseDeviceIndependentFonts =>
			Flags.HasFlag (SKSurfacePropsFlags.UseDeviceIndependentFonts);

		internal static SKSurfaceProperties GetObject (IntPtr handle, bool owns = true) =>
			GetOrAddObject (handle, owns, (h, o) => new SKSurfaceProperties (h, o));
	}
}
