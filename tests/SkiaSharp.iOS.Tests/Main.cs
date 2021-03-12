using System;
using UIKit;

namespace SkiaSharp.Tests
{
    public class Application
    {
        static void Main(string[] args)
        {
            AssetCopier.CopyAssets();

            if (args?.Length > 0 || Environment.GetEnvironmentVariable("NUNIT_AUTOEXIT")?.Length > 0) // usually means this is from xharness
                UIApplication.Main(args, null, nameof(TestApplicationDelegate));
            else
                UIApplication.Main(args, null, nameof(AppDelegate));
        }
    }
}
