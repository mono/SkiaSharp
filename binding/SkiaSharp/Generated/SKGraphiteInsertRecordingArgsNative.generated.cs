using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_graphite_insert_recording_args_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKGraphiteInsertRecordingArgsNative : IEquatable<SKGraphiteInsertRecordingArgsNative> {
		// public sk_graphite_recording_t* fRecording
		public sk_graphite_recording_t fRecording;

		// public sk_surface_t* fTargetSurface
		public sk_surface_t fTargetSurface;

		// public const sk_graphite_mutable_texture_state_t* fTargetTextureState
		public sk_graphite_mutable_texture_state_t fTargetTextureState;

		// public const sk_graphite_backend_semaphore_t* const* fWaitSemaphores
		public sk_graphite_backend_semaphore_t* fWaitSemaphores;

		// public size_t fWaitSemaphoreCount
		public /* size_t */ IntPtr fWaitSemaphoreCount;

		// public const sk_graphite_backend_semaphore_t* const* fSignalSemaphores
		public sk_graphite_backend_semaphore_t* fSignalSemaphores;

		// public size_t fSignalSemaphoreCount
		public /* size_t */ IntPtr fSignalSemaphoreCount;

		public readonly bool Equals (SKGraphiteInsertRecordingArgsNative obj) =>
#pragma warning disable CS8909
			fRecording == obj.fRecording && fTargetSurface == obj.fTargetSurface && fTargetTextureState == obj.fTargetTextureState && fWaitSemaphores == obj.fWaitSemaphores && fWaitSemaphoreCount == obj.fWaitSemaphoreCount && fSignalSemaphores == obj.fSignalSemaphores && fSignalSemaphoreCount == obj.fSignalSemaphoreCount;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKGraphiteInsertRecordingArgsNative f && Equals (f);

		public static bool operator == (SKGraphiteInsertRecordingArgsNative left, SKGraphiteInsertRecordingArgsNative right) =>
			left.Equals (right);

		public static bool operator != (SKGraphiteInsertRecordingArgsNative left, SKGraphiteInsertRecordingArgsNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fRecording);
			hash.Add (fTargetSurface);
			hash.Add (fTargetTextureState);
			hash.Add (fWaitSemaphores);
			hash.Add (fWaitSemaphoreCount);
			hash.Add (fSignalSemaphores);
			hash.Add (fSignalSemaphoreCount);
			return hash.ToHashCode ();
		}

	}
}
