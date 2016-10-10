using Android.App;
using Android.Content;
using Android.Support.V7.App;

namespace SkiaSharpSample.FormsSample.Platform
{
	[Activity(Theme = "@style/MainTheme.Splash", MainLauncher = true, NoHistory = true)]
	public class SplashActivity : AppCompatActivity
	{
		protected override void OnResume()
		{
			base.OnResume();

			SamplesInitializer.Init();

			StartActivity(new Intent(Application.Context, typeof(MainActivity)));
		}
	}
}
