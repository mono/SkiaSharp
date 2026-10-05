using System;
using System.Runtime.InteropServices;

namespace SkiaSharp.Views.Gtk;

internal static class GtkGl
{
	// GTK also uses libepoxy, which dispatches to the current GLX, EGL, WGL or CGL context.
	// Keep the library loaded for the lifetime of the GL function pointers held by Skia.
	private static readonly nint library = LoadLibrary();

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	private delegate void GetIntegerDelegate(uint name, out int value);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	private delegate void GetFramebufferAttachmentDelegate(uint target, uint attachment, uint name, out int value);

	public static nint GetProcedureAddress(string name)
	{
		// Epoxy exports pointer variables, not functions.
		return NativeLibrary.TryGetExport(library, "epoxy_" + name, out var symbol)
			? Marshal.ReadIntPtr(symbol)
			: nint.Zero;
	}

	public static int GetInteger(uint name)
	{
		var getInteger = Marshal.GetDelegateForFunctionPointer<GetIntegerDelegate>(GetProcedureAddress("glGetIntegerv"));
		getInteger(name, out var value);
		return value;
	}

	public static int GetStencilBits()
	{
		const uint framebuffer = 0x8D40;
		const uint stencilAttachment = 0x8D20;
		const uint objectType = 0x8CD0;
		const uint stencilSize = 0x8217;
		var getAttachment = Marshal.GetDelegateForFunctionPointer<GetFramebufferAttachmentDelegate>(
			GetProcedureAddress("glGetFramebufferAttachmentParameteriv"));
		getAttachment(framebuffer, stencilAttachment, objectType, out var type);
		if (type == 0)
			return 0;
		getAttachment(framebuffer, stencilAttachment, stencilSize, out var bits);
		return bits;
	}

	private static nint LoadLibrary()
	{
		var names = OperatingSystem.IsWindows()
			? new[] { "libepoxy-0.dll", "epoxy-0.dll" }
			: OperatingSystem.IsMacOS()
				? new[] { "libepoxy.0.dylib" }
				: new[] { "libepoxy.so.0" };

		foreach (var name in names)
		{
			if (NativeLibrary.TryLoad(name, typeof(GtkGl).Assembly, null, out var handle))
				return handle;
		}

		throw new DllNotFoundException($"Unable to load GTK's OpenGL dispatcher ({string.Join(", ", names)}). Install libepoxy with GTK4 and include its directory in the native library search path.");
	}
}
