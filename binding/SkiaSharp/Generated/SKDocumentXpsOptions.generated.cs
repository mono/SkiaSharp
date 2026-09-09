using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_document_xps_options_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKDocumentXpsOptions : IEquatable<SKDocumentXpsOptions> {
		// public float fDPI
		private Single fDPI;
		public Single Dpi {
			readonly get => fDPI;
			set => fDPI = value;
		}

		// public bool fAllowNoPngs
		private Byte fAllowNoPngs;
		public bool AllowNoPngs {
			readonly get => fAllowNoPngs > 0;
			set => fAllowNoPngs = value ? (byte)1 : (byte)0;
		}

		public readonly bool Equals (SKDocumentXpsOptions obj) =>
#pragma warning disable CS8909
			fDPI == obj.fDPI && fAllowNoPngs == obj.fAllowNoPngs;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKDocumentXpsOptions f && Equals (f);

		public static bool operator == (SKDocumentXpsOptions left, SKDocumentXpsOptions right) =>
			left.Equals (right);

		public static bool operator != (SKDocumentXpsOptions left, SKDocumentXpsOptions right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fDPI);
			hash.Add (fAllowNoPngs);
			return hash.ToHashCode ();
		}

	}
}
