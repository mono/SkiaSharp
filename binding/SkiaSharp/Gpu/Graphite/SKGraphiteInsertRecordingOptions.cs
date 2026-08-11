#nullable enable

using System;

namespace SkiaSharp
{
	/// <summary>Specifies placement, external synchronization, and completion for a Graphite recording.</summary>
	/// <remarks>Descriptors and spans are consumed during insertion; their underlying GPU resources must remain valid until execution completes.</remarks>
	public ref struct SKGraphiteInsertRecordingOptions
	{
		/// <summary>Gets or sets the surface targeted by deferred drawing.</summary>
		/// <value>The target surface, or <see langword="null" /> for a recording without deferred drawing.</value>
		/// <remarks>A target surface is required when a target texture state is supplied.</remarks>
		public SKSurface? TargetSurface { get; set; }

		/// <summary>Gets or sets the translation applied to deferred drawing.</summary>
		/// <value>The translation, in pixels.</value>
		/// <remarks />
		public SKPointI TargetTranslation { get; set; }

		/// <summary>Gets or sets the clip applied to deferred drawing after translation.</summary>
		/// <value>The clip rectangle, or an empty rectangle for no additional clip.</value>
		/// <remarks />
		public SKRectI TargetClip { get; set; }

		/// <summary>Gets or sets the target texture's requested state after execution.</summary>
		/// <value>A state descriptor, or <see langword="null" /> for no requested transition.</value>
		/// <remarks />
		public SKGraphiteMutableTextureState? TargetTextureState { get; set; }

		/// <summary>Gets or sets the semaphores waited on before GPU execution.</summary>
		/// <value>A span of semaphore descriptors.</value>
		/// <remarks>The caller owns the underlying semaphore objects.</remarks>
		public ReadOnlySpan<SKGraphiteBackendSemaphore> WaitSemaphores { get; set; }

		/// <summary>Gets or sets the semaphores signaled after GPU execution.</summary>
		/// <value>A span of semaphore descriptors.</value>
		/// <remarks>The caller owns the underlying semaphore objects.</remarks>
		public ReadOnlySpan<SKGraphiteBackendSemaphore> SignalSemaphores { get; set; }

		/// <summary>Gets or sets the callback notified when execution completes or fails.</summary>
		/// <value>The completion callback, or <see langword="null" /> for no notification.</value>
		/// <remarks>The owning context must process completion to deliver asynchronous notifications.</remarks>
		public SKGraphiteFinishedDelegate? Finished { get; set; }
	}
}
