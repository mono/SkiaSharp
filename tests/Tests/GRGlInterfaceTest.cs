using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	[Parallelizable(ParallelScope.None)]
	public class GRGlInterfaceTest : SKTest
	{
		[Test]
		public void CreateDefaultInterfaceIsValid()
		{
			using (var ctx = CreateGlContext()) {
				ctx.MakeCurrent();

				var glInterface = GRGlInterface.CreateNativeGlInterface();

				Assert.NotNull(glInterface);
				Assert.True(glInterface.Validate());
			}
		}

		[Test]
		public void AssembleInterfaceIsValid()
		{
			using (var ctx = CreateGlContext()) {
				ctx.MakeCurrent();

				if (IsMac) {
					var lib = MacDynamicLibraries.dlopen("/System/Library/Frameworks/OpenGL.framework/Versions/A/Libraries/libGL.dylib", 1);

					var glInterface = GRGlInterface.AssembleGlInterface((context, name) => {
						return MacDynamicLibraries.dlsym(lib, name);
					});

					Assert.NotNull(glInterface);
					Assert.True(glInterface.Validate());

					MacDynamicLibraries.dlclose(lib);
				} else if (IsWindows) {
					var lib = WindowsDynamicLibraries.LoadLibrary("opengl32.dll");

					var glInterface = GRGlInterface.AssembleGlInterface((context, name) => {
						var ptr = WindowsDynamicLibraries.GetProcAddress(lib, name);
						if (ptr == IntPtr.Zero) {
							ptr = wglGetProcAddress(name);
						}
						return ptr;
					});

					Assert.NotNull(glInterface);
					Assert.True(glInterface.Validate());

					WindowsDynamicLibraries.FreeLibrary(lib);
				} else if (IsLinux) {
					var lib = LinuxDynamicLibraries.dlopen("libGL.so.1", 1);

					var glInterface = GRGlInterface.AssembleGlInterface((context, name) => {
						return LinuxDynamicLibraries.dlsym(lib, name);
					});

					Assert.NotNull(glInterface);
					Assert.True(glInterface.Validate());

					LinuxDynamicLibraries.dlclose(lib);
				} else {
					// more platforms !!!
				}
			}
		}

		[DllImport("opengl32.dll", CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr wglGetProcAddress([MarshalAs(UnmanagedType.LPStr)] string lpszProc);
	}
}
