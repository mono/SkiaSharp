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

			listView.ItemsSource = new [] {"Xamagon", "Text Sample"};

			listView.ItemSelected += (sender, e) => {
				if (e.SelectedItem == null) return;
				listView.SelectedItem = null;

				navPage.PushAsync (new ContentPage {
					Content = new SkiaView  (GetDrawHandler (e.SelectedItem.ToString ())),
				});
			};
		}

		Action <SKCanvas, int, int> GetDrawHandler (string selectedItem)
		{
			switch (selectedItem) {

			case "Xamagon":
				return DrawHelpers.DrawXamagon;
			case "Text Sample":
				return DrawHelpers.TextSample;
			}

			throw new NotImplementedException ();
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

