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
#pragma warning disable CA2255 // ModuleInitializer in library code is intentional and acceptable in this case
		[ModuleInitializer]
#pragma warning restore CA2255
		internal static void Initialize ()
		{
			SkiaSharpVersion.CheckNativeLibraryCompatible (true);
		}
	}
}
