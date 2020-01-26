using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	public unsafe class GRGlInterface : SKObject, ISKReferenceCounted
	{
		[Preserve]
		internal GRGlInterface (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		// first try ANGLE, then fall back to the OpenGL-based
		public static GRGlInterface CreateDefaultInterface () =>
			CreateNativeAngleInterface () ?? CreateNativeGlInterface ();

		// the native code will automatically return null on non-OpenGL platforms, such as UWP
		public static GRGlInterface CreateNativeGlInterface () =>
			GetObject<GRGlInterface> (SkiaApi.gr_glinterface_create_native_interface ());

		// return null on non-DirectX platforms: everything except Windows
		public static GRGlInterface CreateNativeAngleInterface () =>
			PlatformConfiguration.IsWindows ? AssembleAngleInterface (AngleLoader.GetProc) : null;

		public static GRGlInterface CreateNativeEvasInterface (IntPtr evas)
		{
			GRGlInterface iface;
#if __TIZEN__
			var evasLoader = new EvasGlLoader (evas);
			iface = AssembleGlesInterface ((ctx, name) => evasLoader.GetFunctionPointer (name));
#else
			iface = null;
#endif
			return iface;
		}

		public static GRGlInterface AssembleInterface (GRGlGetProcDelegate get) =>
			AssembleInterface (null, get);

		public static GRGlInterface AssembleInterface (object context, GRGlGetProcDelegate get)
		{
			// if on Windows, try ANGLE
			if (PlatformConfiguration.IsWindows) {
				var angle = AssembleAngleInterface (context, get);
				if (angle != null)
					return angle;
			}

			// try the native default
			var del = get != null && context != null
				? new GRGlGetProcDelegate ((_, name) => get (context, name))
				: get;
			var proxy = DelegateProxies.Create (del, DelegateProxies.GRGlGetProcDelegateProxy, out var gch, out var ctx);
			try {
				return GetObject<GRGlInterface> (SkiaApi.gr_glinterface_assemble_interface ((void*)ctx, proxy));
			} finally {
				gch.Free ();
			}
		}

		public static GRGlInterface AssembleAngleInterface (GRGlGetProcDelegate get) =>
			AssembleAngleInterface (null, get);

		// ANGLE is just a GLES v2 over DX v9+
		public static GRGlInterface AssembleAngleInterface (object context, GRGlGetProcDelegate get) =>
			AssembleGlesInterface (context, get);

		public static GRGlInterface AssembleGlInterface (GRGlGetProcDelegate get) =>
			AssembleGlInterface (null, get);

		public static GRGlInterface AssembleGlInterface (object context, GRGlGetProcDelegate get)
		{
			var del = get != null && context != null
				? new GRGlGetProcDelegate ((_, name) => get (context, name))
				: get;
			var proxy = DelegateProxies.Create (del, DelegateProxies.GRGlGetProcDelegateProxy, out var gch, out var ctx);
			try {
				return GetObject<GRGlInterface> (SkiaApi.gr_glinterface_assemble_gl_interface ((void*)ctx, proxy));
			} finally {
				gch.Free ();
			}
		}

		public static GRGlInterface AssembleGlesInterface (GRGlGetProcDelegate get) =>
			AssembleGlesInterface (null, get);

		public static GRGlInterface AssembleGlesInterface (object context, GRGlGetProcDelegate get)
		{
			var del = get != null && context != null
				? new GRGlGetProcDelegate ((_, name) => get (context, name))
				: get;
			var proxy = DelegateProxies.Create (del, DelegateProxies.GRGlGetProcDelegateProxy, out var gch, out var ctx);
			try {
				return GetObject<GRGlInterface> (SkiaApi.gr_glinterface_assemble_gles_interface ((void*)ctx, proxy));
			} finally {
				gch.Free ();
			}
		}

		public bool Validate () =>
			SkiaApi.gr_glinterface_validate (Handle);

		public bool HasExtension (string extension) =>
			SkiaApi.gr_glinterface_has_extension (Handle, extension);

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

			private static IntPtr LoadLibrary (string lpFileName) => LoadPackagedLibrary (lpFileName, 0);
#else
			[DllImport ("Kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
			private static extern IntPtr LoadLibrary ([MarshalAs (UnmanagedType.LPStr)] string lpFileName);

			[DllImport ("Kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
			private static extern IntPtr GetProcAddress (IntPtr hModule, [MarshalAs (UnmanagedType.LPStr)] string lpProcName);
#endif

			[DllImport ("libEGL.dll")]
			private static extern IntPtr eglGetProcAddress ([MarshalAs (UnmanagedType.LPStr)] string procname);

			static AngleLoader ()
			{
				// this is not supported at all on non-Windows platforms
				if (!PlatformConfiguration.IsWindows)
					return;

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
				if (!PlatformConfiguration.IsWindows)
					return IntPtr.Zero;

				var proc = GetProcAddress (libGLESv2, name);
				if (proc == IntPtr.Zero)
					proc = GetProcAddress (libEGL, name);
				if (proc == IntPtr.Zero)
					proc = eglGetProcAddress (name);
				return proc;
			}
		}

#if __TIZEN__
		private class EvasGlLoader
		{
			private IntPtr glEvas;
			private EvasGlApi api;

			[DllImport ("libevas.so.1")]
			internal static extern IntPtr evas_gl_api_get (IntPtr evas_gl);

			[DllImport ("libevas.so.1")]
			internal static extern IntPtr evas_gl_proc_address_get (IntPtr evas_gl, string name);

			public EvasGlLoader (IntPtr evas)
			{
				glEvas = evas;

				var unmanagedGlApi = evas_gl_api_get (glEvas);
				api = Marshal.PtrToStructure<EvasGlApi> (unmanagedGlApi);
			}

			public IntPtr GetFunctionPointer (string name)
			{
				var ret = evas_gl_proc_address_get (glEvas, name);
				if (ret != IntPtr.Zero)
					return ret;

				var field = typeof (EvasGlApi).GetField (name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (field?.FieldType == typeof (IntPtr))
					return (IntPtr)field.GetValue (api);

				return IntPtr.Zero;
			}
		}

		// this structure is initialized from a native pointer
		[Preserve (AllMembers = true)]
		private struct EvasGlApi
		{
			// DO NOT change the order, needs to be as specified in struct _Evas_GL_API (/platform/upstream/efl/src/lib/evas/Evas_GL.h)
			// DO NOT change the names, they need to match the OpenGL API
#pragma warning disable 0169
			private readonly int version;
			private readonly IntPtr glActiveTexture;
			private readonly IntPtr glAttachShader;
			private readonly IntPtr glBindAttribLocation;
			private readonly IntPtr glBindBuffer;
			private readonly IntPtr glBindFramebuffer;
			private readonly IntPtr glBindRenderbuffer;
			private readonly IntPtr glBindTexture;
			private readonly IntPtr glBlendColor;
			private readonly IntPtr glBlendEquation;
			private readonly IntPtr glBlendEquationSeparate;
			private readonly IntPtr glBlendFunc;
			private readonly IntPtr glBlendFuncSeparate;
			private readonly IntPtr glBufferData;
			private readonly IntPtr glBufferSubData;
			private readonly IntPtr glCheckFramebufferStatus;
			private readonly IntPtr glClear;
			private readonly IntPtr glClearColor;
			private readonly IntPtr glClearDepthf;
			private readonly IntPtr glClearStencil;
			private readonly IntPtr glColorMask;
			private readonly IntPtr glCompileShader;
			private readonly IntPtr glCompressedTexImage2D;
			private readonly IntPtr glCompressedTexSubImage2D;
			private readonly IntPtr glCopyTexImage2D;
			private readonly IntPtr glCopyTexSubImage2D;
			private readonly IntPtr glCreateProgram;
			private readonly IntPtr glCreateShader;
			private readonly IntPtr glCullFace;
			private readonly IntPtr glDeleteBuffers;
			private readonly IntPtr glDeleteFramebuffers;
			private readonly IntPtr glDeleteProgram;
			private readonly IntPtr glDeleteRenderbuffers;
			private readonly IntPtr glDeleteShader;
			private readonly IntPtr glDeleteTextures;
			private readonly IntPtr glDepthFunc;
			private readonly IntPtr glDepthMask;
			private readonly IntPtr glDepthRangef;
			private readonly IntPtr glDetachShader;
			private readonly IntPtr glDisable;
			private readonly IntPtr glDisableVertexAttribArray;
			private readonly IntPtr glDrawArrays;
			private readonly IntPtr glDrawElements;
			private readonly IntPtr glEnable;
			private readonly IntPtr glEnableVertexAttribArray;
			private readonly IntPtr glFinish;
			private readonly IntPtr glFlush;
			private readonly IntPtr glFramebufferRenderbuffer;
			private readonly IntPtr glFramebufferTexture2D;
			private readonly IntPtr glFrontFace;
			private readonly IntPtr glGenBuffers;
			private readonly IntPtr glGenerateMipmap;
			private readonly IntPtr glGenFramebuffers;
			private readonly IntPtr glGenRenderbuffers;
			private readonly IntPtr glGenTextures;
			private readonly IntPtr glGetActiveAttrib;
			private readonly IntPtr glGetActiveUniform;
			private readonly IntPtr glGetAttachedShaders;
			private readonly IntPtr glGetAttribLocation;
			private readonly IntPtr glGetBooleanv;
			private readonly IntPtr glGetBufferParameteriv;
			private readonly IntPtr glGetError;
			private readonly IntPtr glGetFloatv;
			private readonly IntPtr glGetFramebufferAttachmentParameteriv;
			private readonly IntPtr glGetIntegerv;
			private readonly IntPtr glGetProgramiv;
			private readonly IntPtr glGetProgramInfoLog;
			private readonly IntPtr glGetRenderbufferParameteriv;
			private readonly IntPtr glGetShaderiv;
			private readonly IntPtr glGetShaderInfoLog;
			private readonly IntPtr glGetShaderPrecisionFormat;
			private readonly IntPtr glGetShaderSource;
			private readonly IntPtr glGetString;
			private readonly IntPtr glGetTexParameterfv;
			private readonly IntPtr glGetTexParameteriv;
			private readonly IntPtr glGetUniformfv;
			private readonly IntPtr glGetUniformiv;
			private readonly IntPtr glGetUniformLocation;
			private readonly IntPtr glGetVertexAttribfv;
			private readonly IntPtr glGetVertexAttribiv;
			private readonly IntPtr glGetVertexAttribPointerv;
			private readonly IntPtr glHint;
			private readonly IntPtr glIsBuffer;
			private readonly IntPtr glIsEnabled;
			private readonly IntPtr glIsFramebuffer;
			private readonly IntPtr glIsProgram;
			private readonly IntPtr glIsRenderbuffer;
			private readonly IntPtr glIsShader;
			private readonly IntPtr glIsTexture;
			private readonly IntPtr glLineWidth;
			private readonly IntPtr glLinkProgram;
			private readonly IntPtr glPixelStorei;
			private readonly IntPtr glPolygonOffset;
			private readonly IntPtr glReadPixels;
			private readonly IntPtr glReleaseShaderCompiler;
			private readonly IntPtr glRenderbufferStorage;
			private readonly IntPtr glSampleCoverage;
			private readonly IntPtr glScissor;
			private readonly IntPtr glShaderBinary;
			private readonly IntPtr glShaderSource;
			private readonly IntPtr glStencilFunc;
			private readonly IntPtr glStencilFuncSeparate;
			private readonly IntPtr glStencilMask;
			private readonly IntPtr glStencilMaskSeparate;
			private readonly IntPtr glStencilOp;
			private readonly IntPtr glStencilOpSeparate;
			private readonly IntPtr glTexImage2D;
			private readonly IntPtr glTexParameterf;
			private readonly IntPtr glTexParameterfv;
			private readonly IntPtr glTexParameteri;
			private readonly IntPtr glTexParameteriv;
			private readonly IntPtr glTexSubImage2D;
			private readonly IntPtr glUniform1f;
			private readonly IntPtr glUniform1fv;
			private readonly IntPtr glUniform1i;
			private readonly IntPtr glUniform1iv;
			private readonly IntPtr glUniform2f;
			private readonly IntPtr glUniform2fv;
			private readonly IntPtr glUniform2i;
			private readonly IntPtr glUniform2iv;
			private readonly IntPtr glUniform3f;
			private readonly IntPtr glUniform3fv;
			private readonly IntPtr glUniform3i;
			private readonly IntPtr glUniform3iv;
			private readonly IntPtr glUniform4f;
			private readonly IntPtr glUniform4fv;
			private readonly IntPtr glUniform4i;
			private readonly IntPtr glUniform4iv;
			private readonly IntPtr glUniformMatrix2fv;
			private readonly IntPtr glUniformMatrix3fv;
			private readonly IntPtr glUniformMatrix4fv;
			private readonly IntPtr glUseProgram;
			private readonly IntPtr glValidateProgram;
			private readonly IntPtr glVertexAttrib1f;
			private readonly IntPtr glVertexAttrib1fv;
			private readonly IntPtr glVertexAttrib2f;
			private readonly IntPtr glVertexAttrib2fv;
			private readonly IntPtr glVertexAttrib3f;
			private readonly IntPtr glVertexAttrib3fv;
			private readonly IntPtr glVertexAttrib4f;
			private readonly IntPtr glVertexAttrib4fv;
			private readonly IntPtr glVertexAttribPointer;
			private readonly IntPtr glViewport;
			private readonly IntPtr glEvasGLImageTargetTexture2DOES;
			private readonly IntPtr glEvasGLImageTargetRenderbufferStorageOES;
			private readonly IntPtr glGetProgramBinaryOES;
			private readonly IntPtr glProgramBinaryOES;
			private readonly IntPtr glMapBufferOES;
			private readonly IntPtr glUnmapBufferOES;
			private readonly IntPtr glGetBufferPointervOES;
			private readonly IntPtr glTexImage3DOES;
			private readonly IntPtr glTexSubImage3DOES;
			private readonly IntPtr glCopyTexSubImage3DOES;
			private readonly IntPtr glCompressedTexImage3DOES;
			private readonly IntPtr glCompressedTexSubImage3DOES;
			private readonly IntPtr glFramebufferTexture3DOES;
			private readonly IntPtr glGetPerfMonitorGroupsAMD;
			private readonly IntPtr glGetPerfMonitorCountersAMD;
			private readonly IntPtr glGetPerfMonitorGroupStringAMD;
			private readonly IntPtr glGetPerfMonitorCounterStringAMD;
			private readonly IntPtr glGetPerfMonitorCounterInfoAMD;
			private readonly IntPtr glGenPerfMonitorsAMD;
			private readonly IntPtr glDeletePerfMonitorsAMD;
			private readonly IntPtr glSelectPerfMonitorCountersAMD;
			private readonly IntPtr glBeginPerfMonitorAMD;
			private readonly IntPtr glEndPerfMonitorAMD;
			private readonly IntPtr glGetPerfMonitorCounterDataAMD;
			private readonly IntPtr glDiscardFramebufferEXT;
			private readonly IntPtr glMultiDrawArraysEXT;
			private readonly IntPtr glMultiDrawElementsEXT;
			private readonly IntPtr glDeleteFencesNV;
			private readonly IntPtr glGenFencesNV;
			private readonly IntPtr glIsFenceNV;
			private readonly IntPtr glTestFenceNV;
			private readonly IntPtr glGetFenceivNV;
			private readonly IntPtr glFinishFenceNV;
			private readonly IntPtr glSetFenceNV;
			private readonly IntPtr glGetDriverControlsQCOM;
			private readonly IntPtr glGetDriverControlStringQCOM;
			private readonly IntPtr glEnableDriverControlQCOM;
			private readonly IntPtr glDisableDriverControlQCOM;
			private readonly IntPtr glExtGetTexturesQCOM;
			private readonly IntPtr glExtGetBuffersQCOM;
			private readonly IntPtr glExtGetRenderbuffersQCOM;
			private readonly IntPtr glExtGetFramebuffersQCOM;
			private readonly IntPtr glExtGetTexLevelParameterivQCOM;
			private readonly IntPtr glExtTexObjectStateOverrideiQCOM;
			private readonly IntPtr glExtGetTexSubImageQCOM;
			private readonly IntPtr glExtGetBufferPointervQCOM;
			private readonly IntPtr glExtGetShadersQCOM;
			private readonly IntPtr glExtGetProgramsQCOM;
			private readonly IntPtr glExtIsProgramBinaryQCOM;
			private readonly IntPtr glExtGetProgramBinarySourceQCOM;
			private readonly IntPtr evasglCreateImage;
			private readonly IntPtr evasglDestroyImage;
			private readonly IntPtr evasglCreateImageForContext;
			private readonly IntPtr glAlphaFunc;
			private readonly IntPtr glClipPlanef;
			private readonly IntPtr glColor4f;
			private readonly IntPtr glFogf;
			private readonly IntPtr glFogfv;
			private readonly IntPtr glFrustumf;
			private readonly IntPtr glGetClipPlanef;
			private readonly IntPtr glGetLightfv;
			private readonly IntPtr glGetMaterialfv;
			private readonly IntPtr glGetTexEnvfv;
			private readonly IntPtr glLightModelf;
			private readonly IntPtr glLightModelfv;
			private readonly IntPtr glLightf;
			private readonly IntPtr glLightfv;
			private readonly IntPtr glLoadMatrixf;
			private readonly IntPtr glMaterialf;
			private readonly IntPtr glMaterialfv;
			private readonly IntPtr glMultMatrixf;
			private readonly IntPtr glMultiTexCoord4f;
			private readonly IntPtr glNormal3f;
			private readonly IntPtr glOrthof;
			private readonly IntPtr glPointParameterf;
			private readonly IntPtr glPointParameterfv;
			private readonly IntPtr glPointSize;
			private readonly IntPtr glPointSizePointerOES;
			private readonly IntPtr glRotatef;
			private readonly IntPtr glScalef;
			private readonly IntPtr glTexEnvf;
			private readonly IntPtr glTexEnvfv;
			private readonly IntPtr glTranslatef;
			private readonly IntPtr glAlphaFuncx;
			private readonly IntPtr glClearColorx;
			private readonly IntPtr glClearDepthx;
			private readonly IntPtr glClientActiveTexture;
			private readonly IntPtr glClipPlanex;
			private readonly IntPtr glColor4ub;
			private readonly IntPtr glColor4x;
			private readonly IntPtr glColorPointer;
			private readonly IntPtr glDepthRangex;
			private readonly IntPtr glDisableClientState;
			private readonly IntPtr glEnableClientState;
			private readonly IntPtr glFogx;
			private readonly IntPtr glFogxv;
			private readonly IntPtr glFrustumx;
			private readonly IntPtr glGetClipPlanex;
			private readonly IntPtr glGetFixedv;
			private readonly IntPtr glGetLightxv;
			private readonly IntPtr glGetMaterialxv;
			private readonly IntPtr glGetPointerv;
			private readonly IntPtr glGetTexEnviv;
			private readonly IntPtr glGetTexEnvxv;
			private readonly IntPtr glGetTexParameterxv;
			private readonly IntPtr glLightModelx;
			private readonly IntPtr glLightModelxv;
			private readonly IntPtr glLightx;
			private readonly IntPtr glLightxv;
			private readonly IntPtr glLineWidthx;
			private readonly IntPtr glLoadIdentity;
			private readonly IntPtr glLoadMatrixx;
			private readonly IntPtr glLogicOp;
			private readonly IntPtr glMaterialx;
			private readonly IntPtr glMaterialxv;
			private readonly IntPtr glMatrixMode;
			private readonly IntPtr glMultMatrixx;
			private readonly IntPtr glMultiTexCoord4x;
			private readonly IntPtr glNormal3x;
			private readonly IntPtr glNormalPointer;
			private readonly IntPtr glOrthox;
			private readonly IntPtr glPointParameterx;
			private readonly IntPtr glPointParameterxv;
			private readonly IntPtr glPointSizex;
			private readonly IntPtr glPolygonOffsetx;
			private readonly IntPtr glPopMatrix;
			private readonly IntPtr glPushMatrix;
			private readonly IntPtr glRotatex;
			private readonly IntPtr glSampleCoveragex;
			private readonly IntPtr glScalex;
			private readonly IntPtr glShadeModel;
			private readonly IntPtr glTexCoordPointer;
			private readonly IntPtr glTexEnvi;
			private readonly IntPtr glTexEnvx;
			private readonly IntPtr glTexEnviv;
			private readonly IntPtr glTexEnvxv;
			private readonly IntPtr glTexParameterx;
			private readonly IntPtr glTexParameterxv;
			private readonly IntPtr glTranslatex;
			private readonly IntPtr glVertexPointer;
			private readonly IntPtr glBlendEquationSeparateOES;
			private readonly IntPtr glBlendFuncSeparateOES;
			private readonly IntPtr glBlendEquationOES;
			private readonly IntPtr glDrawTexsOES;
			private readonly IntPtr glDrawTexiOES;
			private readonly IntPtr glDrawTexxOES;
			private readonly IntPtr glDrawTexsvOES;
			private readonly IntPtr glDrawTexivOES;
			private readonly IntPtr glDrawTexxvOES;
			private readonly IntPtr glDrawTexfOES;
			private readonly IntPtr glDrawTexfvOES;
			private readonly IntPtr glAlphaFuncxOES;
			private readonly IntPtr glClearColorxOES;
			private readonly IntPtr glClearDepthxOES;
			private readonly IntPtr glClipPlanexOES;
			private readonly IntPtr glColor4xOES;
			private readonly IntPtr glDepthRangexOES;
			private readonly IntPtr glFogxOES;
			private readonly IntPtr glFogxvOES;
			private readonly IntPtr glFrustumxOES;
			private readonly IntPtr glGetClipPlanexOES;
			private readonly IntPtr glGetFixedvOES;
			private readonly IntPtr glGetLightxvOES;
			private readonly IntPtr glGetMaterialxvOES;
			private readonly IntPtr glGetTexEnvxvOES;
			private readonly IntPtr glGetTexParameterxvOES;
			private readonly IntPtr glLightModelxOES;
			private readonly IntPtr glLightModelxvOES;
			private readonly IntPtr glLightxOES;
			private readonly IntPtr glLightxvOES;
			private readonly IntPtr glLineWidthxOES;
			private readonly IntPtr glLoadMatrixxOES;
			private readonly IntPtr glMaterialxOES;
			private readonly IntPtr glMaterialxvOES;
			private readonly IntPtr glMultMatrixxOES;
			private readonly IntPtr glMultiTexCoord4xOES;
			private readonly IntPtr glNormal3xOES;
			private readonly IntPtr glOrthoxOES;
			private readonly IntPtr glPointParameterxOES;
			private readonly IntPtr glPointParameterxvOES;
			private readonly IntPtr glPointSizexOES;
			private readonly IntPtr glPolygonOffsetxOES;
			private readonly IntPtr glRotatexOES;
			private readonly IntPtr glSampleCoveragexOES;
			private readonly IntPtr glScalexOES;
			private readonly IntPtr glTexEnvxOES;
			private readonly IntPtr glTexEnvxvOES;
			private readonly IntPtr glTexParameterxOES;
			private readonly IntPtr glTexParameterxvOES;
			private readonly IntPtr glTranslatexOES;
			private readonly IntPtr glIsRenderbufferOES;
			private readonly IntPtr glBindRenderbufferOES;
			private readonly IntPtr glDeleteRenderbuffersOES;
			private readonly IntPtr glGenRenderbuffersOES;
			private readonly IntPtr glRenderbufferStorageOES;
			private readonly IntPtr glGetRenderbufferParameterivOES;
			private readonly IntPtr glIsFramebufferOES;
			private readonly IntPtr glBindFramebufferOES;
			private readonly IntPtr glDeleteFramebuffersOES;
			private readonly IntPtr glGenFramebuffersOES;
			private readonly IntPtr glCheckFramebufferStatusOES;
			private readonly IntPtr glFramebufferRenderbufferOES;
			private readonly IntPtr glFramebufferTexture2DOES;
			private readonly IntPtr glGetFramebufferAttachmentParameterivOES;
			private readonly IntPtr glGenerateMipmapOES;
			private readonly IntPtr glCurrentPaletteMatrixOES;
			private readonly IntPtr glLoadPaletteFromModelViewMatrixOES;
			private readonly IntPtr glMatrixIndexPointerOES;
			private readonly IntPtr glWeightPointerOES;
			private readonly IntPtr glQueryMatrixxOES;
			private readonly IntPtr glDepthRangefOES;
			private readonly IntPtr glFrustumfOES;
			private readonly IntPtr glOrthofOES;
			private readonly IntPtr glClipPlanefOES;
			private readonly IntPtr glGetClipPlanefOES;
			private readonly IntPtr glClearDepthfOES;
			private readonly IntPtr glTexGenfOES;
			private readonly IntPtr glTexGenfvOES;
			private readonly IntPtr glTexGeniOES;
			private readonly IntPtr glTexGenivOES;
			private readonly IntPtr glTexGenxOES;
			private readonly IntPtr glTexGenxvOES;
			private readonly IntPtr glGetTexGenfvOES;
			private readonly IntPtr glGetTexGenivOES;
			private readonly IntPtr glGetTexGenxvOES;
			private readonly IntPtr glBindVertexArrayOES;
			private readonly IntPtr glDeleteVertexArraysOES;
			private readonly IntPtr glGenVertexArraysOES;
			private readonly IntPtr glIsVertexArrayOES;
			private readonly IntPtr glCopyTextureLevelsAPPLE;
			private readonly IntPtr glRenderbufferStorageMultisampleAPPLE;
			private readonly IntPtr glResolveMultisampleFramebufferAPPLE;
			private readonly IntPtr glFenceSyncAPPLE;
			private readonly IntPtr glIsSyncAPPLE;
			private readonly IntPtr glDeleteSyncAPPLE;
			private readonly IntPtr glClientWaitSyncAPPLE;
			private readonly IntPtr glWaitSyncAPPLE;
			private readonly IntPtr glGetInteger64vAPPLE;
			private readonly IntPtr glGetSyncivAPPLE;
			private readonly IntPtr glMapBufferRangeEXT;
			private readonly IntPtr glFlushMappedBufferRangeEXT;
			private readonly IntPtr glRenderbufferStorageMultisampleEXT;
			private readonly IntPtr glFramebufferTexture2DMultisampleEXT;
			private readonly IntPtr glGetGraphicsResetStatusEXT;
			private readonly IntPtr glReadnPixelsEXT;
			private readonly IntPtr glGetnUniformfvEXT;
			private readonly IntPtr glGetnUniformivEXT;
			private readonly IntPtr glTexStorage1DEXT;
			private readonly IntPtr glTexStorage2DEXT;
			private readonly IntPtr glTexStorage3DEXT;
			private readonly IntPtr glTextureStorage1DEXT;
			private readonly IntPtr glTextureStorage2DEXT;
			private readonly IntPtr glTextureStorage3DEXT;
			private readonly IntPtr glClipPlanefIMG;
			private readonly IntPtr glClipPlanexIMG;
			private readonly IntPtr glRenderbufferStorageMultisampleIMG;
			private readonly IntPtr glFramebufferTexture2DMultisampleIMG;
			private readonly IntPtr glStartTilingQCOM;
			private readonly IntPtr glEndTilingQCOM;
			private readonly IntPtr evasglCreateSync;
			private readonly IntPtr evasglDestroySync;
			private readonly IntPtr evasglClientWaitSync;
			private readonly IntPtr evasglSignalSync;
			private readonly IntPtr evasglGetSyncAttrib;
			private readonly IntPtr evasglWaitSync;
			private readonly IntPtr evasglBindWaylandDisplay;
			private readonly IntPtr evasglUnbindWaylandDisplay;
			private readonly IntPtr evasglQueryWaylandBuffer;
			private readonly IntPtr glBeginQuery;
			private readonly IntPtr glBeginTransformFeedback;
			private readonly IntPtr glBindBufferBase;
			private readonly IntPtr glBindBufferRange;
			private readonly IntPtr glBindSampler;
			private readonly IntPtr glBindTransformFeedback;
			private readonly IntPtr glBindVertexArray;
			private readonly IntPtr glBlitFramebuffer;
			private readonly IntPtr glClearBufferfi;
			private readonly IntPtr glClearBufferfv;
			private readonly IntPtr glClearBufferiv;
			private readonly IntPtr glClearBufferuiv;
			private readonly IntPtr glClientWaitSync;
			private readonly IntPtr glCompressedTexImage3D;
			private readonly IntPtr glCompressedTexSubImage3D;
			private readonly IntPtr glCopyBufferSubData;
			private readonly IntPtr glCopyTexSubImage3D;
			private readonly IntPtr glDeleteQueries;
			private readonly IntPtr glDeleteSamplers;
			private readonly IntPtr glDeleteSync;
			private readonly IntPtr glDeleteTransformFeedbacks;
			private readonly IntPtr glDeleteVertexArrays;
			private readonly IntPtr glDrawArraysInstanced;
			private readonly IntPtr glDrawBuffers;
			private readonly IntPtr glDrawElementsInstanced;
			private readonly IntPtr glDrawRangeElements;
			private readonly IntPtr glEndQuery;
			private readonly IntPtr glEndTransformFeedback;
			private readonly IntPtr glFenceSync;
			private readonly IntPtr glFlushMappedBufferRange;
			private readonly IntPtr glFramebufferTextureLayer;
			private readonly IntPtr glGenQueries;
			private readonly IntPtr glGenSamplers;
			private readonly IntPtr glGenTransformFeedbacks;
			private readonly IntPtr glGenVertexArrays;
			private readonly IntPtr glGetActiveUniformBlockiv;
			private readonly IntPtr glGetActiveUniformBlockName;
			private readonly IntPtr glGetActiveUniformsiv;
			private readonly IntPtr glGetBufferParameteri64v;
			private readonly IntPtr glGetBufferPointerv;
			private readonly IntPtr glGetFragDataLocation;
			private readonly IntPtr glGetInteger64i_v;
			private readonly IntPtr glGetInteger64v;
			private readonly IntPtr glGetIntegeri_v;
			private readonly IntPtr glGetInternalformativ;
			private readonly IntPtr glGetProgramBinary;
			private readonly IntPtr glGetQueryiv;
			private readonly IntPtr glGetQueryObjectuiv;
			private readonly IntPtr glGetSamplerParameterfv;
			private readonly IntPtr glGetSamplerParameteriv;
			private readonly IntPtr glGetStringi;
			private readonly IntPtr glGetSynciv;
			private readonly IntPtr glGetTransformFeedbackVarying;
			private readonly IntPtr glGetUniformBlockIndex;
			private readonly IntPtr glGetUniformIndices;
			private readonly IntPtr glGetUniformuiv;
			private readonly IntPtr glGetVertexAttribIiv;
			private readonly IntPtr glGetVertexAttribIuiv;
			private readonly IntPtr glInvalidateFramebuffer;
			private readonly IntPtr glInvalidateSubFramebuffer;
			private readonly IntPtr glIsQuery;
			private readonly IntPtr glIsSampler;
			private readonly IntPtr glIsSync;
			private readonly IntPtr glIsTransformFeedback;
			private readonly IntPtr glIsVertexArray;
			private readonly IntPtr glMapBufferRange;
			private readonly IntPtr glPauseTransformFeedback;
			private readonly IntPtr glProgramBinary;
			private readonly IntPtr glProgramParameteri;
			private readonly IntPtr glReadBuffer;
			private readonly IntPtr glRenderbufferStorageMultisample;
			private readonly IntPtr glResumeTransformFeedback;
			private readonly IntPtr glSamplerParameterf;
			private readonly IntPtr glSamplerParameterfv;
			private readonly IntPtr glSamplerParameteri;
			private readonly IntPtr glSamplerParameteriv;
			private readonly IntPtr glTexImage3D;
			private readonly IntPtr glTexStorage2D;
			private readonly IntPtr glTexStorage3D;
			private readonly IntPtr glTexSubImage3D;
			private readonly IntPtr glTransformFeedbackVaryings;
			private readonly IntPtr glUniform1ui;
			private readonly IntPtr glUniform1uiv;
			private readonly IntPtr glUniform2ui;
			private readonly IntPtr glUniform2uiv;
			private readonly IntPtr glUniform3ui;
			private readonly IntPtr glUniform3uiv;
			private readonly IntPtr glUniform4ui;
			private readonly IntPtr glUniform4uiv;
			private readonly IntPtr glUniformBlockBinding;
			private readonly IntPtr glUniformMatrix2x3fv;
			private readonly IntPtr glUniformMatrix3x2fv;
			private readonly IntPtr glUniformMatrix2x4fv;
			private readonly IntPtr glUniformMatrix4x2fv;
			private readonly IntPtr glUniformMatrix3x4fv;
			private readonly IntPtr glUniformMatrix4x3fv;
			private readonly IntPtr glUnmapBuffer;
			private readonly IntPtr glVertexAttribDivisor;
			private readonly IntPtr glVertexAttribI4i;
			private readonly IntPtr glVertexAttribI4iv;
			private readonly IntPtr glVertexAttribI4ui;
			private readonly IntPtr glVertexAttribI4uiv;
			private readonly IntPtr glVertexAttribIPointer;
			private readonly IntPtr glWaitSync;
			private readonly IntPtr glDispatchCompute;
			private readonly IntPtr glDispatchComputeIndirect;
			private readonly IntPtr glDrawArraysIndirect;
			private readonly IntPtr glDrawElementsIndirect;
			private readonly IntPtr glFramebufferParameteri;
			private readonly IntPtr glGetFramebufferParameteriv;
			private readonly IntPtr glGetProgramInterfaceiv;
			private readonly IntPtr glGetProgramResourceIndex;
			private readonly IntPtr glGetProgramResourceName;
			private readonly IntPtr glGetProgramResourceiv;
			private readonly IntPtr glGetProgramResourceLocation;
			private readonly IntPtr glUseProgramStages;
			private readonly IntPtr glActiveShaderProgram;
			private readonly IntPtr glCreateShaderProgramv;
			private readonly IntPtr glBindProgramPipeline;
			private readonly IntPtr glDeleteProgramPipelines;
			private readonly IntPtr glGenProgramPipelines;
			private readonly IntPtr glIsProgramPipeline;
			private readonly IntPtr glGetProgramPipelineiv;
			private readonly IntPtr glProgramUniform1i;
			private readonly IntPtr glProgramUniform2i;
			private readonly IntPtr glProgramUniform3i;
			private readonly IntPtr glProgramUniform4i;
			private readonly IntPtr glProgramUniform1ui;
			private readonly IntPtr glProgramUniform2ui;
			private readonly IntPtr glProgramUniform3ui;
			private readonly IntPtr glProgramUniform4ui;
			private readonly IntPtr glProgramUniform1f;
			private readonly IntPtr glProgramUniform2f;
			private readonly IntPtr glProgramUniform3f;
			private readonly IntPtr glProgramUniform4f;
			private readonly IntPtr glProgramUniform1iv;
			private readonly IntPtr glProgramUniform2iv;
			private readonly IntPtr glProgramUniform3iv;
			private readonly IntPtr glProgramUniform4iv;
			private readonly IntPtr glProgramUniform1uiv;
			private readonly IntPtr glProgramUniform2uiv;
			private readonly IntPtr glProgramUniform3uiv;
			private readonly IntPtr glProgramUniform4uiv;
			private readonly IntPtr glProgramUniform1fv;
			private readonly IntPtr glProgramUniform2fv;
			private readonly IntPtr glProgramUniform3fv;
			private readonly IntPtr glProgramUniform4fv;
			private readonly IntPtr glProgramUniformMatrix2fv;
			private readonly IntPtr glProgramUniformMatrix3fv;
			private readonly IntPtr glProgramUniformMatrix4fv;
			private readonly IntPtr glProgramUniformMatrix2x3fv;
			private readonly IntPtr glProgramUniformMatrix3x2fv;
			private readonly IntPtr glProgramUniformMatrix2x4fv;
			private readonly IntPtr glProgramUniformMatrix4x2fv;
			private readonly IntPtr glProgramUniformMatrix3x4fv;
			private readonly IntPtr glProgramUniformMatrix4x3fv;
			private readonly IntPtr glValidateProgramPipeline;
			private readonly IntPtr glGetProgramPipelineInfoLog;
			private readonly IntPtr glBindImageTexture;
			private readonly IntPtr glGetBooleani_v;
			private readonly IntPtr glMemoryBarrier;
			private readonly IntPtr glMemoryBarrierByRegion;
			private readonly IntPtr glTexStorage2DMultisample;
			private readonly IntPtr glGetMultisamplefv;
			private readonly IntPtr glSampleMaski;
			private readonly IntPtr glGetTexLevelParameteriv;
			private readonly IntPtr glGetTexLevelParameterfv;
			private readonly IntPtr glBindVertexBuffer;
			private readonly IntPtr glVertexAttribFormat;
			private readonly IntPtr glVertexAttribIFormat;
			private readonly IntPtr glVertexAttribBinding;
			private readonly IntPtr glVertexBindingDivisor;
			private readonly IntPtr glBlendBarrier;
			private readonly IntPtr glCopyImageSubData;
			private readonly IntPtr glDebugMessageControl;
			private readonly IntPtr glDebugMessageInsert;
			private readonly IntPtr glDebugMessageCallback;
			private readonly IntPtr glGetDebugMessageLog;
			private readonly IntPtr glPushDebugGroup;
			private readonly IntPtr glPopDebugGroup;
			private readonly IntPtr glObjectLabel;
			private readonly IntPtr glGetObjectLabel;
			private readonly IntPtr glObjectPtrLabel;
			private readonly IntPtr glGetObjectPtrLabel;
			private readonly IntPtr glEnablei;
			private readonly IntPtr glDisablei;
			private readonly IntPtr glBlendEquationi;
			private readonly IntPtr glBlendEquationSeparatei;
			private readonly IntPtr glBlendFunci;
			private readonly IntPtr glBlendFuncSeparatei;
			private readonly IntPtr glColorMaski;
			private readonly IntPtr glIsEnabledi;
			private readonly IntPtr glDrawElementsBaseVertex;
			private readonly IntPtr glDrawRangeElementsBaseVertex;
			private readonly IntPtr glDrawElementsInstancedBaseVertex;
			private readonly IntPtr glFramebufferTexture;
			private readonly IntPtr glPrimitiveBoundingBox;
			private readonly IntPtr glGetGraphicsResetStatus;
			private readonly IntPtr glReadnPixels;
			private readonly IntPtr glGetnUniformfv;
			private readonly IntPtr glGetnUniformiv;
			private readonly IntPtr glGetnUniformuiv;
			private readonly IntPtr glMinSampleShading;
			private readonly IntPtr glPatchParameteri;
			private readonly IntPtr glTexParameterIiv;
			private readonly IntPtr glTexParameterIuiv;
			private readonly IntPtr glGetTexParameterIiv;
			private readonly IntPtr glGetTexParameterIuiv;
			private readonly IntPtr glSamplerParameterIiv;
			private readonly IntPtr glSamplerParameterIuiv;
			private readonly IntPtr glGetSamplerParameterIiv;
			private readonly IntPtr glGetSamplerParameterIuiv;
			private readonly IntPtr glTexBuffer;
			private readonly IntPtr glTexBufferRange;
			private readonly IntPtr glTexStorage3DMultisample;
#pragma warning restore 0169
		}
#endif
	}
}
