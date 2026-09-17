using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_graphite_insert_recording_info_t
	/// <summary>Specifies a recording to insert into a <see cref="T:SkiaSharp.SKGraphiteContext" /> along with the target surface and placement.</summary>
	/// <remarks />
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKGraphiteInsertRecordingInfo : IEquatable<SKGraphiteInsertRecordingInfo> {
		// public sk_graphite_recording_t* fRecording
		private sk_graphite_recording_t fRecording;
		/// <summary>Gets or sets the handle to the recording to insert.</summary>
		/// <value>A handle to the recording.</value>
		/// <remarks />
		public sk_graphite_recording_t Recording {
			readonly get => fRecording;
			set => fRecording = value;
		}

		// public sk_surface_t* fTargetSurface
		private sk_surface_t fTargetSurface;
		/// <summary>Gets or sets the handle to the surface that the recording targets.</summary>
		/// <value>A handle to the target surface, or <see cref="F:System.IntPtr.Zero" /> to use the surface the recording was made for.</value>
		/// <remarks />
		public sk_surface_t TargetSurface {
			readonly get => fTargetSurface;
			set => fTargetSurface = value;
		}

		// public int32_t fTargetTranslationX
		private Int32 fTargetTranslationX;
		/// <summary>Gets or sets the horizontal translation applied to the recording on the target surface.</summary>
		/// <value>The horizontal translation, in pixels.</value>
		/// <remarks />
		public Int32 TargetTranslationX {
			readonly get => fTargetTranslationX;
			set => fTargetTranslationX = value;
		}

		// public int32_t fTargetTranslationY
		private Int32 fTargetTranslationY;
		/// <summary>Gets or sets the vertical translation applied to the recording on the target surface.</summary>
		/// <value>The vertical translation, in pixels.</value>
		/// <remarks />
		public Int32 TargetTranslationY {
			readonly get => fTargetTranslationY;
			set => fTargetTranslationY = value;
		}

		// public sk_irect_t fTargetClip
		private SKRectI fTargetClip;
		/// <summary>Gets or sets the clip rectangle applied to the recording on the target surface.</summary>
		/// <value>The clip rectangle.</value>
		/// <remarks />
		public SKRectI TargetClip {
			readonly get => fTargetClip;
			set => fTargetClip = value;
		}

		/// <summary>Determines whether the specified recording insertion info is equal to the current recording insertion info.</summary>
		/// <param name="obj">The recording insertion info to compare with the current recording insertion info.</param>
		/// <returns><see langword="true" /> if the two values are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKGraphiteInsertRecordingInfo obj) =>
#pragma warning disable CS8909
			fRecording == obj.fRecording && fTargetSurface == obj.fTargetSurface && fTargetTranslationX == obj.fTargetTranslationX && fTargetTranslationY == obj.fTargetTranslationY && fTargetClip == obj.fTargetClip;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to the current recording insertion info.</summary>
		/// <param name="obj">The object to compare with the current recording insertion info.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current value; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKGraphiteInsertRecordingInfo f && Equals (f);

		/// <summary>Indicates whether two recording insertion info values are equal.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true" /> if the two values are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKGraphiteInsertRecordingInfo left, SKGraphiteInsertRecordingInfo right) =>
			left.Equals (right);

		/// <summary>Indicates whether two recording insertion info values are not equal.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true" /> if the two values are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKGraphiteInsertRecordingInfo left, SKGraphiteInsertRecordingInfo right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this recording insertion info.</summary>
		/// <returns>A hash code for the current value.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fRecording);
			hash.Add (fTargetSurface);
			hash.Add (fTargetTranslationX);
			hash.Add (fTargetTranslationY);
			hash.Add (fTargetClip);
			return hash.ToHashCode ();
		}

	}
}
