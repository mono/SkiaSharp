using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace SkiaSharp.Tests.Goldens
{
	internal sealed class GoldenRequest
	{
		public GoldenRequest(string profile, string version, string caseName, string architecture, string variant = null,
			bool allowNeutralVariant = false, string minimumVersion = null, bool exactVersion = false)
		{
			GoldenPolicy.ValidateIdentity(profile, caseName, variant, architecture);
			var current = GoldenPolicy.ParseVersion(profile, version, false);
			if (allowNeutralVariant && (!GoldenPolicy.IsBrowserCase(profile, caseName) ||
				(variant != null && !GoldenPolicy.IsBrowserEngine(variant))))
				throw new ArgumentException("Only browser cases permit engine-neutral variant fallback.", nameof(allowNeutralVariant));
			if (minimumVersion != null)
			{
				var minimum = GoldenPolicy.ParseVersion(profile, minimumVersion, false);
				if (current == null || minimum == null || current.Family != minimum.Family || minimum.CompareTo(current) > 0)
					throw new ArgumentException("Minimum version must be numeric, no newer than the runtime, and in the same version family.", nameof(minimumVersion));
			}

			Profile = profile;
			Version = version;
			CaseName = caseName;
			Architecture = architecture;
			Variant = variant;
			AllowNeutralVariant = allowNeutralVariant;
			MinimumVersion = minimumVersion;
			ExactVersion = exactVersion;
		}

		public string Profile { get; }
		public string Version { get; }
		public string CaseName { get; }
		public string Architecture { get; }
		public string Variant { get; }
		public bool AllowNeutralVariant { get; }
		public string MinimumVersion { get; }
		public bool ExactVersion { get; }
	}

	internal readonly struct GoldenKey
	{
		private GoldenKey(string profile, string version, string caseName, string variant, string architecture)
		{
			Profile = profile;
			Version = version;
			CaseName = caseName;
			Variant = variant;
			Architecture = architecture;
		}

		public string Profile { get; }
		public string Version { get; }
		public string CaseName { get; }
		public string Variant { get; }
		public string Architecture { get; }

		public static GoldenKey Parse(string relative)
		{
			if (string.IsNullOrEmpty(relative) || relative.IndexOf('\\') >= 0)
				throw new ArgumentException("A golden key must be a canonical forward-slash relative PNG path.", nameof(relative));
			var parts = relative.Split('/');
			if (parts.Length != 3 || !parts[2].EndsWith(".png", StringComparison.Ordinal))
				throw new ArgumentException("Expected profile/version/case[.variant][.architecture].png.", nameof(relative));
			var tokens = parts[2].Substring(0, parts[2].Length - 4).Split('.');
			if (tokens.Length < 1 || tokens.Length > 3)
				throw new ArgumentException("A golden filename has at most one functional variant followed by one architecture.", nameof(relative));

			string variant = null;
			string architecture = null;
			if (tokens.Length == 2)
			{
				if (GoldenPolicy.IsArchitecture(tokens[1]))
					architecture = tokens[1];
				else
					variant = tokens[1];
			}
			else if (tokens.Length == 3)
			{
				variant = tokens[1];
				architecture = tokens[2];
				if (!GoldenPolicy.IsArchitecture(architecture))
					throw new ArgumentException("Architecture must be the last filename suffix.", nameof(relative));
			}
			GoldenPolicy.ValidateIdentity(parts[0], tokens[0], variant, architecture);
			GoldenPolicy.ParseVersion(parts[0], parts[1], true);
			return new GoldenKey(parts[0], parts[1], tokens[0], variant, architecture);
		}

		public override string ToString()
		{
			if (Profile == null)
				throw new InvalidOperationException("An uninitialized golden key is not a reference identity.");
			return Profile + "/" + Version + "/" + CaseName +
				(Variant == null ? "" : "." + Variant) +
				(Architecture == null ? "" : "." + Architecture) + ".png";
		}
	}

	internal sealed class GoldenAsset
	{
		private readonly Lazy<byte[]> bytes;

		public GoldenAsset(GoldenKey key, string location, Func<byte[]> readBytes)
		{
			key.ToString();
			if (string.IsNullOrEmpty(location))
				throw new ArgumentException("A golden asset needs a source location.", nameof(location));
			if (readBytes == null)
				throw new ArgumentNullException(nameof(readBytes));
			Key = key;
			Location = location;
			bytes = new Lazy<byte[]>(() =>
			{
				var value = readBytes();
				if (value == null)
					throw new InvalidOperationException("The golden provider returned no bytes: " + location);
				return (byte[])value.Clone();
			});
		}

		public GoldenKey Key { get; }
		public string Location { get; }
		public byte[] ReadBytes() => (byte[])bytes.Value.Clone();
	}

	internal sealed class GoldenResolution
	{
		internal GoldenResolution(GoldenRequest request, GoldenAsset asset, List<string> candidates)
		{
			Request = request;
			Asset = asset;
			Candidates = candidates.AsReadOnly();
		}

		public GoldenRequest Request { get; }
		public GoldenAsset Asset { get; }
		public IReadOnlyList<string> Candidates { get; }

		public string Describe()
		{
			var text = new StringBuilder();
			text.Append("Golden request: ").Append(Request.Profile).Append('/').Append(Request.Version)
				.Append('/').Append(Request.CaseName).Append("; variant=").Append(Request.Variant ?? "neutral")
				.Append("; architecture=").Append(Request.Architecture ?? "neutral")
				.Append("; minimum=").Append(Request.MinimumVersion ?? "none").Append("; exact=").Append(Request.ExactVersion);
			text.AppendLine().Append(Asset == null ? "MISSING reference" : "Selected: " + Asset.Key + " at " + Asset.Location);
			text.AppendLine().Append("Attempted (in order):");
			foreach (var candidate in Candidates)
				text.AppendLine().Append("  ").Append(candidate);
			return text.ToString();
		}
	}

	internal sealed class GoldenStoreIndex
	{
		public const string ResourcePrefix = "SkiaSharp.Goldens.v1/";
		private readonly Dictionary<string, GoldenAsset> assets;

		private GoldenStoreIndex(IEnumerable<GoldenAsset> providers)
		{
			assets = new Dictionary<string, GoldenAsset>(StringComparer.Ordinal);
			foreach (var asset in providers)
			{
				if (asset == null)
					throw new ArgumentException("A golden provider cannot be null.", nameof(providers));
				var key = asset.Key.ToString();
				if (assets.TryGetValue(key, out var existing))
				{
					if (!existing.ReadBytes().SequenceEqual(asset.ReadBytes()))
						throw new InvalidOperationException("Conflicting golden bytes for " + key + ": " + existing.Location + " and " + asset.Location);
				}
				else
					assets.Add(key, asset);
			}
		}

		// Also permits pure policy tests without filesystem or graphics dependencies.
		public static GoldenStoreIndex FromAssets(params GoldenAsset[] assets) =>
			new GoldenStoreIndex(assets ?? throw new ArgumentNullException(nameof(assets)));

		public static GoldenStoreIndex FromDirectory(string root)
		{
			if (string.IsNullOrEmpty(root))
				throw new ArgumentException("Supply the v1 golden directory explicitly.", nameof(root));
			var fullRoot = Path.GetFullPath(root);
			if (!Directory.Exists(fullRoot))
				throw new DirectoryNotFoundException("Golden directory does not exist: " + fullRoot);
			var prefix = fullRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
			var found = Directory.EnumerateFiles(fullRoot, "*", SearchOption.AllDirectories)
				.Where(p => p.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
				.OrderBy(p => p, StringComparer.Ordinal)
				.Select(p => new GoldenAsset(GoldenKey.Parse(p.Substring(prefix.Length).Replace(Path.DirectorySeparatorChar, '/')),
					p, () => File.ReadAllBytes(p)));
			return new GoldenStoreIndex(found);
		}

		public static GoldenStoreIndex FromAssembly(Assembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException(nameof(assembly));
			var found = assembly.GetManifestResourceNames()
				.Where(n => n.StartsWith(ResourcePrefix, StringComparison.Ordinal))
				.OrderBy(n => n, StringComparer.Ordinal)
				.Select(n => new GoldenAsset(GoldenKey.Parse(n.Substring(ResourcePrefix.Length)), assembly.FullName + ":" + n, () =>
				{
					using (var stream = assembly.GetManifestResourceStream(n))
					{
						if (stream == null)
							throw new InvalidOperationException("Indexed golden resource cannot be opened: " + n);
						using (var memory = new MemoryStream())
						{
							stream.CopyTo(memory);
							return memory.ToArray();
						}
					}
				}));
			return new GoldenStoreIndex(found);
		}

		public static GoldenStoreIndex Combine(params GoldenStoreIndex[] stores)
		{
			if (stores == null || stores.Any(s => s == null))
				throw new ArgumentException("Golden stores cannot be null.", nameof(stores));
			return new GoldenStoreIndex(stores.SelectMany(s => s.assets.Values));
		}

		public GoldenResolution Resolve(GoldenRequest request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));
			var attempted = new List<string>();
			foreach (var profile in GoldenPolicy.Profiles(request))
			{
				var versions = profile == request.Profile
					? GoldenPolicy.Versions(request, assets.Values.Select(a => a.Key))
					: new[] { "base" };
				foreach (var version in versions)
				{
					foreach (var variant in Variants(request))
					{
						foreach (var architecture in request.Architecture == null ? new string[] { null } : new[] { request.Architecture, null })
						{
							var key = profile + "/" + version + "/" + request.CaseName +
								(variant == null ? "" : "." + variant) + (architecture == null ? "" : "." + architecture) + ".png";
							attempted.Add(key);
							if (assets.TryGetValue(key, out var asset))
								return new GoldenResolution(request, asset, attempted);
						}
					}
				}
			}
			return new GoldenResolution(request, null, attempted);
		}

		private static IEnumerable<string> Variants(GoldenRequest request)
		{
			yield return request.Variant;
			if (request.Variant != null && request.AllowNeutralVariant)
				yield return null;
		}
	}

	internal static class GoldenNames
	{
		public static string Case(string sceneName)
		{
			if (string.IsNullOrEmpty(sceneName))
				throw new ArgumentException("A visual scene needs a name.", nameof(sceneName));
			var name = Regex.Replace(sceneName, "([A-Z]+)([A-Z][a-z])", "$1-$2");
			name = Regex.Replace(name, "([a-z0-9])([A-Z])", "$1-$2").ToLowerInvariant();
			GoldenPolicy.ValidateSlug(name, nameof(sceneName));
			return name.StartsWith("visual-", StringComparison.Ordinal) ? name : "visual-" + name;
		}
	}

	internal static class GoldenPolicy
	{
		private static readonly HashSet<string> profiles = new HashSet<string>(new[]
		{
			"portable", "mac", "windows", "linux", "ios", "android", "maccatalyst", "tvos", "browser",
			"windows-nanoserver", "windows-client", "windows-server", "ubuntu", "alpine", "azurelinux", "debian",
			"iphone-16-pro", "pixel-9",
		}, StringComparer.Ordinal);

		private static readonly HashSet<string> architectures = new HashSet<string>(new[]
		{
			"x64", "arm64", "x86", "arm", "wasm", "loongarch64", "riscv64",
		}, StringComparer.Ordinal);

		private static readonly HashSet<string> backends = new HashSet<string>(new[]
		{
			"raster", "ganesh-gl", "ganesh-metal", "ganesh-vulkan", "ganesh-vulkan-sharpvk", "ganesh-direct3d",
			"graphite-metal", "graphite-vulkan", "graphite-dawn",
		}, StringComparer.Ordinal);

		public static bool IsArchitecture(string value) => value != null && architectures.Contains(value);
		public static bool IsBrowserEngine(string value) => value == "chromium" || value == "firefox" || value == "webkit";
		private static bool IsVisual(string name) => name.StartsWith("visual-", StringComparison.Ordinal);
		private static bool IsNeutral(string profile) => profile == "portable" || profile == "windows" || profile == "linux" || profile == "browser";
		private static bool IsAndroid(string profile) => profile == "android" || profile == "pixel-9";
		private static bool IsDistro(string profile) => profile == "ubuntu" || profile == "alpine" || profile == "azurelinux" || profile == "debian";

		public static bool IsBrowserCase(string profile, string name) =>
			!IsVisual(name) && (profile == "browser" || name.StartsWith("blazor-", StringComparison.Ordinal) ||
				name.StartsWith("browser-", StringComparison.Ordinal) || name.StartsWith("uno-", StringComparison.Ordinal) || name == "web-page");

		public static void ValidateSlug(string value, string parameter)
		{
			if (string.IsNullOrEmpty(value) || !Regex.IsMatch(value, "\\A[a-z][a-z0-9]*(?:-[a-z0-9]+)*\\z") ||
				Regex.IsMatch(value, "\\A(con|prn|aux|nul|com[1-9]|lpt[1-9])\\z"))
				throw new ArgumentException("Expected a lowercase, Windows-safe kebab-case slug: " + value, parameter);
		}

		public static void ValidateIdentity(string profile, string caseName, string variant, string architecture)
		{
			if (profile == null || !profiles.Contains(profile))
				throw new ArgumentException("Unknown golden rendering profile: " + profile, nameof(profile));
			ValidateSlug(caseName, nameof(caseName));
			if (IsArchitecture(caseName) || backends.Contains(caseName) || IsBrowserEngine(caseName) || caseName == "noto-sans")
				throw new ArgumentException("Case names cannot collide with functional or architecture suffixes.", nameof(caseName));
			if (architecture != null && !IsArchitecture(architecture))
				throw new ArgumentException("Unknown or noncanonical rendering architecture: " + architecture, nameof(architecture));
			if (IsVisual(caseName))
			{
				if (variant == null || !backends.Contains(variant))
					throw new ArgumentException("Visual cases require an exact, known backend variant.", nameof(variant));
			}
			else if (variant != null)
			{
				var browserEngine = IsBrowserCase(profile, caseName) && IsBrowserEngine(variant);
				var bundledFont = caseName == "basic-console" && variant == "noto-sans";
				if (!browserEngine && !bundledFont)
					throw new ArgumentException("Unknown or incompatible functional variant: " + variant, nameof(variant));
			}
		}

		public static IEnumerable<string> Profiles(GoldenRequest request)
		{
			yield return request.Profile;
			// Barriers invalidate unversioned sharing, including parent-profile base assets.
			if (request.MinimumVersion != null || request.ExactVersion)
				yield break;
			if (IsDistro(request.Profile))
				yield return "linux";
			if (request.Profile == "windows-client" || request.Profile == "windows-server")
				yield return "windows";
			if (!IsVisual(request.CaseName) || request.Profile == "portable" ||
				request.Profile == "iphone-16-pro" || request.Profile == "pixel-9")
				yield break;
			if (request.Profile == "windows-nanoserver")
				yield return "windows";
			yield return "portable";
		}

		public static GoldenVersion ParseVersion(string profile, string text, bool asset)
		{
			if (text == "base")
				return null;
			if (string.IsNullOrEmpty(text))
				throw new ArgumentException("Supply actual runtime version metadata or explicit 'base'; unknown/latest versions are not references.", nameof(text));
			if (asset && IsNeutral(profile))
				throw new ArgumentException("The version-neutral profile " + profile + " only permits base assets.", nameof(text));

			var parts = text.Split('.');
			int? revision = null;
			if (IsAndroid(profile) && parts[parts.Length - 1].StartsWith("r", StringComparison.Ordinal))
			{
				revision = Number(parts[parts.Length - 1].Substring(1), false);
				if (revision == 0)
					throw new ArgumentException("Android image revisions start at r1.", nameof(text));
				parts = parts.Take(parts.Length - 1).ToArray();
			}
			var values = new int[parts.Length];
			for (var i = 0; i < parts.Length; i++)
				values[i] = Number(parts[i], profile == "ubuntu" && i == 1);
			if (values.Length == 0 || values[0] == 0)
				throw new ArgumentException("Version must identify a positive product release/API/build.", nameof(text));

			var familyLength = 1;
			var maxLength = IsNeutral(profile) ? 4 : 3;
			var minLength = 1;
			if (IsAndroid(profile))
				maxLength = 2;
			else if (profile.StartsWith("windows-", StringComparison.Ordinal))
			{
				maxLength = 2;
				if (values[0] < 10000)
					throw new ArgumentException("Windows versions use a build train (for example 26100), not an NT/release version.", nameof(text));
			}
			else if (profile == "ubuntu" || profile == "alpine" || profile == "azurelinux" || (profile == "mac" && values[0] == 10))
			{
				familyLength = 2;
				minLength = 2;
			}
			else if (profile == "debian")
				maxLength = 2;
			if (values.Length < minLength || values.Length > maxLength)
				throw new ArgumentException("Invalid version precision for " + profile + ": " + text, nameof(text));
			var family = string.Join(".", values.Take(familyLength).Select(n => n.ToString(CultureInfo.InvariantCulture)));
			return new GoldenVersion(text, values, revision, family);
		}

		private static int Number(string token, bool ubuntuMinor)
		{
			if (!Regex.IsMatch(token, ubuntuMinor ? "\\A[0-9]{2}\\z" : "\\A(0|[1-9][0-9]*)\\z") ||
				!int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out var value))
				throw new ArgumentException("Noncanonical or unknown version component: " + token);
			return value;
		}

		public static IEnumerable<string> Versions(GoldenRequest request, IEnumerable<GoldenKey> keys)
		{
			if (request.Version == "base" || IsNeutral(request.Profile))
			{
				if (request.MinimumVersion == null && (!request.ExactVersion || request.Version == "base"))
					yield return "base";
				yield break;
			}
			var current = ParseVersion(request.Profile, request.Version, false);
			var minimum = request.MinimumVersion == null ? null : ParseVersion(request.Profile, request.MinimumVersion, false);
			var tiers = new List<string> { request.Version };
			if (!request.ExactVersion)
			{
				if (current.Revision.HasValue)
					tiers.Add(current.NumericText);
				if (current.Values.Length > 2)
					tiers.Add(current.Prefix(2));
				var familyLength = (request.Profile == "mac" && current.Values[0] == 10) ||
					request.Profile == "ubuntu" || request.Profile == "alpine" || request.Profile == "azurelinux" ? 2 : 1;
				tiers.Add(current.Prefix(familyLength));
			}
			var seen = new HashSet<string>(StringComparer.Ordinal);
			foreach (var tier in tiers)
			{
				if (seen.Add(tier) && (minimum == null || ParseVersion(request.Profile, tier, false).CompareTo(minimum) >= 0))
					yield return tier;
			}
			if (request.ExactVersion)
				yield break;

			// Only indexed versions for this exact case/variant/architecture participate.
			// Matching prefix baselines above always outrank these older captures.
			var older = keys.Where(k => k.Profile == request.Profile && k.CaseName == request.CaseName && k.Version != "base" &&
					(k.Architecture == null || k.Architecture == request.Architecture) &&
					(k.Variant == request.Variant || (request.AllowNeutralVariant && k.Variant == null)))
				.Select(k => k.Version).Distinct(StringComparer.Ordinal)
				.Select(v => ParseVersion(request.Profile, v, true))
				.Where(v => v.Family == current.Family && v.CompareTo(current) < 0 && (minimum == null || v.CompareTo(minimum) >= 0))
				.OrderByDescending(v => v);
			foreach (var version in older)
			{
				if (seen.Add(version.Text))
					yield return version.Text;
			}
			if (minimum == null)
				yield return "base";
		}
	}

	internal sealed class GoldenVersion : IComparable<GoldenVersion>
	{
		public GoldenVersion(string text, int[] values, int? revision, string family)
		{
			Text = text;
			Values = values;
			Revision = revision;
			Family = family;
		}

		public string Text { get; }
		public int[] Values { get; }
		public int? Revision { get; }
		public string Family { get; }
		public string NumericText => Revision.HasValue ? Text.Substring(0, Text.LastIndexOf('.')) : Text;
		public string Prefix(int length) => string.Join(".", NumericText.Split('.').Take(length));

		public int CompareTo(GoldenVersion other)
		{
			if (other == null)
				return 1;
			for (var i = 0; i < Math.Max(Values.Length, other.Values.Length); i++)
			{
				var comparison = (i < Values.Length ? Values[i] : 0).CompareTo(i < other.Values.Length ? other.Values[i] : 0);
				if (comparison != 0)
					return comparison;
			}
			var revision = (Revision ?? -1).CompareTo(other.Revision ?? -1);
			return revision != 0 ? revision : Values.Length.CompareTo(other.Values.Length);
		}
	}
}
