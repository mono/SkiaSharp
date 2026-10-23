#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SkiaSharp.Tests.Shared
{
	public sealed class GoldenStore
	{
		public const string ResourcePrefix = "SkiaSharp.Goldens/";
		private readonly string[] diskRoots;
		private readonly Assembly[] resourceAssemblies;

		public GoldenStore(IEnumerable<string> diskRoots, IEnumerable<Assembly>? resourceAssemblies = null)
		{
			this.diskRoots = (diskRoots ?? throw new ArgumentNullException(nameof(diskRoots)))
				.Select(Path.GetFullPath).Distinct(StringComparer.Ordinal).ToArray();
			this.resourceAssemblies = (resourceAssemblies ?? Array.Empty<Assembly>()).Distinct().ToArray();
		}

		public GoldenResolution Resolve(GoldenRequest request)
		{
			if (request == null) throw new ArgumentNullException(nameof(request));
			var sources = new Dictionary<string, (string Location, Func<byte[]> Read)>(StringComparer.Ordinal);
			foreach (var root in diskRoots)
			{
				if (!Directory.Exists(root)) continue;
				var prefix = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
				foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
				{
					if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;
					var key = path.Substring(prefix.Length).Replace(Path.DirectorySeparatorChar, '/');
					Add(key, path, () => File.ReadAllBytes(path));
				}
			}
			foreach (var assembly in resourceAssemblies)
			foreach (var resource in assembly.GetManifestResourceNames().OrderBy(n => n, StringComparer.Ordinal))
			{
				if (!resource.StartsWith(ResourcePrefix, StringComparison.Ordinal) ||
					!resource.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;
				Add(resource.Substring(ResourcePrefix.Length), assembly.GetName().Name + "!" + resource, () =>
				{
					using var stream = assembly.GetManifestResourceStream(resource)
						?? throw new InvalidOperationException("Missing embedded golden: " + resource);
					using var memory = new MemoryStream();
					stream.CopyTo(memory);
					return memory.ToArray();
				});
			}

			var attempted = new List<string>();
			foreach (var profile in GoldenPlatform.Profiles(request.Profile))
			{
				var versions = sources.Keys.Select(k => k.Split('/')).Where(p => p[0] == profile)
					.Select(p => Version.Parse(p[1])).Where(v => v <= request.Version)
					.Concat(new[] { new Version(0, 0, 0) }).Distinct().OrderByDescending(v => v);
				foreach (var version in new[] { request.Version }.Concat(versions).Distinct())
				foreach (var file in request.FileNames())
				{
					var key = profile + "/" + version + "/" + file;
					attempted.Add(key);
					if (sources.TryGetValue(key, out var source))
						return new GoldenResolution(request, key, source.Location, attempted, source.Read);
				}
			}
			return new GoldenResolution(request, null, null, attempted, null);

			void Add(string key, string location, Func<byte[]> read)
			{
				GoldenPaths.ValidateKey(key);
				if (!sources.ContainsKey(key)) sources.Add(key, (location, read));
			}
		}
	}

	public sealed class GoldenResolution
	{
		private readonly Func<byte[]>? read;

		internal GoldenResolution(GoldenRequest request, string? key, string? location, IEnumerable<string> candidates, Func<byte[]>? read)
		{
			Request = request;
			Key = key;
			Location = location;
			Candidates = candidates.ToList().AsReadOnly();
			this.read = read;
		}

		public GoldenRequest Request { get; }
		public string? Key { get; }
		public string? Location { get; }
		public IReadOnlyList<string> Candidates { get; }

		public byte[] ReadBytes() => read == null ? throw new InvalidOperationException(Describe()) : read();

		public string Describe() =>
			"Golden request: " + Request.Profile + "/" + Request.Version + "/" + Request.CasePath + Environment.NewLine +
			(Key == null ? "MISSING reference" : "Selected: " + Key + " at " + Location) +
			Environment.NewLine + "Attempted (in order):" + Environment.NewLine + "  " +
			string.Join(Environment.NewLine + "  ", Candidates);
	}
}
