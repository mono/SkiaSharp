using Tizen.Applications;
using Uno.UI.Runtime.Skia;

namespace SkiaSharpSample.Skia.Tizen
{
	class Program
{
	static void Main(string[] args)
	{
		var host = new TizenHost(() => new SkiaSharpSample.App(), args);
		host.Run();
	}
}
}
