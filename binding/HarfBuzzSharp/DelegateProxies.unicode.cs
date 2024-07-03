#nullable disable

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
#if USE_LIBRARY_IMPORT
		public static readonly delegate* unmanaged[Cdecl] <nint, uint, void*, int> CombiningClassProxy = &CombiningClassProxyImplementation;
		public static readonly delegate* unmanaged[Cdecl] <nint, uint, void*, int> GeneralCategoryProxy = &GeneralCategoryProxyImplementation;
		public static readonly delegate* unmanaged[Cdecl] <nint, uint, void*, uint> MirroringProxy = &MirroringProxyImplementation;
		public static readonly delegate* unmanaged[Cdecl] <nint, uint, void*, uint> ScriptProxy = &ScriptProxyImplementation;
		public static readonly delegate* unmanaged[Cdecl] <nint, uint, uint, uint*, void*, bool> ComposeProxy = &ComposeProxyImplementation;
		public static readonly delegate* unmanaged[Cdecl] <nint, uint, uint*, uint*, void*, bool> DecomposeProxy = &DecomposeProxyImplementation;
#else
		public static readonly UnicodeCombiningClassProxyDelegate CombiningClassProxy = CombiningClassProxyImplementation;
		public static readonly UnicodeGeneralCategoryProxyDelegate GeneralCategoryProxy = GeneralCategoryProxyImplementation;
		public static readonly UnicodeMirroringProxyDelegate MirroringProxy = MirroringProxyImplementation;
		public static readonly UnicodeScriptProxyDelegate ScriptProxy = ScriptProxyImplementation;
		public static readonly UnicodeComposeProxyDelegate ComposeProxy = ComposeProxyImplementation;
		public static readonly UnicodeDecomposeProxyDelegate DecomposeProxy = DecomposeProxyImplementation;
#endif

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (UnicodeCombiningClassProxyDelegate))]
#endif
		private static int CombiningClassProxyImplementation (IntPtr ufuncs, uint unicode, void* context)
		{
			GetMultiUserData<CombiningClassDelegate, UnicodeFunctions> ((IntPtr)context, out var del, out var functions, out _);
			return (int)del.Invoke (functions, unicode);
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (UnicodeGeneralCategoryProxyDelegate))]
#endif
		private static int GeneralCategoryProxyImplementation (IntPtr ufuncs, uint unicode, void* context)
		{
			GetMultiUserData<GeneralCategoryDelegate, UnicodeFunctions> ((IntPtr)context, out var del, out var functions, out _);
			return (int)del.Invoke (functions, unicode);
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (UnicodeMirroringProxyDelegate))]
#endif
		private static uint MirroringProxyImplementation (IntPtr ufuncs, uint unicode, void* context)
		{
			GetMultiUserData<MirroringDelegate, UnicodeFunctions> ((IntPtr)context, out var del, out var functions, out _);
			return del.Invoke (functions, unicode);
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (UnicodeScriptProxyDelegate))]
#endif
		private static uint ScriptProxyImplementation (IntPtr ufuncs, uint unicode, void* context)
		{
			GetMultiUserData<ScriptDelegate, UnicodeFunctions> ((IntPtr)context, out var del, out var functions, out _);
			return del.Invoke (functions, unicode);
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (UnicodeComposeProxyDelegate))]
#endif
		private static bool ComposeProxyImplementation (IntPtr ufuncs, uint a, uint b, uint* ab, void* context)
		{
			GetMultiUserData<ComposeDelegate, UnicodeFunctions> ((IntPtr)context, out var del, out var functions, out _);
			var result = del.Invoke (functions, a, b, out var abManaged);
			if (ab != null)
				*ab = abManaged;
			return result;
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (UnicodeDecomposeProxyDelegate))]
#endif
		private static bool DecomposeProxyImplementation (IntPtr ufuncs, uint ab, uint* a, uint* b, void* context)
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
