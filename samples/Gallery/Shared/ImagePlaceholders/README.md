# Image placeholders

Copy **all `.cs` files in this directory** into a project referencing SkiaSharp; no Gallery assembly or third-party codec is needed. Keep the three `BlurHashCodec` partial files and `PixelBuffers.cs` together. The code targets modern .NET (uses `Span<T>` and SkiaSharp). The codec files carry the Wolt attribution and MIT notice; keep those headers when copying or redistributing.

```csharp
using SkiaSharpSample.ImagePlaceholders;

using var bitmap = SKBitmap.Decode("picture.png");
string blurHash = BlurHashCodec.Encode(bitmap, componentsX: 4, componentsY: 3);
using var blurPreview = BlurHashCodec.DecodeBitmap(blurHash, width: 64, height: 64);
using var vividPreview = BlurHashCodec.DecodeBitmap(blurHash, width: 64, height: 64, punch: 1.5f);
canvas.DrawBitmap(blurPreview, rectangle);

// For repeated previews, write directly into a caller-owned RGBA8888 pixmap:
using var pixels = new SKBitmap(new SKImageInfo(64, 64, SKColorType.Rgba8888, SKAlphaType.Unpremul));
using var destination = pixels.PeekPixels();
BlurHashCodec.DecodeInto(blurHash, destination);
pixels.NotifyPixelsChanged();
```

`BlurHashCodec` produces standard base83 RGB strings, with explicit 1–9 X/Y components (the Gallery initially uses 4×3). Decode-time `punch` defaults to 1 and scales its color-variation terms: 1 is unchanged, 0 is a flat average, and higher values increase contrast without changing the hash. The Gallery offers a 0–2 slider. **Every bitmap/image/array decode requires explicit exact width and height**; there is no implicit 64×64 preview size. Transparent Skia source pixels are **composited over white** because BlurHash has no alpha channel. A hash is only a placeholder: it does not contain the original image or a URL.

The codec can encode from a caller-owned `SKBitmap`, `SKImage`, or `SKPixmap`, and decode to a caller-owned bitmap or image, a provided RGBA8 span, or a provided pixmap. The Skia adapters normalize supported color types, premultiplication, row stride, and color space to unpremultiplied sRGB RGBA8, resampling to at most 100×100 without stretching aspect ratio. Small raster `SKImage`s in a compatible opaque sRGB RGBA8 layout can be read directly via `PeekPixels()`; incompatible, larger, or non-raster images use the normalization path. `SKPixmap` **does not own its pixels**: keep its backing image or bitmap alive for the entire call, and keep that backing storage alive while using any pixmap returned by `PeekPixels()`. Decode into a caller-provided pixmap rather than returning a pixmap with ambiguous lifetime. The byte-span encoder requires positive dimensions, RGBA8 row-major unpremultiplied pixels, and an explicit stride; it reads RGB and ignores alpha rather than compositing it. The input span must end exactly at the last pixel (row padding on earlier rows is allowed). Arrays implicitly convert to spans; there is no separate byte-array input overload. A null array converts to an empty span and fails input validation. Decode-into spans may be larger and preserve existing padding. Source Skia images and low-level byte inputs are limited to 16 MP; bitmap and pixmap decoded previews are limited to 1 MP. Invalid dimensions and payloads throw instead of allocating unbounded memory. The codec retains no input or output image or buffer; caller-owned arrays must not be returned to a pool while in use.

This folder contains **the codec only**. The Gallery-specific `Samples/PlaceholderImageView.cs` owns rendering and its 6-second two-way transition; it is deliberately not part of the copy-out source. The caller owns and disposes input bitmaps and decoded preview bitmaps. The codec does not retain an image, start a timer, or perform network I/O.
