using System;
using System.IO;
using SkiaSharp;

var assemblyLocation = Path.GetDirectoryName(typeof(SKBitmap).Assembly.Location);
var packageLocation = Path.GetDirectoryName(Path.GetDirectoryName(assemblyLocation));
var isMac = Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix;
var runtime = isMac ? "osx" : ("win7-" + (Environment.Is64BitProcess ? "x64" : "x86"));
var nativeName = isMac ? "libSkiaSharp.dylib" : "libSkiaSharp.dll";
var nativeLocation = Path.Combine(packageLocation, "runtimes", runtime, "native", nativeName);
var newLocation = Path.Combine(assemblyLocation, nativeName);

if (!File.Exists(newLocation)) {
    File.Copy(nativeLocation, newLocation, true);
}
