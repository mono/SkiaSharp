#nullable disable

using System;
using System.Runtime.CompilerServices;

#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
	[AttributeUsage (AttributeTargets.Method, Inherited = false)]
	internal sealed class ModuleInitializerAttribute : Attribute
	{
	}
}
#endif

namespace SkiaSharp
{
	internal static class SkiaSharpModuleInitializer
	{
		// Runs at assembly load, before any user code. The native version check is
		// here (rather than in SKObject.cctor) so it fires before any SkiaApi P/Invoke
		// in the lazy singleton getters — otherwise a missing or wrong-version
		// libSkiaSharp surfaces as a raw EntryPointNotFoundException from the singleton
		// getter's first P/Invoke instead of the more actionable "incompatible version
		// range" message from CheckNativeLibraryCompatible.
		//
		// This file does NOT pre-register any singletons. The singleton-init refactor
		// (#3817) intentionally moved that to lazy on first access; reintroducing
		// pre-registration here would re-open the cross-cctor cycle.
#pragma warning disable CA2255 // ModuleInitializer in library code is intentional — see comment above.
		[ModuleInitializer]
#pragma warning restore CA2255
		internal static void Initialize ()
		{
			SkiaSharpVersion.CheckNativeLibraryCompatible (true);
		}
	}
}
