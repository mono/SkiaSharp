using Foundation;
using ObjCRuntime;

[assembly: LinkerSafe]
[assembly: LinkWith("libharfbuzz.a", IsCxx = true, ForceLoad = true, SmartLink = true)]
