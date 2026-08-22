using System.IO.Compression;
using System.Security.Cryptography;

sealed class AppleSymbolAddition
{
    public FilePath Source { get; set; }
    public string PackagePath { get; set; }
}

sealed class AppleSymbolPackageSpec
{
    public string PackageId { get; set; }
    public string PackageVersion { get; set; }
    public string LibraryName { get; set; }
    public string Platform { get; set; }
    public List<string> RequiredPackageEntries { get; } = new List<string>();
    public List<AppleSymbolAddition> Additions { get; } = new List<AppleSymbolAddition>();
}

int CreateAppleSymbolPackages(
    DirectoryPath packageDirectory,
    DirectoryPath nativeDirectory,
    DirectoryPath symbolPackageDirectory,
    bool requireAll)
{
    var specs = CreateAppleSymbolPackageSpecs(nativeDirectory);
    var ready = new List<Tuple<AppleSymbolPackageSpec, FilePath>>();
    var failures = new List<string>();

    EnsureDirectoryExists(symbolPackageDirectory);
    foreach (var spec in specs)
        DeleteFiles($"{symbolPackageDirectory}/{spec.PackageId}.*.symbols.nupkg");

    foreach (var spec in specs) {
        var package = packageDirectory.CombineWithFilePath(
            $"{spec.PackageId}.{spec.PackageVersion}.nupkg");
        var missing = GetMissingAppleSymbolInputs(spec, package);
        if (missing.Count == 0) {
            ready.Add(Tuple.Create(spec, package));
            continue;
        }

        var message = $"{spec.PackageId}: {string.Join("; ", missing)}";
        if (requireAll)
            failures.Add(message);
        else
            Information("Skipping Apple symbol package because its complete inputs were not produced: {0}", message);
    }

    if (failures.Count > 0) {
        throw new InvalidOperationException(
            "Full package builds require all eight Apple symbol packages and arm64/x86_64 inputs:" +
            Environment.NewLine + string.Join(Environment.NewLine, failures.Select(f => $"  - {f}")));
    }

    foreach (var item in ready)
        CreateAppleSymbolPackage(item.Item1, item.Item2, symbolPackageDirectory);

    if (requireAll && ready.Count != specs.Count)
        throw new InvalidOperationException($"Expected {specs.Count} Apple symbol packages, but created {ready.Count}.");

    Information("Created {0} Apple symbol package(s).", ready.Count);
    return ready.Count;
}

List<AppleSymbolPackageSpec> CreateAppleSymbolPackageSpecs(DirectoryPath nativeDirectory)
{
    var specs = new List<AppleSymbolPackageSpec>();
    foreach (var product in new[] {
        new { PackagePrefix = "SkiaSharp", LibraryName = "libSkiaSharp", Version = GetVersion("SkiaSharp") },
        new { PackagePrefix = "HarfBuzzSharp", LibraryName = "libHarfBuzzSharp", Version = GetVersion("HarfBuzzSharp") },
    }) {
        specs.Add(CreateMacOSSymbolSpec(
            nativeDirectory, product.PackagePrefix, product.LibraryName, product.Version));
        specs.Add(CreateFrameworkSymbolSpec(
            nativeDirectory, product.PackagePrefix, product.LibraryName, product.Version,
            "iOS", new[] { "ios", "iossimulator" }));
        specs.Add(CreateMacCatalystSymbolSpec(
            nativeDirectory, product.PackagePrefix, product.LibraryName, product.Version));
        specs.Add(CreateFrameworkSymbolSpec(
            nativeDirectory, product.PackagePrefix, product.LibraryName, product.Version,
            "tvOS", new[] { "tvos", "tvossimulator" }));
    }
    return specs;
}

AppleSymbolPackageSpec CreateMacOSSymbolSpec(
    DirectoryPath nativeDirectory,
    string packagePrefix,
    string libraryName,
    string version)
{
    var spec = new AppleSymbolPackageSpec {
        PackageId = $"{packagePrefix}.NativeAssets.macOS",
        PackageVersion = version,
        LibraryName = libraryName,
        Platform = "macOS",
    };
    spec.RequiredPackageEntries.Add($"runtimes/osx/native/{libraryName}.dylib");
    foreach (var arch in new[] { "arm64", "x86_64" }) {
        spec.Additions.Add(new AppleSymbolAddition {
            Source = nativeDirectory.CombineWithFilePath(
                $"osx/{libraryName}/{arch}.xcarchive/dSYMs/{libraryName}.dylib.dSYM/Contents/Resources/DWARF/{libraryName}.dylib"),
            PackagePath = $"runtimes/osx/native/symbols/{arch}/{libraryName}.dylib.dwarf",
        });
    }
    return spec;
}

