using System;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;
using SkiaSharp.Views.GlesInterop;

using EGLDisplay = System.IntPtr;
using EGLContext = System.IntPtr;
using EGLConfig = System.IntPtr;
using EGLSurface = System.IntPtr;

namespace SkiaSharp.Views.UWP
{
	public enum GlesMultisampling
	{
		None = 0,
		FourTimes = 4
	}

	public enum GlesBackingOption
	{
		Detroyed = 0,
		Retained = 1
	}

	public enum GlesRenderTarget
	{
		Renderbuffer = Gles.GL_RENDERBUFFER
	}

	public enum GlesDepthFormat
	{
		None,

		Format16,
		Format24,
	}

	public enum GlesStencilFormat
	{
		None,

		Format8,
	}

	public class GlesContext : IDisposable
	{
		private static readonly EGLDisplay eglDisplay;

		[ThreadStatic]
		private static GlesContext currentContext;

		private EGLContext eglContext;
		private EGLConfig eglConfig;

		private EGLSurface eglSurface;

		static GlesContext()
		{
			eglDisplay = InitializeDisplay();
		}

		public GlesContext()
		{
			eglContext = Egl.EGL_NO_CONTEXT;
			eglConfig = default(EGLConfig);

			eglSurface = Egl.EGL_NO_SURFACE;

			Initialize();
		}

		public static GlesContext CurrentContext
		{
			get { return currentContext; }
			set
			{
				var surface = value?.eglSurface ?? Egl.EGL_NO_SURFACE;
				var context = value?.eglContext ?? Egl.EGL_NO_CONTEXT;

				if (Egl.eglMakeCurrent(eglDisplay, surface, surface, context) == Egl.EGL_FALSE)
				{
					throw new Exception($"Failed to make EGLSurface current: {Egl.eglGetError():x}");
				}

				currentContext = value;
			}
		}
		
		public void SetSurface(
			SwapChainPanel swapChainPanel,
			int rbWidth, int rbHeight,
			GlesBackingOption backingOption = GlesBackingOption.Detroyed,
			GlesMultisampling multisampling = GlesMultisampling.None,
			GlesRenderTarget renderTarget = GlesRenderTarget.Renderbuffer)
		{
			if (eglSurface != Egl.EGL_NO_SURFACE)
			{
				DestroySurface(eglSurface);
				eglSurface = Egl.EGL_NO_SURFACE;
			}

			if (swapChainPanel != null)
			{
				CreateSurface(swapChainPanel, rbWidth, rbHeight, backingOption, multisampling, renderTarget);
			}
		}

		public bool HasValidSurface
		{
			get { return eglSurface != Egl.EGL_NO_SURFACE; }
		}

		public bool SwapBuffers(GlesRenderTarget renderTarget, int width, int height)
		{
			int curFB;
			Gles.glGetIntegerv(Gles.GL_FRAMEBUFFER_BINDING, out curFB);
			Gles.glBindFramebuffer(Gles.GL_DRAW_FRAMEBUFFER_ANGLE, 0);
			Gles.glBindFramebuffer(Gles.GL_READ_FRAMEBUFFER_ANGLE, (uint)curFB);
			Gles.glBlitFramebufferANGLE(0, 0, width, height, 0, 0, width, height, Gles.GL_COLOR_BUFFER_BIT, Gles.GL_NEAREST);
			Gles.glBindFramebuffer(Gles.GL_DRAW_FRAMEBUFFER_ANGLE, (uint)curFB);
			Gles.glBindFramebuffer(Gles.GL_READ_FRAMEBUFFER_ANGLE, (uint)curFB);

			return Egl.eglSwapBuffers(eglDisplay, eglSurface) == Egl.EGL_TRUE;
		}

		public void SetViewportSize(int width, int height)
		{
			Gles.glViewport(0, 0, width, height);
		}

		public void Reset()
		{
			Cleanup();
			Initialize();
		}

		public void Dispose()
		{
			Cleanup();
		}

