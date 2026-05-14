#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>
	/// An immutable command list produced by <see cref="SKGraphiteRecorder.Snap"/>.
	/// Insert via <see cref="SKGraphiteContext.InsertRecording(SKGraphiteRecording)"/> to execute
	/// on the GPU; reusing a recording after insertion is undefined behavior.
	/// </summary>
	public unsafe class SKGraphiteRecording : SKObject
	{
		internal SKGraphiteRecording (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void DisposeNative () =>
			SkiaApi.sk_graphite_recording_delete (Handle);
	}
}
