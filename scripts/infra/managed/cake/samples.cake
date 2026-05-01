////////////////////////////////////////////////////////////////////////////////////////////////////
// SAMPLES TASKS
////////////////////////////////////////////////////////////////////////////////////////////////////

var SAMPLE_FILTER = Argument ("sample", "");

Task ("samples-generate")
    .Description ("Generate and zip the samples directory structure.")
    .Does (() =>
{
    EnsureDirectoryExists ("./output/");

    // create the interactive archive
    Zip ("./interactive", "./output/interactive.zip");

    // create the samples archive
    CreateSamplesDirectory ("./samples/", "./output/samples/");
    Zip ("./output/samples/", "./output/samples.zip");

    // create the preview samples archive
    CreateSamplesDirectory ("./samples/", "./output/samples-preview/", PREVIEW_NUGET_SUFFIX);
    Zip ("./output/samples-preview/", "./output/samples-preview.zip");
});

Task ("samples-prepare")
    .IsDependentOn ("samples-generate")
    .Description ("Prepare the generated samples for building (copy NuGet packages, etc.).")
    .Does (() =>
{
    // clear cached SkiaSharp/HarfBuzzSharp packages so fresh ones are restored
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/skiasharp*");
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/harfbuzzsharp*");
});

Task ("samples-run")
    .Description ("Build and run the generated samples from the output directory.")
    .Does(() =>
{
    var actualSamples = PREVIEW_ONLY_NUGETS.Count > 0
        ? "samples-preview"
        : "samples";

    // discover all samples: solutions for dotnet build, run.ps1 for Docker
    var solutions =
        GetFiles ($"./output/{actualSamples}/**/*.sln").Union (
        GetFiles ($"./output/{actualSamples}/**/*.slnf")).Union (
        GetFiles ($"./output/{actualSamples}/**/*.slnx"))
        .OrderBy (x => x.FullPath)
        .ToArray ();
    var dockerRuns = GetFiles ($"./output/{actualSamples}/**/run.ps1")
        .OrderBy (x => x.FullPath)
        .ToArray ();

    // apply --sample filter if specified
    if (!string.IsNullOrEmpty (SAMPLE_FILTER)) {
        solutions = solutions.Where (s => s.FullPath.Contains (SAMPLE_FILTER)).ToArray ();
        dockerRuns = dockerRuns.Where (r => r.FullPath.Contains (SAMPLE_FILTER)).ToArray ();
        Information ($"Filtered to {solutions.Length} solution(s) and {dockerRuns.Length} Docker sample(s) matching '{SAMPLE_FILTER}'");
    }

    // classify each solution: build, skip (has platform variant), or skip (wrong platform)
    var samplesToBuild = new List<FilePath> ();
    var samplesToSkip = new List<(FilePath sln, string reason)> ();

    foreach (var sln in solutions) {
        var name = sln.GetFilenameWithoutExtension ();
        var slnPlatform = (name.GetExtension () ?? "").ToLower ();

        // check if this sample has a Docker run.ps1 (Docker samples are built via run.ps1, not dotnet build)
        if (dockerRuns.Any (r => r.GetDirectory ().FullPath == sln.GetDirectory ().FullPath)) {
            samplesToSkip.Add ((sln, "Docker (built via run.ps1)"));
            continue;
        }

        if (string.IsNullOrEmpty (slnPlatform)) {
            // main solution — check for platform-specific variants
            var variants =
                GetFiles (sln.GetDirectory ().CombineWithFilePath (name) + ".*.sln").Union (
                GetFiles (sln.GetDirectory ().CombineWithFilePath (name) + ".*.slnf")).Union (
                GetFiles (sln.GetDirectory ().CombineWithFilePath (name) + ".*.slnx"));
            if (variants.Any ()) {
                samplesToSkip.Add ((sln, "has platform-specific variant"));
            } else {
                samplesToBuild.Add (sln);
            }
        } else if (slnPlatform == $".{CURRENT_PLATFORM.ToLower ()}") {
            samplesToBuild.Add (sln);
        } else {
            samplesToSkip.Add ((sln, $"wrong platform (need {slnPlatform})"));
        }
    }

    // check if Docker is available
    var dockerAvailable = false;
    try {
        RunProcess ("docker", new ProcessSettings {
            Arguments = "info",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            Silent = true,
        });
        dockerAvailable = true;
    } catch {
        Warning ("Docker is not available. Docker samples will be skipped.");
    }

    // log the plan
    Information ("Sample plan:");
    foreach (var sln in samplesToBuild) {
        Information ($"    BUILD       {sln}");
    }
    foreach (var (sln, reason) in samplesToSkip) {
        Information ($"    SKIP        {sln} ({reason})");
    }
    foreach (var run in dockerRuns) {
        Information ($"    {(dockerAvailable ? "DOCKER" : "SKIP  ")}      {run}{(dockerAvailable ? "" : " (Docker not available)")}");
    }

    // build dotnet samples
    var failedSamples = new List<(string name, string error)> ();

    foreach (var sln in samplesToBuild) {
        if (!FileExists (sln))
            continue;
        var platform = sln.GetDirectory ().GetDirectoryName ().ToLower ();
        Information ($"Building sample {sln} ({platform})...");
        try {
            RunDotNetBuild (sln);
        } catch (Exception ex) {
            Error ($"FAILED: {sln}");
            failedSamples.Add ((sln.FullPath, ex.Message));
        }
        CleanDir (sln.GetDirectory ().FullPath);
    }

    // build and run Docker samples
    // To conserve disk space, nupkg files are copied per-sample and cleaned up
    // after each build instead of bulk-copying all packages upfront.
    if (!dockerAvailable) {
        Information ("Skipping Docker samples (Docker not available).");
    }
    foreach (var run in dockerRuns) {
        if (!dockerAvailable)
            continue;

        var sampleDir = run.GetDirectory ();

        // stage nupkg files for this Docker sample
        var packagesDir = sampleDir.Combine ("packages");
        EnsureDirectoryExists (packagesDir);
        CopyFiles ($"{OUTPUT_NUGETS_PATH}/*.nupkg", packagesDir);

        Information ($"Running Docker sample: {run}");
        try {
            RunProcess ("pwsh", new ProcessSettings {
                Arguments = run.FullPath,
                WorkingDirectory = sampleDir,
            });
        } catch (Exception ex) {
            Error ($"FAILED: {run}");
            failedSamples.Add ((run.FullPath, ex.Message));
        }

        // clean up to reclaim disk space before the next sample
        CleanDir (packagesDir);
        DeleteDir (packagesDir);

        // prune all unused Docker images and layers to reclaim disk space
        try {
            RunProcess ("docker", new ProcessSettings {
                Arguments = "system prune --all --force",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Silent = true,
            });
        } catch {
            // non-fatal: best-effort cleanup
        }
    }

    // report results
    if (failedSamples.Count > 0) {
        Information ("");
        Error ($"{failedSamples.Count} sample(s) failed:");
        foreach (var (name, error) in failedSamples) {
            Error ($"    ✗ {name}");
        }
        throw new Exception ($"{failedSamples.Count} sample(s) failed to build.");
    } else {
        Information ("All samples built successfully.");
    }

    CleanDir ("./output/samples/");
    DeleteDir ("./output/samples/");
    CleanDir ("./output/samples-preview/");
    DeleteDir ("./output/samples-preview/");
});

