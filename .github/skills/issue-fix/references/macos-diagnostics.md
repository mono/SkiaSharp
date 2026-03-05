# macOS Diagnostics Reference

macOS-specific investigation techniques for the issue-fix skill.

## GL State Diagnostics

macOS has known GL driver quirks. Always use these diagnostic queries:

```csharp
// Check stencil — CAUTION: returns 0 on macOS default framebuffer even when allocated
GL.glGetIntegerv(GL.GL_STENCIL_BITS, out int stencilBits);

// Check MSAA
GL.glGetIntegerv(GL.GL_SAMPLES, out int samples);

// Check which framebuffer is bound
GL.glGetIntegerv(GL.GL_FRAMEBUFFER_BINDING, out int fb);

// Check GL version and renderer
var version = GL.glGetString(GL.GL_VERSION);
var renderer = GL.glGetString(GL.GL_RENDERER);
```

### The Stencil Trap (Root Cause of #3525)

`glGetIntegerv(GL_STENCIL_BITS)` returns 0 for the default framebuffer (fb=0) on macOS, even when the pixel format allocates 8 stencil bits. This is a macOS GL driver quirk.

**Correct approach** — read from `NSOpenGLPixelFormat`:
```csharp
var stencil = 0;
if (PixelFormat is not null)
    PixelFormat.GetValue(ref stencil, NSOpenGLPixelFormatAttribute.StencilSize, 0);
if (stencil == 0)
    Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, out stencil);
```

**Native Skia does this correctly** — see `extern/skia/tools/window/mac/GLWindowContext_mac.mm:127`:
```objc
[fPixelFormat getValues:&stencilBits forAttribute:NSOpenGLPFAStencilSize forVirtualScreen:0];
```

## VSync Control

VSync masks real performance. Disable it for accurate measurement:

```csharp
const int NSOpenGLCPSwapInterval = 222;

[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
static extern void objc_msgSend_setValues(IntPtr receiver, IntPtr selector, ref int val, int param);

// Disable VSync
var setValuesSelector = ObjCRuntime.Selector.GetHandle("setValues:forParameter:");
var val = 0;
objc_msgSend_setValues(openGLContext.Handle, setValuesSelector, ref val, NSOpenGLCPSwapInterval);
```

## NSOpenGLContext Setup from C#

Working pattern for creating an OpenGL context with correct attributes:

```csharp
var attrs = new NSOpenGLPixelFormatAttribute[] {
    NSOpenGLPixelFormatAttribute.Accelerated,
    NSOpenGLPixelFormatAttribute.ClosestPolicy,
    NSOpenGLPixelFormatAttribute.DoubleBuffer,
    NSOpenGLPixelFormatAttribute.OpenGLProfile,
    NSOpenGLPixelFormatAttribute.Version3_2Core,
    NSOpenGLPixelFormatAttribute.ColorSize, (NSOpenGLPixelFormatAttribute)24,
    NSOpenGLPixelFormatAttribute.AlphaSize, (NSOpenGLPixelFormatAttribute)8,
    NSOpenGLPixelFormatAttribute.DepthSize, (NSOpenGLPixelFormatAttribute)0,
    NSOpenGLPixelFormatAttribute.StencilSize, (NSOpenGLPixelFormatAttribute)8,
    // For MSAA (optional but recommended for perf investigation):
    NSOpenGLPixelFormatAttribute.Multisample,
    NSOpenGLPixelFormatAttribute.SampleBuffers, (NSOpenGLPixelFormatAttribute)1,
    NSOpenGLPixelFormatAttribute.Samples, (NSOpenGLPixelFormatAttribute)4,
    (NSOpenGLPixelFormatAttribute)0
};
var pixelFormat = new NSOpenGLPixelFormat(attrs);
var context = new NSOpenGLContext(pixelFormat, null);
```

## Metal vs GL Backend Switching

To test both backends, create separate views:

```csharp
// Metal
var metalView = new SKMetalView(frame);

// OpenGL
var glView = new SKGLView(frame);
```

Or for bare-metal testing (bypassing SkiaSharp views):
- Metal: Create `MTKView` directly, use `GRContext.CreateMetal()`
- GL: Create `NSOpenGLView` + `NSOpenGLContext`, use `GRContext.CreateGl()`

## Skia Source Reading Locations

Key files for macOS GL investigation:

| File | What It Contains |
|------|-----------------|
| `externals/skia/tools/window/mac/GLWindowContext_mac.mm` | How native Skia sets up GL on macOS (pixel format, stencil reading) |
| `externals/skia/src/gpu/ganesh/ops/TessellationPathRenderer.cpp` | Path renderer selection — requires stencil + drawInstancedSupport |
| `externals/skia/src/gpu/ganesh/GrDrawingManager.cpp` | Path renderer fallback chain |
| `externals/skia/src/gpu/ganesh/gl/GrGLCaps.cpp` | GL capability detection |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs` | C# GL view implementation |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKMetalView.cs` | C# Metal view implementation |

## macOS GUI App Debugging

1. **No stdout?** Run binary directly, not via `open -n`:
   ```bash
   ./bin/Release/net10.0-macos/osx-arm64/App.app/Contents/MacOS/App
   ```

2. **Window not visible?** Add activation:
   ```csharp
   NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Regular;
   NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
   ```

3. **Build fails with signing error?** Add to .csproj:
   ```xml
   <EnableCodeSigning>false</EnableCodeSigning>
   ```

4. **GL renders black?** Check:
   - Is the OpenGL context current? (`context.MakeCurrentContext()`)
   - Is the framebuffer bound? (`glBindFramebuffer(GL_FRAMEBUFFER, 0)`)
   - Is stencil correctly reported? (See stencil trap above)
   - Is the GL profile correct? (Must be 3.2 Core on modern macOS)

## Apple's GL Deprecation

macOS OpenGL is frozen at version 4.1 (deprecated since macOS 10.14). Key implications:
- No new GL features or bug fixes from Apple
- GL driver quirks (like stencil reporting) will never be fixed
- Metal is the supported GPU API going forward
- `NSOpenGLView` is deprecated — `SKGLView` uses it but it still works
- GL performance may be artificially capped compared to Metal
