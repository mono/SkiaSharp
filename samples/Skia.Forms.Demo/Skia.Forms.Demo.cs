using System;

using Xamarin.Forms;
using SkiaSharp;

namespace Skia.Forms.Demo
{
	public class App : Application
	{
		public App ()
		{
			ListView listView;
			var listPage = new ContentPage {
				Content = listView = new ListView (),
				Title = "Skia Demo",
			};

			NavigationPage navPage;
			MainPage = navPage = new NavigationPage (listPage) {
				BarBackgroundColor = new Xamarin.Forms.Color (0x34/255.0, 0x98/255.0, 0xdb/255.0),
				BarTextColor = Xamarin.Forms.Color.White,
			};

			listView.ItemsSource = Demos.SamplesForPlatform (Demos.Platform.iOS | Demos.Platform.Android);

			listView.ItemSelected += (sender, e) => {
				if (e.SelectedItem == null) return;
				listView.SelectedItem = null;

				navPage.PushAsync (new ContentPage {
					Content = new SkiaView  (SkiaSharp.Demos.MethodForSample (e.SelectedItem.ToString ())),
				});
			};
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}

