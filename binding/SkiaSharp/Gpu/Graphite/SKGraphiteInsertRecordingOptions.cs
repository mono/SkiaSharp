#nullable enable

using System;

namespace SkiaSharp
{
	/// <summary>Specifies target surface, external synchronization, and completion for a Graphite recording.</summary>
	/// <remarks>Descriptors and spans are consumed during insertion; their underlying GPU resources must remain valid until execution completes.</remarks>
	public ref struct SKGraphiteInsertRecordingOptions
	{
		/// <summary>Gets or sets the target surface for the recording.</summary>
		/// <value>The target surface, or <see langword="null" /> for a recording without an explicit target surface.</value>
		/// <remarks>A target surface is required when a target texture state is supplied.</remarks>
		public SKSurface? TargetSurface { get; set; }

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
		/// <remarks>The owning context must process completion to deliver asynchronous notifications. The callback must not throw exceptions.</remarks>
		public SKGraphiteFinishedDelegate? Finished { get; set; }
	}
}
