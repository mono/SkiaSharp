using System.Text.RegularExpressions;

namespace SkiaSharp.Tests.Shared;

/// <summary>Identifies the runtime and case used to resolve one golden reference.</summary>
public sealed record GoldenCase
{
	public GoldenCase(string profile, string casePath, Version? version = null, string? architecture = null, string? variant = null)
	{
		Profile = NormalizeProfile(profile);
		CasePath = ValidateCase(casePath);
		Version = NormalizeVersion(version);
		if (architecture != null)
		{
			ValidateSegment(architecture);
			if (!IsArchitecture(architecture))
				throw new ArgumentException($"Expected a supported .NET architecture: {architecture}", nameof(architecture));
		}
		if (variant != null)
		{
			ValidateSegment(variant);
			if (IsArchitecture(variant))
				throw new ArgumentException($"Architecture tokens cannot be used as variants: {variant}", nameof(variant));
		}
		Architecture = architecture;
		Variant = variant;
	}

	public string Profile { get; }
	public string CasePath { get; }
	public Version? Version { get; }
	public string? Architecture { get; }
	public string? Variant { get; }
	public string CaptureKey => $"{Profile}/{FileNames().First()}";

	internal IEnumerable<string> FileNames()
	{
		var name = Variant == null ? CasePath : $"{CasePath}.{Variant}";
		if (Architecture != null)
			yield return $"{name}.{Architecture}.png";
		yield return $"{name}.png";
	}

	internal IEnumerable<string> FallbackChain()
	{
		yield return Profile;
		if (Profile == "nanoserver")
			yield return "windows";
		if (Profile != "portable")
			yield return "portable";
	}

	internal static void ValidateSegment(string value, bool caseSegment = false)
	{
		var pattern = caseSegment
			? @"\A[A-Za-z0-9]+(?:-[A-Za-z0-9]+)*\z"
			: @"\A[a-z0-9]+(?:-[a-z0-9]+)*\z";
		if (string.IsNullOrEmpty(value) || !Regex.IsMatch(value, pattern) ||
			Regex.IsMatch(value, @"\A(con|prn|aux|nul|com[1-9]|lpt[1-9])\z", RegexOptions.IgnoreCase))
			throw new ArgumentException($"Expected a safe golden path segment: {value}");
	}

	internal static bool IsArchitecture(string value)
	{
		switch (value)
		{
			case "x86": case "x64": case "arm": case "arm64": case "armv6":
			case "wasm": case "s390x": case "loongarch64": case "ppc64le": case "riscv64":
				return true;
			default:
				return false;
		}
	}

	private static string NormalizeProfile(string profile)
	{
		ValidateSegment(profile);
		return profile == "macos" ? "mac" : profile;
	}

	private static string ValidateCase(string value)
	{
		if (value == null) throw new ArgumentNullException(nameof(value));
		foreach (var segment in value.Split('/'))
			ValidateSegment(segment, caseSegment: true);
		return value;
	}

	private static Version? NormalizeVersion(Version? version) =>
		version == null ? null : version.Build < 0 ? new Version(version.Major, version.Minor, 0) : version;
}
