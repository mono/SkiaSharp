using System;
using Gtk;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;

namespace SkiaSharpSample.GTK
{
	class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			GtkOpenGL.Init();

			Gtk.Application.Init();
			Forms.Init();

			var app = new App();
			var window = new FormsWindow();
			window.LoadApplication(app);
			window.SetApplicationTitle("SkiaSharpSample.GTK");
			window.Show();

			Gtk.Application.Run();
		}
	}
}
