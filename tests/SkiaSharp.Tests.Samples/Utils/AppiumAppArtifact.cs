using System.Formats.Tar;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace SkiaSharp.Tests.Samples.Utils;

internal enum AppiumMode { All, Build, Run }

internal sealed record AppiumAppSpec(
	string Platform, string Architecture, string Folder, string Project, string Framework, string RuntimeIdentifier)
{
	internal string Key => $"{Platform}/{Architecture}/{Folder}";

	internal void Validate()
	{
		if (Platform is not ("android" or "ios") || Architecture is not ("x64" or "arm64") ||
			Framework != (Platform == "android" ? "net10.0-android" : "net10.0-ios") ||
			RuntimeIdentifier != (Platform == "android" ? $"android-{Architecture}" : $"iossimulator-{Architecture}"))
			throw new InvalidOperationException($"Unsupported Appium artifact target: {this}");
		AppiumAppArtifact.RelativePath(Folder);
		AppiumAppArtifact.RelativePath(Project);
	}
}

internal sealed record AppiumPackage(string Id, string Version, string Sha256);
internal sealed record AppiumAppManifest(
	int Schema, AppiumAppSpec App, string ApplicationId, string ArtifactRelativePath,
	string? BundleName, string Sha256, AppiumPackage[] Packages);

// This object deliberately owns no device, server, or copied sample workspace.
internal sealed record AppiumPreparedApp(
	AppiumMode Mode, string Directory, AppiumAppManifest Manifest,
	[property: JsonIgnore] Func<SampleWorkspace, string, AppiumRunningApp> SessionFactory)
{
	internal AppiumRunningApp CreateRunningApp(SampleWorkspace workspace)
	{
		var artifact = AppiumAppArtifact.Materialize(Directory, Manifest, workspace.Root);
		var app = SessionFactory(workspace, artifact);
		app.ExpectedApplicationId = Manifest.ApplicationId;
		return app;
	}
}

internal static class AppiumAppArtifact
{
	internal static AppiumMode ParseMode(string? value) => value switch
	{
		null or "all" => AppiumMode.All,
		"build" => AppiumMode.Build,
		"run" => AppiumMode.Run,
		_ => throw new ArgumentException($"Invalid TEST_APPIUM_MODE '{value}'; expected all, build or run.")
	};

	internal static string RelativePath(string value)
	{
		if (string.IsNullOrWhiteSpace(value) || value.Contains('\\') || value.Contains(':') ||
			Path.IsPathRooted(value) ||
			value.Split('/').Any(part => part is "" or "." or ".." || part.Any(char.IsControl)))
			throw new InvalidDataException($"Expected a safe artifact-relative path: {value}");
		return value;
	}

	internal static string DirectoryFor(string root, AppiumAppSpec spec)
	{
		spec.Validate();
		return Path.Combine(Path.GetFullPath(root), spec.Key);
	}

	internal static string Hash(string file)
	{
		using var stream = File.OpenRead(file);
		return Convert.ToHexString(SHA256.HashData(stream));
	}

