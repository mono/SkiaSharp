using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SkiaSharp.Tests
{
	internal class WglContext : GlContext
	{
		private static readonly object fLock = new object();

		private static ushort gWC;

		private static IntPtr fWindow;
		private static IntPtr fDeviceContext;

		private IntPtr fPbuffer;
		private IntPtr fPbufferDC;
		private IntPtr fPbufferGlContext;

		static WglContext()
		{
			var wc = new WNDCLASS
			{
				cbClsExtra = 0,
				cbWndExtra = 0,
				hbrBackground = IntPtr.Zero,
				hCursor = User32.LoadCursor(IntPtr.Zero, (int)User32.IDC_ARROW),
				hIcon = User32.LoadIcon(IntPtr.Zero, (IntPtr)User32.IDI_APPLICATION),
				hInstance = Kernel32.CurrentModuleHandle,
				lpfnWndProc = (WNDPROC)User32.DefWindowProc,
				lpszClassName = "Griffin",
				lpszMenuName = null,
				style = User32.CS_HREDRAW | User32.CS_VREDRAW | User32.CS_OWNDC
			};

			gWC = User32.RegisterClass(ref wc);
			if (gWC == 0)
			{
				throw new Exception("Could not register window class.");
			}

			fWindow = User32.CreateWindow(
				"Griffin",
				"The Invisible Man",
				WindowStyles.WS_OVERLAPPEDWINDOW,
				0, 0,
				1, 1,
				IntPtr.Zero, IntPtr.Zero, Kernel32.CurrentModuleHandle, IntPtr.Zero);
			if (fWindow == IntPtr.Zero)
			{
				throw new Exception($"Could not create window.");
			}

			fDeviceContext = User32.GetDC(fWindow);
			if (fDeviceContext == IntPtr.Zero)
			{
				DestroyWindow();
				throw new Exception("Could not get device context.");
			}

			if (!Wgl.HasExtension(fDeviceContext, "WGL_ARB_pixel_format") ||
				!Wgl.HasExtension(fDeviceContext, "WGL_ARB_pbuffer"))
			{
				DestroyWindow();
				throw new Exception("DC does not have extensions.");
			}
		}

		public WglContext()
		{
			var iAttrs = new int[]
			{
				Wgl.WGL_ACCELERATION_ARB, Wgl.WGL_FULL_ACCELERATION_ARB,
				Wgl.WGL_DRAW_TO_WINDOW_ARB, Wgl.TRUE,
				//Wgl.WGL_DOUBLE_BUFFER_ARB, (doubleBuffered ? TRUE : FALSE),
				Wgl.WGL_SUPPORT_OPENGL_ARB, Wgl.TRUE,
				Wgl.WGL_RED_BITS_ARB, 8,
				Wgl.WGL_GREEN_BITS_ARB, 8,
				Wgl.WGL_BLUE_BITS_ARB, 8,
				Wgl.WGL_ALPHA_BITS_ARB, 8,
				Wgl.WGL_STENCIL_BITS_ARB, 8,
				Wgl.NONE, Wgl.NONE
			};
			var piFormats = new int[1];
			uint nFormats;
			lock (fLock)
			{
				// HACK: This call seems to cause deadlocks on some systems.
				Wgl.wglChoosePixelFormatARB(fDeviceContext, iAttrs, null, (uint)piFormats.Length, piFormats, out nFormats);
			}
			if (nFormats == 0)
			{
				Destroy();
				throw new Exception("Could not get pixel formats.");
			}

			fPbuffer = Wgl.wglCreatePbufferARB(fDeviceContext, piFormats[0], 1, 1, null);
			if (fPbuffer == IntPtr.Zero)
			{
				Destroy();
				throw new Exception("Could not create Pbuffer.");
			}

			fPbufferDC = Wgl.wglGetPbufferDCARB(fPbuffer);
			if (fPbufferDC == IntPtr.Zero)
			{
				Destroy();
				throw new Exception("Could not get Pbuffer DC.");
			}

			var prevDC = Wgl.wglGetCurrentDC();
			var prevGLRC = Wgl.wglGetCurrentContext();

			fPbufferGlContext = Wgl.wglCreateContext(fPbufferDC);

			Wgl.wglMakeCurrent(prevDC, prevGLRC);

			if (fPbufferGlContext == IntPtr.Zero)
			{
				Destroy();
				throw new Exception("Could not creeate Pbuffer GL context.");
			}
		}

		public override void MakeCurrent()
		{
			if (!Wgl.wglMakeCurrent(fPbufferDC, fPbufferGlContext))
			{
				Destroy();
				throw new Exception("Could not set the context.");
			}
		}

		public override void SwapBuffers()
		{
			if (!Gdi32.SwapBuffers(fPbufferDC))
			{
				Destroy();
				throw new Exception("Could not complete SwapBuffers.");
			}
		}

		public override void Destroy()
		{
			if (!Wgl.HasExtension(fPbufferDC, "WGL_ARB_pbuffer"))
			{
				// ASSERT
			}

			Wgl.wglDeleteContext(fPbufferGlContext);

			Wgl.wglReleasePbufferDCARB?.Invoke(fPbuffer, fPbufferDC);

			Wgl.wglDestroyPbufferARB?.Invoke(fPbuffer);
		}

		private static void DestroyWindow()
		{
			if (fWindow != IntPtr.Zero)
			{
				if (fDeviceContext != IntPtr.Zero)
				{
					User32.ReleaseDC(fWindow, fDeviceContext);
					fDeviceContext = IntPtr.Zero;
				}

				User32.DestroyWindow(fWindow);
				fWindow = IntPtr.Zero;
			}

			User32.UnregisterClass("Griffin", Kernel32.CurrentModuleHandle);
		}

		public override GRGlTextureInfo CreateTexture(SKSizeI textureSize)
		{
			var textures = new uint[1];
			Wgl.glGenTextures(textures.Length, textures);
			var textureId = textures[0];

			Wgl.glBindTexture(Wgl.GL_TEXTURE_2D, textureId);
			Wgl.glTexImage2D(Wgl.GL_TEXTURE_2D, 0, Wgl.GL_RGBA, textureSize.Width, textureSize.Height, 0, Wgl.GL_RGBA, Wgl.GL_UNSIGNED_BYTE, IntPtr.Zero);
			Wgl.glBindTexture(Wgl.GL_TEXTURE_2D, 0);

			return new GRGlTextureInfo
			{
				Id = textureId,
				Target = Wgl.GL_TEXTURE_2D,
				Format = Wgl.GL_RGBA8
			};
		}

		public override void DestroyTexture(uint texture)
		{
			Wgl.glDeleteTextures(1, new[] { texture });
		}
	}
}
