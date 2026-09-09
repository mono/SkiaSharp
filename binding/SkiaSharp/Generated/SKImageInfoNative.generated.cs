using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_imageinfo_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKImageInfoNative : IEquatable<SKImageInfoNative> {
		// public sk_colorspace_t* colorspace
		public IntPtr colorspace;

		// public int32_t width
		public Int32 width;

		// public int32_t height
		public Int32 height;

		// public sk_colortype_t colorType
		public SKColorTypeNative colorType;

		// public sk_alphatype_t alphaType
		public SKAlphaType alphaType;

		public readonly bool Equals (SKImageInfoNative obj) =>
#pragma warning disable CS8909
			colorspace == obj.colorspace && width == obj.width && height == obj.height && colorType == obj.colorType && alphaType == obj.alphaType;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKImageInfoNative f && Equals (f);

		public static bool operator == (SKImageInfoNative left, SKImageInfoNative right) =>
			left.Equals (right);

		public static bool operator != (SKImageInfoNative left, SKImageInfoNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (colorspace);
			hash.Add (width);
			hash.Add (height);
			hash.Add (colorType);
			hash.Add (alphaType);
			return hash.ToHashCode ();
		}

	}
}
