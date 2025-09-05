#nullable disable
// ReSharper disable PartialMethodParameterNameMismatch

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	public delegate UnicodeCombiningClass CombiningClassDelegate (UnicodeFunctions ufuncs, uint unicode);

	public delegate UnicodeGeneralCategory GeneralCategoryDelegate (UnicodeFunctions ufuncs, uint unicode);

	public delegate uint MirroringDelegate (UnicodeFunctions ufuncs, uint unicode);

	public delegate Script ScriptDelegate (UnicodeFunctions ufuncs, uint unicode);

	public delegate bool ComposeDelegate (UnicodeFunctions ufuncs, uint a, uint b, out uint ab);

	public delegate bool DecomposeDelegate (UnicodeFunctions ufuncs, uint ab, out uint a, out uint b);

	internal static unsafe partial class DelegateProxies
	{
		private static partial int UnicodeCombiningClassProxyImplementation (IntPtr ufuncs, uint unicode, void* context)
		{
			GetMultiUserData<CombiningClassDelegate, UnicodeFunctions> ((IntPtr)context, out var del, out var functions, out _);
			return (int)del.Invoke (functions, unicode);
		}

		private static partial int UnicodeGeneralCategoryProxyImplementation (IntPtr ufuncs, uint unicode, void* context)
		{
			GetMultiUserData<GeneralCategoryDelegate, UnicodeFunctions> ((IntPtr)context, out var del, out var functions, out _);
			return (int)del.Invoke (functions, unicode);
		}

		private static partial uint UnicodeMirroringProxyImplementation (IntPtr ufuncs, uint unicode, void* context)
		{
			GetMultiUserData<MirroringDelegate, UnicodeFunctions> ((IntPtr)context, out var del, out var functions, out _);
			return del.Invoke (functions, unicode);
		}

		private static partial uint UnicodeScriptProxyImplementation (IntPtr ufuncs, uint unicode, void* context)
		{
			GetMultiUserData<ScriptDelegate, UnicodeFunctions> ((IntPtr)context, out var del, out var functions, out _);
			return del.Invoke (functions, unicode);
		}

		private static partial bool UnicodeComposeProxyImplementation (IntPtr ufuncs, uint a, uint b, uint* ab, void* context)
		{
			GetMultiUserData<ComposeDelegate, UnicodeFunctions> ((IntPtr)context, out var del, out var functions, out _);
			var result = del.Invoke (functions, a, b, out var abManaged);
			if (ab != null)
				*ab = abManaged;
			return result;
		}

		private static partial bool UnicodeDecomposeProxyImplementation (IntPtr ufuncs, uint ab, uint* a, uint* b, void* context)
		{
			GetMultiUserData<DecomposeDelegate, UnicodeFunctions> ((IntPtr)context, out var del, out var functions, out _);
			var result = del.Invoke (functions, ab, out var aManaged, out var bManaged);
			if (a != null)
				*a = aManaged;
			if (b != null)
				*b = bManaged;
			return result;
		}
	}
}
