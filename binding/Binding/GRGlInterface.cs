using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	public class GRGlInterface : SKObject
	{
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

		public static GRGlInterface CreateNativeEvasInterface (IntPtr evas)
		{
#if __TIZEN__
			var evasLoader = new EvasGlLoader (evas);
			return AssembleGlesInterface ((ctx, name) => evasLoader.GetFunctionPointer (name));
#else
			return null;
#endif
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
			var del = get != null && context != null
				? new GRGlGetProcDelegate ((_, name) => get (context, name))
				: get;
			var proxy = DelegateProxies.Create (del, DelegateProxies.GRGlGetProcDelegateProxy, out var gch, out var ctx);
			try {
				return GetObject<GRGlInterface> (SkiaApi.gr_glinterface_assemble_interface (ctx, proxy));
			} finally {
				gch.Free ();
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
			var del = get != null && context != null
				? new GRGlGetProcDelegate ((_, name) => get (context, name))
				: get;
			var proxy = DelegateProxies.Create (del, DelegateProxies.GRGlGetProcDelegateProxy, out var gch, out var ctx);
			try {
				return GetObject<GRGlInterface> (SkiaApi.gr_glinterface_assemble_gl_interface (ctx, proxy));
			} finally {
				gch.Free ();
			}
		}

		public static GRGlInterface AssembleGlesInterface (GRGlGetProcDelegate get)
		{
			return AssembleGlesInterface (null, get);
		}

		public static GRGlInterface AssembleGlesInterface (object context, GRGlGetProcDelegate get)
		{
			var del = get != null && context != null
				? new GRGlGetProcDelegate ((_, name) => get (context, name))
				: get;
			var proxy = DelegateProxies.Create (del, DelegateProxies.GRGlGetProcDelegateProxy, out var gch, out var ctx);
			try {
				return GetObject<GRGlInterface> (SkiaApi.gr_glinterface_assemble_gles_interface (ctx, proxy));
			} finally {
				gch.Free ();
			}
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

			public IntPtr GetFunctionPointer(string name)
			{
				var ret = evas_gl_proc_address_get (glEvas, name);

				if (ret == IntPtr.Zero) {
					var field = typeof (EvasGlApi).GetField (name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

					if (field?.FieldType == typeof (IntPtr))
						ret = (IntPtr) field.GetValue (api);
				}

				return ret;
			}
		}

		// this structure is initialized from a native pointer
		[Preserve (AllMembers = true)]
		private struct EvasGlApi
		{
			// DO NOT change the order, needs to be as specified in struct _Evas_GL_API (/platform/upstream/efl/src/lib/evas/Evas_GL.h)
			// DO NOT change the names, they need to match the OpenGL API
#pragma warning disable 0169
			private int version;
			private IntPtr glActiveTexture;
			private IntPtr glAttachShader;
			private IntPtr glBindAttribLocation;
			private IntPtr glBindBuffer;
			private IntPtr glBindFramebuffer;
			private IntPtr glBindRenderbuffer;
			private IntPtr glBindTexture;
			private IntPtr glBlendColor;
			private IntPtr glBlendEquation;
			private IntPtr glBlendEquationSeparate;
			private IntPtr glBlendFunc;
			private IntPtr glBlendFuncSeparate;
			private IntPtr glBufferData;
			private IntPtr glBufferSubData;
			private IntPtr glCheckFramebufferStatus;
			private IntPtr glClear;
			private IntPtr glClearColor;
			private IntPtr glClearDepthf;
			private IntPtr glClearStencil;
			private IntPtr glColorMask;
			private IntPtr glCompileShader;
			private IntPtr glCompressedTexImage2D;
			private IntPtr glCompressedTexSubImage2D;
			private IntPtr glCopyTexImage2D;
			private IntPtr glCopyTexSubImage2D;
			private IntPtr glCreateProgram;
			private IntPtr glCreateShader;
			private IntPtr glCullFace;
			private IntPtr glDeleteBuffers;
			private IntPtr glDeleteFramebuffers;
			private IntPtr glDeleteProgram;
			private IntPtr glDeleteRenderbuffers;
			private IntPtr glDeleteShader;
			private IntPtr glDeleteTextures;
			private IntPtr glDepthFunc;
			private IntPtr glDepthMask;
			private IntPtr glDepthRangef;
			private IntPtr glDetachShader;
			private IntPtr glDisable;
			private IntPtr glDisableVertexAttribArray;
			private IntPtr glDrawArrays;
			private IntPtr glDrawElements;
			private IntPtr glEnable;
			private IntPtr glEnableVertexAttribArray;
			private IntPtr glFinish;
			private IntPtr glFlush;
			private IntPtr glFramebufferRenderbuffer;
			private IntPtr glFramebufferTexture2D;
			private IntPtr glFrontFace;
			private IntPtr glGenBuffers;
			private IntPtr glGenerateMipmap;
			private IntPtr glGenFramebuffers;
			private IntPtr glGenRenderbuffers;
			private IntPtr glGenTextures;
			private IntPtr glGetActiveAttrib;
			private IntPtr glGetActiveUniform;
			private IntPtr glGetAttachedShaders;
			private IntPtr glGetAttribLocation;
			private IntPtr glGetBooleanv;
			private IntPtr glGetBufferParameteriv;
			private IntPtr glGetError;
			private IntPtr glGetFloatv;
			private IntPtr glGetFramebufferAttachmentParameteriv;
			private IntPtr glGetIntegerv;
			private IntPtr glGetProgramiv;
			private IntPtr glGetProgramInfoLog;
			private IntPtr glGetRenderbufferParameteriv;
			private IntPtr glGetShaderiv;
			private IntPtr glGetShaderInfoLog;
			private IntPtr glGetShaderPrecisionFormat;
			private IntPtr glGetShaderSource;
			private IntPtr glGetString;
			private IntPtr glGetTexParameterfv;
			private IntPtr glGetTexParameteriv;
			private IntPtr glGetUniformfv;
			private IntPtr glGetUniformiv;
			private IntPtr glGetUniformLocation;
			private IntPtr glGetVertexAttribfv;
			private IntPtr glGetVertexAttribiv;
			private IntPtr glGetVertexAttribPointerv;
			private IntPtr glHint;
			private IntPtr glIsBuffer;
			private IntPtr glIsEnabled;
			private IntPtr glIsFramebuffer;
			private IntPtr glIsProgram;
			private IntPtr glIsRenderbuffer;
			private IntPtr glIsShader;
			private IntPtr glIsTexture;
			private IntPtr glLineWidth;
			private IntPtr glLinkProgram;
			private IntPtr glPixelStorei;
			private IntPtr glPolygonOffset;
			private IntPtr glReadPixels;
			private IntPtr glReleaseShaderCompiler;
			private IntPtr glRenderbufferStorage;
			private IntPtr glSampleCoverage;
			private IntPtr glScissor;
			private IntPtr glShaderBinary;
			private IntPtr glShaderSource;
			private IntPtr glStencilFunc;
			private IntPtr glStencilFuncSeparate;
			private IntPtr glStencilMask;
			private IntPtr glStencilMaskSeparate;
			private IntPtr glStencilOp;
			private IntPtr glStencilOpSeparate;
			private IntPtr glTexImage2D;
			private IntPtr glTexParameterf;
			private IntPtr glTexParameterfv;
			private IntPtr glTexParameteri;
			private IntPtr glTexParameteriv;
			private IntPtr glTexSubImage2D;
			private IntPtr glUniform1f;
			private IntPtr glUniform1fv;
			private IntPtr glUniform1i;
			private IntPtr glUniform1iv;
			private IntPtr glUniform2f;
			private IntPtr glUniform2fv;
			private IntPtr glUniform2i;
			private IntPtr glUniform2iv;
			private IntPtr glUniform3f;
			private IntPtr glUniform3fv;
			private IntPtr glUniform3i;
			private IntPtr glUniform3iv;
			private IntPtr glUniform4f;
			private IntPtr glUniform4fv;
			private IntPtr glUniform4i;
			private IntPtr glUniform4iv;
			private IntPtr glUniformMatrix2fv;
			private IntPtr glUniformMatrix3fv;
			private IntPtr glUniformMatrix4fv;
			private IntPtr glUseProgram;
			private IntPtr glValidateProgram;
			private IntPtr glVertexAttrib1f;
			private IntPtr glVertexAttrib1fv;
			private IntPtr glVertexAttrib2f;
			private IntPtr glVertexAttrib2fv;
			private IntPtr glVertexAttrib3f;
			private IntPtr glVertexAttrib3fv;
			private IntPtr glVertexAttrib4f;
			private IntPtr glVertexAttrib4fv;
			private IntPtr glVertexAttribPointer;
			private IntPtr glViewport;
			private IntPtr glEvasGLImageTargetTexture2DOES;
			private IntPtr glEvasGLImageTargetRenderbufferStorageOES;
			private IntPtr glGetProgramBinaryOES;
			private IntPtr glProgramBinaryOES;
			private IntPtr glMapBufferOES;
			private IntPtr glUnmapBufferOES;
			private IntPtr glGetBufferPointervOES;
			private IntPtr glTexImage3DOES;
			private IntPtr glTexSubImage3DOES;
			private IntPtr glCopyTexSubImage3DOES;
			private IntPtr glCompressedTexImage3DOES;
			private IntPtr glCompressedTexSubImage3DOES;
			private IntPtr glFramebufferTexture3DOES;
			private IntPtr glGetPerfMonitorGroupsAMD;
			private IntPtr glGetPerfMonitorCountersAMD;
			private IntPtr glGetPerfMonitorGroupStringAMD;
			private IntPtr glGetPerfMonitorCounterStringAMD;
			private IntPtr glGetPerfMonitorCounterInfoAMD;
			private IntPtr glGenPerfMonitorsAMD;
			private IntPtr glDeletePerfMonitorsAMD;
			private IntPtr glSelectPerfMonitorCountersAMD;
			private IntPtr glBeginPerfMonitorAMD;
			private IntPtr glEndPerfMonitorAMD;
			private IntPtr glGetPerfMonitorCounterDataAMD;
			private IntPtr glDiscardFramebufferEXT;
			private IntPtr glMultiDrawArraysEXT;
			private IntPtr glMultiDrawElementsEXT;
			private IntPtr glDeleteFencesNV;
			private IntPtr glGenFencesNV;
			private IntPtr glIsFenceNV;
			private IntPtr glTestFenceNV;
			private IntPtr glGetFenceivNV;
			private IntPtr glFinishFenceNV;
			private IntPtr glSetFenceNV;
			private IntPtr glGetDriverControlsQCOM;
			private IntPtr glGetDriverControlStringQCOM;
			private IntPtr glEnableDriverControlQCOM;
			private IntPtr glDisableDriverControlQCOM;
			private IntPtr glExtGetTexturesQCOM;
			private IntPtr glExtGetBuffersQCOM;
			private IntPtr glExtGetRenderbuffersQCOM;
			private IntPtr glExtGetFramebuffersQCOM;
			private IntPtr glExtGetTexLevelParameterivQCOM;
			private IntPtr glExtTexObjectStateOverrideiQCOM;
			private IntPtr glExtGetTexSubImageQCOM;
			private IntPtr glExtGetBufferPointervQCOM;
			private IntPtr glExtGetShadersQCOM;
			private IntPtr glExtGetProgramsQCOM;
			private IntPtr glExtIsProgramBinaryQCOM;
			private IntPtr glExtGetProgramBinarySourceQCOM;
			private IntPtr evasglCreateImage;
			private IntPtr evasglDestroyImage;
			private IntPtr evasglCreateImageForContext;
			private IntPtr glAlphaFunc;
			private IntPtr glClipPlanef;
			private IntPtr glColor4f;
			private IntPtr glFogf;
			private IntPtr glFogfv;
			private IntPtr glFrustumf;
			private IntPtr glGetClipPlanef;
			private IntPtr glGetLightfv;
			private IntPtr glGetMaterialfv;
			private IntPtr glGetTexEnvfv;
			private IntPtr glLightModelf;
			private IntPtr glLightModelfv;
			private IntPtr glLightf;
			private IntPtr glLightfv;
			private IntPtr glLoadMatrixf;
			private IntPtr glMaterialf;
			private IntPtr glMaterialfv;
			private IntPtr glMultMatrixf;
			private IntPtr glMultiTexCoord4f;
			private IntPtr glNormal3f;
			private IntPtr glOrthof;
			private IntPtr glPointParameterf;
			private IntPtr glPointParameterfv;
			private IntPtr glPointSize;
			private IntPtr glPointSizePointerOES;
			private IntPtr glRotatef;
			private IntPtr glScalef;
			private IntPtr glTexEnvf;
			private IntPtr glTexEnvfv;
			private IntPtr glTranslatef;
			private IntPtr glAlphaFuncx;
			private IntPtr glClearColorx;
			private IntPtr glClearDepthx;
			private IntPtr glClientActiveTexture;
			private IntPtr glClipPlanex;
			private IntPtr glColor4ub;
			private IntPtr glColor4x;
			private IntPtr glColorPointer;
			private IntPtr glDepthRangex;
			private IntPtr glDisableClientState;
			private IntPtr glEnableClientState;
			private IntPtr glFogx;
			private IntPtr glFogxv;
			private IntPtr glFrustumx;
			private IntPtr glGetClipPlanex;
			private IntPtr glGetFixedv;
			private IntPtr glGetLightxv;
			private IntPtr glGetMaterialxv;
			private IntPtr glGetPointerv;
			private IntPtr glGetTexEnviv;
			private IntPtr glGetTexEnvxv;
			private IntPtr glGetTexParameterxv;
			private IntPtr glLightModelx;
			private IntPtr glLightModelxv;
			private IntPtr glLightx;
			private IntPtr glLightxv;
			private IntPtr glLineWidthx;
			private IntPtr glLoadIdentity;
			private IntPtr glLoadMatrixx;
			private IntPtr glLogicOp;
			private IntPtr glMaterialx;
			private IntPtr glMaterialxv;
			private IntPtr glMatrixMode;
			private IntPtr glMultMatrixx;
			private IntPtr glMultiTexCoord4x;
			private IntPtr glNormal3x;
			private IntPtr glNormalPointer;
			private IntPtr glOrthox;
			private IntPtr glPointParameterx;
			private IntPtr glPointParameterxv;
			private IntPtr glPointSizex;
			private IntPtr glPolygonOffsetx;
			private IntPtr glPopMatrix;
			private IntPtr glPushMatrix;
			private IntPtr glRotatex;
			private IntPtr glSampleCoveragex;
			private IntPtr glScalex;
			private IntPtr glShadeModel;
			private IntPtr glTexCoordPointer;
			private IntPtr glTexEnvi;
			private IntPtr glTexEnvx;
			private IntPtr glTexEnviv;
			private IntPtr glTexEnvxv;
			private IntPtr glTexParameterx;
			private IntPtr glTexParameterxv;
			private IntPtr glTranslatex;
			private IntPtr glVertexPointer;
			private IntPtr glBlendEquationSeparateOES;
			private IntPtr glBlendFuncSeparateOES;
			private IntPtr glBlendEquationOES;
			private IntPtr glDrawTexsOES;
			private IntPtr glDrawTexiOES;
			private IntPtr glDrawTexxOES;
			private IntPtr glDrawTexsvOES;
			private IntPtr glDrawTexivOES;
			private IntPtr glDrawTexxvOES;
			private IntPtr glDrawTexfOES;
			private IntPtr glDrawTexfvOES;
			private IntPtr glAlphaFuncxOES;
			private IntPtr glClearColorxOES;
			private IntPtr glClearDepthxOES;
			private IntPtr glClipPlanexOES;
			private IntPtr glColor4xOES;
			private IntPtr glDepthRangexOES;
			private IntPtr glFogxOES;
			private IntPtr glFogxvOES;
			private IntPtr glFrustumxOES;
			private IntPtr glGetClipPlanexOES;
			private IntPtr glGetFixedvOES;
			private IntPtr glGetLightxvOES;
			private IntPtr glGetMaterialxvOES;
			private IntPtr glGetTexEnvxvOES;
			private IntPtr glGetTexParameterxvOES;
			private IntPtr glLightModelxOES;
			private IntPtr glLightModelxvOES;
			private IntPtr glLightxOES;
			private IntPtr glLightxvOES;
			private IntPtr glLineWidthxOES;
			private IntPtr glLoadMatrixxOES;
			private IntPtr glMaterialxOES;
			private IntPtr glMaterialxvOES;
			private IntPtr glMultMatrixxOES;
			private IntPtr glMultiTexCoord4xOES;
			private IntPtr glNormal3xOES;
			private IntPtr glOrthoxOES;
			private IntPtr glPointParameterxOES;
			private IntPtr glPointParameterxvOES;
			private IntPtr glPointSizexOES;
			private IntPtr glPolygonOffsetxOES;
			private IntPtr glRotatexOES;
			private IntPtr glSampleCoveragexOES;
			private IntPtr glScalexOES;
			private IntPtr glTexEnvxOES;
			private IntPtr glTexEnvxvOES;
			private IntPtr glTexParameterxOES;
			private IntPtr glTexParameterxvOES;
			private IntPtr glTranslatexOES;
			private IntPtr glIsRenderbufferOES;
			private IntPtr glBindRenderbufferOES;
			private IntPtr glDeleteRenderbuffersOES;
			private IntPtr glGenRenderbuffersOES;
			private IntPtr glRenderbufferStorageOES;
			private IntPtr glGetRenderbufferParameterivOES;
			private IntPtr glIsFramebufferOES;
			private IntPtr glBindFramebufferOES;
			private IntPtr glDeleteFramebuffersOES;
			private IntPtr glGenFramebuffersOES;
			private IntPtr glCheckFramebufferStatusOES;
			private IntPtr glFramebufferRenderbufferOES;
			private IntPtr glFramebufferTexture2DOES;
			private IntPtr glGetFramebufferAttachmentParameterivOES;
			private IntPtr glGenerateMipmapOES;
			private IntPtr glCurrentPaletteMatrixOES;
			private IntPtr glLoadPaletteFromModelViewMatrixOES;
			private IntPtr glMatrixIndexPointerOES;
			private IntPtr glWeightPointerOES;
			private IntPtr glQueryMatrixxOES;
			private IntPtr glDepthRangefOES;
			private IntPtr glFrustumfOES;
			private IntPtr glOrthofOES;
			private IntPtr glClipPlanefOES;
			private IntPtr glGetClipPlanefOES;
			private IntPtr glClearDepthfOES;
			private IntPtr glTexGenfOES;
			private IntPtr glTexGenfvOES;
			private IntPtr glTexGeniOES;
			private IntPtr glTexGenivOES;
			private IntPtr glTexGenxOES;
			private IntPtr glTexGenxvOES;
			private IntPtr glGetTexGenfvOES;
			private IntPtr glGetTexGenivOES;
			private IntPtr glGetTexGenxvOES;
			private IntPtr glBindVertexArrayOES;
			private IntPtr glDeleteVertexArraysOES;
			private IntPtr glGenVertexArraysOES;
			private IntPtr glIsVertexArrayOES;
			private IntPtr glCopyTextureLevelsAPPLE;
			private IntPtr glRenderbufferStorageMultisampleAPPLE;
			private IntPtr glResolveMultisampleFramebufferAPPLE;
			private IntPtr glFenceSyncAPPLE;
			private IntPtr glIsSyncAPPLE;
			private IntPtr glDeleteSyncAPPLE;
			private IntPtr glClientWaitSyncAPPLE;
			private IntPtr glWaitSyncAPPLE;
			private IntPtr glGetInteger64vAPPLE;
			private IntPtr glGetSyncivAPPLE;
			private IntPtr glMapBufferRangeEXT;
			private IntPtr glFlushMappedBufferRangeEXT;
			private IntPtr glRenderbufferStorageMultisampleEXT;
			private IntPtr glFramebufferTexture2DMultisampleEXT;
			private IntPtr glGetGraphicsResetStatusEXT;
			private IntPtr glReadnPixelsEXT;
			private IntPtr glGetnUniformfvEXT;
			private IntPtr glGetnUniformivEXT;
			private IntPtr glTexStorage1DEXT;
			private IntPtr glTexStorage2DEXT;
			private IntPtr glTexStorage3DEXT;
			private IntPtr glTextureStorage1DEXT;
			private IntPtr glTextureStorage2DEXT;
			private IntPtr glTextureStorage3DEXT;
			private IntPtr glClipPlanefIMG;
			private IntPtr glClipPlanexIMG;
			private IntPtr glRenderbufferStorageMultisampleIMG;
			private IntPtr glFramebufferTexture2DMultisampleIMG;
			private IntPtr glStartTilingQCOM;
			private IntPtr glEndTilingQCOM;
			private IntPtr evasglCreateSync;
			private IntPtr evasglDestroySync;
			private IntPtr evasglClientWaitSync;
			private IntPtr evasglSignalSync;
			private IntPtr evasglGetSyncAttrib;
			private IntPtr evasglWaitSync;
			private IntPtr evasglBindWaylandDisplay;
			private IntPtr evasglUnbindWaylandDisplay;
			private IntPtr evasglQueryWaylandBuffer;
			private IntPtr glBeginQuery;
			private IntPtr glBeginTransformFeedback;
			private IntPtr glBindBufferBase;
			private IntPtr glBindBufferRange;
			private IntPtr glBindSampler;
			private IntPtr glBindTransformFeedback;
			private IntPtr glBindVertexArray;
			private IntPtr glBlitFramebuffer;
			private IntPtr glClearBufferfi;
			private IntPtr glClearBufferfv;
			private IntPtr glClearBufferiv;
			private IntPtr glClearBufferuiv;
			private IntPtr glClientWaitSync;
			private IntPtr glCompressedTexImage3D;
			private IntPtr glCompressedTexSubImage3D;
			private IntPtr glCopyBufferSubData;
			private IntPtr glCopyTexSubImage3D;
			private IntPtr glDeleteQueries;
			private IntPtr glDeleteSamplers;
			private IntPtr glDeleteSync;
			private IntPtr glDeleteTransformFeedbacks;
			private IntPtr glDeleteVertexArrays;
			private IntPtr glDrawArraysInstanced;
			private IntPtr glDrawBuffers;
			private IntPtr glDrawElementsInstanced;
			private IntPtr glDrawRangeElements;
			private IntPtr glEndQuery;
			private IntPtr glEndTransformFeedback;
			private IntPtr glFenceSync;
			private IntPtr glFlushMappedBufferRange;
			private IntPtr glFramebufferTextureLayer;
			private IntPtr glGenQueries;
			private IntPtr glGenSamplers;
			private IntPtr glGenTransformFeedbacks;
			private IntPtr glGenVertexArrays;
			private IntPtr glGetActiveUniformBlockiv;
			private IntPtr glGetActiveUniformBlockName;
			private IntPtr glGetActiveUniformsiv;
			private IntPtr glGetBufferParameteri64v;
			private IntPtr glGetBufferPointerv;
			private IntPtr glGetFragDataLocation;
			private IntPtr glGetInteger64i_v;
			private IntPtr glGetInteger64v;
			private IntPtr glGetIntegeri_v;
			private IntPtr glGetInternalformativ;
			private IntPtr glGetProgramBinary;
			private IntPtr glGetQueryiv;
			private IntPtr glGetQueryObjectuiv;
			private IntPtr glGetSamplerParameterfv;
			private IntPtr glGetSamplerParameteriv;
			private IntPtr glGetStringi;
			private IntPtr glGetSynciv;
			private IntPtr glGetTransformFeedbackVarying;
			private IntPtr glGetUniformBlockIndex;
			private IntPtr glGetUniformIndices;
			private IntPtr glGetUniformuiv;
			private IntPtr glGetVertexAttribIiv;
			private IntPtr glGetVertexAttribIuiv;
			private IntPtr glInvalidateFramebuffer;
			private IntPtr glInvalidateSubFramebuffer;
			private IntPtr glIsQuery;
			private IntPtr glIsSampler;
			private IntPtr glIsSync;
			private IntPtr glIsTransformFeedback;
			private IntPtr glIsVertexArray;
			private IntPtr glMapBufferRange;
			private IntPtr glPauseTransformFeedback;
			private IntPtr glProgramBinary;
			private IntPtr glProgramParameteri;
			private IntPtr glReadBuffer;
			private IntPtr glRenderbufferStorageMultisample;
			private IntPtr glResumeTransformFeedback;
			private IntPtr glSamplerParameterf;
			private IntPtr glSamplerParameterfv;
			private IntPtr glSamplerParameteri;
			private IntPtr glSamplerParameteriv;
			private IntPtr glTexImage3D;
			private IntPtr glTexStorage2D;
			private IntPtr glTexStorage3D;
			private IntPtr glTexSubImage3D;
			private IntPtr glTransformFeedbackVaryings;
			private IntPtr glUniform1ui;
			private IntPtr glUniform1uiv;
			private IntPtr glUniform2ui;
			private IntPtr glUniform2uiv;
			private IntPtr glUniform3ui;
			private IntPtr glUniform3uiv;
			private IntPtr glUniform4ui;
			private IntPtr glUniform4uiv;
			private IntPtr glUniformBlockBinding;
			private IntPtr glUniformMatrix2x3fv;
			private IntPtr glUniformMatrix3x2fv;
			private IntPtr glUniformMatrix2x4fv;
			private IntPtr glUniformMatrix4x2fv;
			private IntPtr glUniformMatrix3x4fv;
			private IntPtr glUniformMatrix4x3fv;
			private IntPtr glUnmapBuffer;
			private IntPtr glVertexAttribDivisor;
			private IntPtr glVertexAttribI4i;
			private IntPtr glVertexAttribI4iv;
			private IntPtr glVertexAttribI4ui;
			private IntPtr glVertexAttribI4uiv;
			private IntPtr glVertexAttribIPointer;
			private IntPtr glWaitSync;
			private IntPtr glDispatchCompute;
			private IntPtr glDispatchComputeIndirect;
			private IntPtr glDrawArraysIndirect;
			private IntPtr glDrawElementsIndirect;
			private IntPtr glFramebufferParameteri;
			private IntPtr glGetFramebufferParameteriv;
			private IntPtr glGetProgramInterfaceiv;
			private IntPtr glGetProgramResourceIndex;
			private IntPtr glGetProgramResourceName;
			private IntPtr glGetProgramResourceiv;
			private IntPtr glGetProgramResourceLocation;
			private IntPtr glUseProgramStages;
			private IntPtr glActiveShaderProgram;
			private IntPtr glCreateShaderProgramv;
			private IntPtr glBindProgramPipeline;
			private IntPtr glDeleteProgramPipelines;
			private IntPtr glGenProgramPipelines;
			private IntPtr glIsProgramPipeline;
			private IntPtr glGetProgramPipelineiv;
			private IntPtr glProgramUniform1i;
			private IntPtr glProgramUniform2i;
			private IntPtr glProgramUniform3i;
			private IntPtr glProgramUniform4i;
			private IntPtr glProgramUniform1ui;
			private IntPtr glProgramUniform2ui;
			private IntPtr glProgramUniform3ui;
			private IntPtr glProgramUniform4ui;
			private IntPtr glProgramUniform1f;
			private IntPtr glProgramUniform2f;
			private IntPtr glProgramUniform3f;
			private IntPtr glProgramUniform4f;
			private IntPtr glProgramUniform1iv;
			private IntPtr glProgramUniform2iv;
			private IntPtr glProgramUniform3iv;
			private IntPtr glProgramUniform4iv;
			private IntPtr glProgramUniform1uiv;
			private IntPtr glProgramUniform2uiv;
			private IntPtr glProgramUniform3uiv;
			private IntPtr glProgramUniform4uiv;
			private IntPtr glProgramUniform1fv;
			private IntPtr glProgramUniform2fv;
			private IntPtr glProgramUniform3fv;
			private IntPtr glProgramUniform4fv;
			private IntPtr glProgramUniformMatrix2fv;
			private IntPtr glProgramUniformMatrix3fv;
			private IntPtr glProgramUniformMatrix4fv;
			private IntPtr glProgramUniformMatrix2x3fv;
			private IntPtr glProgramUniformMatrix3x2fv;
			private IntPtr glProgramUniformMatrix2x4fv;
			private IntPtr glProgramUniformMatrix4x2fv;
			private IntPtr glProgramUniformMatrix3x4fv;
			private IntPtr glProgramUniformMatrix4x3fv;
			private IntPtr glValidateProgramPipeline;
			private IntPtr glGetProgramPipelineInfoLog;
			private IntPtr glBindImageTexture;
			private IntPtr glGetBooleani_v;
			private IntPtr glMemoryBarrier;
			private IntPtr glMemoryBarrierByRegion;
			private IntPtr glTexStorage2DMultisample;
			private IntPtr glGetMultisamplefv;
			private IntPtr glSampleMaski;
			private IntPtr glGetTexLevelParameteriv;
			private IntPtr glGetTexLevelParameterfv;
			private IntPtr glBindVertexBuffer;
			private IntPtr glVertexAttribFormat;
			private IntPtr glVertexAttribIFormat;
			private IntPtr glVertexAttribBinding;
			private IntPtr glVertexBindingDivisor;
			private IntPtr glBlendBarrier;
			private IntPtr glCopyImageSubData;
			private IntPtr glDebugMessageControl;
			private IntPtr glDebugMessageInsert;
			private IntPtr glDebugMessageCallback;
			private IntPtr glGetDebugMessageLog;
			private IntPtr glPushDebugGroup;
			private IntPtr glPopDebugGroup;
			private IntPtr glObjectLabel;
			private IntPtr glGetObjectLabel;
			private IntPtr glObjectPtrLabel;
			private IntPtr glGetObjectPtrLabel;
			private IntPtr glEnablei;
			private IntPtr glDisablei;
			private IntPtr glBlendEquationi;
			private IntPtr glBlendEquationSeparatei;
			private IntPtr glBlendFunci;
			private IntPtr glBlendFuncSeparatei;
			private IntPtr glColorMaski;
			private IntPtr glIsEnabledi;
			private IntPtr glDrawElementsBaseVertex;
			private IntPtr glDrawRangeElementsBaseVertex;
			private IntPtr glDrawElementsInstancedBaseVertex;
			private IntPtr glFramebufferTexture;
			private IntPtr glPrimitiveBoundingBox;
			private IntPtr glGetGraphicsResetStatus;
			private IntPtr glReadnPixels;
			private IntPtr glGetnUniformfv;
			private IntPtr glGetnUniformiv;
			private IntPtr glGetnUniformuiv;
			private IntPtr glMinSampleShading;
			private IntPtr glPatchParameteri;
			private IntPtr glTexParameterIiv;
			private IntPtr glTexParameterIuiv;
			private IntPtr glGetTexParameterIiv;
			private IntPtr glGetTexParameterIuiv;
			private IntPtr glSamplerParameterIiv;
			private IntPtr glSamplerParameterIuiv;
			private IntPtr glGetSamplerParameterIiv;
			private IntPtr glGetSamplerParameterIuiv;
			private IntPtr glTexBuffer;
			private IntPtr glTexBufferRange;
			private IntPtr glTexStorage3DMultisample;
#pragma warning restore 0169
		}
#endif
	}
}