	internal static AppiumPackage[] ReadPackages(string directory)
	{
		var packages = new List<AppiumPackage>();
		foreach (var file in Directory.EnumerateFiles(directory, "*.nupkg", SearchOption.AllDirectories)
			.Order(StringComparer.Ordinal))
		{
			var name = Path.GetFileName(file);
			if ((!name.StartsWith("SkiaSharp.", StringComparison.OrdinalIgnoreCase) &&
				!name.StartsWith("HarfBuzzSharp.", StringComparison.OrdinalIgnoreCase)) ||
				name.EndsWith(".symbols.nupkg", StringComparison.OrdinalIgnoreCase))
				continue;
			using var archive = ZipFile.OpenRead(file);
			var nuspec = archive.Entries.Single(entry => entry.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase));
			using var stream = nuspec.Open();
			var metadata = XDocument.Load(stream).Root!.Elements().Single(element => element.Name.LocalName == "metadata");
			string Field(string field) => metadata.Elements().Single(element => element.Name.LocalName == field).Value;
			packages.Add(new(Field("id"), Field("version"), Hash(file)));
		}
		if (!packages.Any(package => package.Id == "SkiaSharp") || !packages.Any(package => package.Id == "HarfBuzzSharp") ||
			packages.GroupBy(package => package.Id, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() != 1))
			throw new InvalidDataException("Acquire exactly one SkiaSharp/HarfBuzzSharp package cohort, including both core packages.");
		return packages.OrderBy(package => package.Id, StringComparer.Ordinal).ToArray();
	}

	internal static AppiumAppManifest Export(
		string directory, AppiumAppSpec spec, string artifact, string applicationId, AppiumPackage[] packages)
	{
		spec.Validate();
		ValidateApplicationId(applicationId);
		// A prior invocation's export is never a build cache. Replace it only after a successful build.
		if (Directory.Exists(directory))
			SampleWorkspace.DeleteDirectory(directory);
		Directory.CreateDirectory(directory);
		var relative = spec.Platform == "android" ? "app.apk" : "app.tar";
		string? bundle = null;
		var target = Path.Combine(directory, relative);
		if (spec.Platform == "android")
		{
			if (!artifact.EndsWith("-Signed.apk", StringComparison.Ordinal))
				throw new InvalidDataException("Export the exact signed APK.");
			File.Copy(artifact, target);
		}
		else
		{
			bundle = Path.GetFileName(artifact);
			ValidateBundleName(bundle);
			TarFile.CreateFromDirectory(artifact, target, includeBaseDirectory: true);
		}
		var manifest = new AppiumAppManifest(1, spec, applicationId, relative, bundle, Hash(target), packages);
		File.WriteAllText(Path.Combine(directory, "manifest.json"), JsonSerializer.Serialize(manifest,
			new JsonSerializerOptions { WriteIndented = true }));
		return manifest;
	}

	internal static AppiumAppManifest Load(string directory, AppiumAppSpec spec, AppiumPackage[] packages)
	{
		spec.Validate();
		var path = Path.Combine(directory, "manifest.json");
		if (!File.Exists(path))
			throw new FileNotFoundException($"Build and acquire the exact Appium app first: {spec.Key}", path);
		var manifest = JsonSerializer.Deserialize<AppiumAppManifest>(File.ReadAllText(path))
			?? throw new InvalidDataException($"Invalid Appium manifest: {path}");
		if (manifest.Schema != 1 || manifest.App != spec ||
			manifest.Packages is null || !manifest.Packages.SequenceEqual(packages))
			throw new InvalidDataException($"Incompatible Appium app target or producing package cohort: {path}");
		ValidateApplicationId(manifest.ApplicationId);
		RelativePath(manifest.ArtifactRelativePath);
		if (manifest.ArtifactRelativePath != (spec.Platform == "android" ? "app.apk" : "app.tar") ||
			(spec.Platform == "android" && manifest.BundleName is not null))
			throw new InvalidDataException($"Unexpected Appium artifact path: {path}");
		if (spec.Platform == "ios")
			ValidateBundleName(manifest.BundleName);
		var artifact = Path.Combine(directory, manifest.ArtifactRelativePath);
		if (!File.Exists(artifact) || Hash(artifact) != manifest.Sha256)
			throw new InvalidDataException($"Missing or corrupt Appium artifact: {artifact}");
		return manifest;
	}

	internal static string Materialize(string directory, AppiumAppManifest manifest, string destination)
	{
		var source = Path.Combine(directory, manifest.ArtifactRelativePath);
		if (manifest.App.Platform == "android")
		{
			var apk = Path.Combine(destination, "app.apk");
			File.Copy(source, apk);
			return apk;
		}
		ValidateBundleName(manifest.BundleName);
		// Tar's extractor rejects entries and links escaping destination, and preserves Unix metadata.
		TarFile.ExtractToDirectory(source, destination, overwriteFiles: false);
		var bundle = Path.Combine(destination, manifest.BundleName!);
		if (!Directory.Exists(bundle) || !File.Exists(Path.Combine(bundle, "Info.plist")))
			throw new InvalidDataException($"The archived simulator bundle is incomplete: {bundle}");
		return bundle;
	}

	private static void ValidateApplicationId(string? id)
	{
		if (string.IsNullOrWhiteSpace(id) || id.Any(char.IsWhiteSpace))
			throw new InvalidDataException("A built Appium artifact must declare its actual application identity.");
	}

	private static void ValidateBundleName(string? name)
	{
		if (name is null || RelativePath(name).Contains('/') || !name.EndsWith(".app", StringComparison.Ordinal))
			throw new InvalidDataException("Expected one simulator .app bundle name.");
	}
}
