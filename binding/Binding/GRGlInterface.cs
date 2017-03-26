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
		private static readonly GRGlGetProcDelegateInternal getProcDelegateInternal;
		private static readonly IntPtr getProcDelegate;
		static GRGlInterface ()
		{
			getProcDelegateInternal = new GRGlGetProcDelegateInternal (GrGLGetProcInternal);
			getProcDelegate = Marshal.GetFunctionPointerForDelegate (getProcDelegateInternal);
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

		[Obsolete ("Use CreateNativeGlInterface() or CreateDefaultInterface() instead.", true)]
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
			if (PlatformConfiguration.IsWindows) {
				return AssembleAngleInterface (AngleLoader.GetProc);
			} else {
				// return null on non-DirectX platforms: everything except Windows
				return null;
			}
		}

		public static GRGlInterface AssembleInterface (GRGlGetProcDelegate get)
		{
			return AssembleInterface (null, get);
		}

		public static GRGlInterface AssembleInterface (object context, GRGlGetProcDelegate get)
		{
			// if on Windows, try ANGLE
			if (PlatformConfiguration.IsWindows) {
				var angle = AssembleAngleInterface (context, get);
				if (angle != null) {
					return angle;
				}
			}

			// try the native default
			using (var ctx = new NativeDelegateContext (context, get)) {
				return GetObject<GRGlInterface> (SkiaApi.gr_glinterface_assemble_interface (ctx.NativeContext, getProcDelegate));
			}
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
			using (var ctx = new NativeDelegateContext (context, get)) {
				return GetObject<GRGlInterface> (SkiaApi.gr_glinterface_assemble_gl_interface (ctx.NativeContext, getProcDelegate));
			}
		}

		public static GRGlInterface AssembleGlesInterface (GRGlGetProcDelegate get)
		{
			return AssembleGlesInterface (null, get);
		}

		public static GRGlInterface AssembleGlesInterface (object context, GRGlGetProcDelegate get)
		{
			using (var ctx = new NativeDelegateContext (context, get)) {
				return GetObject<GRGlInterface> (SkiaApi.gr_glinterface_assemble_gles_interface (ctx.NativeContext, getProcDelegate));
			}
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
			var ctx = NativeDelegateContext.Unwrap (context);
			return ctx.GetDelegate<GRGlGetProcDelegate> () (ctx.ManagedContext, name);
		}

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
				// this is not supported at all on non-Windows platforms
				if (!PlatformConfiguration.IsWindows) {
					return;
				}

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
				// this is not supported at all on non-Windows platforms
				if (!PlatformConfiguration.IsWindows) {
					return IntPtr.Zero;
				}

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
	}
}

