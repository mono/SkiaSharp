using Foundation;
using ObjCRuntime;

[assembly: LinkerSafe]
[assembly: LinkWith("libSkiaSharp.dylib", IsCxx = true, ForceLoad = true, SmartLink = true)]
