//
// Bindings for GRContext
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2016 Xamarin Inc
//

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	// public delegates
	public delegate IntPtr GRGlGetProcDelegate (object context, string name);

	// internal proxy delegates
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate IntPtr GRGlGetProcDelegateInternal (IntPtr context, [MarshalAs(UnmanagedType.LPStr)] string name);

	public class GRGlInterface : SKObject
	{
		// so the GC doesn't collect the delegate
		private static readonly GRGlGetProcDelegateInternal getProcDelegate;
		static GRGlInterface ()
		{
			getProcDelegate = new GRGlGetProcDelegateInternal (GrGLGetProcInternal);
		}

		[Preserve]
		internal GRGlInterface (IntPtr h, bool owns)
			: base (h, owns)
		{
		}
		
		public static GRGlInterface CreateDefaultInterface ()
		{
			return GetObject<GRGlInterface> (SkiaApi.gr_glinterface_default_interface ());
		}

		public static GRGlInterface CreateNativeInterface ()
		{
			return GetObject<GRGlInterface> (SkiaApi.gr_glinterface_create_native_interface ());
		}

		public static GRGlInterface AssembleInterface (GRGlGetProcDelegate get)
		{
			return AssembleInterface (null, get);
		}

		public static GRGlInterface AssembleInterface (object context, GRGlGetProcDelegate get)
		{
			var del = Marshal.GetFunctionPointerForDelegate (getProcDelegate);

			var ctx = new GRGlGetProcDelegateContext (context, get);
			var ptr = ctx.Wrap ();
			var glInterface = GetObject<GRGlInterface> (SkiaApi.gr_glinterface_assemble_interface (ptr, del));
			GRGlGetProcDelegateContext.Free (ptr);
			return glInterface;
		}

		public static GRGlInterface AssembleGlInterface (GRGlGetProcDelegate get)
		{
			return AssembleGlInterface (null, get);
		}

		public static GRGlInterface AssembleGlInterface (object context, GRGlGetProcDelegate get)
		{
			var del = Marshal.GetFunctionPointerForDelegate (getProcDelegate);

			var ctx = new GRGlGetProcDelegateContext (context, get);
			var ptr = ctx.Wrap ();
			var glInterface = GetObject<GRGlInterface> (SkiaApi.gr_glinterface_assemble_gl_interface (ptr, del));
			GRGlGetProcDelegateContext.Free (ptr);
			return glInterface;
		}

		public static GRGlInterface AssembleGlesInterface (GRGlGetProcDelegate get)
		{
			return AssembleGlesInterface (null, get);
		}

		public static GRGlInterface AssembleGlesInterface (object context, GRGlGetProcDelegate get)
		{
			var del = Marshal.GetFunctionPointerForDelegate (getProcDelegate);

			var ctx = new GRGlGetProcDelegateContext (context, get);
			var ptr = ctx.Wrap ();
			var glInterface = GetObject<GRGlInterface> (SkiaApi.gr_glinterface_assemble_gles_interface (ptr, del));
			GRGlGetProcDelegateContext.Free (ptr);
			return glInterface;
		}

		public GRGlInterface Clone ()
		{
			return GetObject<GRGlInterface> (SkiaApi.gr_glinterface_clone (Handle));
		}

		public bool Validate ()
		{
			return SkiaApi.gr_glinterface_validate (Handle);
		}

		public bool HasExtension (string extension)
		{
			return SkiaApi.gr_glinterface_has_extension (Handle, extension);
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.gr_glinterface_unref (Handle);
			}

			base.Dispose (disposing);
		}

		// internal proxy
		#if __IOS__
		[ObjCRuntime.MonoPInvokeCallback (typeof (GRGlGetProcDelegateInternal))]
		#endif
		private static IntPtr GrGLGetProcInternal (IntPtr context, string name)
		{
			var ctx = GRGlGetProcDelegateContext.Unwrap (context);
			return ctx.GetProc (ctx.Context, name);
		}

		// This is the actual context passed to native code.
		// Instead of marshalling the user's data as an IntPtr and requiring 
		// him to wrap/unwarp, we do it via a proxy class. This also prevents 
		// us from having to marshal the user's callback too. 
		private struct GRGlGetProcDelegateContext
		{
			// the "managed version" of the callback 
			public readonly GRGlGetProcDelegate GetProc;
			public readonly object Context;

			public GRGlGetProcDelegateContext (object context, GRGlGetProcDelegate get)
			{
				Context = context;
				GetProc = get;
			}

			// wrap this context into a "native" pointer
			public IntPtr Wrap ()
			{
				var gc = GCHandle.Alloc (this, GCHandleType.Pinned);
				return GCHandle.ToIntPtr (gc);
			}

			// unwrap the "native" pointer into a managed context
			public static GRGlGetProcDelegateContext Unwrap (IntPtr ptr)
			{
				var gchandle = GCHandle.FromIntPtr (ptr);
				return (GRGlGetProcDelegateContext) gchandle.Target;
			}

			// unwrap and free the context
			public static void Free (IntPtr ptr)
			{
				var gchandle = GCHandle.FromIntPtr (ptr);
				gchandle.Free ();
			}
		}
	}
}

