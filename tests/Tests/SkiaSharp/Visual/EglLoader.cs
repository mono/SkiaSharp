using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Process-singleton EGL display + GL context, used by
	/// <see cref="GaneshGlRenderer"/> for headless GL on Linux.
	///
	/// <para>
	/// Prefers an EGL device that advertises <c>EGL_MESA_device_software</c>,
	/// so any software OpenGL ICD on the host wins over a real GPU. That
	/// gives us deterministic pixels for goldens even on machines that also
	/// have hardware-accelerated GL.
	/// </para>
	///
	/// <para>
	/// No display server. We use <c>EGL_EXT_platform_device</c> to create the
	/// EGLDisplay directly from a device handle (skipping <c>eglGetDisplay</c>'s
	/// implicit X11/Wayland connection) and <c>EGL_KHR_surfaceless_context</c>
	/// so the context is current without a Pbuffer. Skia allocates its own
	/// offscreen FBO inside the GR context for every <see cref="SKSurface"/>.
	/// </para>
	///
	/// <para>
	/// The instance is process-wide, but the EGL context is THREAD-AFFINE.
	/// Callers (the renderer) must make it current on the thread they intend
	/// to issue GL commands from via <see cref="MakeCurrent"/>.
	/// </para>
	/// </summary>
	internal sealed unsafe class EglLoader
	{
		// Stock Linux ships libEGL.so.1 but not the unversioned libEGL.so
		// (that's an -dev package symlink). Reference the versioned name
		// directly so we resolve on a bare runtime install.
		private const string libegl = "libEGL.so.1";

		private static EglLoader instance;
		private static readonly object instanceLock = new object ();

		public IntPtr Display { get; private set; }
		public IntPtr Context { get; private set; }
		public IntPtr Config  { get; private set; }
		public bool   IsAvailable   { get; private set; }
		public string FailureReason { get; private set; }

		public static EglLoader Shared {
			get {
				if (instance == null) {
					lock (instanceLock) {
						if (instance == null) {
							instance = new EglLoader ();
							instance.Initialize ();
						}
					}
				}
				return instance;
			}
		}

		/// <summary>
		/// Bind this loader's context to the calling thread. Idempotent on a
		/// thread that already holds the context. Must be called once per
		/// thread that intends to issue GL commands.
		/// </summary>
		public void MakeCurrent ()
		{
			if (!IsAvailable)
				throw new InvalidOperationException ($"EGL unavailable: {FailureReason}");
			if (eglMakeCurrent (Display, IntPtr.Zero, IntPtr.Zero, Context) == 0) {
				var err = eglGetError ();
				throw new InvalidOperationException ($"eglMakeCurrent failed: 0x{err:X}");
			}
		}

		/// <summary>Procedure-address adapter for <see cref="GRGlInterface.Create"/>.</summary>
		public IntPtr GetProc (string name) => eglGetProcAddress (name);

		private void Initialize ()
		{
			try {
				if (!PickDevice (out var device, out var failure)) { FailureReason = failure; return; }
				if (!CreateDisplay (device, out failure)) { FailureReason = failure; return; }
				if (!ChooseConfigAndCreateContext (out failure)) { FailureReason = failure; return; }
				IsAvailable = true;
			} catch (DllNotFoundException ex) {
				FailureReason = $"libEGL.so not loadable: {ex.Message}";
			} catch (Exception ex) {
				FailureReason = $"EGL init failed: {ex.GetType ().Name}: {ex.Message}";
			}
		}

		private bool PickDevice (out IntPtr device, out string failure)
		{
			device = IntPtr.Zero;
			failure = null;

			// EGL_EXT_platform_device must be advertised by the client driver.
			var clientExts = Marshal.PtrToStringAnsi (eglQueryString (IntPtr.Zero, EGL_EXTENSIONS)) ?? "";
			if (!clientExts.Contains ("EGL_EXT_platform_device") || !clientExts.Contains ("EGL_EXT_device_enumeration")) {
				failure = $"EGL client extensions missing platform_device/device_enumeration. Have: {clientExts}";
				return false;
			}

			var pQueryDevices = LoadProc<eglQueryDevicesEXT_t> ("eglQueryDevicesEXT");
			var pQueryDevStr  = LoadProc<eglQueryDeviceStringEXT_t> ("eglQueryDeviceStringEXT");
			if (pQueryDevices == null || pQueryDevStr == null) {
				failure = "eglQueryDevicesEXT / eglQueryDeviceStringEXT not exported";
				return false;
			}

			int n = 0;
			if (pQueryDevices (0, null, &n) == 0 || n == 0) {
				failure = $"eglQueryDevicesEXT reported {n} devices (err 0x{eglGetError ():X})";
				return false;
			}
			var devices = new IntPtr[n];
			fixed (IntPtr* pd = devices) {
				if (pQueryDevices (n, pd, &n) == 0) {
					failure = $"eglQueryDevicesEXT enumeration failed (err 0x{eglGetError ():X})";
					return false;
				}
			}

			// Prefer software device for determinism (matches VulkanLoader's
			// VK_PHYSICAL_DEVICE_TYPE_CPU preference).
			device = devices[0];
			for (int i = 0; i < n; i++) {
				var ext = Marshal.PtrToStringAnsi (pQueryDevStr (devices[i], EGL_EXTENSIONS)) ?? "";
				if (ext.Contains ("EGL_MESA_device_software")) {
					device = devices[i];
					return true;
				}
			}
			return true;
		}

		private bool CreateDisplay (IntPtr device, out string failure)
		{
			failure = null;
			var pGetPlatform = LoadProc<eglGetPlatformDisplayEXT_t> ("eglGetPlatformDisplayEXT");
			if (pGetPlatform == null) {
				failure = "eglGetPlatformDisplayEXT not exported";
				return false;
			}
			Display = pGetPlatform (EGL_PLATFORM_DEVICE_EXT, device, null);
			if (Display == IntPtr.Zero) {
				failure = $"eglGetPlatformDisplayEXT returned EGL_NO_DISPLAY (err 0x{eglGetError ():X})";
				return false;
			}
			if (eglInitialize (Display, out var major, out var minor) == 0) {
				failure = $"eglInitialize failed (err 0x{eglGetError ():X})";
				return false;
			}
			var dispExts = Marshal.PtrToStringAnsi (eglQueryString (Display, EGL_EXTENSIONS)) ?? "";
			if (!dispExts.Contains ("EGL_KHR_surfaceless_context")) {
				failure = $"Display does not support EGL_KHR_surfaceless_context. Have: {dispExts}";
				return false;
			}
			return true;
		}

		private bool ChooseConfigAndCreateContext (out string failure)
		{
			failure = null;
			if (eglBindAPI (EGL_OPENGL_API) == 0) {
				failure = $"eglBindAPI(EGL_OPENGL_API) failed (err 0x{eglGetError ():X})";
				return false;
			}
			// Minimal config — surfaceless context binds to no surface, but
			// eglCreateContext still requires a config. Any framebuffer-like
			// config works; we ask for a basic 8888.
			var configAttribs = stackalloc int[] {
				EGL_RED_SIZE, 8, EGL_GREEN_SIZE, 8, EGL_BLUE_SIZE, 8, EGL_ALPHA_SIZE, 8,
				EGL_SURFACE_TYPE, EGL_PBUFFER_BIT,
				EGL_RENDERABLE_TYPE, EGL_OPENGL_BIT,
				EGL_NONE,
			};
			IntPtr config;
			int numConfigs = 0;
			if (eglChooseConfig (Display, configAttribs, &config, 1, &numConfigs) == 0 || numConfigs == 0) {
				failure = $"eglChooseConfig found 0 configs (err 0x{eglGetError ():X})";
				return false;
			}
			Config = config;

			var ctxAttribs = stackalloc int[] {
				EGL_CONTEXT_MAJOR_VERSION, 3,
				EGL_CONTEXT_MINOR_VERSION, 3,
				EGL_NONE,
			};
			Context = eglCreateContext (Display, Config, IntPtr.Zero, ctxAttribs);
			if (Context == IntPtr.Zero) {
				failure = $"eglCreateContext failed (err 0x{eglGetError ():X})";
				return false;
			}
			return true;
		}

		private static T LoadProc<T> (string name) where T : Delegate
		{
			var addr = eglGetProcAddress (name);
			return addr == IntPtr.Zero ? null : Marshal.GetDelegateForFunctionPointer<T> (addr);
		}

		// ---- EGL constants (matches /usr/include/EGL/eglext.h numbering) ----

		private const int EGL_PLATFORM_DEVICE_EXT       = 0x313F;
		private const int EGL_EXTENSIONS                = 0x3055;
		private const int EGL_NONE                      = 0x3038;
		private const int EGL_OPENGL_API                = 0x30A2;
		private const int EGL_OPENGL_BIT                = 0x0008;
		private const int EGL_RENDERABLE_TYPE           = 0x3040;
		private const int EGL_SURFACE_TYPE              = 0x3033;
		private const int EGL_PBUFFER_BIT               = 0x0001;
		private const int EGL_RED_SIZE                  = 0x3024;
		private const int EGL_GREEN_SIZE                = 0x3023;
		private const int EGL_BLUE_SIZE                 = 0x3022;
		private const int EGL_ALPHA_SIZE                = 0x3021;
		private const int EGL_CONTEXT_MAJOR_VERSION     = 0x3098;
		private const int EGL_CONTEXT_MINOR_VERSION     = 0x30FB;

		// ---- P/Invoke ----

		[DllImport (libegl, CharSet = CharSet.Ansi)] private static extern IntPtr eglGetProcAddress (string name);
		[DllImport (libegl, CharSet = CharSet.Ansi)] private static extern IntPtr eglQueryString (IntPtr display, int name);
		[DllImport (libegl)] private static extern uint eglGetError ();
		[DllImport (libegl)] private static extern int  eglInitialize (IntPtr display, out int major, out int minor);
		[DllImport (libegl)] private static extern int  eglBindAPI (int api);
		[DllImport (libegl)] private static extern int  eglChooseConfig (IntPtr display, int* attribs, IntPtr* configs, int configSize, int* numConfigs);
		[DllImport (libegl)] private static extern IntPtr eglCreateContext (IntPtr display, IntPtr config, IntPtr shareContext, int* attribs);
		[DllImport (libegl)] private static extern int eglMakeCurrent (IntPtr display, IntPtr draw, IntPtr read, IntPtr context);

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		private delegate int eglQueryDevicesEXT_t (int maxDevices, IntPtr* devices, int* numDevices);

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		private delegate IntPtr eglQueryDeviceStringEXT_t (IntPtr device, int name);

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		private delegate IntPtr eglGetPlatformDisplayEXT_t (int platform, IntPtr nativeDisplay, int* attribs);
	}
}
