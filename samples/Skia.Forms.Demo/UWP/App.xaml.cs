using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Skia.Forms.Demo.UWP
{
	sealed partial class App : Application
	{
		public App ()
		{
			this.InitializeComponent ();
			this.Suspending += OnSuspending;
		}

		protected override void OnLaunched (LaunchActivatedEventArgs e)
		{
#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached) {
				this.DebugSettings.EnableFrameRateCounter = true;
			}
#endif

			Frame rootFrame = Window.Current.Content as Frame;

			if (rootFrame == null) {
				rootFrame = new Frame ();
				rootFrame.NavigationFailed += OnNavigationFailed;

				Xamarin.Forms.Forms.Init (e);

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated) {
					//TODO: Load state from previously suspended application
				}

				Window.Current.Content = rootFrame;
			}

			if (rootFrame.Content == null) {
				rootFrame.Navigate (typeof(MainPage), e.Arguments);
			}
			Window.Current.Activate ();
		}

		private void OnNavigationFailed (object sender, NavigationFailedEventArgs e)
		{
			throw new Exception ("Failed to load Page " + e.SourcePageType.FullName);
		}

		private void OnSuspending (object sender, SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral ();
			//TODO: Save application state and stop any background activity
			deferral.Complete ();
		}
	}
}
