using System;

namespace SkiaSharpGenerator
{
	public class ConsoleLogger : ILogger
	{
		public bool Verbose { get; set; }

		public void Log(string message)
		{
			Console.WriteLine(message);
		}

		public void LogError(string message)
		{
			Console.Error.WriteLine("ERROR: " + message);
		}

		public void LogError(Exception exception)
		{
			Console.Error.WriteLine(exception);
		}

		public void LogWarning(string message)
		{
			Console.WriteLine("WARNING: " + message);
		}

		public void LogVerbose(string message)
		{
			if (Verbose)
				Console.WriteLine("VERBOSE: " + message);
		}
	}
}
