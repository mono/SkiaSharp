"""Render and read back a pixel with the built ANGLE libraries using D3D11 WARP."""

import argparse
import ctypes as ct
import os
from pathlib import Path


def render(directory):
    with os.add_dll_directory(str(directory)):
        egl = ct.WinDLL(str(directory / "libEGL.dll"))
        gles = ct.WinDLL(str(directory / "libGLESv2.dll"))

        def function(library, name, result, *arguments):
            entry = getattr(library, name)
            entry.restype = result
            entry.argtypes = arguments
            return entry

        integer = ct.c_int
        pointer = ct.c_void_p
        integers = ct.POINTER(integer)
        get_error = function(egl, "eglGetError", integer)

        def require(value, operation):
            if not value:
                raise RuntimeError(f"{operation} failed: EGL error 0x{get_error():04x}")
            return value

        get_proc = function(egl, "eglGetProcAddress", pointer, ct.c_char_p)
        address = require(get_proc(b"eglGetPlatformDisplayEXT"), "eglGetPlatformDisplayEXT lookup")
        get_display = ct.WINFUNCTYPE(pointer, ct.c_uint, pointer, integers)(address)
        # EGL_PLATFORM_ANGLE, D3D11 renderer, WARP device, EGL_NONE.
        attributes = (integer * 5)(0x3203, 0x3208, 0x3209, 0x320B, 0x3038)
        display = require(get_display(0x3202, None, attributes), "get WARP display")
        initialize = function(egl, "eglInitialize", integer, pointer, integers, integers)
        major, minor = integer(), integer()
        require(initialize(display, ct.byref(major), ct.byref(minor)), "eglInitialize")
        terminate = function(egl, "eglTerminate", integer, pointer)
        context = surface = None
        try:
            bind_api = function(egl, "eglBindAPI", integer, ct.c_uint)
            require(bind_api(0x30A0), "eglBindAPI GLES")
            choose = function(egl, "eglChooseConfig", integer, pointer, integers,
                              ct.POINTER(pointer), integer, integers)
            # Pbuffer, GLES2, RGBA8.
            config_attributes = (integer * 13)(
                0x3033, 1, 0x3040, 4, 0x3024, 8, 0x3023, 8, 0x3022, 8, 0x3021, 8, 0x3038)
            config, count = pointer(), integer()
            require(choose(display, config_attributes, ct.byref(config), 1, ct.byref(count)),
                    "eglChooseConfig")
            require(count.value, "find RGBA8 config")
            create_surface = function(egl, "eglCreatePbufferSurface", pointer, pointer, pointer, integers)
            surface = require(create_surface(display, config, (integer * 5)(0x3057, 4, 0x3056, 4, 0x3038)),
                              "eglCreatePbufferSurface")
            create_context = function(egl, "eglCreateContext", pointer, pointer, pointer, pointer, integers)
            context = require(create_context(display, config, None, (integer * 3)(0x3098, 2, 0x3038)),
                              "eglCreateContext")
            make_current = function(egl, "eglMakeCurrent", integer, pointer, pointer, pointer, pointer)
            require(make_current(display, surface, surface, context), "eglMakeCurrent")
            renderer = function(gles, "glGetString", ct.c_char_p, ct.c_uint)(0x1F01)
            if not renderer or b"Direct3D11" not in renderer:
                raise RuntimeError(f"Expected D3D11 renderer, got {renderer!r}")
            function(gles, "glClearColor", None, ct.c_float, ct.c_float, ct.c_float, ct.c_float)(1, 0, 0, 1)
            function(gles, "glClear", None, ct.c_uint)(0x4000)
            pixel = (ct.c_ubyte * 4)()
            function(gles, "glReadPixels", None, integer, integer, integer, integer,
                     ct.c_uint, ct.c_uint, pointer)(0, 0, 1, 1, 0x1908, 0x1401, pixel)
            error = function(gles, "glGetError", ct.c_uint)()
            if error or list(pixel) != [255, 0, 0, 255]:
                raise RuntimeError(f"Rendering failed: GL error 0x{error:04x}, pixel {list(pixel)}")
            print(f"ANGLE EGL {major.value}.{minor.value}: {renderer.decode()}, RGBA {list(pixel)}")
        finally:
            if context or surface:
                require(function(egl, "eglMakeCurrent", integer, pointer, pointer, pointer, pointer)(
                    display, None, None, None), "release current context")
            if context:
                require(function(egl, "eglDestroyContext", integer, pointer, pointer)(display, context),
                        "eglDestroyContext")
            if surface:
                require(function(egl, "eglDestroySurface", integer, pointer, pointer)(display, surface),
                        "eglDestroySurface")
            require(terminate(display), "eglTerminate")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("directory", type=Path)
    render(parser.parse_args().directory.resolve())
