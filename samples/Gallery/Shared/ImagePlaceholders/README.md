# Image placeholders

Copy **all `.cs` files in this directory** into a project referencing SkiaSharp; no Gallery assembly or third-party codec is needed. The code targets modern .NET (uses `Span<T>` and SkiaSharp). Author credits and the applicable MIT notices are at the top of each codec file, so keep those headers when copying or redistributing. The BlurHash wire format and color conversion are informed by the Wolt reference; ThumbHash's LPQA transform and binary layout are adapted from Evan Wallace's reference.

```csharp
using SkiaSharpSample.ImagePlaceholders;

using var bitmap = SKBitmap.Decode("picture.png");
string blurHash = BlurHashCodec.Encode(bitmap, componentsX: 4, componentsY: 3);
using var blurPreview = BlurHashCodec.DecodeBitmap(blurHash, width: 64, height: 64);
canvas.DrawBitmap(blurPreview, rectangle);

byte[] thumbHash = ThumbHashCodec.Encode(bitmap);
string storedHash = Convert.ToBase64String(thumbHash);
using var thumbPreview = ThumbHashCodec.DecodeBitmap(Convert.FromBase64String(storedHash));
canvas.DrawBitmap(thumbPreview, rectangle);
```

`BlurHashCodec` produces standard base83 RGB BlurHash, with 1–9 X/Y components (default 4×3). Decode-time `punch` scales its color-variation terms: 1 is unchanged, 0 is a flat average, and higher values increase contrast without changing the hash. The Gallery offers a 0–2 slider. Transparent source pixels are **composited over white** because BlurHash has no alpha channel. `ThumbHashCodec` produces standard **binary bytes**; the Gallery uses Base64 only for text display/storage. ThumbHash retains approximate transparency and aspect ratio; its decode width/height are *maximum bounds*, not exact dimensions. A hash is only a placeholder: it does not contain the original image or a URL.

Each codec offers both bitmap methods and lower-level byte-span methods. The bitmap adapter normalizes supported Skia color types, premultiplication, row stride, and color space to unpremultiplied sRGB RGBA8, resampling to at most 100×100 without stretching aspect ratio. The byte-span methods require positive dimensions, RGBA8 row-major unpremultiplied pixels, and an explicit stride; the byte span must end exactly at the last pixel (row padding on earlier rows is allowed). ThumbHash pixels may be at most 100×100; source bitmaps and low-level BlurHash pixels are limited to 16 MP. Bitmap decoded previews are limited to 1 MP. Invalid dimensions and payloads throw instead of allocating unbounded memory.

This folder contains **codecs only**. The Gallery-specific `Samples/PlaceholderImageView.cs` owns rendering and its 6-second two-way transition; it is deliberately not part of the copy-out source. The caller owns and disposes input bitmaps and decoded preview bitmaps. No codec retains an image, starts a timer, or performs network I/O.
