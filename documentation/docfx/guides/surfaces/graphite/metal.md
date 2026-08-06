---
title: "Graphite with Metal"
description: "Create a Graphite Metal context, wrap Metal textures, and account for iOS and tvOS Simulator pipeline limitations."
---

# Use Graphite with Metal

Graphite Metal is available on macOS, iOS, Mac Catalyst, and tvOS, including Apple Silicon simulators. Before creating a context, confirm the backend is compiled into the current native library:

```csharp
if (!SKGraphiteContext.IsBackendAvailable(SKGraphiteBackend.Metal))
    throw new PlatformNotSupportedException("Graphite Metal is unavailable.");
```

This check only confirms that the Metal factory was compiled into the native library. It does not validate the selected `MTLDevice`.

## Create the Graphite context

> [!WARNING]
> Before creating the command queue or calling `CreateMetal`, use the platform binding for `supportsFamily:` to confirm that the device reports a GPU family accepted by the current Skia build. Current macOS builds check Apple 7 through 9 and Mac 2; iOS-family builds also check Apple 2 through 6. If none of those families is reported, Skia calls `SK_ABORT` instead of returning `null`, which terminates the process.

Supply an `MTLDevice` and `MTLCommandQueue`. On Apple target frameworks you can assign typed `IMTLDevice` and `IMTLCommandQueue` objects; from other targets, assign their native handles:

```csharp
using var backendContext = new SKGraphiteMtlBackendContext
{
    MtlDevice = mtlDeviceHandle,
    MtlQueue = mtlCommandQueueHandle,
};

using var context = SKGraphiteContext.CreateMetal(backendContext)
    ?? throw new InvalidOperationException("Unable to create the Graphite Metal context.");
```

## Wrap Metal textures

Describe an existing `MTLTexture` with `SKGraphiteBackendTexture.CreateMetal`, then use the shared surface-wrapping call:

```csharp
using var backendTexture = SKGraphiteBackendTexture.CreateMetal(
    width, height, mtlTextureHandle)
    ?? throw new InvalidOperationException("Unable to describe the Metal texture.");
using var surface = SKSurface.Create(recorder, backendTexture, SKColorType.Rgba8888)
    ?? throw new InvalidOperationException("Unable to wrap the Metal texture.");
```

The native `MTLTexture` remains owned by the code that created it. If you supply a release callback, release the native allocation only after the callback fires.

## Simulator caveats

Graphite Metal works on the iOS and tvOS Simulator on Apple Silicon because it uses the host GPU. The simulator's `MTLDevice` can under-report its family, so detect the Apple Simulator explicitly and allow it as an exception to the family probe. Do not apply that exception to other devices or virtualized Metal environments.

The simulator's Metal shader compiler cannot build some Graphite pipelines, including some gradient shaders. In that case, `recorder.Snap()` returns `null` for the frame. The same content renders with Graphite Metal on macOS and physical iOS hardware, and with Ganesh Metal on the simulator. See [mono/SkiaSharp#4555](https://github.com/mono/SkiaSharp/issues/4555), and always check the result of `Snap()`.

## Next steps

Continue with the shared [Graphite recording, submission, readback, texture, and resource flow](index.md).

## Related links

- [Graphite GPU surfaces](index.md)
- [Ganesh with Metal](../ganesh/metal.md)
- [Migrate from Ganesh to Graphite](migrate-from-ganesh.md)
