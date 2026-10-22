using SkiaSharpSample.Pages;

namespace SkiaSharpSample;

public sealed partial class MainPage : Page
{
    public static MainPage? Current { get; private set; }

    private bool isDark;

    public MainPage()
    {
        this.InitializeComponent();
        Current = this;
        // ActualTheme is unreliable before the visual tree is loaded.
        Loaded += (_, _) =>
        {
            isDark = ActualTheme == ElementTheme.Dark;
            UpdateThemeIcon();
        };
#if HAS_UNO
        // WinAppSDK has no SystemNavigationManager; on Uno it carries the Android back button.
        Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += (_, args) =>
        {
            if (ContentFrame.Content is SamplePage)
            {
                args.Handled = true;
                NavigateHome();
            }
        };
#endif
    }

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (ContentFrame.Content is null)
        {
            ContentFrame.Navigate(typeof(HomePage));
        }
    }

    public void NavigateToSample(SampleBase sample)
    {
        ContentFrame.Navigate(typeof(SamplePage), sample.Title);
    }

    public void NavigateHome()
    {
        if (ContentFrame.CanGoBack)
        {
            ContentFrame.GoBack();
        }
        else
        {
            ContentFrame.Navigate(typeof(HomePage));
        }
    }

    private void OnHomeClicked(object sender, RoutedEventArgs e) => NavigateHome();

    private void OnThemeToggleClicked(object sender, RoutedEventArgs e)
    {
        // RequestedTheme is unreliable to read when its initial value is Default;
        // track it explicitly and propagate via the page so both the top bar and
        // ContentFrame pick it up.
        isDark = !isDark;
        this.RequestedTheme = isDark ? ElementTheme.Dark : ElementTheme.Light;
        UpdateThemeIcon();
    }

    private void UpdateThemeIcon() =>
        ThemeToggleIcon.Text = isDark ? SampleManager.IconMoonStars : SampleManager.IconSun;
}
