using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_graphite_insert_recording_info_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKGraphiteInsertRecordingInfo : IEquatable<SKGraphiteInsertRecordingInfo> {
		// public sk_graphite_recording_t* fRecording
		private IntPtr fRecording;
		public IntPtr Recording {
			readonly get => fRecording;
			set => fRecording = value;
		}

		// public sk_surface_t* fTargetSurface
		private IntPtr fTargetSurface;
		public IntPtr TargetSurface {
			readonly get => fTargetSurface;
			set => fTargetSurface = value;
		}

		// public int32_t fTargetTranslationX
		private Int32 fTargetTranslationX;
		public Int32 TargetTranslationX {
			readonly get => fTargetTranslationX;
			set => fTargetTranslationX = value;
		}

		// public int32_t fTargetTranslationY
		private Int32 fTargetTranslationY;
		public Int32 TargetTranslationY {
			readonly get => fTargetTranslationY;
			set => fTargetTranslationY = value;
		}

		// public sk_irect_t fTargetClip
		private SKRectI fTargetClip;
		public SKRectI TargetClip {
			readonly get => fTargetClip;
			set => fTargetClip = value;
		}

		public readonly bool Equals (SKGraphiteInsertRecordingInfo obj) =>
#pragma warning disable CS8909
			fRecording == obj.fRecording && fTargetSurface == obj.fTargetSurface && fTargetTranslationX == obj.fTargetTranslationX && fTargetTranslationY == obj.fTargetTranslationY && fTargetClip == obj.fTargetClip;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKGraphiteInsertRecordingInfo f && Equals (f);

		public static bool operator == (SKGraphiteInsertRecordingInfo left, SKGraphiteInsertRecordingInfo right) =>
			left.Equals (right);

		public static bool operator != (SKGraphiteInsertRecordingInfo left, SKGraphiteInsertRecordingInfo right) =>
			!left.Equals (right);

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
