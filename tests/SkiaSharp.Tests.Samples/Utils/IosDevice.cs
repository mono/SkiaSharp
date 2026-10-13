using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed record IosDevice(string Name, string Version)
{
	internal static IosDevice Selected
	{
		get
		{
			var json = Environment.GetEnvironmentVariable("SAMPLE_TEST_IOS_DEVICE");
			var device = json is null
				? new IosDevice("iPhone 16 Pro", "26.2")
				: JsonSerializer.Deserialize<IosDevice>(json, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
					UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
				}) ?? throw new JsonException("The iOS device profile must be a JSON object.");
			device.Validate();
			return device;
		}
	}

	internal void Validate()
	{
		if (string.IsNullOrWhiteSpace(Name) || Name != Name.Trim() || Name.Any(char.IsControl))
			throw new ArgumentException(
				"Declare the exact available simulator name.", nameof(Name));

		if (string.IsNullOrWhiteSpace(Version) || !Regex.IsMatch(Version, @"^[1-9][0-9]*(\.[0-9]+){1,2}$"))
			throw new ArgumentException(
				"Declare an exact iOS runtime version, including major and minor components.",
				nameof(Version));
	}
}
