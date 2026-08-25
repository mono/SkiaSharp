using System.Xml.Linq;
using System.IO.Compression;

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"

var REQUIRE_NATIVE_SYMBOLS = Argument("requireNativeSymbols", false);

// CI packs the complete matrix, so a package that is expected but missing is a failure there. A
// local pack is frequently partial, so the strict check is opt-out via --requireAll=false.
var VALIDATE_REQUIRE_ALL = Argument ("requireAll", true);
var VALIDATE_RUN_TESTS = Argument ("validatorTests", true);

////////////////////////////////////////////////////////////////////////////////////////////////////
// NUGET — pack NuGet packages
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("nuget-normal")
    .Description ("Pack all NuGets (build all required dependencies).")
    .Does (() =>
{
    EnsureDirectoryExists ($"{OUTPUT_NUGETS_PATH}");
    DeleteFiles ($"{OUTPUT_NUGETS_PATH}/*.nupkg");
    DeleteFiles ($"{OUTPUT_NUGETS_PATH}/*.snupkg");

    var props = new Dictionary<string, string> (MSBUILD_VERSION_PROPERTIES) {
        { "BuildingInsideUnoSourceGenerator", "true" },
        { "BuildProjectReferences", "false" },
    };

    // pack stable
    RunDotNetPack ($"{ROOT_PATH}/source/SkiaSharpSource.{CURRENT_PLATFORM}.slnf", bl: ".pack", properties: props);

    // pack preview
    props ["VersionSuffix"] = PREVIEW_NUGET_SUFFIX;
    RunDotNetPack ($"{ROOT_PATH}/source/SkiaSharpSource.{CURRENT_PLATFORM}.slnf", bl: ".pre.pack", properties: props);

    if (REQUIRE_NATIVE_SYMBOLS)
        ValidateAppleSymbolPackages();

    // move symbols to a special location to avoid signing
    EnsureDirectoryExists ($"{OUTPUT_SYMBOLS_NUGETS_PATH}");
    DeleteFiles ($"{OUTPUT_SYMBOLS_NUGETS_PATH}/*.nupkg");
    DeleteFiles ($"{OUTPUT_SYMBOLS_NUGETS_PATH}/*.snupkg");
    MoveFiles ($"{OUTPUT_NUGETS_PATH}/*.snupkg", OUTPUT_SYMBOLS_NUGETS_PATH);
    MoveFiles ($"{OUTPUT_NUGETS_PATH}/*.symbols.nupkg", OUTPUT_SYMBOLS_NUGETS_PATH);
});

