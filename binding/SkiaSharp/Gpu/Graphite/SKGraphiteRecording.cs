#nullable disable

using System;

namespace SkiaSharp
{
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
