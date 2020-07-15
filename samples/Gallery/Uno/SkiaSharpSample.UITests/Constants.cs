using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno.UITest.Helpers.Queries;

namespace SkiaSharpSample.UITests
{
	public class Constants
	{
		public readonly static string WebAssemblyDefaultUri = "http://localhost:50720/";
		public readonly static string iOSAppName = "uno.platform.uitestsample";
		public readonly static string AndroidAppName = "uno.platform.uitestsample";
		public readonly static string iOSDeviceNameOrId = "iPad Pro (12.9-inch) (3rd generation)";

		public readonly static Platform CurrentPlatform = Platform.Browser;
	}
}
