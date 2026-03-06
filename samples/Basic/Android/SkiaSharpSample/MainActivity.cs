using Android.App;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using Google.Android.Material.Navigation;

namespace SkiaSharpSample
{
	[Activity(Label = "SkiaSharp", MainLauncher = true, Theme = "@style/Theme.SkiaSharpSample")]
	public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener, IOnApplyWindowInsetsListener
	{
		private DrawerLayout drawerLayout;
		private ActionBarDrawerToggle toggle;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			WindowCompat.SetDecorFitsSystemWindows(Window, false);

			SetContentView(Resource.Layout.main);

			var toolbar = FindViewById<Google.Android.Material.AppBar.MaterialToolbar>(Resource.Id.toolbar);
			SetSupportActionBar(toolbar);

			drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			toggle = new ActionBarDrawerToggle(this, drawerLayout, toolbar, 0, 0);
			drawerLayout.AddDrawerListener(toggle);
			toggle.SyncState();

			var navView = FindViewById<NavigationView>(Resource.Id.nav_view);
			navView.SetNavigationItemSelectedListener(this);

			var contentFrame = FindViewById<View>(Resource.Id.content_frame);
			ViewCompat.SetOnApplyWindowInsetsListener(contentFrame, this);

			if (savedInstanceState == null)
				ShowFragment(new CpuFragment());
		}

		public WindowInsetsCompat OnApplyWindowInsets(View v, WindowInsetsCompat insets)
		{
			var bars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
			v.SetPadding(0, 0, 0, bars.Bottom);
			return insets;
		}

		public bool OnNavigationItemSelected(Android.Views.IMenuItem item)
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
			item.SetChecked(true);
			drawerLayout.CloseDrawers();
			return true;
		}

		private void ShowFragment(AndroidX.Fragment.App.Fragment fragment)
		{
			SupportFragmentManager.BeginTransaction()
				.Replace(Resource.Id.content_frame, fragment)
				.Commit();
		}
	}
}
