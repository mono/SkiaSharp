using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;

namespace SkiaSharpSample.Platform
{
	public class Program : FormsApplication
	{
		protected override void OnCreate()
		{
			base.OnCreate();

			SamplesInitializer.Init();

			LoadApplication(new App());
		}

		public static void Main(string[] args)
		{
			var app = new Program();

			Forms.Init(app, true);

			app.Run(args);
		}
	}
}
