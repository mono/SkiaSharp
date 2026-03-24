using Gtk;

namespace SkiaSharpSample
{
	class Program
	{
		public static int Main(string[] args)
		{
			var app = Application.New("com.companyname.skiasharpsample", Gio.ApplicationFlags.FlagsNone);

			app.OnActivate += (sender, e) =>
			{
				var win = new MainWindow((Application)sender);
				win.Show();
			};

			return app.RunWithSynchronizationContext(null);
		}
	}
}
