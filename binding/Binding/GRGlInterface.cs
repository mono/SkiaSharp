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
			// first try ANGLE, then fall back to the OpenGL-based
			return CreateNativeAngleInterface () ?? CreateNativeGlInterface ();
		}

		[Obsolete ("Use CreateNativeGlInterface() or CreateDefaultInterface() instead. This method will be removed in the next release.")]
		public static GRGlInterface CreateNativeInterface ()
		{
			return CreateNativeGlInterface ();
		}

		public static GRGlInterface CreateNativeGlInterface ()
		{
			// the native code will automatically return null on non-OpenGL platforms, such as UWP
			return GetObject<GRGlInterface> (SkiaApi.gr_glinterface_create_native_interface ());
		}
		
		public static GRGlInterface CreateNativeAngleInterface ()
		{
#if DESKTOP || WINDOWS_UWP
			return AssembleAngleInterface (AngleLoader.GetProc);
#else
			// return null on non-DirectX platforms: everything except Windows
			return null;
#endif
		}

		public static GRGlInterface AssembleInterface (GRGlGetProcDelegate get)
		{
			return AssembleInterface (null, get);
		}

		public static GRGlInterface AssembleInterface (object context, GRGlGetProcDelegate get)
		{
#if DESKTOP || WINDOWS_UWP
			var angle = AssembleAngleInterface (context, get);
#endif
			// always use ANGLE on UWP
#if WINDOWS_UWP
			return angle;
#else
			// first try ANGLE on Desktop
#if DESKTOP
			if (angle != null)
				return angle;
#endif
			// always use the default on non-Windows
			var del = Marshal.GetFunctionPointerForDelegate (getProcDelegate);

			var ctx = new GRGlGetProcDelegateContext (context, get);
			var ptr = ctx.Wrap ();
			var glInterface = GetObject<GRGlInterface> (SkiaApi.gr_glinterface_assemble_interface (ptr, del));
			GRGlGetProcDelegateContext.Free (ptr);
			return glInterface;
#endif
		}

		public static GRGlInterface AssembleAngleInterface (GRGlGetProcDelegate get)
		{
			return AssembleAngleInterface (null, get);
		}

		public static GRGlInterface AssembleAngleInterface (object context, GRGlGetProcDelegate get)
		{
			// ANGLE is just a GLES v2 over DX v9+
			return AssembleGlesInterface (context, get);
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
			// instead of pinning the struct, we pin a GUID which is paired to the struct
			private static readonly IDictionary<Guid, GRGlGetProcDelegateContext> contexts = new Dictionary<Guid, GRGlGetProcDelegateContext>();

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
				var guid = Guid.NewGuid ();
				lock (contexts) {
					contexts.Add (guid, this);
				}
				var gc = GCHandle.Alloc (guid, GCHandleType.Pinned);
				return GCHandle.ToIntPtr (gc);
			}

			// unwrap the "native" pointer into a managed context
			public static GRGlGetProcDelegateContext Unwrap (IntPtr ptr)
			{
				var gchandle = GCHandle.FromIntPtr (ptr);
				var guid = (Guid) gchandle.Target;
				lock (contexts) {
					GRGlGetProcDelegateContext value;
					contexts.TryGetValue (guid, out value);
					return value;
				}
			}

			// unwrap and free the context
			public static void Free (IntPtr ptr)
			{
				var gchandle = GCHandle.FromIntPtr (ptr);
				var guid = (Guid) gchandle.Target;
				lock (contexts) {
					contexts.Remove (guid);
				}
				gchandle.Free ();
			}
		}

#if DESKTOP || WINDOWS_UWP
		private static class AngleLoader
		{
			private static readonly IntPtr libEGL;
			private static readonly IntPtr libGLESv2;

#if WINDOWS_UWP
			// https://msdn.microsoft.com/en-us/library/windows/desktop/mt186421(v=vs.85).aspx

			[DllImport ("api-ms-win-core-libraryloader-l2-1-0.dll", SetLastError = true, CharSet = CharSet.Ansi)]
			private static extern IntPtr LoadPackagedLibrary ([MarshalAs (UnmanagedType.LPWStr)] string lpFileName, uint Reserved);

			[DllImport ("api-ms-win-core-libraryloader-l1-2-0.dll", SetLastError = true, CharSet = CharSet.Ansi)]
			private static extern IntPtr GetProcAddress (IntPtr hModule, [MarshalAs (UnmanagedType.LPStr)] string lpProcName);

			private static IntPtr LoadLibrary (string lpFileName) => LoadPackagedLibrary(lpFileName, 0);
#else
			[DllImport ("Kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
			private static extern IntPtr LoadLibrary ([MarshalAs (UnmanagedType.LPStr)] string lpFileName);

			[DllImport ("Kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
			private static extern IntPtr GetProcAddress (IntPtr hModule, [MarshalAs (UnmanagedType.LPStr)] string lpProcName);
#endif

			[DllImport ("libEGL.dll")]
			private static extern IntPtr eglGetProcAddress ([MarshalAs (UnmanagedType.LPStr)] string procname);

			static AngleLoader()
			{
				libEGL = LoadLibrary ("libEGL.dll");
				if (Marshal.GetLastWin32Error () != 0 || libEGL == IntPtr.Zero)
					throw new DllNotFoundException ("Unable to load libEGL.dll.");

				libGLESv2 = LoadLibrary ("libGLESv2.dll");
				if (Marshal.GetLastWin32Error () != 0 || libGLESv2 == IntPtr.Zero)
					throw new DllNotFoundException ("Unable to load libGLESv2.dll.");
			}

			// function to assemble the ANGLE interface
			public static IntPtr GetProc (object context, string name)
			{
				IntPtr proc = GetProcAddress (libGLESv2, name);
				if (proc == IntPtr.Zero)
				{
					proc = GetProcAddress (libEGL, name);
				}
				if (proc == IntPtr.Zero)
				{
					proc = eglGetProcAddress (name);
				}
				return proc;
			}
		}
#endif
	}
}

