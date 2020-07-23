using Android.App;
using Android.Views;

namespace SkiaSharpSample.Droid
{
	[Activity(
		MainLauncher = true,
		ConfigurationChanges = Uno.UI.ActivityHelper.AllConfigChanges,
		WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden)]
	public class MainActivity : Windows.UI.Xaml.ApplicationActivity
	{
	}
}
