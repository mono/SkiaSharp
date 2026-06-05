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
	// Runs the native-library compatibility check once, at assembly load, before any SkiaSharp API is
	// used. This is intentionally SEPARATE from process-global singleton initialization (see
	// SkiaSharpStatics): the compatibility guard must run for EVERY consumer — including code paths that
	// only touch non-singleton types like SKBitmap or SKCanvas — so it cannot live in the singleton path,
	// which only initializes when a singleton is first touched. Conversely, the singleton handles must not
	// be forced at module load. Do NOT fold the singleton init into this module initializer: a
	// [ModuleInitializer] compiles to the module's type initializer and would reintroduce the #3817
	// type-initializer re-entrancy that SkiaSharpStatics exists to avoid.
	internal static class SkiaSharpModuleInitializer
	{
#pragma warning disable CA2255 // ModuleInitializer in library code is intentional and acceptable in this case
		[ModuleInitializer]
#pragma warning restore CA2255
		internal static void Initialize ()
		{
			SkiaSharpVersion.CheckNativeLibraryCompatible (true);
		}
	}
}
