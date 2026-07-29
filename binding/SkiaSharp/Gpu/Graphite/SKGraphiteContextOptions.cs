#nullable disable

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
#if THROW_OBJECT_EXCEPTIONS
	using GCHandle = SkiaSharp.GCHandleProxy;
#endif

	// Managed options passed to SKGraphiteContext.CreateDawn / CreateMetal / CreateVulkan.
	//
	// Kept as a struct so callers get value semantics + object-initializer syntax
	// (`new SKGraphiteContextOptions { GpuBudgetInBytes = ..., ShaderErrorHandler = ... }`),
	// but declared as a hand-written wrapper — NOT the generated ABI struct — so the
	// ShaderErrorHandler property can be a real managed delegate instead of a raw
	// IntPtr. The ABI shape lives in SKGraphiteContextOptionsNative.
	/// <summary>Options that control how a <see cref="T:SkiaSharp.SKGraphiteContext" /> is created.</summary>
	/// <remarks />
	public struct SKGraphiteContextOptions : IEquatable<SKGraphiteContextOptions>
	{
		/// <summary>Gets or sets a value indicating whether driver correctness workarounds are disabled.</summary>
		/// <value><see langword="true" /> to disable the workarounds; otherwise <see langword="false" />.</value>
		/// <remarks />
		public bool DisableDriverCorrectnessWorkarounds { get; set; }

		// 0 (default) means "use Skia's default"; valid non-default values are 1, 2, 4, 8, 16.
		/// <summary>Gets or sets the sample count used for internal multisampled render targets.</summary>
		/// <value>The sample count, or 0 to use the Skia default.</value>
		/// <remarks />
		public int InternalMultisampleCount { get; set; }

		private long gpuBudgetInBytes;

		// Negative values (default: -1 via SKGraphiteContextOptions()) tell Skia to use its
		// built-in GPU resource cache size. A non-negative value caps the cache at that size.
		// The parameterless constructor seeds -1; a `default(SKGraphiteContextOptions)` will
		// still zero this out — that is treated as "use Skia default" at marshalling time so
		// zero-init callers do not silently disable the resource cache.
		/// <summary>Gets or sets the maximum size of the GPU resource cache.</summary>
		/// <value>The budget in bytes, or a negative value to use the Skia default.</value>
		/// <remarks />
		public long GpuBudgetInBytes {
			readonly get => gpuBudgetInBytes;
			set => gpuBudgetInBytes = value;
		}

		/// <summary>Gets or sets a value indicating whether recordings must be played back in the order they were recorded.</summary>
		/// <value><see langword="true" /> to require ordered playback; otherwise <see langword="false" />.</value>
		/// <remarks />
		public bool RequireOrderedRecordings { get; set; }

		/// <summary>Gets or sets a value indicating whether debug labels are attached to backend objects.</summary>
		/// <value><see langword="true" /> to attach labels; otherwise <see langword="false" />.</value>
		/// <remarks />
		public bool SetBackendLabels { get; set; }

		// Optional shader-compile diagnostic. Held by the Context for its entire lifetime,
		// so this must be a delegate that survives past CreateDawn/Metal/Vulkan (i.e. NOT
		// a stack-scoped lambda whose captures die at end-of-method).
		/// <summary>Gets or sets the callback invoked when a shader fails to compile.</summary>
		/// <value>The handler to invoke, or <see langword="null" /> to receive no notification.</value>
		/// <remarks>The handler is installed for the lifetime of the context created from these options.</remarks>
		public SKGraphiteShaderErrorHandlerDelegate ShaderErrorHandler { get; set; }

		// The parameterless constructor exists to seed GpuBudgetInBytes = -1 so
		// `new SKGraphiteContextOptions()` uses Skia's default resource-cache budget.
		// `default(SKGraphiteContextOptions)` skips this and lands with 0, which is
		// normalized to -1 at ToNative time.
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKGraphiteContextOptions" /> structure with Skia default values.</summary>
		/// <remarks>Seeds <see cref="P:SkiaSharp.SKGraphiteContextOptions.GpuBudgetInBytes" /> so the context uses the Skia default resource budget rather than a zero-byte cache.</remarks>
		public SKGraphiteContextOptions ()
		{
			gpuBudgetInBytes = -1;
		}

		// Translate to the ABI struct and (if a handler was supplied) allocate the native
		// bridge + pin the delegate. Returns the two disposal artefacts by out-param; the
		// caller must free both after the Skia Context they were installed on has been
		// destroyed (see SKGraphiteContext.DisposeNative). On any handler present the ABI
		// struct's fShaderErrorHandler is set to the bridge handle; otherwise both out-params
		// come back as their default zero values.
		internal unsafe SKGraphiteContextOptionsNative ToNative (
			out GCHandle pinnedHandler,
			out IntPtr nativeHandlerHandle)
		{
			pinnedHandler = default;
			nativeHandlerHandle = IntPtr.Zero;

			var native = new SKGraphiteContextOptionsNative {
				fDisableDriverCorrectnessWorkarounds = DisableDriverCorrectnessWorkarounds ? (byte)1 : (byte)0,
				fInternalMultisampleCount            = InternalMultisampleCount,
				// Zero-init callers land here without touching the constructor. Normalize to
				// -1 so `default(SKGraphiteContextOptions)` does not disable Skia's cache.
				fGpuBudgetInBytes                    = gpuBudgetInBytes == 0 ? -1 : gpuBudgetInBytes,
				fRequireOrderedRecordings            = RequireOrderedRecordings ? (byte)1 : (byte)0,
				fSetBackendLabels                    = SetBackendLabels ? (byte)1 : (byte)0,
			};

			if (ShaderErrorHandler is null)
				return native;

			DelegateProxies.Create (ShaderErrorHandler, out var gch, out var ctx);
			IntPtr handle = SkiaApi.sk_graphite_shader_error_handler_new (
				DelegateProxies.SKGraphiteShaderErrorHandlerProxy,
				(void*)ctx);
			if (handle == IntPtr.Zero) {
				gch.Free ();
				throw new InvalidOperationException (
					"sk_graphite_shader_error_handler_new failed (Graphite not built into libSkiaSharp?)");
			}

			pinnedHandler = gch;
			nativeHandlerHandle = handle;
			native.fShaderErrorHandler = handle;
			return native;
		}

		/// <summary>Indicates whether this instance is equal to another instance of the same type.</summary>
		/// <param name="other">The instance to compare with this instance.</param>
		/// <returns><see langword="true" /> if the instances are equal; otherwise <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKGraphiteContextOptions other) =>
			DisableDriverCorrectnessWorkarounds == other.DisableDriverCorrectnessWorkarounds &&
			InternalMultisampleCount == other.InternalMultisampleCount &&
			gpuBudgetInBytes == other.gpuBudgetInBytes &&
			RequireOrderedRecordings == other.RequireOrderedRecordings &&
			SetBackendLabels == other.SetBackendLabels &&
			ReferenceEquals (ShaderErrorHandler, other.ShaderErrorHandler);

		/// <summary>Indicates whether this instance is equal to the specified object.</summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true" /> if the object is an instance of the same type and is equal; otherwise <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKGraphiteContextOptions o && Equals (o);

		/// <summary>Determines whether two instances are equal.</summary>
		/// <param name="left">The first instance to compare.</param>
		/// <param name="right">The second instance to compare.</param>
		/// <returns><see langword="true" /> if the instances are equal; otherwise <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKGraphiteContextOptions left, SKGraphiteContextOptions right) =>
			left.Equals (right);

		/// <summary>Determines whether two instances are not equal.</summary>
		/// <param name="left">The first instance to compare.</param>
		/// <param name="right">The second instance to compare.</param>
		/// <returns><see langword="true" /> if the instances are not equal; otherwise <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKGraphiteContextOptions left, SKGraphiteContextOptions right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A hash code for the current instance.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (DisableDriverCorrectnessWorkarounds);
			hash.Add (InternalMultisampleCount);
			hash.Add (gpuBudgetInBytes);
			hash.Add (RequireOrderedRecordings);
			hash.Add (SetBackendLabels);
			hash.Add (ShaderErrorHandler);
			return hash.ToHashCode ();
		}
	}
}
