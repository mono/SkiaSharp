# Hardware-Accelerated / GPU Surface Changes

As of SkiaSharp v1.68.0, there has been a fairly major change to the way
hardware-accelerated (GPU) surfaces are created. 

These changes were made to better support the new GPU backends. Right now, we
just support OpenGL, but the changes open the door to Vulkan and Metal
support.

The good news is that even though there were many changes, we have been able
to preserve the existing functionality so that you aren't forced to update
 your code immediately after upgrading. However, you should migrate your code
as soon as possible as there is no guarantee that the next update will still
support all the existing functionality.

## Migration

The most visible change is that we are no longer using the
`GRBackendRenderTargetDesc` and `GRBackendTextureDesc` types, but rather we
use the similarly-named `GRBackendRenderTarget` and `GRBackendTexture`. The
properties are mostly the same, except that `PixelConfig` and `Origin` have
been moved to the surface construction - and should not affect the manner in
which you draw to that surface.

Another change is that the old structures have been split into two parts, one
to describe the surface we are creating, and the other to explicitly describe
the backend-specific configuration needed to write to that surface.

For example, `GRBackendRenderTargetDesc` has been split into
`GRBackendRenderTarget` which describes the general surface (such as width,
height, and samples) and `GRGlFramebufferInfo` which describes OpenGL-specific
surface (with the FBO ID and color format). If we were to support Vulkan, this
may be swapped out with `GRVkImageInfo`, which will describe the Vulkan-
specific surface (with the image handle and format).

#### Existing Code

```csharp
// create the GPU context (UNCHANGED)
var glInterface = GRGlInterface.CreateNativeGlInterface();
var context = GRContext.Create(GRBackend.OpenGL, glInterface);

// create the surface (OLD)
var renderTarget = new GRBackendRenderTargetDesc {
    Width = 1024,
    Height = 1024,
    Config = GRPixelConfig.Rgba8888,
    Origin = GRSurfaceOrigin.BottomLeft,
    SampleCount = 0,
    StencilBits = 0,
    RenderTargetHandle = IntPtr.Zero,
};
var surface = SKSurface.Create(context, renderTarget);

// draw (UNCHANGED)
var canvas = surface.Canvas;
// ...
```

#### New Code

```csharp
// create the GPU context (UNCHANGED)
var glInterface = GRGlInterface.CreateNativeGlInterface();
var context = GRContext.Create(GRBackend.OpenGL, glInterface);

// create the surface (UPDATED)
var glInfo = new GRGlFramebufferInfo(
    fboId: 0,
    format: SKColorType.Rgba8888.ToGlSizedFormat());
var renderTarget = new GRBackendRenderTarget(
    width: 1024,
    height: 1024,
    sampleCount: 0,
    stencilBits: 0,
    glInfo: glInfo);
var surface = SKSurface.Create(
    context, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);

// draw (UNCHANGED)
var canvas = surface.Canvas;
// ...
```
