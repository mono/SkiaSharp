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
	public struct SKGraphiteContextOptions : IEquatable<SKGraphiteContextOptions>
	{
		public bool DisableDriverCorrectnessWorkarounds { get; set; }

		// 0 (default) means "use Skia's default"; valid non-default values are 1, 2, 4, 8, 16.
		public int InternalMultisampleCount { get; set; }

		private long gpuBudgetInBytes;

		// Negative values (default: -1 via SKGraphiteContextOptions()) tell Skia to use its
		// built-in GPU resource cache size. A non-negative value caps the cache at that size.
		// The parameterless constructor seeds -1; a `default(SKGraphiteContextOptions)` will
		// still zero this out — that is treated as "use Skia default" at marshalling time so
		// zero-init callers do not silently disable the resource cache.
		public long GpuBudgetInBytes {
			readonly get => gpuBudgetInBytes;
			set => gpuBudgetInBytes = value;
		}

		public bool RequireOrderedRecordings { get; set; }

		public bool SetBackendLabels { get; set; }

		// Optional shader-compile diagnostic. Held by the Context for its entire lifetime,
		// so this must be a delegate that survives past CreateDawn/Metal/Vulkan (i.e. NOT
		// a stack-scoped lambda whose captures die at end-of-method).
		public SKGraphiteShaderErrorHandlerDelegate ShaderErrorHandler { get; set; }

		// The parameterless constructor exists to seed GpuBudgetInBytes = -1 so
		// `new SKGraphiteContextOptions()` uses Skia's default resource-cache budget.
		// `default(SKGraphiteContextOptions)` skips this and lands with 0, which is
		// normalized to -1 at ToNative time.
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

		public readonly bool Equals (SKGraphiteContextOptions other) =>
			DisableDriverCorrectnessWorkarounds == other.DisableDriverCorrectnessWorkarounds &&
			InternalMultisampleCount == other.InternalMultisampleCount &&
			gpuBudgetInBytes == other.gpuBudgetInBytes &&
			RequireOrderedRecordings == other.RequireOrderedRecordings &&
			SetBackendLabels == other.SetBackendLabels &&
			ReferenceEquals (ShaderErrorHandler, other.ShaderErrorHandler);

		public readonly override bool Equals (object obj) =>
			obj is SKGraphiteContextOptions o && Equals (o);

		public static bool operator == (SKGraphiteContextOptions left, SKGraphiteContextOptions right) =>
			left.Equals (right);

		public static bool operator != (SKGraphiteContextOptions left, SKGraphiteContextOptions right) =>
			!left.Equals (right);

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
