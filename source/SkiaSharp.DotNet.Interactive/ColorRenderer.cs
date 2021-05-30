using Microsoft.AspNetCore.Html;

using static Microsoft.DotNet.Interactive.Formatting.PocketViewTags;

namespace SkiaSharp.DotNet.Interactive
{
	public static class ColorRenderer
	{
		public static IHtmlContent Render(this SKColor color)
		{
			var colorString = color.Alpha == 255
				? $"rgb({color.Red},{color.Green},{color.Blue})"
				: $"rgba({color.Red},{color.Green},{color.Blue},{color.Alpha / 255.0:0.0##})";

			return GetHtml(colorString);
		}

		public static IHtmlContent Render(this SKColorF color) =>
			((SKColor)color).Render();

		private static IHtmlContent GetHtml(string colorString) =>
			div(
				span[style:
						$"width: 2em; " +
						$"background: {colorString}; " +
						$"display: inline-block; " +
						$"border: 1px solid black; "](
					new HtmlString("&nbsp;")),
				span(
					new HtmlString("&nbsp;"),
					colorString));
	}
}
