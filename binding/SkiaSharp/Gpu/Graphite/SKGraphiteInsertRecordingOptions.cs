#nullable disable

using System;

namespace SkiaSharp
{
	public ref struct SKGraphiteInsertRecordingOptions
	{
		public SKSurface TargetSurface { get; set; }

		public SKPointI TargetTranslation { get; set; }

		public SKRectI TargetClip { get; set; }

		public SKGraphiteMutableTextureState TargetTextureState { get; set; }

		public ReadOnlySpan<SKGraphiteBackendSemaphore> WaitSemaphores { get; set; }

		public ReadOnlySpan<SKGraphiteBackendSemaphore> SignalSemaphores { get; set; }

		public SKGraphiteFinishedDelegate Finished { get; set; }
	}
}