void ValidateAppleSymbolPackages()
{
    var specs = new[] {
        (Product: "SkiaSharp", Library: "libSkiaSharp", Platform: "macOS", Rids: new[] { "osx" }, IsDylib: true, IsCatalyst: false),
        (Product: "SkiaSharp", Library: "libSkiaSharp", Platform: "iOS", Rids: new[] { "ios", "iossimulator" }, IsDylib: false, IsCatalyst: false),
        (Product: "SkiaSharp", Library: "libSkiaSharp", Platform: "MacCatalyst", Rids: new[] { "maccatalyst" }, IsDylib: false, IsCatalyst: true),
        (Product: "SkiaSharp", Library: "libSkiaSharp", Platform: "tvOS", Rids: new[] { "tvos", "tvossimulator" }, IsDylib: false, IsCatalyst: false),
        (Product: "HarfBuzzSharp", Library: "libHarfBuzzSharp", Platform: "macOS", Rids: new[] { "osx" }, IsDylib: true, IsCatalyst: false),
        (Product: "HarfBuzzSharp", Library: "libHarfBuzzSharp", Platform: "iOS", Rids: new[] { "ios", "iossimulator" }, IsDylib: false, IsCatalyst: false),
        (Product: "HarfBuzzSharp", Library: "libHarfBuzzSharp", Platform: "MacCatalyst", Rids: new[] { "maccatalyst" }, IsDylib: false, IsCatalyst: true),
        (Product: "HarfBuzzSharp", Library: "libHarfBuzzSharp", Platform: "tvOS", Rids: new[] { "tvos", "tvossimulator" }, IsDylib: false, IsCatalyst: false),
    };

    var expectedPackages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var spec in specs) {
        var packageId = $"{spec.Product}.NativeAssets.{spec.Platform}";
        var stableVersion = GetVersion(packageId);
        expectedPackages.Add($"{packageId}.{stableVersion}.symbols.nupkg");
        expectedPackages.Add($"{packageId}.{stableVersion}-{PREVIEW_NUGET_SUFFIX}.symbols.nupkg");
    }

    var actualPackages = GetFiles($"{OUTPUT_NUGETS_PATH}/*.symbols.nupkg")
        .Select(package => package.GetFilename().FullPath)
        .Where(name => specs.Any(spec => name.StartsWith(
            $"{spec.Product}.NativeAssets.{spec.Platform}.",
            StringComparison.OrdinalIgnoreCase)))
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    if (!expectedPackages.SetEquals(actualPackages)) {
        throw new InvalidOperationException(
            $"Expected exactly {expectedPackages.Count} Apple symbol packages. " +
            $"Missing: {string.Join(", ", expectedPackages.Except(actualPackages))}; " +
            $"Unexpected: {string.Join(", ", actualPackages.Except(expectedPackages))}");
    }

    foreach (var spec in specs) {
        var packageId = $"{spec.Product}.NativeAssets.{spec.Platform}";
        var stableVersion = GetVersion(packageId);
        foreach (var version in new[] { stableVersion, $"{stableVersion}-{PREVIEW_NUGET_SUFFIX}" }) {
            var package = OUTPUT_NUGETS_PATH.CombineWithFilePath($"{packageId}.{version}.symbols.nupkg");
            var runtimePackage = OUTPUT_NUGETS_PATH.CombineWithFilePath($"{packageId}.{version}.nupkg");
            using (var archive = ZipFile.OpenRead(runtimePackage.FullPath)) {
                var leakedSymbols = archive.Entries
                    .Select(entry => entry.FullName.Replace('\\', '/'))
                    .Where(entry =>
                        entry.Contains(".dSYM/", StringComparison.OrdinalIgnoreCase) ||
                        entry.Contains("/native/symbols/", StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                if (leakedSymbols.Length > 0)
                    throw new InvalidOperationException(
                        $"The customer package contains Apple symbols: {runtimePackage}: {string.Join(", ", leakedSymbols)}");
            }

            using (var archive = ZipFile.OpenRead(package.FullPath)) {
                var entries = archive.Entries.ToDictionary(
                    entry => entry.FullName.Replace('\\', '/').TrimStart('/'),
                    StringComparer.OrdinalIgnoreCase);

                foreach (var rid in spec.Rids) {
                    var module = spec.IsDylib
                        ? $"runtimes/{rid}/native/{spec.Library}.dylib"
                        : spec.IsCatalyst
                            ? $"runtimes/{rid}/native/{spec.Library}.framework/Versions/A/{spec.Library}"
                            : $"runtimes/{rid}/native/{spec.Library}.framework/{spec.Library}";
                    RequirePackageEntry(entries, module, package);
                    if (spec.IsCatalyst && !IsMachO(entries[module]))
                        throw new InvalidOperationException($"The Catalyst module is not a real Mach-O file in {package}: {module}");

                    var bundle = spec.IsDylib
                        ? $"{spec.Library}.dylib.dSYM"
                        : $"{spec.Library}.framework.dSYM";
                    var dwarf = spec.IsDylib
                        ? $"{spec.Library}.dylib"
                        : spec.Library;
                    foreach (var arch in new[] { "arm64", "x86_64" }) {
                        var root = $"runtimes/{rid}/native/symbols/{arch}/{bundle}/Contents";
                        RequirePackageEntry(entries, $"{root}/Info.plist", package);
                        var dwarfPath = $"{root}/Resources/DWARF/{dwarf}";
                        RequirePackageEntry(entries, dwarfPath, package);
                        if (!IsMachO(entries[dwarfPath]))
                            throw new InvalidOperationException($"The dSYM payload is not a real Mach-O file in {package}: {dwarfPath}");
                    }
                }
            }
        }
    }
}

void RequirePackageEntry(
    Dictionary<string, ZipArchiveEntry> entries,
    string entry,
    FilePath package)
{
    if (!entries.ContainsKey(entry))
        throw new InvalidOperationException($"The Apple symbol package is missing '{entry}': {package}");
}

bool IsMachO(ZipArchiveEntry entry)
{
    using (var stream = entry.Open()) {
        var magic = new byte[4];
        if (stream.Read(magic, 0, magic.Length) != magic.Length)
            return false;
        return
            (magic[0] == 0xca && magic[1] == 0xfe && magic[2] == 0xba && magic[3] == 0xbe) ||
            (magic[0] == 0xbe && magic[1] == 0xba && magic[2] == 0xfe && magic[3] == 0xca) ||
            (magic[0] == 0xcf && magic[1] == 0xfa && magic[2] == 0xed && magic[3] == 0xfe) ||
            (magic[0] == 0xfe && magic[1] == 0xed && magic[2] == 0xfa && magic[3] == 0xcf);
    }
}

Task ("nuget-validate")
    .Description ("Validate the packed NuGets, including the native symbol packages.")
    .Does (() =>
{
    var validationRoot = ROOT_PATH.Combine ("scripts/infra/package/validation");
    var validator = validationRoot.CombineWithFilePath ("NativeSymbolValidator/NativeSymbolValidator.csproj");
    var validatorTests = validationRoot.CombineWithFilePath ("NativeSymbolValidator.Tests/NativeSymbolValidator.Tests.csproj");

    // The validator's own regression suite runs first: if its logic has regressed then it cannot be
    // trusted to sign off on the packages, and a green validation would be meaningless.
    if (VALIDATE_RUN_TESTS) {
        DotNetTest (MakeAbsolute (validatorTests).FullPath, new DotNetTestSettings {
            Configuration = CONFIGURATION,
            Verbosity = DotNetVerbosity.Minimal,
        });
    }

    var args = new ProcessArgumentBuilder ()
        .Append ("--packages")
        .AppendQuoted (OUTPUT_NUGETS_PATH.FullPath)
        .Append ("--symbol-packages")
        .AppendQuoted (OUTPUT_SYMBOLS_NUGETS_PATH.FullPath)
        .Append ("--versions-file")
        .AppendQuoted (MakeAbsolute (ROOT_PATH.CombineWithFilePath ("scripts/VERSIONS.txt")).FullPath);

    if (!string.IsNullOrEmpty (PREVIEW_NUGET_SUFFIX)) {
        args.Append ("--preview-suffix");
        args.AppendQuoted (PREVIEW_NUGET_SUFFIX);
    }

    // CI packs the entire matrix, so a missing package there is a failure. Local runs often pack
    // only a subset, so the strict check is opt-out rather than assumed.
    if (VALIDATE_REQUIRE_ALL)
        args.Append ("--require-all");

    if (VERBOSITY >= Verbosity.Verbose)
        args.Append ("--verbose");

    // DotNetRun throws on a non-zero exit code, so validation failures fail the build. Verbosity is
    // deliberately not set: Cake appends it after the `--` separator, where it becomes an argument
    // to the validator rather than to `dotnet run`.
    DotNetRun (MakeAbsolute (validator).FullPath, args, new DotNetRunSettings {
        Configuration = CONFIGURATION,
    });
});

Task ("nuget-special")
    .Description ("Pack all special NuGets.")
    .Does (() =>
{
    EnsureDirectoryExists ($"{OUTPUT_SPECIAL_NUGETS_PATH}");
    DeleteFiles ($"{OUTPUT_SPECIAL_NUGETS_PATH}/*.nupkg");

    // get a list of all the version number variants
    var versions = new Dictionary<string, string> ();
    if (!string.IsNullOrEmpty (PREVIEW_LABEL) && PREVIEW_LABEL.StartsWith ("pr.")) {
        var v = $"0.0.0-{PREVIEW_LABEL}";
        if (!string.IsNullOrEmpty (BUILD_COUNTER))
            v += $".{BUILD_COUNTER}";
        versions.Add ("pr", v);
    } else {
        if (!string.IsNullOrEmpty (GIT_SHA)) {
            var v = $"0.0.0-commit.{GIT_SHA}";
            if (!string.IsNullOrEmpty (BUILD_COUNTER))
                v += $".{BUILD_COUNTER}";
            versions.Add ("commit", v);
        }
        if (!string.IsNullOrEmpty (GIT_BRANCH_NAME)) {
            var v = $"0.0.0-branch.{GIT_BRANCH_NAME.Replace ("/", ".")}";
            if (!string.IsNullOrEmpty (BUILD_COUNTER))
                v += $".{BUILD_COUNTER}";
            versions.Add ("branch", v);
        }
    }
    Information ("Detected {0} special versions to process:", versions.Count);
    var max = 0;
    foreach (var version in versions) {
        if (version.Key.Length > max)
            max = version.Key.Length + 1;
    }
    foreach (var version in versions) {
        Information ("  - {0}" + " ".PadRight(max - version.Key.Length) + "=> {1}", version.Key, version.Value);
    }

    // _NativeAssets handling (per-platform raw native binaries)
    var nativePlatforms = GetDirectories ($"{ROOT_PATH}/output/native/*")
        .Select (d => d.GetDirectoryName ())
        .ToArray ();
    if (nativePlatforms.Length > 0) {
        var nativeSpecials = new Dictionary<string, string> ();
        nativeSpecials["_NativeAssets"] = "native";
        foreach (var platform in nativePlatforms) {
            nativeSpecials[$"_NativeAssets.{platform}"] = $"native/{platform}";
        }

        Information ("Detected {0} native asset artifacts to process:", nativeSpecials.Count);
        max = 0;
        foreach (var special in nativeSpecials) {
            if (special.Key.Length > max)
                max = special.Key.Length + 1;
        }
        foreach (var special in nativeSpecials) {
            Information ("  - {0}" + " ".PadRight(max - special.Key.Length) + "=> {1}", special.Key, special.Value);
        }

        foreach (var pair in nativeSpecials) {
            var id = pair.Key;
            var path = pair.Value;
            var nuspec = $"{ROOT_PATH}/output/{path}/{id}.nuspec";

            DeleteFiles ($"{ROOT_PATH}/output/{path}/*.nuspec");

            foreach (var version in versions) {
                var packageVersion = version.Value;

                var xdoc = XDocument.Load ($"{ROOT_PATH}/scripts/infra/package/nuget/_NativeAssets.nuspec");
                var metadata = xdoc.Root.Element ("metadata");
                metadata.Element ("version").Value = packageVersion;
                metadata.Element ("id").Value = id;

                if (id == "_NativeAssets") {
                    var dependencies = metadata.Element ("dependencies");
                    foreach (var platform in nativePlatforms) {
                        dependencies.Add (new XElement ("dependency",
                            new XAttribute ("id", $"_NativeAssets.{platform}"),
                            new XAttribute ("version", packageVersion)));
                    }
                } else {
                    var platform = id.Substring (id.IndexOf (".") + 1);
                    var files = xdoc.Root.Element ("files");
                    files.Add (new XElement ("file",
                        new XAttribute ("src", "**"),
                        new XAttribute ("target", $"tools/{platform}")));
                }
                {
                    var files = xdoc.Root.Element ("files");
                    files.Add (new XElement ("file",
                        new XAttribute ("src", MakeAbsolute (File ($"{ROOT_PATH}/scripts/infra/package/nuget/README.md")).FullPath),
                        new XAttribute ("target", "README.md")));
                }

                xdoc.Save (nuspec);
                RunDotNetPack (
                    $"{ROOT_PATH}/scripts/infra/package/nuget/NuGet.csproj",
                    OUTPUT_SPECIAL_NUGETS_PATH,
                    bl: $".{id}.{version.Key}",
                    additionalArgs: "/restore /nologo",
                    properties: new Dictionary<string, string> {
                        { "NuspecFile", MakeAbsolute (File (nuspec)).FullPath },
                    });
            }

            DeleteFiles ($"{ROOT_PATH}/output/{path}/*.nuspec");
        }
    }

    // NuGets and Symbols: bin-pack all nupkgs into ~200 MB numbered chunks
    if (GetFiles ($"{ROOT_PATH}/output/nugets/*.nupkg").Count > 0) {
        const long MAX_CHUNK_SIZE = 200L * 1024 * 1024;

        var metaPackages = new[] {
            new { Id = "_NuGets",         SourceDir = "nugets",         IncludeSnupkg = false, IsPreview = false },
            new { Id = "_NuGetsPreview",  SourceDir = "nugets",         IncludeSnupkg = false, IsPreview = true },
            new { Id = "_Symbols",        SourceDir = "nugets-symbols", IncludeSnupkg = true,  IsPreview = false },
            new { Id = "_SymbolsPreview", SourceDir = "nugets-symbols", IncludeSnupkg = true,  IsPreview = true },
        };

        foreach (var meta in metaPackages) {
            // enumerate matching files
            var allFiles = GetFiles ($"{ROOT_PATH}/output/{meta.SourceDir}/*.nupkg").ToList ();
            if (meta.IncludeSnupkg)
                allFiles.AddRange (GetFiles ($"{ROOT_PATH}/output/{meta.SourceDir}/*.snupkg"));

            var matchingFiles = allFiles
                .Where (f => {
                    var name = f.GetFilename ().ToString ();
                    if (name.StartsWith ("_")) return false;
                    return meta.IsPreview ? name.Contains ("-") : !name.Contains ("-");
                })
                .Select (f => new { Path = f, Size = new FileInfo (f.FullPath).Length })
                .OrderByDescending (f => f.Size)
                .ToList ();

            if (matchingFiles.Count == 0)
                continue;

            // bin-pack using first-fit decreasing
            var chunks = new List<List<FilePath>> ();
            var chunkSizes = new List<long> ();

            foreach (var file in matchingFiles) {
                var placed = false;
                for (int i = 0; i < chunks.Count; i++) {
                    if (chunkSizes[i] + file.Size <= MAX_CHUNK_SIZE) {
                        chunks[i].Add (file.Path);
                        chunkSizes[i] += file.Size;
                        placed = true;
                        break;
                    }
                }
                if (!placed) {
                    chunks.Add (new List<FilePath> { file.Path });
                    chunkSizes.Add (file.Size);
                }
            }

            Information ("{0}: {1} files -> {2} chunk(s)", meta.Id, matchingFiles.Count, chunks.Count);
            for (int i = 0; i < chunks.Count; i++) {
                Information ("  Chunk {0}: {1} files, {2:F1} MB",
                    i + 1, chunks[i].Count, chunkSizes[i] / 1024.0 / 1024.0);
            }

            foreach (var version in versions) {
                var packageVersion = version.Value;

                // pack each chunk as a numbered dependency
                for (int i = 0; i < chunks.Count; i++) {
                    var chunkId = $"{meta.Id}.Dependencies.{i + 1}";
                    var nuspec = $"{ROOT_PATH}/output/{meta.SourceDir}/{chunkId}.nuspec";

                    DeleteFiles ($"{ROOT_PATH}/output/{meta.SourceDir}/*.nuspec");

                    var xdoc = XDocument.Load ($"{ROOT_PATH}/scripts/infra/package/nuget/_Dependencies.nuspec");
                    var xmeta = xdoc.Root.Element ("metadata");
                    xmeta.Element ("id").Value = chunkId;
                    xmeta.Element ("version").Value = packageVersion;
                    xmeta.Element ("title").Value = $"{meta.Id.TrimStart ('_')} (Part {i + 1})";
                    xmeta.Element ("description").Value =
                        $"Part {i + 1} of {chunks.Count} of the {meta.Id.TrimStart ('_')} packages.";
                    xmeta.Element ("summary").Value = xmeta.Element ("description").Value;

                    var files = xdoc.Root.Element ("files");
                    foreach (var file in chunks[i]) {
                        files.Add (new XElement ("file",
                            new XAttribute ("src", MakeAbsolute (file).FullPath),
                            new XAttribute ("target", "tools/")));
                    }
                    files.Add (new XElement ("file",
                        new XAttribute ("src", MakeAbsolute (File ($"{ROOT_PATH}/scripts/infra/package/nuget/README.md")).FullPath),
                        new XAttribute ("target", "README.md")));

                    xdoc.Save (nuspec);
                    RunDotNetPack (
                        $"{ROOT_PATH}/scripts/infra/package/nuget/NuGet.csproj",
                        OUTPUT_SPECIAL_NUGETS_PATH,
                        bl: $".{chunkId}.{version.Key}",
                        additionalArgs: "/restore /nologo",
                        properties: new Dictionary<string, string> {
                            { "NuspecFile", MakeAbsolute (File (nuspec)).FullPath },
                        });
                }

                // pack the parent meta-package with dependencies on all chunks
                {
                    var nuspec = $"{ROOT_PATH}/output/{meta.SourceDir}/{meta.Id}.nuspec";

                    DeleteFiles ($"{ROOT_PATH}/output/{meta.SourceDir}/*.nuspec");

                    var xdoc = XDocument.Load (ROOT_PATH + $"/scripts/infra/package/nuget/{meta.Id}.nuspec");
                    var xmeta = xdoc.Root.Element ("metadata");
                    xmeta.Element ("version").Value = packageVersion;

                    var dependencies = xmeta.Element ("dependencies");
                    for (int i = 0; i < chunks.Count; i++) {
                        dependencies.Add (new XElement ("dependency",
                            new XAttribute ("id", $"{meta.Id}.Dependencies.{i + 1}"),
                            new XAttribute ("version", packageVersion)));
                    }

                    var files = xdoc.Root.Element ("files");
                    files.Add (new XElement ("file",
                        new XAttribute ("src", MakeAbsolute (File ($"{ROOT_PATH}/scripts/infra/package/nuget/README.md")).FullPath),
                        new XAttribute ("target", "README.md")));

                    xdoc.Save (nuspec);
                    RunDotNetPack (
                        $"{ROOT_PATH}/scripts/infra/package/nuget/NuGet.csproj",
                        OUTPUT_SPECIAL_NUGETS_PATH,
                        bl: $".{meta.Id}.{version.Key}",
                        additionalArgs: "/restore /nologo",
                        properties: new Dictionary<string, string> {
                            { "NuspecFile", MakeAbsolute (File (nuspec)).FullPath },
                        });
                }

                DeleteFiles ($"{ROOT_PATH}/output/{meta.SourceDir}/*.nuspec");
            }
        }
    }
});

RunTarget(TARGET);
