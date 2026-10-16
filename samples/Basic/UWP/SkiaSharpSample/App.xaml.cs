using System;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SkiaSharpSample
{
	public sealed partial class App : Application
	{
		public App()
		{
			InitializeComponent();
		}

		protected override void OnLaunched(LaunchActivatedEventArgs e)
		{
			if (!(Window.Current.Content is Frame rootFrame))
			{
				rootFrame = new Frame();
				rootFrame.NavigationFailed += OnNavigationFailed;
				Window.Current.Content = rootFrame;
			}

			if (!e.PrelaunchActivated)
			{
				if (rootFrame.Content == null)
					rootFrame.Navigate(typeof(MainPage), e.Arguments);

				Window.Current.Activate();
			}
		}

		private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new Exception($"Failed to load page {e.SourcePageType.FullName}");
		}
	}
}
