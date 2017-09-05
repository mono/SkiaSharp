using Foundation;
using ObjCRuntime;

[assembly: LinkerSafe]
[assembly: LinkWith("libHarfBuzzSharp.a", IsCxx = true, ForceLoad = true, SmartLink = true)]
