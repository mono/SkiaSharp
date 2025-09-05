#nullable disable
// ReSharper disable InconsistentNaming
// ReSharper disable PartialMethodParameterNameMismatch

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	// public delegates

	/// <summary>
	/// The delegate that is used when releasing the memory for a bitmap.
	/// </summary>
	/// <param name="address">The memory address of the pixels being released.</param>
	/// <param name="context">The user data that was provided when installing the pixels.</param>
	public delegate void SKBitmapReleaseDelegate (IntPtr address, object context);

	/// <summary>
	/// The delegate that is used when a <see cref="T:SkiaSharp.SKData" /> instance is about to be released.
	/// </summary>
	/// <param name="address">The pointer to the byte buffer.</param>
	/// <param name="context">The user state passed to <see cref="M:SkiaSharp.SKData.Create(System.IntPtr,System.Int32,SkiaSharp.SKDataReleaseDelegate,System.Object)" />.</param>
	public delegate void SKDataReleaseDelegate (IntPtr address, object context);

	/// <summary>
	/// The delegate that is used when releasing the memory for a raster-based image.
	/// </summary>
	/// <param name="pixels">The memory address of the pixels being released.</param>
	/// <param name="context">The user data that was provided when creating the image.</param>
	public delegate void SKImageRasterReleaseDelegate (IntPtr pixels, object context);

	/// <summary>
	/// The delegate that is used when releasing the memory for a texture-based image.
	/// </summary>
	/// <param name="context">The context of the image.</param>
	public delegate void SKImageTextureReleaseDelegate (object context);

	/// <summary>
	/// The delegate that is used when releasing the memory for a surface.
	/// </summary>
	/// <param name="address">The memory address of the pixels being released.</param>
	/// <param name="context">The user data that was provided when creating the surface.</param>
	public delegate void SKSurfaceReleaseDelegate (IntPtr address, object context);

	/// <param name="name"></param>
	public delegate IntPtr GRGlGetProcedureAddressDelegate (string name);

	/// <param name="name"></param>
	/// <param name="instance"></param>
	/// <param name="device"></param>
	public delegate IntPtr GRVkGetProcedureAddressDelegate (string name, IntPtr instance, IntPtr device);

	/// <param name="path"></param>
	/// <param name="matrix"></param>
	public delegate void SKGlyphPathDelegate (SKPath path, SKMatrix matrix);

	internal static unsafe partial class DelegateProxies
	{
		// internal proxy implementations

		private static partial void SKBitmapReleaseProxyImplementation (void* address, void* context)
		{
			var del = Get<SKBitmapReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)address, null);
			} finally {
				gch.Free ();
			}
		}

		private static partial void SKDataReleaseProxyImplementation (void* address, void* context)
		{
			var del = Get<SKDataReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)address, null);
			} finally {
				gch.Free ();
			}
		}

		private static partial void SKImageRasterReleaseProxyImplementation (void* pixels, void* context)
		{
			var del = Get<SKImageRasterReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)pixels, null);
			} finally {
				gch.Free ();
			}
		}

		private static partial void SKImageTextureReleaseProxyImplementation (void* context)
		{
			var del = Get<SKImageTextureReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke (null);
			} finally {
				gch.Free ();
			}
		}

		private static partial void SKSurfaceRasterReleaseProxyImplementation (void* address, void* context)
		{
			var del = Get<SKSurfaceReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ((IntPtr)address, null);
			} finally {
				gch.Free ();
			}
		}

		private static partial void SKImageRasterReleaseProxyImplementationForCoTaskMem (void* pixels, void* context)
		{
			Marshal.FreeCoTaskMem ((IntPtr)pixels);
		}

		private static partial IntPtr GRGlGetProcProxyImplementation (void* context, void* name)
		{
			var del = Get<GRGlGetProcedureAddressDelegate> ((IntPtr)context, out _);
			return del.Invoke (Marshal.PtrToStringAnsi ((IntPtr)name));
		}

		private static partial IntPtr GRVkGetProcProxyImplementation (void* context, void* name, IntPtr instance, IntPtr device)
		{
			var del = Get<GRVkGetProcedureAddressDelegate> ((IntPtr)context, out _);

			return del.Invoke (Marshal.PtrToStringAnsi ((IntPtr)name), instance, device);
		}

		private static partial void SKGlyphPathProxyImplementation (IntPtr pathOrNull, SKMatrix* matrix, void* context)
		{
			var del = Get<SKGlyphPathDelegate> ((IntPtr)context, out _);
			var path = SKPath.GetObject (pathOrNull, false);
			del.Invoke (path, *matrix);
		}
	}
}
