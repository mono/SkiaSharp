using System;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;

using SkiaSharp.Views.UWP.Interop;

using EGLDisplay = System.IntPtr;
using EGLContext = System.IntPtr;
using EGLConfig = System.IntPtr;
using EGLSurface = System.IntPtr;

namespace SkiaSharp.Views.GlesInterop
{
	internal class GlesContext : IDisposable
	{
		private bool isDisposed = false;

		private EGLDisplay eglDisplay;
		private EGLContext eglContext;
		private EGLSurface eglSurface;
		private EGLConfig eglConfig;

		private static readonly object displayLock = new object();
		private static readonly Dictionary<EGLDisplay, int> displayReferenceCounts = new Dictionary<EGLDisplay, int>();

		public GlesContext()
		{
			eglConfig = Egl.EGL_NO_CONFIG;
			eglDisplay = Egl.EGL_NO_DISPLAY;
			eglContext = Egl.EGL_NO_CONTEXT;
			eglSurface = Egl.EGL_NO_SURFACE;

			Initialize();
		}

		public bool HasSurface => eglSurface != Egl.EGL_NO_SURFACE;

		protected virtual void Dispose(bool disposing)
		{
			if (!isDisposed)
			{
				if (disposing)
				{
					// dispose managed resources
				}

				// free unmanaged resources
				DestroySurface();
				Cleanup();

				isDisposed = true;
			}
		}

		~GlesContext()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void CreateSurface(SwapChainPanel panel, Size? renderSurfaceSize, float? resolutionScale)
		{
			if (panel == null)
			{
				throw new ArgumentNullException("SwapChainPanel parameter is invalid");
			}

			EGLSurface surface = Egl.EGL_NO_SURFACE;

			int[] surfaceAttributes = new[]
			{
				Egl.EGL_NONE
			};

			// Create a PropertySet and initialize with the EGLNativeWindowType.
			PropertySet surfaceCreationProperties = new PropertySet();
			surfaceCreationProperties.Add(Egl.EGLNativeWindowTypeProperty, panel);

			// If a render surface size is specified, add it to the surface creation properties
			if (renderSurfaceSize.HasValue)
			{
				PropertySetExtensions.AddSize(surfaceCreationProperties, Egl.EGLRenderSurfaceSizeProperty, renderSurfaceSize.Value);
			}

			// If a resolution scale is specified, add it to the surface creation properties
			if (resolutionScale.HasValue)
			{
				PropertySetExtensions.AddSingle(surfaceCreationProperties, Egl.EGLRenderResolutionScaleProperty, resolutionScale.Value);
			}

			surface = Egl.eglCreateWindowSurface(eglDisplay, eglConfig, surfaceCreationProperties, surfaceAttributes);
			if (surface == Egl.EGL_NO_SURFACE)
			{
				throw new Exception("Failed to create EGL surface");
			}

			eglSurface = surface;
		}

		public void GetSurfaceDimensions(out int width, out int height)
		{
			Egl.eglQuerySurface(eglDisplay, eglSurface, Egl.EGL_WIDTH, out width);
			Egl.eglQuerySurface(eglDisplay, eglSurface, Egl.EGL_HEIGHT, out height);
		}

		public void SetViewportSize(int width, int height)
		{
			Gles.glViewport(0, 0, width, height);
		}

		public void DestroySurface()
		{
			if (eglDisplay != Egl.EGL_NO_DISPLAY && eglSurface != Egl.EGL_NO_SURFACE)
			{
				Egl.eglDestroySurface(eglDisplay, eglSurface);
				eglSurface = Egl.EGL_NO_SURFACE;
			}
		}

		public void MakeCurrent()
		{
			if (Egl.eglMakeCurrent(eglDisplay, eglSurface, eglSurface, eglContext) == Egl.EGL_FALSE)
			{
				throw new Exception("Failed to make EGLSurface current");
			}
		}

