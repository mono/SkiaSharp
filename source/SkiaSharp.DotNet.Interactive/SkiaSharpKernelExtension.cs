using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Formatting;

namespace SkiaSharp.DotNet.Interactive
{
	public class SkiaSharpKernelExtension : IKernelExtension
	{
		public Task OnLoadAsync(Kernel kernel)
		{
			// colors
			Formatter.Register<SKColor>((date, writer) => writer.Write(date.RenderColor()), "text/html");
			Formatter.Register<SKColorF>((date, writer) => writer.Write(date.RenderColor()), "text/html");

			// TODO: bitmap, image, pixmap, picture, surface, canvas, etc...

			return Task.CompletedTask;
		}
	}
}