AppleSymbolPackageSpec CreateFrameworkSymbolSpec(
    DirectoryPath nativeDirectory,
    string packagePrefix,
    string libraryName,
    string version,
    string platform,
    string[] runtimeIdentifiers)
{
    var spec = new AppleSymbolPackageSpec {
        PackageId = $"{packagePrefix}.NativeAssets.{platform}",
        PackageVersion = version,
        LibraryName = libraryName,
        Platform = platform,
    };
    foreach (var runtimeIdentifier in runtimeIdentifiers) {
        spec.RequiredPackageEntries.Add(
            $"runtimes/{runtimeIdentifier}/native/{libraryName}.framework/{libraryName}");
        foreach (var arch in new[] { "arm64", "x86_64" }) {
            spec.Additions.Add(new AppleSymbolAddition {
                Source = nativeDirectory.CombineWithFilePath(
                    $"{runtimeIdentifier}/{libraryName}/{arch}.xcarchive/dSYMs/{libraryName}.framework.dSYM/Contents/Resources/DWARF/{libraryName}"),
                PackagePath = $"runtimes/{runtimeIdentifier}/native/symbols/{arch}/{libraryName}.dwarf",
            });
        }
    }
    return spec;
}

AppleSymbolPackageSpec CreateMacCatalystSymbolSpec(
    DirectoryPath nativeDirectory,
    string packagePrefix,
    string libraryName,
    string version)
{
    var spec = new AppleSymbolPackageSpec {
        PackageId = $"{packagePrefix}.NativeAssets.MacCatalyst",
        PackageVersion = version,
        LibraryName = libraryName,
        Platform = "Mac Catalyst",
    };
    spec.RequiredPackageEntries.Add(
        $"runtimes/maccatalyst/native/{libraryName}.framework.zip");
    spec.Additions.Add(new AppleSymbolAddition {
        Source = nativeDirectory.CombineWithFilePath(
            $"maccatalyst/{libraryName}.framework/Versions/A/{libraryName}"),
        PackagePath = $"runtimes/maccatalyst/native/{libraryName}.framework/Versions/A/{libraryName}",
    });
    foreach (var arch in new[] { "arm64", "x86_64" }) {
        spec.Additions.Add(new AppleSymbolAddition {
            Source = nativeDirectory.CombineWithFilePath(
                $"maccatalyst/{libraryName}/{arch}.xcarchive/dSYMs/{libraryName}.framework.dSYM/Contents/Resources/DWARF/{libraryName}"),
            PackagePath = $"runtimes/maccatalyst/native/symbols/{arch}/{libraryName}.dwarf",
        });
    }
    return spec;
}

List<string> GetMissingAppleSymbolInputs(AppleSymbolPackageSpec spec, FilePath package)
{
    var missing = new List<string>();
    if (!FileExists(package)) {
        missing.Add($"normal package '{package.GetFilename()}'");
        return missing;
    }

    using (var archive = ZipFile.OpenRead(package.FullPath)) {
        var entries = new HashSet<string>(
            archive.Entries.Select(e => e.FullName.Replace('\\', '/')),
            StringComparer.Ordinal);
        foreach (var required in spec.RequiredPackageEntries) {
            if (!entries.Contains(required))
                missing.Add($"runtime payload '{required}'");
        }
    }

    foreach (var addition in spec.Additions) {
        if (!FileExists(addition.Source))
            missing.Add($"native input '{addition.Source}'");
    }

    var catalystRuntime = spec.Additions.FirstOrDefault(a =>
        a.PackagePath.EndsWith($"/Versions/A/{spec.LibraryName}", StringComparison.Ordinal));
    if (catalystRuntime != null &&
        FileExists(catalystRuntime.Source) &&
        !IsMachO(catalystRuntime.Source)) {
        missing.Add($"real Mach-O runtime '{catalystRuntime.Source}'");
    }

    return missing;
}

void CreateAppleSymbolPackage(
    AppleSymbolPackageSpec spec,
    FilePath normalPackage,
    DirectoryPath symbolPackageDirectory)
{
    var beforeHash = GetSha256(normalPackage);
    var symbolPackage = symbolPackageDirectory.CombineWithFilePath(
        $"{spec.PackageId}.{spec.PackageVersion}.symbols.nupkg");
    CopyFile(normalPackage, symbolPackage);

    using (var archive = ZipFile.Open(symbolPackage.FullPath, ZipArchiveMode.Update)) {
        ValidatePackageIdentity(archive, spec);
        foreach (var addition in spec.Additions)
            AddFileToPackage(archive, addition);
        UpdatePackageContentTypes(archive, spec.Additions);
    }

    var afterHash = GetSha256(normalPackage);
    if (!string.Equals(beforeHash, afterHash, StringComparison.Ordinal))
        throw new InvalidOperationException($"Creating {symbolPackage.GetFilename()} modified {normalPackage.GetFilename()}.");

    Information("Created {0} from {1}.", symbolPackage.GetFilename(), normalPackage.GetFilename());
}

