# Using SkiaSharp with Python

One popular request is a Python API for SkiaSharp. While we don't provide official Python bindings, you can use the native C libraries that SkiaSharp wraps to create Python scripts.

This guide shows one way to make use of the native Skia bits in your Python applications using `ctypes`.

## Getting the Native Libraries

You can obtain the native libraries in two ways:

### Option 1: Build from Source

Build the native bits you need:

```bash
# Windows
.\bootstrapper.ps1 -Target externals-windows

# macOS
./bootstrapper.sh -t externals-mac

# Linux
./bootstrapper.sh -t externals-linux
```

This will output the native bits to `<skiasharp-root>/output/<platform>/libSkiaSharp.xxx`.

### Option 2: Extract from NuGet Package

1. Download the [SkiaSharp NuGet package](https://www.nuget.org/packages/SkiaSharp)
2. Rename the `.nupkg` file to `.zip`
3. Extract the archive
4. Find the native binaries in the `runtimes` folder

## Example: Drawing an Image

Here's a complete example that draws a simple image:

```python
# create the surface
info = SKImageInfo(512, 512, SKPlatformColorType, SKAlphaType.Premul)
surface = SKSurface(info)

# get and clear the canvas for drawing
canvas = surface.canvas()
canvas.clear(0xFF3498DB)  # "Xamarin Light Blue"

# create the paint fill
paint = SKPaint()
paint.setAntiAlias(True)
paint.setStyle(SKPaintStyle.Fill)

# draw a circle
paint.setColor(0xFF2C3E50)  # "Xamarin Dark Blue"
canvas.drawCircle(256, 256, 200, paint)

# draw text
center_text = 256 + 16
paint.setTextSize(64)
paint.setTextAlign(SKTextAlign.Center)
paint.setColor(0xFFFFFFFF)  # "White"
canvas.drawText("SkiaSharp", 256, center_text - 64, paint)
canvas.drawText("+", 256, center_text, paint)
canvas.drawText("Python", 256, center_text + 64, paint)

# create a PNG from the surface
image = surface.snapshot()
data = image.encode()

# save the PNG to disk
stream = SKFileWStream("image.png")
stream.writeData(data)
stream.flush()

# clean up
del paint
del stream
del data
del image
del canvas
del surface
```

## Python Bindings Definition

The definitions part of the script wraps the C API using `ctypes`:

```python
from ctypes import *

def enum(**enums):
    return type('Enum', (), enums)

# Load the native library
# Windows: "libSkiaSharp.dll"
# macOS: "libSkiaSharp.dylib"  
# Linux: "libSkiaSharp.so"
libSkiaSharp = cdll.LoadLibrary("libSkiaSharp.dll")

# Enumerations
SKAlphaType = enum(Unknown=0, Opaque=1, Premul=2, Unpremul=3)
SKTextAlign = enum(Left=0, Center=1, Right=2)
SKPaintStyle = enum(Fill=0, Stroke=1, StrokeAndFill=2)

# Get the default color type for the platform
SKPlatformColorType = libSkiaSharp.sk_colortype_get_default_8888()

# Structure definitions
class SKImageInfo(Structure):
    _fields_ = [("width", c_int),
                ("height", c_int),
                ("colorType", c_int),
                ("alphaType", c_int)]

# Wrapper classes
class SKSurface(object):
    def __init__(self, info):
        self.__HANDLE = libSkiaSharp.sk_surface_new_raster(byref(info), 0)

    def handle(self):
        return self.__HANDLE

    def canvas(self):
        cnv = libSkiaSharp.sk_surface_get_canvas(self.__HANDLE)
        return SKCanvas(cnv)

    def snapshot(self):
        img = libSkiaSharp.sk_surface_new_image_snapshot(self.__HANDLE)
        return SKImage(img)

    def __del__(self):
        libSkiaSharp.sk_surface_unref(self.__HANDLE)

class SKCanvas(object):
    def __init__(self, handle):
        self.__HANDLE = handle

    def handle(self):
        return self.__HANDLE

    def clear(self, color):
        libSkiaSharp.sk_canvas_draw_color(self.__HANDLE, color)

    def drawCircle(self, cx, cy, radius, paint):
        libSkiaSharp.sk_canvas_draw_circle(
            self.__HANDLE, 
            c_float(cx), 
            c_float(cy), 
            c_float(radius), 
            paint.handle())

    def drawText(self, text, x, y, paint):
        libSkiaSharp.sk_canvas_draw_text(
            self.__HANDLE, 
            text, 
            len(text), 
            c_float(x), 
            c_float(y), 
            paint.handle())

    def __del__(self):
        # Canvas is owned by surface, don't delete
        pass

class SKPaint(object):
    def __init__(self):
        self.__HANDLE = libSkiaSharp.sk_paint_new()

    def handle(self):
        return self.__HANDLE

    def setAntiAlias(self, value):
        libSkiaSharp.sk_paint_set_antialias(self.__HANDLE, c_bool(value))

    def setStyle(self, style):
        libSkiaSharp.sk_paint_set_style(self.__HANDLE, style)

    def setColor(self, color):
        libSkiaSharp.sk_paint_set_color(self.__HANDLE, color)

    def setTextSize(self, size):
        libSkiaSharp.sk_paint_set_textsize(self.__HANDLE, c_float(size))

    def setTextAlign(self, alignment):
        libSkiaSharp.sk_paint_set_text_align(self.__HANDLE, alignment)

    def __del__(self):
        libSkiaSharp.sk_paint_delete(self.__HANDLE)

class SKImage(object):
    def __init__(self, handle):
        self.__HANDLE = handle

    def handle(self):
        return self.__HANDLE

    def encode(self):
        data = libSkiaSharp.sk_image_encode(self.__HANDLE)
        return SKData(data)

    def __del__(self):
        libSkiaSharp.sk_image_unref(self.__HANDLE)

class SKData(object):
    def __init__(self, handle):
        self.__HANDLE = handle

    def handle(self):
        return self.__HANDLE

    def data(self):
        return libSkiaSharp.sk_data_get_data(self.__HANDLE)

    def size(self):
        return libSkiaSharp.sk_data_get_size(self.__HANDLE)

    def __del__(self):
        libSkiaSharp.sk_data_unref(self.__HANDLE)

class SKFileWStream(object):
    def __init__(self, filename):
        self.__HANDLE = libSkiaSharp.sk_filewstream_new(filename)

    def handle(self):
        return self.__HANDLE

    def writeData(self, data):
        return self.write(data.data(), data.size())

    def write(self, data, size):
        return libSkiaSharp.sk_wstream_write(self.__HANDLE, data, size)

    def flush(self):
        libSkiaSharp.sk_wstream_flush(self.__HANDLE)

    def __del__(self):
        libSkiaSharp.sk_filewstream_destroy(self.__HANDLE)
```

## Complete Script

The full working script is available as a [GitHub Gist](https://gist.github.com/mattleibow/d4fa01931b2f8e283b61f5fa294dd894).

## Important Notes

- This is just one approach to using SkiaSharp with Python
- The native libraries are plain C/C++ binaries that can be used from any language
- You'll need to manage memory carefully (note the `__del__` methods)
- Refer to the [C API headers](../../externals/skia/include/c/) for available functions
- The C API may change between versions, so pin to a specific SkiaSharp version

## Related Documentation

- [SkiaSharp Architecture](../../design/architecture-overview.md) - How the layers work together
- [Building SkiaSharp](../building/building-skiasharp.md) - Building from source
