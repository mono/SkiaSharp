using Microsoft.Maui.Controls;
using SkiaSharpSample.Pages;

namespace SkiaSharpSample
{
	public partial class AppShell : Shell
	{
		public AppShell()
		{
			InitializeComponent();

			Routing.RegisterRoute(nameof(SampleDetailPage), typeof(SampleDetailPage));
		}
	}
}
