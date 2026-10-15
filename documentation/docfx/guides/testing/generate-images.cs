#!/usr/bin/env dotnet
#:project ../../../../source/SkiaSharp.Testing/SkiaSharp.Testing.csproj
#:property PublishAot=false
#:property NoWarn=SKIA4001

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SkiaSharp;
using SkiaSharp.Testing;

var directory = Path.GetDirectoryName(SourceFile())!;
var root = Path.GetFullPath(Path.Combine(directory, "../../../.."));
var images = Path.Combine(directory, "images");
Directory.CreateDirectory(images);

var architecture = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
var native = OperatingSystem.IsMacOS()
    ? Path.Combine(root, "output/native/osx/libSkiaSharp.dylib")
    : OperatingSystem.IsWindows()
        ? Path.Combine(root, $"output/native/windows/{architecture}/libSkiaSharp.dll")
        : OperatingSystem.IsLinux()
            ? Path.Combine(root, $"output/native/linux/{architecture}/libSkiaSharp.so")
            : throw new PlatformNotSupportedException("Run this generator on macOS, Windows or Linux.");
NativeLibrary.SetDllImportResolver(typeof(SKImage).Assembly,
    (name, _, _) => name == "libSkiaSharp" ? NativeLibrary.Load(native) : IntPtr.Zero);

var bluePanel = SKRect.Create(60, 110, 420, 160);
using var expected = RenderScene(actual: false);
using var actual = RenderScene(actual: true);
using var toleranceMask = RenderMask(ignore: false);
using var ignoreMask = RenderMask(ignore: true);

SaveImage("expected.png", expected);
SaveImage("actual.png", actual);
SaveImage("tolerance-mask.png", toleranceMask);
SaveImage("ignore-mask.png", ignoreMask);
using var exact = SKPixelComparer.GenerateDifferenceImage(expected, actual);
SaveImage("comparison-exact.png", exact);

var toleranceOptions = RgbTolerance();
using var tolerant = SKPixelComparer.GenerateDifferenceImage(expected, actual, options: toleranceOptions);
SaveImage("comparison-tolerant.png", tolerant);

var styleOptions = RgbTolerance();
styleOptions.AlphaTolerance = SKPixelTolerance.Exact;
using var overlay = SKPixelComparer.GenerateDifferenceImage(expected, actual, options: styleOptions);
using var map = SKPixelComparer.GenerateDifferenceImage(expected, actual, SKPixelDifferenceStyle.ThresholdMap, styleOptions);
using var binary = SKPixelComparer.GenerateDifferenceImage(expected, actual, SKPixelDifferenceStyle.BinaryMask, styleOptions);
using var channels = SKPixelComparer.GenerateDifferenceImage(expected, actual, SKPixelDifferenceStyle.ChannelDifference, styleOptions);
SaveImage("threshold-overlay.png", overlay);
SaveImage("threshold-map.png", map);
SaveImage("binary-mask.png", binary);
SaveImage("channel-difference.png", channels);

var regionalTolerance = new SKPixelComparerOptions
{
    AlphaTolerance = null,
    ToleranceMask = toleranceMask,
};
using var regionTolerated = SKPixelComparer.GenerateDifferenceImage(expected, actual, options: regionalTolerance);
SaveImage("comparison-tolerance.png", regionTolerated);

var regionalIgnore = new SKPixelComparerOptions
{
    AlphaTolerance = null,
    IgnoreMask = ignoreMask,
};
using var regionIgnored = SKPixelComparer.GenerateDifferenceImage(expected, actual, options: regionalIgnore);
SaveImage("comparison-ignore.png", regionIgnored);

Console.WriteLine($"Generated comparison images in {images}");

static string SourceFile([CallerFilePath] string file = "") => file;
static SKPixelComparerOptions RgbTolerance() => new()
{
    RedTolerance = SKPixelTolerance.Absolute(2),
    GreenTolerance = SKPixelTolerance.Absolute(2),
    BlueTolerance = SKPixelTolerance.Absolute(2),
    AlphaTolerance = null,
};

SKImage RenderScene(bool actual)
{
    using var bitmap = new SKBitmap(new SKImageInfo(800, 600, SKColorType.Bgra8888, SKAlphaType.Unpremul));
    using var canvas = new SKCanvas(bitmap);
    using var paint = new SKPaint { IsAntialias = false, BlendMode = SKBlendMode.Src };
    using var gradient = SKShader.CreateLinearGradient(
        new SKPoint(0, 0), new SKPoint(800, 600),
        new[] { new SKColor(247, 245, 252), new SKColor(231, 241, 246) },
        SKShaderTileMode.Clamp);
    using var background = new SKPaint { Shader = gradient };
    canvas.DrawPaint(background);
    paint.Color = actual ? new SKColor(74, 106, 194) : new SKColor(72, 104, 192);
    canvas.DrawRoundRect(bluePanel, 24, 24, paint);
    paint.Color = new SKColor(48, 185, 131, actual ? (byte)128 : (byte)255);
    canvas.DrawCircle(640, 205, 90, paint);
    if (actual)
    {
        paint.Color = new SKColor(221, 91, 103);
        canvas.DrawRoundRect(80, 345, 180, 180, 28, 28, paint);
    }
    return SKImage.FromBitmap(bitmap)
        ?? throw new InvalidOperationException("Unable to snapshot the example scene.");
}

SKImage RenderMask(bool ignore)
{
    using var bitmap = new SKBitmap(new SKImageInfo(800, 600, SKColorType.Bgra8888, SKAlphaType.Opaque));
    using var canvas = new SKCanvas(bitmap);
    using var paint = new SKPaint { Color = ignore ? SKColors.White : new SKColor(32, 32, 32) };
    canvas.Clear(SKColors.Black);
    canvas.DrawRoundRect(bluePanel, 24, 24, paint);
    return SKImage.FromBitmap(bitmap)
        ?? throw new InvalidOperationException("Unable to snapshot the example mask.");
}

void SaveImage(string name, SKImage image)
{
    using var data = image.Encode(SKEncodedImageFormat.Png, 100)
        ?? throw new InvalidOperationException($"Unable to encode {name}.");
    File.WriteAllBytes(Path.Combine(images, name), data.ToArray());
}
