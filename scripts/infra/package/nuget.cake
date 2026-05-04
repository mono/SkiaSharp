using System.Xml.Linq;

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../native/windows/msbuild.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// NUGET — pack NuGet packages
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("nuget")
    .Description ("Pack all NuGets.")
    .IsDependentOn ("nuget-normal")
    .IsDependentOn ("nuget-special");

Task ("nuget-normal")
    .Description ("Pack all NuGets (build all required dependencies).")
    .Does (() =>
{
    var props = new Dictionary<string, string> (MSBUILD_VERSION_PROPERTIES) {
        { "BuildingInsideUnoSourceGenerator", "true" },
        { "BuildProjectReferences", "false" },
    };

    // pack stable
    RunDotNetPack ($"./source/SkiaSharpSource.{CURRENT_PLATFORM}.slnf", bl: ".pack", properties: props);

    // pack preview
    props ["VersionSuffix"] = PREVIEW_NUGET_SUFFIX;
    RunDotNetPack ($"./source/SkiaSharpSource.{CURRENT_PLATFORM}.slnf", bl: ".pre.pack", properties: props);

    // move symbols to a special location to avoid signing
    EnsureDirectoryExists ($"{OUTPUT_SYMBOLS_NUGETS_PATH}");
    DeleteFiles ($"{OUTPUT_SYMBOLS_NUGETS_PATH}/*.nupkg");
    MoveFiles ($"{OUTPUT_NUGETS_PATH}/*.snupkg", OUTPUT_SYMBOLS_NUGETS_PATH);
    MoveFiles ($"{OUTPUT_NUGETS_PATH}/*.symbols.nupkg", OUTPUT_SYMBOLS_NUGETS_PATH);
});

Task ("nuget-special")
    .Description ("Pack all special NuGets.")
    .IsDependentOn ("nuget-normal")
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
    var nativePlatforms = GetDirectories ("./output/native/*")
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
            var nuspec = $"./output/{path}/{id}.nuspec";

            DeleteFiles ($"./output/{path}/*.nuspec");

            foreach (var version in versions) {
                var packageVersion = version.Value;

                var xdoc = XDocument.Load ("./scripts/infra/package/nuget/_NativeAssets.nuspec");
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
                        new XAttribute ("src", MakeAbsolute (File ("./scripts/infra/package/nuget/README.md")).FullPath),
                        new XAttribute ("target", "README.md")));
                }

                xdoc.Save (nuspec);
                RunDotNetPack (
                    "./scripts/infra/package/nuget/NuGet.csproj",
                    OUTPUT_SPECIAL_NUGETS_PATH,
                    bl: $".{id}.{version.Key}",
                    additionalArgs: "/restore /nologo",
                    properties: new Dictionary<string, string> {
                        { "NuspecFile", MakeAbsolute (File (nuspec)).FullPath },
                    });
            }

            DeleteFiles ($"./output/{path}/*.nuspec");
        }
    }

    // NuGets and Symbols: bin-pack all nupkgs into ~200 MB numbered chunks
    if (GetFiles ("./output/nugets/*.nupkg").Count > 0) {
        const long MAX_CHUNK_SIZE = 200L * 1024 * 1024;

        var metaPackages = new[] {
            new { Id = "_NuGets",         SourceDir = "nugets",         IncludeSnupkg = false, IsPreview = false },
            new { Id = "_NuGetsPreview",  SourceDir = "nugets",         IncludeSnupkg = false, IsPreview = true },
            new { Id = "_Symbols",        SourceDir = "nugets-symbols", IncludeSnupkg = true,  IsPreview = false },
            new { Id = "_SymbolsPreview", SourceDir = "nugets-symbols", IncludeSnupkg = true,  IsPreview = true },
        };

        foreach (var meta in metaPackages) {
            // enumerate matching files
            var allFiles = GetFiles ($"./output/{meta.SourceDir}/*.nupkg").ToList ();
            if (meta.IncludeSnupkg)
                allFiles.AddRange (GetFiles ($"./output/{meta.SourceDir}/*.snupkg"));

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
                    var nuspec = $"./output/{meta.SourceDir}/{chunkId}.nuspec";

                    DeleteFiles ($"./output/{meta.SourceDir}/*.nuspec");

                    var xdoc = XDocument.Load ("./scripts/infra/package/nuget/_Dependencies.nuspec");
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
                        new XAttribute ("src", MakeAbsolute (File ("./scripts/infra/package/nuget/README.md")).FullPath),
                        new XAttribute ("target", "README.md")));

                    xdoc.Save (nuspec);
                    RunDotNetPack (
                        "./scripts/infra/package/nuget/NuGet.csproj",
                        OUTPUT_SPECIAL_NUGETS_PATH,
                        bl: $".{chunkId}.{version.Key}",
                        additionalArgs: "/restore /nologo",
                        properties: new Dictionary<string, string> {
                            { "NuspecFile", MakeAbsolute (File (nuspec)).FullPath },
                        });
                }

                // pack the parent meta-package with dependencies on all chunks
                {
                    var nuspec = $"./output/{meta.SourceDir}/{meta.Id}.nuspec";

                    DeleteFiles ($"./output/{meta.SourceDir}/*.nuspec");

                    var xdoc = XDocument.Load ($"./scripts/infra/package/nuget/{meta.Id}.nuspec");
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
                        new XAttribute ("src", MakeAbsolute (File ("./scripts/infra/package/nuget/README.md")).FullPath),
                        new XAttribute ("target", "README.md")));

                    xdoc.Save (nuspec);
                    RunDotNetPack (
                        "./scripts/infra/package/nuget/NuGet.csproj",
                        OUTPUT_SPECIAL_NUGETS_PATH,
                        bl: $".{meta.Id}.{version.Key}",
                        additionalArgs: "/restore /nologo",
                        properties: new Dictionary<string, string> {
                            { "NuspecFile", MakeAbsolute (File (nuspec)).FullPath },
                        });
                }

                DeleteFiles ($"./output/{meta.SourceDir}/*.nuspec");
            }
        }
    }
});

Task ("Default")
    .IsDependentOn ("nuget");

RunTarget(TARGET);
