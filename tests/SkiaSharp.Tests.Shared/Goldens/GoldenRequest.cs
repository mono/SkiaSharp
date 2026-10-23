using System;
using System.Collections.Generic;

namespace SkiaSharp.Tests.Shared;

/// <summary>Identifies one golden case and the runtime environment selecting its reference.</summary>
public sealed class GoldenRequest
{
	public GoldenRequest(string platform, string casePath, Version? version = null, string? architecture = null, string? variant = null)
	{
		Platform = GoldenPlatform.Normalize(platform);
		Version = version == null ? null :
			version.Build < 0 ? new Version(version.Major, version.Minor, 0) : version;
		GoldenPaths.ValidateCase(casePath);
		if (architecture != null)
		{
			GoldenPaths.ValidateSegment(architecture);
			if (!GoldenPaths.IsArchitecture(architecture))
				throw new ArgumentException("Expected a supported .NET architecture: " + architecture, nameof(architecture));
		}
		if (variant != null)
		{
			GoldenPaths.ValidateSegment(variant);
			if (GoldenPaths.IsArchitecture(variant))
				throw new ArgumentException("Architecture tokens cannot be used as variants: " + variant, nameof(variant));
		}
		CasePath = casePath;
		Architecture = architecture;
		Variant = variant;
	}

	public string Platform { get; }
	public Version? Version { get; }
	public string CasePath { get; }
	public string? Architecture { get; }
	public string? Variant { get; }
	public string CaptureKey => Platform + "/" + FileNames().First();

	internal IEnumerable<string> FileNames()
	{
		var name = CasePath + (Variant == null ? "" : "." + Variant);
		if (Architecture != null) yield return name + "." + Architecture + ".png";
		yield return name + ".png";
	}
}
