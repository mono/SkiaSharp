using System;
using Microsoft.UI.Xaml;

namespace SkiaSharpSample.Wasm
{
	public class Program
	{
		static int Main(string[] args)
		{
			Microsoft.UI.Xaml.Application.Start(_ => new App());

			return 0;
		}
	}
}
