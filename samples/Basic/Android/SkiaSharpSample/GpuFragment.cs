using System;
using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using Google.Android.Material.Tabs;

namespace SkiaSharpSample;

public class GpuFragment : Fragment
{
	/// <summary>
	/// Change this to start the GPU page on a different backend.
	/// </summary>
	public static GpuBackend DefaultGpuBackend
	{
		get => MainActivity.DefaultGpuBackend;
		set => MainActivity.DefaultGpuBackend = value;
	}

	public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
	{
		var view = inflater.Inflate(Resource.Layout.fragment_gpu, container, false);
		var tabLayout = view.FindViewById<TabLayout>(Resource.Id.gpu_tabs);

		var activeBackend = ChildFragmentManager.FindFragmentById(Resource.Id.gpu_child_container) switch
		{
			GpuSurfaceFragment => GpuBackend.Surface,
			GpuTextureFragment => GpuBackend.Texture,
			_ => DefaultGpuBackend,
		};

		foreach (var backend in Enum.GetValues<GpuBackend>())
		{
			var tab = tabLayout.NewTab().SetText(backend.ToString());
			tab.SetTag((int)backend);
			tabLayout.AddTab(tab, backend == activeBackend);
		}

		tabLayout.TabSelected += OnTabSelected;

		if (savedInstanceState == null)
			ShowBackend(activeBackend);

		return view;
	}

	private void OnTabSelected(object sender, TabLayout.TabSelectedEventArgs e)
	{
		if (e.Tab?.Tag is Java.Lang.Integer index)
			ShowBackend((GpuBackend)index.IntValue());
	}

	private void ShowBackend(GpuBackend backend)
	{
		Fragment fragment = backend switch
		{
			GpuBackend.Texture => new GpuTextureFragment(),
			GpuBackend.Surface => new GpuSurfaceFragment(),
			_ => new GpuTextureFragment(),
		};

		ChildFragmentManager.BeginTransaction()
			.Replace(Resource.Id.gpu_child_container, fragment)
			.Commit();
	}
}
