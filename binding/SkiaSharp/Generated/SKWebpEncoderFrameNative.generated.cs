using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_webpencoder_frame_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKWebpEncoderFrameNative : IEquatable<SKWebpEncoderFrameNative> {
		// public const sk_pixmap_t* pixmap
		public sk_pixmap_t pixmap;

		// public int duration
		public Int32 duration;

		public readonly bool Equals (SKWebpEncoderFrameNative obj) =>
#pragma warning disable CS8909
			pixmap == obj.pixmap && duration == obj.duration;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKWebpEncoderFrameNative f && Equals (f);

		public static bool operator == (SKWebpEncoderFrameNative left, SKWebpEncoderFrameNative right) =>
			left.Equals (right);

		public static bool operator != (SKWebpEncoderFrameNative left, SKWebpEncoderFrameNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (pixmap);
			hash.Add (duration);
			return hash.ToHashCode ();
		}

	}
}
