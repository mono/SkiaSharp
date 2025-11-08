# Views Support Matrix

This page provides a comprehensive overview of the UI views that SkiaSharp supports across different platforms.

## Platform Support

<table>
  <tr>
    <th></th>
    <th colspan="2">CPU / Raster</th>
    <th colspan="2">GPU / Accelerated</th>
  </tr>
  <tr>
    <th></th>
    <th>Native</th>
    <th>.NET MAUI / Xamarin.Forms</th>
    <th>Native</th>
    <th>.NET MAUI / Xamarin.Forms</th>
  </tr>
  <tr>
    <td align="right"><b>Android</b></td>
    <!-- CPU -->
    <td align="center">✔️<br/><code>SKCanvasView</code><br/><code>SKSurfaceView</code><sup>1</sup></td>
    <td align="center">✔️<br/><code>SKCanvasView</code></td>
    <!-- GPU -->
    <td align="center">✔️<br/><code>SKGLTextureView</code><sup>1 2</sup><br/><code>SKGLSurfaceView</code><sup>1 2</sup></td>
    <td align="center">✔️<br/><code>SKGLView</code></td>
  </tr>
  <tr>
    <td align="right"><b>GTK# 2</b></td>
    <!-- CPU -->
    <td align="center">✔️<br/><code>SKWidget</code></td>
    <td align="center">✔️<br/><code>SKCanvasView</code></td>
    <!-- GPU -->
    <td align="center">🕵️<br/><em>(under investigation)</em></td>
    <td align="center">✔️<br/><code>SKGLView</code></td>
  </tr>
  <tr>
    <td align="right"><b>GTK# 3</b></td>
    <!-- CPU -->
    <td align="center">✔️<br/><code>SKDrawingArea</code></td>
    <td align="center">❌<br/><em>(Xamarin.Forms)</em></td>
    <!-- GPU -->
    <td align="center">🕵️<br/><em>(under investigation)</em></td>
    <td align="center">❌<br/><em>(Xamarin.Forms)</em></td>
  </tr>
  <tr>
    <td align="right"><b>iOS</b></td>
    <!-- CPU -->
    <td align="center">✔️<br/><code>SKCanvasView</code><br/><code>SKCanvasLayer</code></td>
    <td align="center">✔️<br/><code>SKCanvasView</code></td>
    <!-- GPU -->
    <td align="center">✔️<br/><code>SKGLView</code><br/><code>SKGLLayer</code></td>
    <td align="center">✔️<br/><code>SKGLView</code></td>
  </tr>
  <tr>
    <td align="right"><b>macOS</b></td>
    <!-- CPU -->
    <td align="center">✔️<br/><code>SKCanvasView</code>️<br/><code>SKCanvasLayer</code></td>
    <td align="center">✔️<br/><code>SKCanvasView</code></td>
    <!-- GPU -->
    <td align="center">✔️<br/><code>SKGLView</code><br/><code>SKGLLayer</code></td>
    <td align="center">✔️<br/><code>SKGLView</code></td>
  </tr>
  <tr>
    <td align="right"><b>Tizen</b></td>
    <!-- CPU -->
    <td align="center">✔️<br/><code>SKCanvasView</code><sup>2</sup></td>
    <td align="center">✔️<br/><code>SKCanvasView</code></td>
    <!-- GPU -->
    <td align="center">✔️<br/><code>SKGLSurfaceView</code><sup>2</sup></td>
    <td align="center">✔️<br/><code>SKGLView</code></td>
  </tr>
  <tr>
    <td align="right"><b>tvOS</b></td>
    <!-- CPU -->
    <td align="center">✔️<br/><code>SKCanvasView</code><br/><code>SKCanvasLayer</code></td>
    <td align="center">❌<br/><em>(Xamarin.Forms)</em></td>
    <!-- GPU -->
    <td align="center">✔️<br/><code>SKGLView</code><br/><code>SKGLLayer</code></td>
    <td align="center">❌<br/><em>(Xamarin.Forms)</em></td>
  </tr>
  <tr>
    <td align="right"><b>Windows (UWP)</b></td>
    <!-- CPU -->
    <td align="center">✔️<br/><code>SKXamlCanvas</code></td>
    <td align="center">✔️<br/><code>SKCanvasView</code></td>
    <!-- GPU -->
    <td align="center">✔️<br/><code>SKSwapChainPanel</code><sup>1 2</sup></td>
    <td align="center">✔️<br/><code>SKGLView</code></td>
  </tr>
  <tr>
    <td align="right"><b>Windows (WinUI 3)</b></td>
    <!-- CPU -->
    <td align="center">✔️<br/><code>SKXamlCanvas</code></td>
    <td align="center">✔️<br/><code>SKCanvasView</code></td>
    <!-- GPU -->
    <td align="center">✔️<br/><code>SKSwapChainPanel</code><sup>1 2</sup></td>
    <td align="center">✔️<br/><code>SKGLView</code></td>
  </tr>
  <tr>
    <td align="right"><b>watchOS</b></td>
    <!-- CPU -->
    <td align="center">🕵️<br/><em>(under investigation)</em></td>
    <td align="center">❌<br/><em>(Xamarin.Forms)</em></td>
    <!-- GPU -->
    <td align="center">🕵️<br/><em>(under investigation)</em></td>
    <td align="center">❌<br/><em>(Xamarin.Forms)</em></td>
  </tr>
  <tr>
    <td align="right"><b>WinForms</b></td>
    <!-- CPU -->
    <td align="center">✔️<br/><code>SKControl</code></td>
    <td align="center">❌<br/><em>(Xamarin.Forms)</em></td>
    <!-- GPU -->
    <td align="center">✔️<br/><code>SKGLControl</code></td>
    <td align="center">❌<br/><em>(Xamarin.Forms)</em></td>
  </tr>
  <tr>
    <td align="right"><b>WPF</b></td>
    <!-- CPU -->
    <td align="center">✔️<br/><code>SKElement</code></td>
    <td align="center">✔️<br/><code>SKCanvasView</code></td>
    <!-- GPU -->
    <td align="center">❌<br/><em>(use <code>SKGLControl</code> and <code>WindowsFormsHost</code>)</em></td>
    <td align="center">✔️<br/><code>SKGLView</code></td>
  </tr>
</table>

**Notes:**

1. Supports rendering to the view on a background thread.
2. Provides a built-in render loop.

## .NET MAUI Support

For .NET MAUI applications, SkiaSharp provides native integration through the `SkiaSharp.Views.Maui` package. This package includes:

- **SKCanvasView** - For CPU-based rendering
- **SKGLView** - For GPU-accelerated rendering

Both views work consistently across all .NET MAUI platforms (Android, iOS, macOS, Windows).

## Xamarin.Forms Support

Legacy Xamarin.Forms support is available through the `SkiaSharp.Views.Forms` package, which provides the same view types as the MAUI package.

## Related Documentation

- [SkiaSharp Samples](../../samples/) - Examples of using SkiaSharp views
- [Building SkiaSharp](../building/building-skiasharp.md) - How to build SkiaSharp from source
