using System;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Controls;
using SkiaSharp.Views.GlesInterop;

using EGLDisplay = System.IntPtr;
using EGLContext = System.IntPtr;
using EGLConfig = System.IntPtr;
using EGLSurface = System.IntPtr;

namespace SkiaSharp.Views
{
	internal class GlesContext : IDisposable
	{
		private static GlesContext currentContext;

		private EGLDisplay eglDisplay;
		private EGLContext eglContext;
		private EGLConfig eglConfig;

		private EGLSurface eglSurface;

		public GlesContext()
		{
			eglDisplay = Egl.EGL_NO_DISPLAY;
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
				if (value == null)
				{
					// need to nuke surface and context
					// iOS has a static egl display for this:
					// eglMakeCurrent(<display>, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT)
				}
				else
				{
					if (Egl.eglMakeCurrent(value.eglDisplay, value.eglSurface, value.eglSurface, value.eglContext) == Egl.EGL_FALSE)
					{
						throw new Exception("Failed to make EGLSurface current");
					}
					currentContext = value;
				}
			}
		}

		public void SetSurface(SwapChainPanel swapChainPanel)
		{
			if (swapChainPanel == null)
			{
				DestroySurface(eglSurface);
				eglSurface = Egl.EGL_NO_SURFACE;
			}
			else
			{
				eglSurface = CreateSurface(swapChainPanel);
			}
		}

		public bool HasValidSurface
		{
			get { return eglSurface != Egl.EGL_NO_SURFACE; }
		}

		public void GetSurfaceDimensions(out int width, out int height)
		{
			Egl.eglQuerySurface(eglDisplay, eglSurface, Egl.EGL_WIDTH, out width);
			Egl.eglQuerySurface(eglDisplay, eglSurface, Egl.EGL_HEIGHT, out height);
		}

