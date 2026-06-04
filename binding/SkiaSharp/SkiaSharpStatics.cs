#nullable disable

using System.Threading;

namespace SkiaSharp
{
	// Centralized, eager-on-first-touch initialization of every process-global singleton wrapper
	// (the sRGB color spaces, SKData.Empty, the gamma color filters, the blend-mode blenders, the
	// preset font styles, the default font manager, and the default/empty typefaces).
	//
	// WHY THIS EXISTS / WHY IT IS NOT A TYPE OR MODULE INITIALIZER
	// ------------------------------------------------------------
	// Historically these singletons were created from SKObject's static constructor (.cctor), which
	// chained into every singleton type. That put the whole graph inside the CLR type-initializer
	// machinery and produced the re-entrant crash in #3817:
	//
	//     SKFontManager.Default
	//       -> SKFontManager..cctor (constructs the default manager wrapper)
	//         -> SKObject..ctor (base) -> triggers SKObject..cctor for the FIRST time, mid-cctor
	//           -> SKTypeface..cctor -> reads SKFontManager.Default
	//             -> but SKFontManager..cctor is already running on this thread
	//               -> the CLR hands back the half-built type with a null field -> NullReferenceException
	//
	// Two properties of CLR type initializers make that class of bug unavoidable while init lives in
	// a .cctor (or a [ModuleInitializer], which compiles to the module's type initializer):
	//   1. Re-entrancy on the same thread returns the PARTIALLY initialized type instead of blocking,
	//      so a cross-type dependency (typeface needs the font manager) can observe null fields.
	//   2. A throw is CACHED for the lifetime of the process: the type/module is permanently poisoned
	//      and every later access rethrows the same TypeInitializationException, with no way to retry.
	//
	// Moving initialization OUT of the type-initializer graph into this plain self-synchronized method
	// dissolves both problems:
	//   * Re-entrancy is handled explicitly (the Initializing state below) instead of by the CLR's
	//     "return the half-built type" rule, so cross-type wiring reads already-assigned fields.
	//   * A failure throws an ORDINARY exception at the real call site (with a meaningful stack) and
	//     resets the state so a later call can retry — the assembly is never poisoned.
	//
	// EAGER-ON-FIRST-TOUCH
	// --------------------
	// EnsureInitialized() is called from two places:
	//   * Every public singleton accessor (SKColorSpace.CreateSrgb, SKFontManager.Default, ...), so the
	//     accessor can simply return its eagerly-populated backing field.
	//   * The top of HandleDictionary.GetOrAddObject (BEFORE its lock is taken), so the very first time
	//     any native handle is looked up the singletons are already registered as immortal wrappers.
	//     That restores the pre-#4080 (3.119.x) guarantee: a handle that the singleton shares with a
	//     getter route (e.g. sk_image_get_colorspace returning the process-global sRGB handle) dedups to
	//     the dispose-proof immortal wrapper instead of a fresh mortal one that a caller could dispose
	//     and thereby corrupt every alias of that handle.
	//
	// LOCK ORDERING (deadlock-freedom)
	// --------------------------------
	// EnsureInitialized must run BEFORE the HandleDictionary lock, never while it is held. The fast path
	// is a single volatile read and takes no lock once initialization has completed, so the only time
	// the gate is taken is the first touch, and always in the order gate -> HandleDictionary lock. No
	// path takes the HandleDictionary lock and then the gate, so the two locks can never invert.
	internal static class SkiaSharpStatics
	{
		private const int Uninitialized = 0;
		private const int Initializing = 1;
		private const int Initialized = 2;

		private static int state;
		private static readonly object gate = new object ();

		// Eagerly initialize every process-global singleton on the first touch of any SkiaSharp object.
		// Cheap and lock-free after initialization (one volatile read).
		internal static void EnsureInitialized ()
		{
			if (Volatile.Read (ref state) == Initialized)
				return;

			EnsureInitializedCore ();
		}

		private static void EnsureInitializedCore ()
		{
			lock (gate) {
				// Initialized: another thread already finished — nothing to do.
				// Initializing: this is a re-entrant call from inside InitializeStatics (constructing a
				// registered singleton routes back through here via the GetOrAddObject hook). The monitor
				// is re-entrant, so we get here on the same thread; bail without re-running init. Inter-
				// singleton wiring during init may read the PUBLIC accessor of an EARLIER-initialized
				// dependency (e.g. the typeface reads SKFontManager.Default): that accessor re-enters here,
				// bails on this branch, and returns the already-assigned backing field — safe because of
				// same-thread program order. It must never read a dependency initialized LATER in the
				// chain (or its own not-yet-assigned field), which would observe null.
				if (state != Uninitialized)
					return;

				state = Initializing;
				try {
					// Dependency order: a singleton that references another must come AFTER it, because
					// the dependant reads the dependency's already-assigned backing field.
					SKColorSpace.InitializeStatics ();
					SKColorFilter.InitializeStatics ();
					SKData.InitializeStatics ();
					SKFontStyle.InitializeStatics ();    // leaf; the default typeface reads SKFontStyle.Normal
					SKBlender.InitializeStatics ();
					SKFontManager.InitializeStatics ();  // must precede the typeface (typeface needs the manager)
					SKTypeface.InitializeStatics ();

					Volatile.Write (ref state, Initialized);
				} catch {
					// Do not poison: reset so a subsequent call can retry, and surface the original
					// exception at the caller's site instead of a cached TypeInitializationException.
					state = Uninitialized;
					throw;
				}
			}
		}
	}
}
