using Xamarin.Forms.Platform.Tizen;

namespace SkiaSharpSample.Platform
{
	class Program : FormsApplication
	{
		protected override void OnCreate()
		{
			base.OnCreate();

			SamplesInitializer.Init();

			LoadApplication(new App());
		}

		static void Main(string[] args)
		{
			var app = new Program();

			Forms.Init(app);

			app.Run(args);
		}
	}
}
