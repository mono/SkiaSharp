using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace SkiaSharp.Tests
{
	internal static class VulkanTestEnvironment
	{
		private const string RequireVulkanVariable = "SKIASHARP_REQUIRE_VULKAN";

		public static bool IsVulkanRequired =>
			string.Equals(
				Environment.GetEnvironmentVariable(RequireVulkanVariable),
				"1",
				StringComparison.Ordinal);

		[DoesNotReturn]
		public static void SkipOrThrow(string message, Exception exception)
		{
			if (IsVulkanRequired)
				throw new InvalidOperationException($"{message} Vulkan is required by this test run.", exception);

			Assert.Skip(message);
		}
	}
}
