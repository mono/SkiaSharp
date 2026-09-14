using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// gr_gl_textureinfo_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GRGlTextureInfo : IEquatable<GRGlTextureInfo> {
		// public unsigned int fTarget
		private UInt32 fTarget;
		public UInt32 Target {
			readonly get => fTarget;
			set => fTarget = value;
		}

		// public unsigned int fID
		private UInt32 fID;
		public UInt32 Id {
			readonly get => fID;
			set => fID = value;
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

		public readonly bool Equals (GRGlTextureInfo obj) =>
#pragma warning disable CS8909
			fTarget == obj.fTarget && fID == obj.fID && fFormat == obj.fFormat && fProtected == obj.fProtected;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is GRGlTextureInfo f && Equals (f);

		public static bool operator == (GRGlTextureInfo left, GRGlTextureInfo right) =>
			left.Equals (right);

		public static bool operator != (GRGlTextureInfo left, GRGlTextureInfo right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fTarget);
			hash.Add (fID);
			hash.Add (fFormat);
			hash.Add (fProtected);
			return hash.ToHashCode ();
		}

	}
}
