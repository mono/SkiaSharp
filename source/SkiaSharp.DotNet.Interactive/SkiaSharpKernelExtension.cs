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
			Formatter.Register<SKColor>((color, writer) => writer.Write(color.Render()), "text/html");
			Formatter.Register<SKColorF>((color, writer) => writer.Write(color.Render()), "text/html");

			// "images"
			Formatter.Register<SKBitmap>((bmp, writer) => writer.Write(bmp.Render()), "text/html");
			Formatter.Register<SKPixmap>((pix, writer) => writer.Write(pix.Render()), "text/html");
			Formatter.Register<SKPicture>((pic, writer) => writer.Write(pic.Render()), "text/html");
			Formatter.Register<SKSurface>((surface, writer) => writer.Write(surface.Render()), "text/html");
			Formatter.Register<SKImage>((img, writer) => writer.Write(img.Render()), "text/html");

			// TODO: colorspaces and other things

			return Task.CompletedTask;
		}
	}
}
