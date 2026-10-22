#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SkiaSharp.Tests.Goldens
{
	internal enum GoldenVersionScope { Unversioned, Major, Minor }

	internal sealed class GoldenRequest
	{
		public GoldenRequest(string profile, string version, string caseName, string? architecture, string? variant = null,
			GoldenVersionScope versionScope = GoldenVersionScope.Major, IReadOnlyList<string>? fallbackProfiles = null,
			bool allowNeutralVariant = false)
		{
			GoldenReference.ValidateSegment(profile);
			GoldenReference.ValidateSegment(caseName);
			if (architecture != null) GoldenReference.ValidateSegment(architecture);
			if (variant != null) GoldenReference.ValidateSegment(variant);
			GoldenReference.ValidateVersion(version);
			if (versionScope < GoldenVersionScope.Unversioned || versionScope > GoldenVersionScope.Minor)
				throw new ArgumentOutOfRangeException(nameof(versionScope));
			Profile = profile;
			Version = version;
			CaseName = caseName;
			Architecture = architecture;
			Variant = variant;
			VersionScope = versionScope;
			FallbackProfiles = (fallbackProfiles ?? Array.Empty<string>()).ToList().AsReadOnly();
			foreach (var parent in FallbackProfiles) GoldenReference.ValidateSegment(parent);
			AllowNeutralVariant = allowNeutralVariant;
		}

		public string Profile { get; }
		public string Version { get; }
		public string CaseName { get; }
		public string? Architecture { get; }
		public string? Variant { get; }
		public GoldenVersionScope VersionScope { get; }
		public IReadOnlyList<string> FallbackProfiles { get; }
		public bool AllowNeutralVariant { get; }
	}

	internal sealed class GoldenResolution
	{
		public GoldenResolution(GoldenRequest request, string? key, string? path, IReadOnlyList<string> candidates)
		{
			Request = request;
			Key = key;
			Path = path;
			Candidates = candidates.ToList().AsReadOnly();
		}

		public GoldenRequest Request { get; }
		public string? Key { get; }
		public string? Path { get; }
		public IReadOnlyList<string> Candidates { get; }

		public string Describe() =>
			"Golden request: " + Request.Profile + "/" + Request.Version + "/" + Request.CaseName + Environment.NewLine +
			(Key == null ? "MISSING reference" : "Selected: " + Key + (Path == null ? "" : " at " + Path)) +
			Environment.NewLine + "Attempted (in order):" + Environment.NewLine + "  " +
			string.Join(Environment.NewLine + "  ", Candidates);
	}

	internal static class GoldenReference
	{
		public static GoldenResolution Resolve(IEnumerable<string> keys, GoldenRequest request)
		{
			if (keys == null) throw new ArgumentNullException(nameof(keys));
			if (request == null) throw new ArgumentNullException(nameof(request));
			var available = new HashSet<string>(StringComparer.Ordinal);
			foreach (var key in keys)
			{
				if (key == null) throw new ArgumentException("Golden keys cannot be null.", nameof(keys));
				if (!key.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;
				ValidateKey(key);
				available.Add(key);
			}
			var files = FileNames(request).Distinct(StringComparer.Ordinal).ToArray();
			var attempted = new List<string>();
			foreach (var profile in new[] { request.Profile }.Concat(request.FallbackProfiles).Distinct(StringComparer.Ordinal))
			{
				var versions = profile == request.Profile ? Versions(request, available, files) : new[] { "base" };
				foreach (var version in versions)
				foreach (var file in files)
				{
					var key = profile + "/" + version + "/" + file;
					attempted.Add(key);
					if (available.Contains(key)) return new GoldenResolution(request, key, null, attempted);
				}
			}
			return new GoldenResolution(request, null, null, attempted);
		}

		public static GoldenResolution Resolve(string root, GoldenRequest request)
		{
			var result = Resolve(Keys(root), request);
			var path = result.Key == null ? null : Path.Combine(Path.GetFullPath(root), result.Key.Replace('/', Path.DirectorySeparatorChar));
			return new GoldenResolution(request, result.Key, path, result.Candidates);
		}

		public static IEnumerable<string> Keys(string root)
		{
			if (string.IsNullOrEmpty(root)) throw new ArgumentException("Supply a golden root directory.", nameof(root));
			var fullRoot = Path.GetFullPath(root);
			if (!Directory.Exists(fullRoot)) throw new DirectoryNotFoundException("Golden directory does not exist: " + fullRoot);
			var prefix = fullRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
			var keys = Directory.EnumerateFiles(fullRoot, "*", SearchOption.AllDirectories)
				.Where(p => p.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
				.Select(p => p.Substring(prefix.Length).Replace(Path.DirectorySeparatorChar, '/'))
				.OrderBy(k => k, StringComparer.Ordinal).ToArray();
			foreach (var key in keys) ValidateKey(key);
			return keys;
		}

		internal static void ValidateSegment(string value)
		{
			if (string.IsNullOrEmpty(value) || !Regex.IsMatch(value, @"\A[a-z0-9]+(?:-[a-z0-9]+)*\z") ||
				Regex.IsMatch(value, @"\A(con|prn|aux|nul|com[1-9]|lpt[1-9])\z"))
				throw new ArgumentException("Expected a safe lowercase alphanumeric/kebab filename segment: " + value);
		}

		internal static void ValidateVersion(string value)
		{
			if (value == "base") return;
			if (value == null || !Regex.IsMatch(value, @"\A[1-9][0-9]*(?:\.[0-9]+)*(?:\.r[1-9][0-9]*)?\z") ||
				value.Split('.').Any(p => !int.TryParse(p[0] == 'r' ? p.Substring(1) : p,
					NumberStyles.None, CultureInfo.InvariantCulture, out _)))
				throw new ArgumentException("Expected an actual numeric version, optional .rN image revision, or base: " + value);
		}

		private static void ValidateKey(string key)
		{
			var parts = key.Split('/');
			if (parts.Length != 3 || !key.EndsWith(".png", StringComparison.Ordinal))
				throw new ArgumentException("Expected profile/version/case[.variant][.architecture].png: " + key);
			ValidateSegment(parts[0]);
			ValidateVersion(parts[1]);
			var names = parts[2].Substring(0, parts[2].Length - 4).Split('.');
			if (names.Length > 3) throw new ArgumentException("Too many golden filename suffixes: " + key);
			foreach (var name in names) ValidateSegment(name);
		}

		private static IEnumerable<string> FileNames(GoldenRequest request)
		{
			var variants = request.Variant != null && request.AllowNeutralVariant ? new[] { request.Variant, null } : new[] { request.Variant };
			foreach (var variant in variants)
			{
				var name = request.CaseName + (variant == null ? "" : "." + variant);
				if (request.Architecture != null) yield return name + "." + request.Architecture + ".png";
				yield return name + ".png";
			}
		}

		private static IEnumerable<string> Versions(GoldenRequest request, IEnumerable<string> keys, string[] files)
		{
			if (request.VersionScope == GoldenVersionScope.Unversioned || request.Version == "base") return new[] { "base" };
			var numeric = request.Version.Split('.').TakeWhile(p => p[0] != 'r').ToArray();
			var familyLength = request.VersionScope == GoldenVersionScope.Minor ? 2 : 1;
			var family = numeric.Take(familyLength).Select(Number).ToArray();
			var tiers = new List<string> { request.Version, string.Join(".", numeric) };
			if (numeric.Length >= 3) tiers.Add(string.Join(".", numeric.Take(2)));
			tiers.Add(string.Join(".", numeric.Take(familyLength)));
			var older = keys.Select(k => k.Split('/'))
				.Where(p => p[0] == request.Profile && p[1] != "base" && files.Contains(p[2]))
				.Select(p => p[1]).Distinct(StringComparer.Ordinal)
				.Where(v => v.Split('.').TakeWhile(p => p[0] != 'r').Take(familyLength).Select(Number).SequenceEqual(family) &&
					CompareVersions(v, request.Version) < 0)
				.OrderByDescending(v => v, Comparer<string>.Create(CompareVersions)).ThenBy(v => v, StringComparer.Ordinal);
			return tiers.Concat(older).Concat(new[] { "base" }).Distinct(StringComparer.Ordinal);
		}

		private static int Number(string value) => int.Parse(value, CultureInfo.InvariantCulture);

		private static int CompareVersions(string left, string right)
		{
			var a = left.Split('.');
			var b = right.Split('.');
			var ac = a.Length - (a[a.Length - 1][0] == 'r' ? 1 : 0);
			var bc = b.Length - (b[b.Length - 1][0] == 'r' ? 1 : 0);
			for (var i = 0; i < Math.Max(ac, bc); i++)
			{
				var comparison = (i < ac ? Number(a[i]) : 0).CompareTo(i < bc ? Number(b[i]) : 0);
				if (comparison != 0) return comparison;
			}
			var revision = (ac < a.Length ? Number(a[ac].Substring(1)) : -1)
				.CompareTo(bc < b.Length ? Number(b[bc].Substring(1)) : -1);
			return revision != 0 ? revision : ac.CompareTo(bc);
		}
	}

	internal static class GoldenNames
	{
		public static string Case(string sceneName)
		{
			if (string.IsNullOrEmpty(sceneName)) throw new ArgumentException("A visual scene needs a name.", nameof(sceneName));
			var name = Regex.Replace(sceneName, "([A-Z]+)([A-Z][a-z])", "$1-$2");
			name = Regex.Replace(name, "([a-z0-9])([A-Z])", "$1-$2").ToLowerInvariant();
			GoldenReference.ValidateSegment(name);
			return name.StartsWith("visual-", StringComparison.Ordinal) ? name : "visual-" + name;
		}
	}
}
