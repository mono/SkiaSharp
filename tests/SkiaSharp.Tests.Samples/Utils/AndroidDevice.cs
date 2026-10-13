using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed record AndroidDevice(
	string Name,
	string Version,
	int Api,
	string Serial,
	int PageSize,
	string Model,
	string ImagePrefix)
{
	// Local runs use one profile; CI supplies a complete profile for each device job.
	internal static AndroidDevice Selected
	{
		get
		{
			var json = Environment.GetEnvironmentVariable("SAMPLE_TEST_ANDROID_DEVICE");
			var device = json is null
				? new AndroidDevice(
					"Pixel_API_37", "17", 37, "emulator-5560", 16384, "pixel_9",
					"system-images;android-37.1;google_apis_ps16k")
				: JsonSerializer.Deserialize<AndroidDevice>(json, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
					UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
				}) ?? throw new JsonException("The Android device profile must be a JSON object.");
			device.Validate();
			return device;
		}
	}

	internal int Validate()
	{
		if (string.IsNullOrWhiteSpace(Name) || !Regex.IsMatch(Name, @"^[A-Za-z0-9_.-]+$"))
			throw new ArgumentException(
				"Declare the exact AVD name, not a path or wildcard.", nameof(Name));

		if (string.IsNullOrWhiteSpace(Version) || !Regex.IsMatch(Version, @"^[1-9][0-9]*(\.[0-9]+)*$") || Api <= 0)
			throw new ArgumentException(
				"Declare the exact Android release and a positive API level.", nameof(Version));

		if (PageSize is < 4096 or > 65536 || (PageSize & (PageSize - 1)) != 0)
			throw new ArgumentOutOfRangeException(
				nameof(PageSize), "Declare the expected power-of-two page size in bytes.");

		if (string.IsNullOrWhiteSpace(Model) || !Regex.IsMatch(Model, @"^[A-Za-z0-9_. -]+$"))
			throw new ArgumentException(
				"Declare one exact installed device model.", nameof(Model));

		if (string.IsNullOrWhiteSpace(ImagePrefix) ||
			!Regex.IsMatch(ImagePrefix, $@"^system-images;android-{Api}(?:\.[0-9]+)?;[A-Za-z0-9_]+$"))
			throw new ArgumentException(
				"Declare one exact installed image prefix matching the API, without an ABI.",
				nameof(ImagePrefix));

		var selected = Regex.Match(Serial ?? "", @"^emulator-([0-9]+)$");
		if (!selected.Success || !int.TryParse(selected.Groups[1].Value, out var port) ||
			port is < 5554 or > 5682 || port % 2 != 0 || Serial != $"emulator-{port}")
			throw new ArgumentException(
				"Declare an exact emulator serial with an even console port from 5554 through 5682.",
				nameof(Serial));

		return port;
	}
}
