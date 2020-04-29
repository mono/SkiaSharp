using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;

namespace SkiaSharpSample
{
	public class Program : FormsApplication
	{
		public static void Main(string[] args)
		{
			var app = new Program();
			Forms.Init(app, true);
			app.Run(args);
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			LoadApplication(new App());
		}
	}
}
