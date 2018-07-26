using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests
{
	internal class GlxContext : GlContext
	{
		private IntPtr fDisplay;
		private IntPtr fPixmap;
		private IntPtr fGlxPixmap;
		private IntPtr fContext;

		public GlxContext()
		{
			fDisplay = Xlib.XOpenDisplay(null);
			if (fDisplay == IntPtr.Zero) {
				Destroy();
				throw new Exception("Failed to open X display.");
			}

			var visualAttribs = new [] {
				Glx.GLX_X_RENDERABLE, Xlib.True,
				Glx.GLX_DRAWABLE_TYPE, Glx.GLX_PIXMAP_BIT,
				Glx.GLX_RENDER_TYPE, Glx.GLX_RGBA_BIT,
				// Glx.GLX_DOUBLEBUFFER, Xlib.True,
				Glx.GLX_RED_SIZE, 8,
				Glx.GLX_GREEN_SIZE, 8,
				Glx.GLX_BLUE_SIZE, 8,
				Glx.GLX_ALPHA_SIZE, 8,
				Glx.GLX_DEPTH_SIZE, 24,
				Glx.GLX_STENCIL_SIZE, 8,
				// Glx.GLX_SAMPLE_BUFFERS, 1,
				// Glx.GLX_SAMPLES, 4,
				Xlib.None
			};
			
			int glxMajor, glxMinor;

			if (!Glx.glXQueryVersion(fDisplay, out glxMajor, out glxMinor) ||
				(glxMajor < 1) ||
				(glxMajor == 1 && glxMinor < 3)) {
				Destroy();
				throw new Exception($"GLX version 1.3 or higher required ({glxMajor}.{glxMinor} provided).");
			}

			var fbc = Glx.ChooseFBConfig(fDisplay, Xlib.XDefaultScreen(fDisplay), visualAttribs);
			if (fbc.Length == 0) {
				Destroy();
				throw new Exception("Failed to retrieve a framebuffer config.");
			}

			var bestFBC = IntPtr.Zero;
			var bestNumSamp = -1;
			for (int i = 0; i < fbc.Length; i++) {

				int sampleBuf, samples;
				Glx.glXGetFBConfigAttrib(fDisplay, fbc[i], Glx.GLX_SAMPLE_BUFFERS, out sampleBuf);
				Glx.glXGetFBConfigAttrib(fDisplay, fbc[i], Glx.GLX_SAMPLES, out samples);

				if (bestFBC == IntPtr.Zero || (sampleBuf > 0 && samples > bestNumSamp)) {
					bestFBC = fbc[i];
					bestNumSamp = samples;
				}
			}
			var vi = Glx.GetVisualFromFBConfig(fDisplay, bestFBC);

			fPixmap = Xlib.XCreatePixmap(fDisplay, Xlib.XRootWindow(fDisplay, vi.screen), 10, 10, (uint)vi.depth);
			if (fPixmap == IntPtr.Zero) {
				Destroy();
				throw new Exception("Failed to create pixmap.");
			}
			
			fGlxPixmap = Glx.glXCreateGLXPixmap(fDisplay, ref vi, fPixmap);

			var glxExts = Glx.QueryExtensions(fDisplay, Xlib.XDefaultScreen(fDisplay));
			if (Array.IndexOf(glxExts, "GLX_ARB_create_context") == -1 ||
				Glx.glXCreateContextAttribsARB == null) {
				Console.WriteLine("OpenGL 3.0 doesn't seem to be available.");
				fContext = Glx.glXCreateNewContext(fDisplay, bestFBC, Glx.GLX_RGBA_TYPE, IntPtr.Zero, Xlib.True);
			} else {
				// Let's just use OpenGL 3.0, but we could try find the highest
				int major = 3, minor = 0;
				var flags = new List<int> {
					Glx.GLX_CONTEXT_MAJOR_VERSION_ARB, major,
					Glx.GLX_CONTEXT_MINOR_VERSION_ARB, minor,
				};
				if (major > 2) {
					flags.AddRange(new[] {
						Glx.GLX_CONTEXT_PROFILE_MASK_ARB, Glx.GLX_CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB,
					});
				}
				flags.Add(Xlib.None);

				fContext = Glx.glXCreateContextAttribsARB(fDisplay, bestFBC, IntPtr.Zero, Xlib.True, flags.ToArray());
			}
			if (fContext == IntPtr.Zero) {
				Destroy();
				throw new Exception("Failed to create an OpenGL context.");
			}

			if (!Glx.glXIsDirect(fDisplay, fContext)) {
				Console.WriteLine("Obtained indirect GLX rendering context.");
			}
		}

		public override void MakeCurrent()
		{
			if (!Glx.glXMakeCurrent(fDisplay, fGlxPixmap, fContext)) {
				Destroy();
				throw new Exception("Failed to set the context.");
			}
		}

		public override void SwapBuffers()
		{
			Glx.glXSwapBuffers(fDisplay, fGlxPixmap);
		}

		public override void Destroy()
		{
			if (fDisplay != IntPtr.Zero) {
				Glx.glXMakeCurrent(fDisplay, IntPtr.Zero, IntPtr.Zero);

				if (fContext != IntPtr.Zero) {
					Glx.glXDestroyContext(fDisplay, fContext);
					fContext = IntPtr.Zero;
				}

				if (fGlxPixmap != IntPtr.Zero) {
					Glx.glXDestroyGLXPixmap(fDisplay, fGlxPixmap);
					fGlxPixmap = IntPtr.Zero;
				}

				if (fPixmap != IntPtr.Zero) {
					Xlib.XFreePixmap(fDisplay, fPixmap);
					fPixmap = IntPtr.Zero;
				}
				
				fDisplay = IntPtr.Zero;
			}
		}

		public override GRGlTextureInfo CreateTexture(SKSizeI textureSize)
		{
			var textures = new uint[1];
			Glx.glGenTextures(textures.Length, textures);
			var textureId = textures[0];

			Glx.glBindTexture(Glx.GL_TEXTURE_2D, textureId);
			Glx.glTexImage2D(Glx.GL_TEXTURE_2D, 0, Glx.GL_RGBA, textureSize.Width, textureSize.Height, 0, Glx.GL_RGBA, Glx.GL_UNSIGNED_BYTE, IntPtr.Zero);
			Glx.glBindTexture(Glx.GL_TEXTURE_2D, 0);

			return new GRGlTextureInfo {
				Id = textureId,
				Target = Glx.GL_TEXTURE_2D,
				Format = Glx.GL_RGBA8
			};
		}

		public override void DestroyTexture(uint texture)
		{
			Glx.glDeleteTextures(1, new[] { texture });
		}
	}
}
