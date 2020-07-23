using Android.App;
using Android.Content;
using AndroidX.AppCompat.App;

namespace SkiaSharpSample.Platform
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
