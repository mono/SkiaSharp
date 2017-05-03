using Foundation;
using ObjCRuntime;

[assembly: LinkerSafe]
[assembly: LinkWith("libHarfBuzzSharp.dylib", IsCxx = true, ForceLoad = true, SmartLink = true)]
