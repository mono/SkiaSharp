# SkiaSharp

Skia# is a cross-platform, managed binding for the 
Skia Graphics Library (https://skia.org/)

## What is Included

Skia# provides a PCL and platform-specific bindings for:

 - Mac OS X
 - Xamarin.Android
 - Xamarin.iOS
 - Windows Desktop
 - Mac Desktop
 
## Using Skia#

### Creating a SKCanvas

Although all platforms can use the same *drawing* logic, each 
platform has a different method for obtaining the surface and
canvas.

#### iOS

    var screenScale = UIScreen.MainScreen.Scale;
    var width = (int)(Bounds.Width * screenScale);
    var height = (int)(Bounds.Height * screenScale);

    IntPtr buff = System.Runtime.InteropServices.Marshal.AllocCoTaskMem (width * height * 4);
    try {
        using (var surface = SKSurface.Create (width, height, SKColorType.N_32, SKAlphaType.Premul, buff, width * 4)) {
            var skcanvas = surface.Canvas;
            skcanvas.Scale ((float)screenScale, (float)screenScale);
            using (new SKAutoCanvasRestore (skcanvas, true)) {
                DoDraw (skcanvas);
            }
        }
        using (var colorSpace = CGColorSpace.CreateDeviceRGB ())
        using (var bContext = new CGBitmapContext (buff, width, height, 8, width * 4, colorSpace, (CGImageAlphaInfo)bitmapInfo))
        using (var image = bContext.ToImage ())
        using (var context = UIGraphics.GetCurrentContext ()) {
            // flip the image for CGContext.DrawImage
            context.TranslateCTM (0, Frame.Height);
            context.ScaleCTM (1, -1);
            context.DrawImage (Bounds, image);
        }
    } finally {
        if (buff != IntPtr.Zero)
            System.Runtime.InteropServices.Marshal.FreeCoTaskMem (buff);
    }

#### Android

    var width = (float)skiaView.Width;
    var height = (float)skiaView.Height;
    
    using (var bitmap = Bitmap.CreateBitmap (canvas.Width, canvas.Height, Bitmap.Config.Argb8888)) {
        try {
            using (var surface = SKSurface.Create (canvas.Width, canvas.Height, SKColorType.Rgba_8888, SKAlphaType.Premul, bitmap.LockPixels (), canvas.Width * 4)) {
                var skcanvas = surface.Canvas;
                skcanvas.Scale (((float)canvas.Width)/width, ((float)canvas.Height)/height);
                DoDraw (skcanvas);
            }
        }
        finally {
            bitmap.UnlockPixels ();
        }
        canvas.DrawBitmap (bitmap, 0, 0, null);
    }

#### OS X

    var screenScale = (int)NSScreen.MainScreen.BackingScaleFactor * 2;
    var width = (int)Bounds.Width * screenScale;
    var height = (int)Bounds.Height * screenScale;

    IntPtr buff = System.Runtime.InteropServices.Marshal.AllocCoTaskMem (width * height * 4);
    try {
        using (var surface = SKSurface.Create (width, height, SKColorType.Rgba_8888, SKAlphaType.Premul, buff, width * 4)) {
            var skcanvas = surface.Canvas;
            skcanvas.Scale (screenScale, screenScale);
            DoDraw (skcanvas);
        }
        int flag = ((int)CoreGraphics.CGBitmapFlags.ByteOrderDefault) | ((int)CoreGraphics.CGImageAlphaInfo.PremultipliedLast);
        using (var colorSpace = CoreGraphics.CGColorSpace.CreateDeviceRGB ())
        using (var bContext = new CoreGraphics.CGBitmapContext (buff, width, height, 8, width * 4, colorSpace, (CoreGraphics.CGImageAlphaInfo) flag))
        using (var image = bContext.ToImage ())
        using (var context = NSGraphicsContext.CurrentContext.GraphicsPort) {
            context.DrawImage (Bounds, image);
        }
    } finally {
        if (buff != IntPtr.Zero)
            System.Runtime.InteropServices.Marshal.FreeCoTaskMem (buff);
    }

#### Windows Desktop / Mac Desktop

    var width = Width;
    var height = Height;

    using (var bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb)) {
        var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
        using (var surface = SKSurface.Create(width, height, SKColorType.Bgra_8888, SKAlphaType.Premul, data.Scan0, width * 4)) {
            var skcanvas = surface.Canvas;
            onDrawCallback(skcanvas, width, height);
        }
        bitmap.UnlockBits(data);
        e.Graphics.DrawImage(bitmap, new Rectangle(0, 0, Width, Height));
    }

### Drawing on SKCanvas

Below are just a few of the many different things that can be done
with Skia#:

