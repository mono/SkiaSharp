#nullable disable

using System;
using System.ComponentModel;

namespace SkiaSharp
{
	/// <summary>Describes properties and constraints of a given <see cref="T:SkiaSharp.SKSurface" />.</summary>
	/// <remarks>The rendering engine can parse these during drawing, and can sometimes optimize its performance (e.g. disabling an expensive feature).</remarks>
	public class SKSurfaceProperties : SKObject
	{
		internal SKSurfaceProperties (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKSurfaceProperties" /> instance.</summary>
		/// <param name="pixelGeometry">The description of how the LCD strips are arranged for each pixel.</param>
		/// <remarks />
		public SKSurfaceProperties (SKPixelGeometry pixelGeometry)
			: this ((uint)0, pixelGeometry)
		{
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKSurfaceProperties" /> instance.</summary>
		/// <param name="flags">The flags to use when creating the surface.</param>
		/// <param name="pixelGeometry">The LCD geometry of each pixel on the surface.</param>
		/// <remarks />
		public SKSurfaceProperties (uint flags, SKPixelGeometry pixelGeometry)
			: this (SkiaApi.sk_surfaceprops_new (flags, pixelGeometry), true)
		{
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKSurfaceProperties" /> instance.</summary>
		/// <param name="flags">The flags to use when creating the surface.</param>
		/// <param name="pixelGeometry">The LCD geometry of each pixel on the surface.</param>
		/// <remarks />
		public SKSurfaceProperties (SKSurfacePropsFlags flags, SKPixelGeometry pixelGeometry)
			: this (SkiaApi.sk_surfaceprops_new ((uint)flags, pixelGeometry), true)
		{
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKSurfaceProperties" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKSurfaceProperties" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
		/// <remarks />
		protected override void DisposeNative () =>
			SkiaApi.sk_surfaceprops_delete (Handle);

		/// <summary>Gets the flags.</summary>
		/// <value>The surface property flags.</value>
		/// <remarks />
		public SKSurfacePropsFlags Flags =>
			(SKSurfacePropsFlags)SkiaApi.sk_surfaceprops_get_flags (Handle);

		/// <summary>Gets the LCD geometry of each pixel on the surface.</summary>
		/// <value>The LCD geometry of each pixel on the surface.</value>
		/// <remarks />
		public SKPixelGeometry PixelGeometry =>
			SkiaApi.sk_surfaceprops_get_pixel_geometry (Handle);

		/// <summary>Gets a value indicating whether the surface should use device independent fonts.</summary>
		/// <value><see langword="true" /> if the surface should use device independent fonts; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IsUseDeviceIndependentFonts =>
			Flags.HasFlag (SKSurfacePropsFlags.UseDeviceIndependentFonts);

		internal static SKSurfaceProperties GetObject (IntPtr handle, bool owns = true) =>
			GetOrAddObject (handle, owns, (h, o) => new SKSurfaceProperties (h, o));
	}
}
