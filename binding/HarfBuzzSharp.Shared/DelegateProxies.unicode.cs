using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	// public delegates

	public delegate UnicodeCombiningClass CombiningClassDelegate (UnicodeFunctions ufuncs, uint unicode);

	public delegate UnicodeGeneralCategory GeneralCategoryDelegate (UnicodeFunctions ufuncs, uint unicode);

	public delegate uint MirroringDelegate (UnicodeFunctions ufuncs, uint unicode);

	public delegate Script ScriptDelegate (UnicodeFunctions ufuncs, uint unicode);

	public delegate bool ComposeDelegate (UnicodeFunctions ufuncs, uint a, uint b, out uint ab);

	public delegate bool DecomposeDelegate (UnicodeFunctions ufuncs, uint ab, out uint a, out uint b);

	// internal proxy delegates

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	public delegate UnicodeCombiningClass hb_unicode_combining_class_func_t (IntPtr ufuncs, uint unicode, IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	public delegate UnicodeGeneralCategory hb_unicode_general_category_func_t (IntPtr ufuncs, uint unicode, IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	public delegate uint hb_unicode_mirroring_func_t (IntPtr ufuncs, uint unicode, IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	public delegate uint hb_unicode_script_func_t (IntPtr ufuncs, uint unicode, IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	public delegate bool hb_unicode_compose_func_t (IntPtr ufuncs, uint a, uint b, out uint ab, IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	public delegate bool hb_unicode_decompose_func_t (IntPtr ufuncs, uint ab, out uint a, out uint b, IntPtr context);

	internal static partial class DelegateProxies
	{
		public static readonly hb_unicode_combining_class_func_t CombiningClassProxy = CombiningClassProxyImplementation;
		public static readonly hb_unicode_general_category_func_t GeneralCategoryProxy = GeneralCategoryProxyImplementation;
		public static readonly hb_unicode_mirroring_func_t MirroringProxy = MirroringProxyImplementation;
		public static readonly hb_unicode_script_func_t ScriptProxy = ScriptProxyImplementation;
		public static readonly hb_unicode_compose_func_t ComposeProxy = ComposeProxyImplementation;
		public static readonly hb_unicode_decompose_func_t DecomposeProxy = DecomposeProxyImplementation;

		[MonoPInvokeCallback (typeof (hb_unicode_combining_class_func_t))]
		private static UnicodeCombiningClass CombiningClassProxyImplementation (IntPtr ufuncs, uint unicode, IntPtr context)
		{
			var del = GetMulti<CombiningClassDelegate> (context, out _);
			var userData = GetUserData<UnicodeFunctions> (context);
			return del.Invoke (userData, unicode);
		}

		[MonoPInvokeCallback (typeof (hb_unicode_general_category_func_t))]
		private static UnicodeGeneralCategory GeneralCategoryProxyImplementation (IntPtr ufuncs, uint unicode, IntPtr context)
		{
			var del = GetMulti<GeneralCategoryDelegate> (context, out _);
			var functions = GetUserData<UnicodeFunctions> (context);
			return del.Invoke (functions, unicode);
		}

		[MonoPInvokeCallback (typeof (hb_unicode_mirroring_func_t))]
		private static uint MirroringProxyImplementation (IntPtr ufuncs, uint unicode, IntPtr context)
		{
			var del = GetMulti<MirroringDelegate> (context, out _);
			var functions = GetUserData<UnicodeFunctions> (context);
			return del.Invoke (functions, unicode);
		}

		[MonoPInvokeCallback (typeof (hb_unicode_script_func_t))]
		private static uint ScriptProxyImplementation (IntPtr ufuncs, uint unicode, IntPtr context)
		{
			var del = GetMulti<ScriptDelegate> (context, out _);
			var functions = GetUserData<UnicodeFunctions> (context);
			return del.Invoke (functions, unicode);
		}

		[MonoPInvokeCallback (typeof (hb_unicode_compose_func_t))]
		private static bool ComposeProxyImplementation (IntPtr ufuncs, uint a, uint b, out uint ab, IntPtr context)
		{
			var del = GetMulti<ComposeDelegate> (context, out _);
			var functions = GetUserData<UnicodeFunctions> (context);
			return del.Invoke (functions, a, b, out ab);
		}

		[MonoPInvokeCallback (typeof (hb_unicode_decompose_func_t))]
		private static bool DecomposeProxyImplementation (IntPtr ufuncs, uint ab, out uint a, out uint b, IntPtr context)
		{
			var del = GetMulti<DecomposeDelegate> (context, out _);
			var functions = GetUserData<UnicodeFunctions> (context);
			return del.Invoke (functions, ab, out a, out b);
		}
	}
}
