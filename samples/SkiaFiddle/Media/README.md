# SkiaFiddle bundled media

These assets are embedded into the SkiaFiddle WebAssembly app and preloaded at
startup. They are exposed to snippet code through the `image` and `typeface`
variables (chosen with the image strip and font picker in the toolbar), mirroring
the "Optional source image" feature on https://fiddle.skia.org/.

The assets are bundled (not downloaded) because the trimmed Release WASM build
removes `HttpClient`'s download methods, so runtime fetching is not possible.

## Images (`1.png`–`6.png`)

The six canonical Skia fiddle source images, taken verbatim from the Skia
buildbot repository:
<https://skia.googlesource.com/buildbot/+/refs/heads/main/fiddlek/source/>

`3.png` is the classic mandrill/baboon and `5.png` is the color wheel — the same
images already shipped in `samples/Gallery/Shared/Media/`. They are licensed
under the Skia project's BSD-3-Clause license (`exports_files` in the buildbot
repo) and are freely redistributable.

## Fonts

| File               | Family          | License                          | Used for          |
|--------------------|-----------------|----------------------------------|-------------------|
| `InterVariable.ttf`| Inter           | SIL Open Font License 1.1        | Variable-font demo (`wght` axis) |
| `Nabla.ttf`        | Nabla           | SIL Open Font License 1.1        | Color-font demo (COLR/CPAL palettes) |
| `DejaVuSerif.ttf`  | DejaVu Serif    | Bitstream Vera / Arev (free)     | Plain serif        |
| `DejaVuSans.ttf`   | DejaVu Sans     | Bitstream Vera / Arev (free)     | Plain sans         |

`InterVariable.ttf` and `Nabla.ttf` are copied from `samples/Gallery/Shared/Media/`.
The DejaVu fonts are the metric-friendly, freely redistributable serif/sans pair
from the [DejaVu fonts project](https://dejavu-fonts.github.io/) (the Bitstream
Vera license permits use, modification, and redistribution, including as part of
a software package). They stand in for proprietary fonts such as Times New Roman
and Arial, which cannot be bundled.
