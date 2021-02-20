using System.Reflection;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Xunit.Runners.UI;

namespace SkiaSharp.Tests
{
	[Activity(MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : RunnerActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			Xamarin.Essentials.Platform.Init(this, bundle);

			AssetCopier.CopyAssets();

			AddTestAssembly(Assembly.GetExecutingAssembly());
			AddExecutionAssembly(Assembly.GetExecutingAssembly());

			base.OnCreate(bundle);
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}
	}
}
