using Android.App;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Core.View;
using Google.Android.Material.BottomNavigation;

namespace SkiaSharpSample;

[Activity(Label = "SkiaSharp", MainLauncher = true, Theme = "@style/Theme.SkiaSharpSample")]
public class MainActivity : AppCompatActivity, BottomNavigationView.IOnItemSelectedListener, IOnApplyWindowInsetsListener
{
	/// <summary>
	/// Change this to start the app on a different page.
	/// </summary>
	public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;

	protected override void OnCreate(Bundle savedInstanceState)
	{
		base.OnCreate(savedInstanceState);

		WindowCompat.SetDecorFitsSystemWindows(Window, false);

		SetContentView(Resource.Layout.main);

		var toolbar = FindViewById<Google.Android.Material.AppBar.MaterialToolbar>(Resource.Id.toolbar);
		SetSupportActionBar(toolbar);

		var bottomNav = FindViewById<BottomNavigationView>(Resource.Id.bottom_nav);
		bottomNav.SetOnItemSelectedListener(this);

		// Apply top status bar inset to the AppBarLayout
		var appBar = FindViewById<View>(Resource.Id.appbar);
		ViewCompat.SetOnApplyWindowInsetsListener(appBar, this);

		if (savedInstanceState == null)
		{
			bottomNav.SelectedItemId = DefaultPage switch
			{
				SamplePage.GpuSurface => Resource.Id.nav_gpu_surface,
				SamplePage.GpuTexture => Resource.Id.nav_gpu_texture,
				SamplePage.Drawing => Resource.Id.nav_drawing,
				_ => Resource.Id.nav_cpu,
			};
		}
	}

	public WindowInsetsCompat OnApplyWindowInsets(View v, WindowInsetsCompat insets)
	{
		var bars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
		v.SetPadding(0, bars.Top, 0, 0);
		return insets;
	}

	public bool OnNavigationItemSelected(IMenuItem item)
	{
		AndroidX.Fragment.App.Fragment fragment = item.ItemId switch
		{
			Resource.Id.nav_cpu => new CpuFragment(),
			Resource.Id.nav_gpu_surface => new GpuSurfaceFragment(),
			Resource.Id.nav_gpu_texture => new GpuTextureFragment(),
			Resource.Id.nav_drawing => new DrawingFragment(),
			_ => new CpuFragment()
		};

		ShowFragment(fragment);
		return true;
	}

	private void ShowFragment(AndroidX.Fragment.App.Fragment fragment)
	{
		SupportFragmentManager.BeginTransaction()
			.Replace(Resource.Id.content_frame, fragment)
			.Commit();
	}
}