#### Drawing Xamagon

    SKCanvas canvas = ...;

    // clear the canvas / fill with white
    canvas.Clear (SKColors.White);
    
    // set up drawing tools
    using (var paint = new SKPaint ()) {
        paint.IsAntialias = true;
        paint.Color = new SKColor (0x2c, 0x3e, 0x50);
        paint.StrokeCap = SKStrokeCap.Round;

        // create the Xamagon path
        using (var path = new SKPath ()) {
            path.MoveTo (71.4311121f, 56f);
            path.CubicTo (68.6763107f, 56.0058575f, 65.9796704f, 57.5737917f, 64.5928855f, 59.965729f);
            path.LineTo (43.0238921f, 97.5342563f);
            path.CubicTo (41.6587026f, 99.9325978f, 41.6587026f, 103.067402f, 43.0238921f, 105.465744f);
            path.LineTo (64.5928855f, 143.034271f);
            path.CubicTo (65.9798162f, 145.426228f, 68.6763107f, 146.994582f, 71.4311121f, 147f);
            path.LineTo (114.568946f, 147f);
            path.CubicTo (117.323748f, 146.994143f, 120.020241f, 145.426228f, 121.407172f, 143.034271f);
            path.LineTo (142.976161f, 105.465744f);
            path.CubicTo (144.34135f, 103.067402f, 144.341209f, 99.9325978f, 142.976161f, 97.5342563f);
            path.LineTo (121.407172f, 59.965729f);
            path.CubicTo (120.020241f, 57.5737917f, 117.323748f, 56.0054182f, 114.568946f, 56f);
            path.LineTo (71.4311121f, 56f);
            path.Close ();
            
            // draw the Xamagon path
            canvas.DrawPath (path, paint);
        }
    }

#### Drawing Text

    SKCanvas canvas = ...;
    
    // clear the canvas / fill with white
    canvas.DrawColor (SKColors.White);

    // set up drawing tools
    using (var paint = new SKPaint ()) {
        paint.TextSize = 64.0f;
        paint.IsAntialias = true;
        paint.Color = new SKColor (0x42, 0x81, 0xA4);
        paint.IsStroke = false;
        
        // draw the text
        canvas.DrawText ("Skia", 0.0f, 64.0f, paint);
    }

#### Drawing Bitmaps

    SKCanvas canvas = ...; 
    Stream fileStream = ...; // open a stream to an image file
    
    // clear the canvas / fill with white
    canvas.DrawColor (SKColors.White);

    // decode the bitmap from the stream
    using (var stream = new SKManagedStream(fileStream))
    using (var bitmap = SKBitmap.Decode(stream))
    using (var paint = new SKPaint()) {
        canvas.DrawBitmap(bitmap, SKRect.Create(Width, Height), paint);
    }

#### Drawing Image Filters

    SKCanvas canvas = ...; 
    Stream fileStream = ...; // open a stream to an image file
    
    // clear the canvas / fill with white
    canvas.DrawColor (SKColors.White);

    // decode the bitmap from the stream
    using (var stream = new SKManagedStream(fileStream))
    using (var bitmap = SKBitmap.Decode(stream))
    using (var paint = new SKPaint()) {
        // create the image filter
        using (var filter = SKImageFilter.CreateBlur(5, 5)) {
            paint.ImageFilter = filter;
            
            // draw the bitmap through the filter
            canvas.DrawBitmap(bitmap, SKRect.Create(width, height), paint);
        }
    }

## How Skia# is Built

Skia# provides the same features as the native C++ library through
a method of wrapping the C++ API with a dumb C API. Then C# wraps the
C API to provide an API similar to that of the C++.

For example, given the C++ API:

    class SK_API SkPaint {
    public:
        bool isAntiAlias() const;
        void setAntiAlias(bool aa);
    };

This is then wrapped in a C API:

    bool sk_paint_is_antialias(const sk_paint_t* cpaint) {
        return AsPaint(*cpaint).isAntiAlias();
    }
    void sk_paint_set_antialias(sk_paint_t* cpaint, bool aa) {
        AsPaint(cpaint)->setAntiAlias(aa);
    }

Which is then pulled into the C# project via P/Invoke:

    public extern static bool sk_paint_is_antialias(sk_paint_t t);
    public extern static void sk_paint_set_antialias(sk_paint_t t, bool aa);

Finally, this is wrapped into a neat C# class:

	public class SKPaint : SKObject
	{
		public bool IsAntialias {
			get { return SkiaApi.sk_paint_is_antialias (Handle); }
			set { SkiaApi.sk_paint_set_antialias (Handle, value); }
		}
    }

As a result, the C# API functions and appears the same as the C++ API.

## Where is Windows Phone / Store
 
At this time, Windows Phone and Windows Store apps are not 
supported. This is due to the native library not supporting 
those platforms: 

 - https://bugs.chromium.org/p/skia/issues/detail?id=2059
 - https://groups.google.com/forum/#!searchin/skia-discuss/windows$20phone/skia-discuss/VHRCLl-XV8E/YpUKZr4OVKgJ
 - https://groups.google.com/forum/#!searchin/skia-discuss/windows$208/skia-discuss/FF4-KzRRDp8/S0Uoy1f0waIJ
