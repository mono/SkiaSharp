using System.Security.Cryptography;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Json
{
	internal static class ArtifactHash
	{
		public static string Compute(ReadOnlySpan<byte> bytes) =>
			Convert.ToHexStringLower(SHA256.HashData(bytes));

		public static string ComputeFile(string path) =>
			Compute(File.ReadAllBytes(path));

		public static byte[] ReadAndVerify(string path, string expectedSha256)
		{
			Validate(expectedSha256);
			if (!File.Exists(path))
				throw new ValidationException($"plan file not found: {path}");

			var bytes = File.ReadAllBytes(path);
			var actual = SHA256.HashData(bytes);
			var expected = Convert.FromHexString(expectedSha256);
			if (!CryptographicOperations.FixedTimeEquals(actual, expected))
			{
				throw new ValidationException(
					$"artifact SHA256 '{Convert.ToHexStringLower(actual)}' does not match expected '{expectedSha256}'");
			}
			return bytes;
		}

		private static void Validate(string value)
		{
			if (value is null ||
				value.Length != 64 ||
				value.Any(static character =>
					character is not (>= '0' and <= '9' or >= 'a' and <= 'f')))
			{
				throw new ValidationException(
					"expected artifact SHA256 must be a lowercase 64-hex digest");
			}
		}
	}
}
