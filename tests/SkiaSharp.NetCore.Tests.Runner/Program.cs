using System;
using System.Reflection;
using NUnit.Common;
using NUnitLite;

using SkiaSharp.Tests;

namespace SkiaSharp.NetCore.Tests.Runner
{
	class Program
	{
		static int Main(string[] args)
		{
			return new AutoRun(typeof(SKTest).GetTypeInfo().Assembly).Execute(args, new ExtendedTextWrapper(Console.Out), Console.In);
		}
	}
}