		public bool SwapBuffers()
		{
			return (Egl.eglSwapBuffers(eglDisplay, eglSurface) == Egl.EGL_TRUE);
		}

		public void Reset()
		{
			Cleanup();
			Initialize();
		}

		private void Initialize()
		{
			int[] configAttributes = new[]
			{
				Egl.EGL_RED_SIZE, 8,
				Egl.EGL_GREEN_SIZE, 8,
				Egl.EGL_BLUE_SIZE, 8,
				Egl.EGL_ALPHA_SIZE, 8,
				Egl.EGL_DEPTH_SIZE, 8,
				Egl.EGL_STENCIL_SIZE, 8,
				Egl.EGL_NONE
			};

			int[] contextAttributes = new[]
			{
				Egl.EGL_CONTEXT_CLIENT_VERSION, 2,
				Egl.EGL_NONE
			};

			int[] defaultDisplayAttributes = new[]
			{
				// These are the default display attributes, used to request ANGLE's D3D11 renderer.
				// eglInitialize will only succeed with these attributes if the hardware supports D3D11 Feature Level 10_0+.
				Egl.EGL_PLATFORM_ANGLE_TYPE_ANGLE, Egl.EGL_PLATFORM_ANGLE_TYPE_D3D11_ANGLE,

				// EGL_ANGLE_DISPLAY_ALLOW_RENDER_TO_BACK_BUFFER is an optimization that can have large performance benefits on mobile devices.
				// Its syntax is subject to change, though. Please update your Visual Studio templates if you experience compilation issues with it.
				Egl.EGL_EXPERIMENTAL_PRESENT_PATH_ANGLE, Egl.EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE, 

				// EGL_PLATFORM_ANGLE_ENABLE_AUTOMATIC_TRIM_ANGLE is an option that enables ANGLE to automatically call 
				// the IDXGIDevice3::Trim method on behalf of the application when it gets suspended. 
				// Calling IDXGIDevice3::Trim when an application is suspended is a Windows Store application certification requirement.
				Egl.EGL_PLATFORM_ANGLE_ENABLE_AUTOMATIC_TRIM_ANGLE, Egl.EGL_TRUE,
				Egl.EGL_NONE,
			};

			int[] fl9_3DisplayAttributes = new[]
			{
				// These can be used to request ANGLE's D3D11 renderer, with D3D11 Feature Level 9_3.
				// These attributes are used if the call to eglInitialize fails with the default display attributes.
				Egl.EGL_PLATFORM_ANGLE_TYPE_ANGLE, Egl.EGL_PLATFORM_ANGLE_TYPE_D3D11_ANGLE,
				Egl.EGL_PLATFORM_ANGLE_MAX_VERSION_MAJOR_ANGLE, 9,
				Egl.EGL_PLATFORM_ANGLE_MAX_VERSION_MINOR_ANGLE, 3,
				Egl.EGL_EXPERIMENTAL_PRESENT_PATH_ANGLE, Egl.EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE,
				Egl.EGL_PLATFORM_ANGLE_ENABLE_AUTOMATIC_TRIM_ANGLE, Egl.EGL_TRUE,
				Egl.EGL_NONE,
			};

			int[] warpDisplayAttributes = new[]
			{
				// These attributes can be used to request D3D11 WARP.
				// They are used if eglInitialize fails with both the default display attributes and the 9_3 display attributes.
				Egl.EGL_PLATFORM_ANGLE_TYPE_ANGLE, Egl.EGL_PLATFORM_ANGLE_TYPE_D3D11_ANGLE,
				Egl.EGL_PLATFORM_ANGLE_DEVICE_TYPE_ANGLE, Egl.EGL_PLATFORM_ANGLE_DEVICE_TYPE_D3D_WARP_ANGLE,
				Egl.EGL_EXPERIMENTAL_PRESENT_PATH_ANGLE, Egl.EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE,
				Egl.EGL_PLATFORM_ANGLE_ENABLE_AUTOMATIC_TRIM_ANGLE, Egl.EGL_TRUE,
				Egl.EGL_NONE,
			};

			EGLConfig config = IntPtr.Zero;

			//
			// To initialize the display, we make three sets of calls to eglGetPlatformDisplayEXT and eglInitialize, with varying 
			// parameters passed to eglGetPlatformDisplayEXT:
			// 1) The first calls uses "defaultDisplayAttributes" as a parameter. This corresponds to D3D11 Feature Level 10_0+.
			// 2) If eglInitialize fails for step 1 (e.g. because 10_0+ isn't supported by the default GPU), then we try again 
			//    using "fl9_3DisplayAttributes". This corresponds to D3D11 Feature Level 9_3.
			// 3) If eglInitialize fails for step 2 (e.g. because 9_3+ isn't supported by the default GPU), then we try again 
			//    using "warpDisplayAttributes".  This corresponds to D3D11 Feature Level 11_0 on WARP, a D3D11 software rasterizer.
			//

			lock (displayLock)
			{
				// This tries to initialize EGL to D3D11 Feature Level 10_0+. See above comment for details.
				eglDisplay = Egl.eglGetPlatformDisplayEXT(Egl.EGL_PLATFORM_ANGLE_ANGLE, Egl.EGL_DEFAULT_DISPLAY, defaultDisplayAttributes);
				if (eglDisplay == Egl.EGL_NO_DISPLAY)
				{
					throw new Exception("Failed to get EGL display");
				}

				if (Egl.eglInitialize(eglDisplay, out int major, out int minor) == Egl.EGL_FALSE)
				{
					// This tries to initialize EGL to D3D11 Feature Level 9_3, if 10_0+ is unavailable (e.g. on some mobile devices).
					eglDisplay = Egl.eglGetPlatformDisplayEXT(Egl.EGL_PLATFORM_ANGLE_ANGLE, Egl.EGL_DEFAULT_DISPLAY, fl9_3DisplayAttributes);
					if (eglDisplay == Egl.EGL_NO_DISPLAY)
					{
						throw new Exception("Failed to get EGL display");
					}

					if (Egl.eglInitialize(eglDisplay, out major, out minor) == Egl.EGL_FALSE)
					{
						// This initializes EGL to D3D11 Feature Level 11_0 on WARP, if 9_3+ is unavailable on the default GPU.
						eglDisplay = Egl.eglGetPlatformDisplayEXT(Egl.EGL_PLATFORM_ANGLE_ANGLE, Egl.EGL_DEFAULT_DISPLAY, warpDisplayAttributes);
						if (eglDisplay == Egl.EGL_NO_DISPLAY)
						{
							throw new Exception("Failed to get EGL display");
						}

						if (Egl.eglInitialize(eglDisplay, out major, out minor) == Egl.EGL_FALSE)
						{
							
							// If all of the calls to eglInitialize returned EGL_FALSE then an error has occurred.
							throw new Exception("Failed to initialize EGL");
						}
					}
				}

				if (eglDisplay != Egl.EGL_NO_DISPLAY)
                {
					int refCount = 0;
					displayReferenceCounts.TryGetValue(eglDisplay, out refCount);
					refCount += 1;
					displayReferenceCounts[eglDisplay] = refCount;
                }
			}

			EGLDisplay[] configs = new EGLDisplay[1];
			if ((Egl.eglChooseConfig(eglDisplay, configAttributes, configs, configs.Length, out int numConfigs) == Egl.EGL_FALSE) || (numConfigs == 0))
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
				lock (displayReferenceCounts)
				{
					int refCount = 0;
					displayReferenceCounts.TryGetValue(eglDisplay, out refCount);
					refCount -= 1;
					displayReferenceCounts[eglDisplay] = refCount;

					if (refCount == 0)
					{
						Egl.eglTerminate(eglDisplay);
						eglDisplay = Egl.EGL_NO_DISPLAY;
					}
				}
			}
		}
	}
}
