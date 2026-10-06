using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_textblob_builder_runbuffer_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKRunBufferInternal : IEquatable<SKRunBufferInternal> {
		// public void* glyphs
		public void* glyphs;

		// public void* pos
		public void* pos;

		// public void* utf8text
		public void* utf8text;

		// public void* clusters
		public void* clusters;

		public readonly bool Equals (SKRunBufferInternal obj) =>
#pragma warning disable CS8909
			glyphs == obj.glyphs && pos == obj.pos && utf8text == obj.utf8text && clusters == obj.clusters;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKRunBufferInternal f && Equals (f);

		public static bool operator == (SKRunBufferInternal left, SKRunBufferInternal right) =>
			left.Equals (right);

		public static bool operator != (SKRunBufferInternal left, SKRunBufferInternal right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (glyphs);
			hash.Add (pos);
			hash.Add (utf8text);
			hash.Add (clusters);
			return hash.ToHashCode ();
		}

	}
}
