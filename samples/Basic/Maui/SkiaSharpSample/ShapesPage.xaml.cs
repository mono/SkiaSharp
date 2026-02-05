using Microsoft.Maui.Controls;

namespace SkiaSharpSample
{
	public partial class ShapesPage : ContentPage
	{
		public ShapesPage()
		{
			InitializeComponent();
		}

		private async void OnBackClicked(object sender, System.EventArgs e)
		{
			await Navigation.PopAsync();
		}
	}
}
