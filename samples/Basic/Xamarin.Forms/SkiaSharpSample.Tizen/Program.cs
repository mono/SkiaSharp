using System;

namespace SkiaSharpSample
{
	public class Program : global::Xamarin.Forms.Platform.Tizen.FormsApplication
	{
		protected override void OnCreate()
		{
			base.OnCreate();
			
			LoadApplication(new App());
		}

		public static void Main(string[] args)
		{
			var app = new Program();
			global::Xamarin.Forms.Platform.Tizen.Forms.Init(app, true);
			app.Run(args);
		}
	}
}