void ValidatePackageIdentity(ZipArchive archive, AppleSymbolPackageSpec spec)
{
    var nuspecs = archive.Entries
        .Where(e => e.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase))
        .ToArray();
    if (nuspecs.Length != 1)
        throw new InvalidOperationException($"Expected one nuspec in {spec.PackageId}, but found {nuspecs.Length}.");

    using (var stream = nuspecs[0].Open()) {
        var document = XDocument.Load(stream);
        var ns = document.Root.Name.Namespace;
        var metadata = document.Root.Element(ns + "metadata");
        var id = metadata.Element(ns + "id").Value;
        var version = metadata.Element(ns + "version").Value;
        if (id != spec.PackageId || version != spec.PackageVersion) {
            throw new InvalidOperationException(
                $"Package identity mismatch. Expected {spec.PackageId} {spec.PackageVersion}, found {id} {version}.");
        }
    }
}

void AddFileToPackage(ZipArchive archive, AppleSymbolAddition addition)
{
    var packagePath = addition.PackagePath.Replace('\\', '/');
    var existing = archive.GetEntry(packagePath);
    if (existing != null)
        existing.Delete();

    var entry = archive.CreateEntry(packagePath, CompressionLevel.Optimal);
    entry.LastWriteTime = System.IO.File.GetLastWriteTimeUtc(addition.Source.FullPath);
    using (var input = System.IO.File.OpenRead(addition.Source.FullPath))
    using (var output = entry.Open())
        input.CopyTo(output);
}

void UpdatePackageContentTypes(
    ZipArchive archive,
    IEnumerable<AppleSymbolAddition> additions)
{
    const string contentTypesPath = "[Content_Types].xml";
    var contentTypesEntry = archive.GetEntry(contentTypesPath);
    if (contentTypesEntry == null)
        throw new InvalidOperationException($"The package is missing {contentTypesPath}.");

    XDocument document;
    var lastWriteTime = contentTypesEntry.LastWriteTime;
    using (var stream = contentTypesEntry.Open())
        document = XDocument.Load(stream);

    var ns = document.Root.Name.Namespace;
    if (!document.Root.Elements(ns + "Default").Any(e =>
        string.Equals((string)e.Attribute("Extension"), "dwarf", StringComparison.OrdinalIgnoreCase))) {
        document.Root.Add(new XElement(ns + "Default",
            new XAttribute("Extension", "dwarf"),
            new XAttribute("ContentType", "application/octet")));
    }

    foreach (var addition in additions.Where(a =>
        string.IsNullOrEmpty(System.IO.Path.GetExtension(a.PackagePath)))) {
        var partName = "/" + addition.PackagePath.Replace('\\', '/');
        if (!document.Root.Elements(ns + "Override").Any(e =>
            string.Equals((string)e.Attribute("PartName"), partName, StringComparison.Ordinal))) {
            document.Root.Add(new XElement(ns + "Override",
                new XAttribute("PartName", partName),
                new XAttribute("ContentType", "application/octet")));
        }
    }

    contentTypesEntry.Delete();
    contentTypesEntry = archive.CreateEntry(contentTypesPath, CompressionLevel.Optimal);
    contentTypesEntry.LastWriteTime = lastWriteTime;
    using (var stream = contentTypesEntry.Open())
        document.Save(stream);
}

bool IsMachO(FilePath file)
{
    var magic = new byte[4];
    using (var stream = System.IO.File.OpenRead(file.FullPath)) {
        if (stream.Read(magic, 0, magic.Length) != magic.Length)
            return false;
    }

    var value = BitConverter.ToUInt32(magic, 0);
    return value == 0xfeedface ||
           value == 0xcefaedfe ||
           value == 0xfeedfacf ||
           value == 0xcffaedfe ||
           value == 0xcafebabe ||
           value == 0xbebafeca ||
           value == 0xcafebabf ||
           value == 0xbfbafeca;
}

string GetSha256(FilePath file)
{
    using (var sha256 = SHA256.Create())
    using (var stream = System.IO.File.OpenRead(file.FullPath))
        return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", "");
}