		private static EGLDisplay InitializeDisplay()
		{
			int[] defaultDisplayAttributes =
			{
				// These are the default display attributes, used to request ANGLE's D3D11 renderer.
				// eglInitialize will only succeed with these attributes if the hardware supports D3D11 Feature Level 10_0+.
				Egl.EGL_PLATFORM_ANGLE_TYPE_ANGLE, Egl.EGL_PLATFORM_ANGLE_TYPE_D3D11_ANGLE,

				// Egl.EGL_ANGLE_DISPLAY_ALLOW_RENDER_TO_BACK_BUFFER is an optimization that can have large performance benefits on mobile devices.
				// Its syntax is subject to change, though. Please update your Visual Studio templates if you experience compilation issues with it.
				Egl.EGL_ANGLE_DISPLAY_ALLOW_RENDER_TO_BACK_BUFFER, Egl.EGL_TRUE, 

				// Egl.EGL_PLATFORM_ANGLE_ENABLE_AUTOMATIC_TRIM_ANGLE is an option that enables ANGLE to automatically call 
				// the IDXGIDevice3.Trim method on behalf of the application when it gets suspended. 
				// Calling IDXGIDevice3.Trim when an application is suspended is a Windows Store application certification requirement.
				Egl.EGL_PLATFORM_ANGLE_ENABLE_AUTOMATIC_TRIM_ANGLE, Egl.EGL_TRUE,
				Egl.EGL_NONE,
			};

			int[] fl9_3DisplayAttributes =
			{
				// These can be used to request ANGLE's D3D11 renderer, with D3D11 Feature Level 9_3.
				// These attributes are used if the call to eglInitialize fails with the default display attributes.
				Egl.EGL_PLATFORM_ANGLE_TYPE_ANGLE, Egl.EGL_PLATFORM_ANGLE_TYPE_D3D11_ANGLE,
				Egl.EGL_PLATFORM_ANGLE_MAX_VERSION_MAJOR_ANGLE, 9,
				Egl.EGL_PLATFORM_ANGLE_MAX_VERSION_MINOR_ANGLE, 3,
				Egl.EGL_ANGLE_DISPLAY_ALLOW_RENDER_TO_BACK_BUFFER, Egl.EGL_TRUE,
				Egl.EGL_PLATFORM_ANGLE_ENABLE_AUTOMATIC_TRIM_ANGLE, Egl.EGL_TRUE,
				Egl.EGL_NONE,
			};

			int[] warpDisplayAttributes =
			{
				// These attributes can be used to request D3D11 WARP.
				// They are used if eglInitialize fails with both the default display attributes and the 9_3 display attributes.
				Egl.EGL_PLATFORM_ANGLE_TYPE_ANGLE, Egl.EGL_PLATFORM_ANGLE_TYPE_D3D11_ANGLE,
				Egl.EGL_PLATFORM_ANGLE_DEVICE_TYPE_ANGLE, Egl.EGL_PLATFORM_ANGLE_DEVICE_TYPE_WARP_ANGLE,
				Egl.EGL_ANGLE_DISPLAY_ALLOW_RENDER_TO_BACK_BUFFER, Egl.EGL_TRUE,
				Egl.EGL_PLATFORM_ANGLE_ENABLE_AUTOMATIC_TRIM_ANGLE, Egl.EGL_TRUE,
				Egl.EGL_NONE,
			};

			//
			// To initialize the display, we make three sets of calls to eglGetPlatformDisplayEXT and eglInitialize, with varying 
			// parameters passed to eglGetPlatformDisplayEXT:
			// 1) The first calls uses "defaultDisplayAttributes" as a parameter. This corresponds to D3D11 Feature Level 10_0+.
			// 2) If eglInitialize fails for step 1 (e.g. because 10_0+ isn't supported by the default GPU), then we try again 
			//    using "fl9_3DisplayAttributes". This corresponds to D3D11 Feature Level 9_3.
			// 3) If eglInitialize fails for step 2 (e.g. because 9_3+ isn't supported by the default GPU), then we try again 
			//    using "warpDisplayAttributes".  This corresponds to D3D11 Feature Level 11_0 on WARP, a D3D11 software rasterizer.
			//

			// This tries to initialize EGL to D3D11 Feature Level 10_0+. See above comment for details.
			var display = Egl.eglGetPlatformDisplayEXT(Egl.EGL_PLATFORM_ANGLE_ANGLE, Egl.EGL_DEFAULT_DISPLAY, defaultDisplayAttributes);
			if (display == Egl.EGL_NO_DISPLAY)
			{
				throw new Exception("Failed to get EGL display (D3D11 10.0+).");
			}

			int major, minor;
			if (Egl.eglInitialize(display, out major, out minor) == Egl.EGL_FALSE)
			{
				// This tries to initialize EGL to D3D11 Feature Level 9_3, if 10_0+ is unavailable (e.g. on some mobile devices).
				display = Egl.eglGetPlatformDisplayEXT(Egl.EGL_PLATFORM_ANGLE_ANGLE, Egl.EGL_DEFAULT_DISPLAY, fl9_3DisplayAttributes);
				if (display == Egl.EGL_NO_DISPLAY)
				{
					throw new Exception("Failed to get EGL display (D3D11 9.3).");
				}

				if (Egl.eglInitialize(display, out major, out minor) == Egl.EGL_FALSE)
				{
					// This initializes EGL to D3D11 Feature Level 11_0 on WARP, if 9_3+ is unavailable on the default GPU.
					display = Egl.eglGetPlatformDisplayEXT(Egl.EGL_PLATFORM_ANGLE_ANGLE, Egl.EGL_DEFAULT_DISPLAY, warpDisplayAttributes);
					if (display == Egl.EGL_NO_DISPLAY)
					{
						throw new Exception("Failed to get EGL display (D3D11 11.0 WARP)");
					}

					if (Egl.eglInitialize(display, out major, out minor) == Egl.EGL_FALSE)
					{
						// If all of the calls to eglInitialize returned Egl.EGL_FALSE then an error has occurred.
						throw new Exception("Failed to initialize EGL");
					}
				}
			}

			Egl.eglBindAPI(Egl.EGL_OPENGL_ES_API);
			if (Egl.eglGetError() != Egl.EGL_SUCCESS)
			{
				throw new Exception("Failed to bind API");
			}

			return display;
		}

