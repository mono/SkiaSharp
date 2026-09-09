using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// gr_gl_framebufferinfo_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GRGlFramebufferInfo : IEquatable<GRGlFramebufferInfo> {
		// public unsigned int fFBOID
		private UInt32 fFBOID;
		public UInt32 FramebufferObjectId {
			readonly get => fFBOID;
			set => fFBOID = value;
		}

		// public unsigned int fFormat
		private UInt32 fFormat;
		public UInt32 Format {
			readonly get => fFormat;
			set => fFormat = value;
		}

		// public bool fProtected
		private Byte fProtected;
		public bool Protected {
			readonly get => fProtected > 0;
			set => fProtected = value ? (byte)1 : (byte)0;
		}

		public readonly bool Equals (GRGlFramebufferInfo obj) =>
#pragma warning disable CS8909
			fFBOID == obj.fFBOID && fFormat == obj.fFormat && fProtected == obj.fProtected;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is GRGlFramebufferInfo f && Equals (f);

		public static bool operator == (GRGlFramebufferInfo left, GRGlFramebufferInfo right) =>
			left.Equals (right);

		public static bool operator != (GRGlFramebufferInfo left, GRGlFramebufferInfo right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fFBOID);
			hash.Add (fFormat);
			hash.Add (fProtected);
			return hash.ToHashCode ();
		}

	}
}
