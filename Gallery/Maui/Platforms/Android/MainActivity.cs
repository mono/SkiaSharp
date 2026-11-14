using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using AndroidX.Core.View;
using Microsoft.Maui;

namespace SkiaSharpSample;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetLightSystemBarIcons(false);
    }

    protected override void OnResume()
    {
        base.OnResume();
        SetLightSystemBarIcons(false);
    }

    public override void OnConfigurationChanged(Configuration newConfig)
    {
        base.OnConfigurationChanged(newConfig);
        SetLightSystemBarIcons(false);
    }

    private void SetLightSystemBarIcons(bool enabled)
    {
        if (Window?.DecorView is { } decor)
        {
            var controller = WindowCompat.GetInsetsController(Window, decor);
            controller.AppearanceLightStatusBars = enabled;
            controller.AppearanceLightNavigationBars = enabled;
        }
    }
}
