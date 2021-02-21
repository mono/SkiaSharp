using System.Reflection;
using Foundation;
using UIKit;

namespace SkiaSharp.Tests
{
    [Register(nameof(AppDelegate))]
    public partial class AppDelegate : Xunit.Runner.RunnerAppDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            // We need this to ensure the execution assembly is part of the app bundle
            AddExecutionAssembly(Assembly.GetExecutingAssembly());

            // tests can be inside the main assembly
            AddTestAssembly(Assembly.GetExecutingAssembly());

            return base.FinishedLaunching(app, options);
        }
    }
}
