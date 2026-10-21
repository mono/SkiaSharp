#nullable enable
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests.Goldens
{
	internal sealed class GoldenEnvironment
	{
		public GoldenEnvironment(string profile, string version, string architecture, string description)
		{
			Profile = profile;
			Version = version;
			Architecture = architecture;
			Description = description;
		}

		public string Profile { get; }
		public string Version { get; }
		public string Architecture { get; }
		public string Description { get; }

		public GoldenRequest Request(string caseName, string? variant = null) =>
			new GoldenRequest(Profile, Version, caseName, Architecture, variant);

		public static GoldenEnvironment Current()
		{
			var architecture = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				using var process = Process.Start(new ProcessStartInfo("sw_vers", "-productVersion")
				{
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				}) ?? throw new InvalidOperationException("Cannot start the macOS product version probe.");
				var version = process.StandardOutput.ReadToEnd().Trim();
				var error = process.StandardError.ReadToEnd();
				if (!process.WaitForExit(10000) || process.ExitCode != 0 || !System.Version.TryParse(version, out _))
					throw new InvalidOperationException("Cannot read the macOS product version: " + error);
				return new GoldenEnvironment("mac", version, architecture, RuntimeInformation.OSDescription);
			}
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return Windows(architecture);
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				return Linux(File.ReadAllText("/etc/os-release"), architecture);
			throw new PlatformNotSupportedException("No host golden environment for " + RuntimeInformation.OSDescription);
		}

		public static GoldenEnvironment Linux(string osRelease, string architecture)
		{
			string? id = null, version = null;
			foreach (var line in osRelease.Split('\n'))
			{
				if (line.StartsWith("ID=", StringComparison.Ordinal))
					id = line.Substring(3).Trim().Trim('"');
				if (line.StartsWith("VERSION_ID=", StringComparison.Ordinal))
					version = line.Substring(11).Trim().Trim('"');
			}
			if (string.IsNullOrEmpty(id))
				throw new InvalidOperationException("The rendering environment has no os-release ID.");
			var profile = id == "ubuntu" || id == "debian" || id == "alpine" || id == "azurelinux" ? id : "linux";
			if (profile != "linux" && !System.Version.TryParse(version != null && version.IndexOf('.') < 0 ? version + ".0" : version, out _))
				throw new InvalidOperationException("The rendering environment has an invalid VERSION_ID: " + version);
			return new GoldenEnvironment(profile, profile == "linux" ? "base" : version ?? throw new InvalidOperationException("VERSION_ID is required."), architecture,
				"os-release ID=" + id + "; VERSION_ID=" + version +
				(profile == "linux" ? "; only explicitly portable Linux baselines are eligible" : ""));
		}

#if NET5_0_OR_GREATER
		[System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
		private static GoldenEnvironment Windows(string architecture)
		{
			using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
			if (key == null)
				throw new InvalidOperationException("Cannot read the Windows rendering environment.");
			var kind = key.GetValue("InstallationType") as string;
			var build = key.GetValue("CurrentBuildNumber") as string;
			var revision = key.GetValue("UBR");
			if (kind == null || !int.TryParse(build, NumberStyles.None, CultureInfo.InvariantCulture, out _) || revision == null)
				throw new InvalidOperationException("Windows build, servicing revision and installation kind are required.");
			var profile = kind == "Nano Server" ? "windows-nanoserver" :
				kind == "Client" ? "windows-client" : "windows-server";
			return new GoldenEnvironment(profile, build + "." + Convert.ToInt32(revision, CultureInfo.InvariantCulture),
				architecture, kind + "; " + RuntimeInformation.OSDescription);
		}
	}
}