		private void Initialize()
		{
			int[] configAttributes =
			{
				Egl.EGL_RENDERABLE_TYPE, Egl.EGL_OPENGL_ES2_BIT,
				Egl.EGL_SURFACE_TYPE, Egl.EGL_PBUFFER_BIT | Egl.EGL_SWAP_BEHAVIOR_PRESERVED_BIT,
				Egl.EGL_RED_SIZE, 8,
				Egl.EGL_GREEN_SIZE, 8,
				Egl.EGL_BLUE_SIZE, 8,
				Egl.EGL_ALPHA_SIZE, 8,
				Egl.EGL_DEPTH_SIZE, 16,
				Egl.EGL_NONE
			};

			int numConfigs = 0;
			EGLDisplay[] configs = new EGLDisplay[1];
			if (Egl.eglChooseConfig(eglDisplay, configAttributes, configs, configs.Length, out numConfigs) == Egl.EGL_FALSE || numConfigs == 0)
			{
				throw new Exception("Failed to choose first EGLConfig");
			}
			eglConfig = configs[0];

			int[] contextAttributes =
			{
				Egl.EGL_CONTEXT_CLIENT_VERSION, 2,
				Egl.EGL_NONE
			};

			eglContext = Egl.eglCreateContext(eglDisplay, eglConfig, Egl.EGL_NO_CONTEXT, contextAttributes);
			if (eglContext == Egl.EGL_NO_CONTEXT)
			{
				throw new Exception("Failed to create EGL context");
			}

			// Create a temporary surface so that we can make the context current prior to creating the
			// renderbuffer surface
			int[] pbufferAttributeList = new[]
			{
				Egl.EGL_WIDTH, 32,
				Egl.EGL_HEIGHT, 32,
				Egl.EGL_NONE
			};

			eglSurface = Egl.eglCreatePbufferSurface(eglDisplay, eglConfig, pbufferAttributeList);

			if (eglSurface == Egl.EGL_NO_SURFACE)
			{
				throw new Exception("Failed to create EGL surface");
			}
		}

