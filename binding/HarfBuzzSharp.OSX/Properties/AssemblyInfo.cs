using Foundation;
using ObjCRuntime;

[assembly: LinkerSafe]
[assembly: LinkWith("libharfbuzz.dylib", IsCxx = true, ForceLoad = true, SmartLink = true)]
