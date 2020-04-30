using System;

namespace SkiaSharpGenerator
{
	public interface ILogger
	{
		void Log(string message);

		void LogError(string message);

		void LogError(Exception exception);

		void LogWarning(string message);

		void LogVerbose(string message);
	}
}