		private void Cleanup()
		{
			if (eglDisplay != Egl.EGL_NO_DISPLAY && eglContext != Egl.EGL_NO_CONTEXT)
			{
				Egl.eglDestroyContext(eglDisplay, eglContext);
				eglContext = Egl.EGL_NO_CONTEXT;
			}

			//if (eglDisplay != Egl.EGL_NO_DISPLAY)
			//{
			//	Egl.eglTerminate(eglDisplay);
			//	eglDisplay = Egl.EGL_NO_DISPLAY;
			//}
		}

		private void CreateSurface(SwapChainPanel panel, int rbWidth, int rbHeight, GlesBackingOption backingOption, GlesMultisampling multisampling, GlesRenderTarget renderTarget)
		{
			if (panel == null)
			{
				throw new ArgumentNullException(nameof(panel), "SwapChainPanel parameter is invalid");
			}
			
			if (rbWidth == 0 || rbHeight == 0)
			{
				rbWidth = 320;
				rbHeight = 480;
			}

			// Create a temporary surface so that we can make the context current
			int[] surfaceAttributes =
			{
				Egl.EGL_WIDTH, rbWidth,
				Egl.EGL_HEIGHT, rbHeight,
				Egl.EGL_FIXED_SIZE_ANGLE, Egl.EGL_TRUE,

				// Egl.EGL_ANGLE_SURFACE_RENDER_TO_BACK_BUFFER is part of the same optimization as Egl.EGL_ANGLE_DISPLAY_ALLOW_RENDER_TO_BACK_BUFFER (see above).
				// If you have compilation issues with it then please update your Visual Studio templates.
				Egl.EGL_ANGLE_SURFACE_RENDER_TO_BACK_BUFFER, Egl.EGL_TRUE,
				Egl.EGL_NONE
			};

			eglSurface = Egl.eglCreateWindowSurface(eglDisplay, eglConfig, panel, surfaceAttributes);
			if (eglSurface == Egl.EGL_NO_SURFACE)
			{
				throw new Exception("Failed to create EGL surface");
			}

			// Set up swap behavior.
			int swapBehavior = backingOption == GlesBackingOption.Retained ? Egl.EGL_BUFFER_PRESERVED : Egl.EGL_BUFFER_DESTROYED;
			if (Egl.eglSurfaceAttrib(eglDisplay, eglSurface, Egl.EGL_SWAP_BEHAVIOR, swapBehavior) == Egl.EGL_FALSE)
			{
				Debug.WriteLine("Unable to set up backbuffer swap behavior, app may experience graphical glitches!");
			}

			if (multisampling == GlesMultisampling.FourTimes)
			{
				Gles.glRenderbufferStorageMultisampleANGLE((uint)renderTarget, 4, Gles.GL_BGRA8_EXT, rbWidth, rbHeight);
			}
			else
			{
				Gles.glRenderbufferStorage((uint)renderTarget, Gles.GL_BGRA8_EXT, rbWidth, rbHeight);
			}

			if (CurrentContext == this)
			{
				Egl.eglMakeCurrent(eglDisplay, eglSurface, eglSurface, eglContext);
			}
		}

		private void DestroySurface(EGLSurface surface)
		{
			if (eglDisplay != Egl.EGL_NO_DISPLAY && surface != Egl.EGL_NO_SURFACE)
			{
				Egl.eglDestroySurface(eglDisplay, surface);
			}
		}
	}
}
