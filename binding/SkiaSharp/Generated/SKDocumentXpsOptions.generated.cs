using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_document_xps_options_t
	/// <summary>Options for creating an XPS document with <see cref="T:SkiaSharp.SKDocument" />.</summary>
	/// <remarks><![CDATA[
	/// ## Remarks
	///
	/// `SKDocumentXpsOptions` controls how an XPS document is created by <xref:SkiaSharp.SKDocument.CreateXps*>. Pass an instance of this struct to specify custom DPI or PNG-embedding behavior.
	///
	/// All fields are zero-initialized by default; a `Dpi` of `0` lets the XPS writer choose its own default resolution.
	///
	/// ## Examples
	///
	/// Creating an XPS document at 96 DPI:
	///
	/// ```csharp
	/// using var stream = File.OpenWrite("output.xps");
	/// var opts = new SKDocumentXpsOptions { Dpi = 96f };
	/// using var doc = SKDocument.CreateXps(stream, opts)
	///     ?? throw new PlatformNotSupportedException("XPS is not supported on this platform.");
	/// var page = doc.BeginPage(800, 600);
	/// // ... draw ...
	/// doc.EndPage();
	/// doc.Close();
	/// ```
	/// ]]></remarks>
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKDocumentXpsOptions : IEquatable<SKDocumentXpsOptions> {
		// public float fDPI
		private Single fDPI;
		/// <summary>Gets or sets the dots-per-inch resolution of the XPS document.</summary>
		/// <value>The target DPI of the XPS document, or <c>0</c> to use the encoder's default.</value>
		/// <remarks></remarks>
		public Single Dpi {
			readonly get => fDPI;
			set => fDPI = value;
		}

		// public bool fAllowNoPngs
		private Byte fAllowNoPngs;
		/// <summary>Gets or sets a value indicating whether the XPS encoder may omit PNG image data.</summary>
		/// <value><see langword="true" /> to allow the encoder to omit PNG images and substitute alternative representations; otherwise, <see langword="false" />.</value>
		/// <remarks></remarks>
		public bool AllowNoPngs {
			readonly get => fAllowNoPngs > 0;
			set => fAllowNoPngs = value ? (byte)1 : (byte)0;
		}

		/// <summary>Indicates whether these options are equal to another <see cref="T:SkiaSharp.SKDocumentXpsOptions" />.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKDocumentXpsOptions" /> to compare with this instance.</param>
		/// <returns><see langword="true" /> if both instances have the same DPI and flags; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		public readonly bool Equals (SKDocumentXpsOptions obj) =>
#pragma warning disable CS8909
			fDPI == obj.fDPI && fAllowNoPngs == obj.fAllowNoPngs;
#pragma warning restore CS8909

		/// <summary>Indicates whether these options are equal to the specified object.</summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true" /> if <paramref name="obj" /> is a <see cref="T:SkiaSharp.SKDocumentXpsOptions" /> with the same values; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		public readonly override bool Equals (object obj) =>
			obj is SKDocumentXpsOptions f && Equals (f);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKDocumentXpsOptions" /> values are equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKDocumentXpsOptions" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKDocumentXpsOptions" /> to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		public static bool operator == (SKDocumentXpsOptions left, SKDocumentXpsOptions right) =>
			left.Equals (right);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKDocumentXpsOptions" /> values are not equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKDocumentXpsOptions" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKDocumentXpsOptions" /> to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		public static bool operator != (SKDocumentXpsOptions left, SKDocumentXpsOptions right) =>
			!left.Equals (right);

		/// <summary>Returns a hash code for these XPS options.</summary>
		/// <returns>A hash code for this <see cref="T:SkiaSharp.SKDocumentXpsOptions" /> instance.</returns>
		/// <remarks></remarks>
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fDPI);
			hash.Add (fAllowNoPngs);
			return hash.ToHashCode ();
		}

	}
}
