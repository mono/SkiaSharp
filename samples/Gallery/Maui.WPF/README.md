# Experimental WPF MAUI Gallery

This Windows-only app hosts the merged MAUI Gallery's shared XAML pages,
catalog, filters, detail views, and CPU/GPU header selection on the
`dotnet/maui-labs` native WPF backend. It is **not** the WinUI Gallery.
`UseMauiAppWPF<App>()` and `UseWPFEssentials()` host the MAUI application,
while `UseSkiaSharpWPF()` installs the native WPF `SKElement` and `SKGLElement`
handlers. The controls, sample catalog, and XAML remain owned by the standard
MAUI Gallery; this project links to them without modifying that app.

Build from Windows with the .NET 10 SDK and desktop workload:

```sh
dotnet build samples/Gallery/Maui.WPF/SkiaSharpSample.Maui.WPF.csproj
```

On macOS, a **cross-build only** can use
`-p:EnableWindowsTargeting=true -p:WindowsDesktopTargetFrameworks=net10.0-windows`.
Do not use the previously downloaded native binaries after the Skia milestone
update; build native binaries from source before exercising Skia runtime tests.
The Gallery host uses MAUI Controls 10.0.51 and the WPF Labs preview
`0.1.0-preview.12.26421.1`; no preview dependency is added to stable packages.

The backend's ordinary `FontImageSource` buttons and images do not map font
glyphs reliably. This host alone maps the bundled Bootstrap Icons font to
WPF glyph text for Gallery labels, image buttons, and buttons with icons; it
does not modify other apps or the shared XAML. WPF numeric `ZIndex` is not
dependable: the shared popup inserts its scrim as the last child.

**Windows runtime validation pending:** verify WPF window launch, catalog and
search/filter/sort popovers at narrow and wide widths, OS System/Light/Dark
theme, glyph labels/buttons, CPU and real OpenGL GPU rendering, resize and
mixed-DPI touch, handler disconnect/navigation, native document open/share,
and clean shutdown. No macOS cross-build is a WPF runtime test.
