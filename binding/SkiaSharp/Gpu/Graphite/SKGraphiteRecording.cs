#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>Represents a set of drawing commands that have been captured from a <see cref="T:SkiaSharp.SKGraphiteRecorder" /> and can be inserted into a <see cref="T:SkiaSharp.SKGraphiteContext" /> for execution.</summary>
	/// <remarks><format type="text/markdown"><![CDATA[
	/// ## Remarks
	///
	/// A recording is produced by calling <xref:SkiaSharp.SKGraphiteRecorder.Snap>. Insert it into a context with <xref:SkiaSharp.SKGraphiteContext.InsertRecording(SkiaSharp.SKGraphiteRecording)> and then submit the context to execute the captured work on the GPU.
	///
	/// This type wraps a native Skia resource and implements `IDisposable`. Dispose it once it has been inserted and is no longer needed.
	/// ]]></format></remarks>
	public unsafe class SKGraphiteRecording : SKObject
	{
		internal SKGraphiteRecording (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Releases the native resources used by the recording.</summary>
		/// <remarks />
		protected override void DisposeNative () =>
			SkiaApi.sk_graphite_recording_delete (Handle);
	}
}
