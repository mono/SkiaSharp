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
		/// <summary>Gets or sets the OpenGL framebuffer ID.</summary>
		/// <value>The OpenGL framebuffer object identifier.</value>
		/// <remarks />
		public UInt32 FramebufferObjectId {
			readonly get => fFBOID;
			set => fFBOID = value;
		}

		// public unsigned int fFormat
		private UInt32 fFormat;
		/// <summary>Gets or sets the sized, internal format of the OpenGL framebuffer.</summary>
		/// <value>The OpenGL internal format.</value>
		/// <remarks />
		public UInt32 Format {
			readonly get => fFormat;
			set => fFormat = value;
		}

		// public bool fProtected
		private Byte fProtected;
		/// <summary>Gets or sets a value indicating whether the framebuffer is protected.</summary>
		/// <value><see langword="true" /> if the framebuffer is protected; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool Protected {
			readonly get => fProtected > 0;
			set => fProtected = value ? (byte)1 : (byte)0;
		}

		/// <summary>Determines whether the specified <see cref="T:SkiaSharp.GRGlFramebufferInfo" /> is equal to the current instance.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.GRGlFramebufferInfo" /> to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (GRGlFramebufferInfo obj) =>
#pragma warning disable CS8909
			fFBOID == obj.fFBOID && fFormat == obj.fFormat && fProtected == obj.fProtected;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to the current instance.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is GRGlFramebufferInfo f && Equals (f);

		/// <summary>Determines whether two specified <see cref="T:SkiaSharp.GRGlFramebufferInfo" /> instances are equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.GRGlFramebufferInfo" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.GRGlFramebufferInfo" /> to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (GRGlFramebufferInfo left, GRGlFramebufferInfo right) =>
			left.Equals (right);

		/// <summary>Determines whether two specified <see cref="T:SkiaSharp.GRGlFramebufferInfo" /> instances are not equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.GRGlFramebufferInfo" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.GRGlFramebufferInfo" /> to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (GRGlFramebufferInfo left, GRGlFramebufferInfo right) =>
			!left.Equals (right);

		/// <summary>Returns a hash code for this instance.</summary>
		/// <returns>A hash code for the current instance.</returns>
		/// <remarks />
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