		public bool SwapBuffers()
		{
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

		private void Initialize()
		{
			int[] configAttributes =
			{
				Egl.EGL_RED_SIZE, 8,
				Egl.EGL_GREEN_SIZE, 8,
				Egl.EGL_BLUE_SIZE, 8,
				Egl.EGL_ALPHA_SIZE, 8,
				Egl.EGL_DEPTH_SIZE, 8,
				Egl.EGL_STENCIL_SIZE, 8,
				Egl.EGL_NONE
			};

			int[] contextAttributes =
			{
				Egl.EGL_CONTEXT_CLIENT_VERSION, 2,
				Egl.EGL_NONE
			};

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
			eglDisplay = Egl.eglGetPlatformDisplayEXT(Egl.EGL_PLATFORM_ANGLE_ANGLE, Egl.EGL_DEFAULT_DISPLAY, defaultDisplayAttributes);
			if (eglDisplay == Egl.EGL_NO_DISPLAY)
			{
				throw new Exception("Failed to get EGL display (D3D11 10.0+).");
			}

			int major, minor;
			if (Egl.eglInitialize(eglDisplay, out major, out minor) == Egl.EGL_FALSE)
			{
				// This tries to initialize EGL to D3D11 Feature Level 9_3, if 10_0+ is unavailable (e.g. on some mobile devices).
				eglDisplay = Egl.eglGetPlatformDisplayEXT(Egl.EGL_PLATFORM_ANGLE_ANGLE, Egl.EGL_DEFAULT_DISPLAY, fl9_3DisplayAttributes);
				if (eglDisplay == Egl.EGL_NO_DISPLAY)
				{
					throw new Exception("Failed to get EGL display (D3D11 9.3).");
				}

				if (Egl.eglInitialize(eglDisplay, out major, out minor) == Egl.EGL_FALSE)
				{
					// This initializes EGL to D3D11 Feature Level 11_0 on WARP, if 9_3+ is unavailable on the default GPU.
					eglDisplay = Egl.eglGetPlatformDisplayEXT(Egl.EGL_PLATFORM_ANGLE_ANGLE, Egl.EGL_DEFAULT_DISPLAY, warpDisplayAttributes);
					if (eglDisplay == Egl.EGL_NO_DISPLAY)
					{
						throw new Exception("Failed to get EGL display (D3D11 11.0 WARP)");
					}

					if (Egl.eglInitialize(eglDisplay, out major, out minor) == Egl.EGL_FALSE)
					{
						// If all of the calls to eglInitialize returned Egl.EGL_FALSE then an error has occurred.
						throw new Exception("Failed to initialize EGL");
					}
				}
			}

			int numConfigs = 0;
			EGLDisplay[] configs = new EGLDisplay[1];
			if (Egl.eglChooseConfig(eglDisplay, configAttributes, configs, configs.Length, out numConfigs) == Egl.EGL_FALSE || numConfigs == 0)
			{
				throw new Exception("Failed to choose first EGLConfig");
			}
			eglConfig = configs[0];

			eglContext = Egl.eglCreateContext(eglDisplay, eglConfig, Egl.EGL_NO_CONTEXT, contextAttributes);
			if (eglContext == Egl.EGL_NO_CONTEXT)
			{
				throw new Exception("Failed to create EGL context");
			}
		}

		private void Cleanup()
		{
			if (eglDisplay != Egl.EGL_NO_DISPLAY && eglContext != Egl.EGL_NO_CONTEXT)
			{
				Egl.eglDestroyContext(eglDisplay, eglContext);
				eglContext = Egl.EGL_NO_CONTEXT;
			}

			if (eglDisplay != Egl.EGL_NO_DISPLAY)
			{
				Egl.eglTerminate(eglDisplay);
				eglDisplay = Egl.EGL_NO_DISPLAY;
			}
		}

		private EGLSurface CreateSurface(SwapChainPanel panel)
		{
			if (panel == null)
			{
				throw new ArgumentNullException("SwapChainPanel parameter is invalid");
			}

			EGLSurface surface = Egl.EGL_NO_SURFACE;

			int[] surfaceAttributes =
			{
				// Egl.EGL_ANGLE_SURFACE_RENDER_TO_BACK_BUFFER is part of the same optimization as Egl.EGL_ANGLE_DISPLAY_ALLOW_RENDER_TO_BACK_BUFFER (see above).
				// If you have compilation issues with it then please update your Visual Studio templates.
				Egl.EGL_ANGLE_SURFACE_RENDER_TO_BACK_BUFFER, Egl.EGL_TRUE,
				Egl.EGL_NONE
			};

			// Create a PropertySet and initialize with the EGLNativeWindowType.
			PropertySet surfaceCreationProperties = new PropertySet();
			surfaceCreationProperties.Add("EGLNativeWindowTypeProperty", panel);

			//// add the scale from the current display
			//var info = Windows.Graphics.Display.DisplayInformation.GetForCurrentView();
			//var dpi = info.LogicalDpi / 96.0f;
			//surfaceCreationProperties.Add("EGLRenderResolutionScaleProperty", PropertyValue.CreateSingle(dpi));

			// You can configure the surface to render at a lower resolution and be scaled up to 
			// the full window size. The scaling is often free on mobile hardware.
			//
			// One way to configure the SwapChainPanel is to specify precisely which resolution it should render at.
			// Size customRenderSurfaceSize = Size(800, 600);
			// surfaceCreationProperties->Insert(ref new String(EGLRenderSurfaceSizeProperty), PropertyValue::CreateSize(customRenderSurfaceSize));
			//
			// Another way is to tell the SwapChainPanel to render at a certain scale factor compared to its size.
			// e.g. if the SwapChainPanel is 1920x1280 then setting a factor of 0.5f will make the app render at 960x640
			// float customResolutionScale = 0.5f;
			// surfaceCreationProperties->Insert(ref new String(EGLRenderResolutionScaleProperty), PropertyValue::CreateSingle(customResolutionScale));

			surface = Egl.eglCreateWindowSurface(eglDisplay, eglConfig, surfaceCreationProperties, surfaceAttributes);
			if (surface == Egl.EGL_NO_SURFACE)
			{
				throw new Exception("Failed to create EGL surface");
			}

			return surface;
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
