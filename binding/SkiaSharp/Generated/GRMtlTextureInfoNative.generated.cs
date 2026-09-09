using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// gr_mtl_textureinfo_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct GRMtlTextureInfoNative : IEquatable<GRMtlTextureInfoNative> {
		// public const void* fTexture
		public void* fTexture;

		public readonly bool Equals (GRMtlTextureInfoNative obj) =>
#pragma warning disable CS8909
			fTexture == obj.fTexture;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is GRMtlTextureInfoNative f && Equals (f);

		public static bool operator == (GRMtlTextureInfoNative left, GRMtlTextureInfoNative right) =>
			left.Equals (right);

		public static bool operator != (GRMtlTextureInfoNative left, GRMtlTextureInfoNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fTexture);
			return hash.ToHashCode ();
		}

	}
}