Task ("samples")
    .Description ("Generate, prepare, and run all samples.")
    .IsDependentOn ("samples-generate")
    .IsDependentOn ("samples-prepare")
    .IsDependentOn ("samples-run");

////////////////////////////////////////////////////////////////////////////////////////////////////
// HELPER FUNCTIONS
////////////////////////////////////////////////////////////////////////////////////////////////////

void CreateSamplesDirectory(DirectoryPath samplesDirPath, DirectoryPath outputDirPath, string versionSuffix = "")
{
    samplesDirPath = MakeAbsolute(samplesDirPath);
    outputDirPath = MakeAbsolute(outputDirPath);

    var solutionProjectRegex = new Regex(@",\s*""(.*?\.\w{2}proj)"", ""(\{.*?\})""");
    var solutionFilterProjectRegex = new Regex(@"\s*""(.+)\.csproj"",?");

    CleanDir (outputDirPath);

    var ignoreBinObj = new GlobberSettings {
        Predicate = fileSystemInfo => {
            var segments = fileSystemInfo.Path.Segments;
            var keep = segments.All(s =>
                !s.Equals("bin", StringComparison.OrdinalIgnoreCase) &&
                !s.Equals("obj", StringComparison.OrdinalIgnoreCase) &&
                !s.Equals("AppPackages", StringComparison.OrdinalIgnoreCase) &&
                !s.Equals(".vs", StringComparison.OrdinalIgnoreCase));
            return keep;
        }
    };

    var files = GetFiles($"{samplesDirPath}/**/*", ignoreBinObj);
    foreach (var file in files) {
        var rel = samplesDirPath.GetRelativePath(file);
        var dest = outputDirPath.CombineWithFilePath(rel);
        var ext = file.GetExtension() ?? "";

        if (ext.Equals(".sln", StringComparison.OrdinalIgnoreCase)) {
            var lines = FileReadLines(file.FullPath).ToList();
            var guids = new List<string>();

            // remove projects that aren't samples
            for(var i = 0; i < lines.Count; i++) {
                var line = lines [i];
                var m = solutionProjectRegex.Match(line);
                if (!m.Success)
                    continue;

                // get the path of the project relative to the samples directory
                var relProjectPath = (FilePath) m.Groups [1].Value;
                var absProjectPath = GetFullPath(file, relProjectPath);
                var relSamplesPath = samplesDirPath.GetRelativePath(absProjectPath);
                if (!relSamplesPath.FullPath.StartsWith(".."))
                    continue;

                Debug($"Removing the project '{relProjectPath}' for solution '{rel}'.");

                // skip the next line as it is the "EndProject" line
                guids.Add(m.Groups [2].Value.ToLower());
                lines.RemoveAt(i--);
                lines.RemoveAt(i--);
            }

            // remove all the other references to this guid
            if (guids.Count > 0) {
                for(var i = 0; i < lines.Count; i++) {
                    var line = lines [i];
                    foreach (var guid in guids) {
                        if (line.ToLower().Contains(guid)) {
                            lines.RemoveAt(i--);
                        }
                    }
                }
            }

            // save the solution
            EnsureDirectoryExists(dest.GetDirectory());
            FileWriteLines(dest, lines.ToArray());
        } else if (ext.Equals(".slnf", StringComparison.OrdinalIgnoreCase)) {
            var lines = FileReadLines(file.FullPath).ToList();

            // remove projects that aren't samples
            for(var i = 0; i < lines.Count; i++) {
                var line = lines [i];
                var m = solutionFilterProjectRegex.Match(line);
                if (!m.Success)
                    continue;

                // get the path of the project relative to the samples directory
                var relProjectPath = (FilePath) m.Groups [1].Value.Replace("\\\\", "\\");
                var absProjectPath = GetFullPath(file, relProjectPath);
                var relSamplesPath = samplesDirPath.GetRelativePath(absProjectPath);
                if (!relSamplesPath.FullPath.StartsWith(".."))
                    continue;

                Debug($"Removing the project '{relProjectPath}' for solution '{rel}'.");

                lines.RemoveAt(i--);
            }

            // save the solution
            EnsureDirectoryExists(dest.GetDirectory());
            FileWriteLines(dest, lines.ToArray());
        } else if (ext.Equals(".slnx", StringComparison.OrdinalIgnoreCase)) {
            var xdoc = XDocument.Load(file.FullPath);

            // remove projects that aren't in the samples directory
            var projectElements = xdoc.Descendants()
                .Where(e => e.Name.LocalName == "Project" && e.Attribute("Path") != null)
                .ToArray();
            foreach (var projElement in projectElements) {
                var relProjectPath = (FilePath) projElement.Attribute("Path").Value;
                var absProjectPath = GetFullPath(file, relProjectPath);
                var relSamplesPath = samplesDirPath.GetRelativePath(absProjectPath);
                if (!relSamplesPath.FullPath.StartsWith(".."))
                    continue;

                Debug($"Removing the project '{relProjectPath}' for solution '{rel}'.");
                projElement.Remove();
            }

            // remove empty folders
            var emptyFolders = xdoc.Descendants()
                .Where(e => e.Name.LocalName == "Folder" && !e.HasElements)
                .ToArray();
            foreach (var folder in emptyFolders) {
                folder.Remove();
            }

            // save the solution
            EnsureDirectoryExists(dest.GetDirectory());
            xdoc.Save(dest.FullPath);
        } else if (ext.Equals(".csproj", StringComparison.OrdinalIgnoreCase)) {
            var xdoc = XDocument.Load(file.FullPath);

            // process all the files and project references
            var projItems = xdoc.Root
                .Elements().Where(e => e.Name.LocalName == "ItemGroup")
                .Elements().Where(e => !string.IsNullOrWhiteSpace(e.Attribute("Include")?.Value))
                .ToArray();
            foreach (var projItem in projItems) {
                var suffix = string.IsNullOrEmpty(versionSuffix) ? "" : $"-{versionSuffix}";

                // update the <PackageReference> versions
                if (projItem.Name.LocalName == "PackageReference") {
                    var packageId = projItem.Attribute("Include").Value;
                    var version = GetVersion(packageId);
                    if (!string.IsNullOrWhiteSpace(version)) {
                        // only add the suffix for our nugets
                        if (packageId.StartsWith("SkiaSharp") || packageId.StartsWith("HarfBuzzSharp")) {
                            version += suffix;
                        }
                        Debug($"Substituting package version {packageId} for {version}.");
                        projItem.Attribute("Version").Value = version;
                    } else if (packageId.StartsWith("SkiaSharp") || packageId.StartsWith("HarfBuzzSharp")) {
                        Warning($"Unable to find version information for package '{packageId}'.");
                    }
                    continue;
                }

                // get files in the include
                var relFilePath = (FilePath) projItem.Attribute("Include").Value;
                var absFilePath = GetFullPath(file, relFilePath);

                // ignore files in the samples directory or are at the root but start with underscore
                var relSamplesPath = samplesDirPath.GetRelativePath(absFilePath);
                if (!relSamplesPath.FullPath.StartsWith("..") && !relSamplesPath.FullPath.StartsWith("_"))
                    continue;

                // substitute <ProjectReference> with <PackageReference>
                if (projItem.Name.LocalName == "ProjectReference" && FileExists(absFilePath)) {
                    var xReference = XDocument.Load(absFilePath.FullPath);
                    var packagingGroup = xReference.Root
                        .Elements().Where(e => e.Name.LocalName == "PropertyGroup")
                        .Elements().Where(e => e.Name.LocalName == "PackagingGroup")
                        .FirstOrDefault()?.Value;
                    var version = GetVersion(packagingGroup);
                    if (!string.IsNullOrWhiteSpace(version)) {
                        Debug($"Substituting project reference {relFilePath} for project {rel}.");
                        var name = projItem.Name.Namespace + "PackageReference";
                        // only add the suffix for our nugets
                        if (packagingGroup.StartsWith("SkiaSharp") || packagingGroup.StartsWith("HarfBuzzSharp")) {
                            version += suffix;
                        }
                        projItem.AddAfterSelf(new XElement(name, new object[] {
                            new XAttribute("Include", packagingGroup),
                            new XAttribute("Version", version),
                        }));
                    } else {
                        Warning($"Unable to find version information for project '{packagingGroup}'.");
                    }
                } else {
                    Debug($"Removing the file '{relFilePath}' for project '{rel}'.");
                }

                // remove files that are outside
                projItem.Remove();
            }

            // process all the imports
            var imports = xdoc.Root
                .Elements().Where(e =>
                    e.Name.LocalName == "Import" &&
                    !string.IsNullOrWhiteSpace(e.Attribute("Project")?.Value))
                .ToArray();
            foreach (var import in imports) {
                var project = import.Attribute("Project").Value;

                // skip files inside the samples directory or do not exist or are at the root but start with underscore
                var absProject = GetFullPath(file, project);
                var relSamplesPath = samplesDirPath.GetRelativePath(absProject);
                if (!relSamplesPath.FullPath.StartsWith("..") && !relSamplesPath.FullPath.StartsWith("_"))
                    continue;

                Debug($"Removing import '{project}' for project '{rel}'.");

                // not inside the samples directory, so needs to be removed
                import.Remove();
            }

            // save the project
            EnsureDirectoryExists(dest.GetDirectory());
            xdoc.Save(dest.FullPath);
        } else {
            // skip files that are at the root but start with underscore
            var relSamplesPath = samplesDirPath.GetRelativePath(file);
            if (relSamplesPath.FullPath.StartsWith("_"))
            {
                Debug($"Removing file '{relSamplesPath}'.");
                continue;
            }

            EnsureDirectoryExists(dest.GetDirectory());
            CopyFile(file, dest);
        }
    }

    DeleteFiles($"{outputDirPath}/README.md");
    MoveFile($"{outputDirPath}/README.zip.md", $"{outputDirPath}/README.md");
}

FilePath GetFullPath(FilePath root, FilePath path)
{
    path = path.FullPath.Replace("*", "_");
    path = root.GetDirectory().CombineWithFilePath(path);
    return (FilePath) System.IO.Path.GetFullPath(path.FullPath);
}
