using Android.App;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.DrawerLayout.Widget;
using Google.Android.Material.Navigation;

namespace SkiaSharpSample
{
	[Activity(Label = "SkiaSharp", MainLauncher = true, Theme = "@style/Theme.SkiaSharpSample")]
	public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
	{
		private DrawerLayout drawerLayout;
		private ActionBarDrawerToggle toggle;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.main);

			var toolbar = FindViewById<Google.Android.Material.AppBar.MaterialToolbar>(Resource.Id.toolbar);
			SetSupportActionBar(toolbar);

			drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			toggle = new ActionBarDrawerToggle(this, drawerLayout, toolbar, 0, 0);
			drawerLayout.AddDrawerListener(toggle);
			toggle.SyncState();

			var navView = FindViewById<NavigationView>(Resource.Id.nav_view);
			navView.SetNavigationItemSelectedListener(this);

			if (savedInstanceState == null)
				ShowFragment(new CpuFragment());
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
