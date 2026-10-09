#addin nuget:?package=Cake.FileHelpers&version=4.0.1

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "../tests/test-shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// SAMPLES TASKS
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("samples-generate")
    .Description ("Generate and zip the samples directory structure.")
    .Does (() =>
{
    EnsureDirectoryExists (ROOT_OUTPUT_PATH);

    // create the samples archive
    CreateSamplesDirectory ($"{ROOT_PATH}/samples/", ROOT_OUTPUT_PATH.Combine("samples"), "");
    Zip (ROOT_OUTPUT_PATH.Combine("samples"), ROOT_OUTPUT_PATH.CombineWithFilePath("samples.zip"));

    // create the preview samples archive
    CreateSamplesDirectory ($"{ROOT_PATH}/samples/", ROOT_OUTPUT_PATH.Combine("samples-preview"), PREVIEW_NUGET_SUFFIX);
    Zip (ROOT_OUTPUT_PATH.Combine("samples-preview"), ROOT_OUTPUT_PATH.CombineWithFilePath("samples-preview.zip"));
});

Task ("samples")
    .IsDependentOn ("samples-generate")
    .Description ("Build generated samples and test their Docker applications.")
    .Does (() =>
{
    var results = ROOT_PATH.Combine($"output/logs/testlogs/samples/{DATE_TIME_STR}");
    // Consumer commands own their timeouts; MTP's activity monitor misdetects these process trees.
    RunDotNetTest(ROOT_PATH.CombineWithFilePath("tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj"),
        results, noBuild: SKIP_BUILD, hangTimeout: null);
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// HELPER FUNCTIONS
////////////////////////////////////////////////////////////////////////////////////////////////////

string GetSamplePackageVersion(string package, string versionSuffix)
{
    var version = GetVersion(package);
    if (string.IsNullOrWhiteSpace(version))
        return version;

    var family = package.StartsWith("SkiaSharp") ? "SkiaSharp"
        : package.StartsWith("HarfBuzzSharp") ? "HarfBuzzSharp"
        : null;
    if (family == null)
        return version;
    return version + (string.IsNullOrEmpty(versionSuffix) ? "" : $"-{versionSuffix}");
}

void CreateSamplesDirectory(DirectoryPath samplesDirPath, DirectoryPath outputDirPath, string versionSuffix)
{
    samplesDirPath = MakeAbsolute(samplesDirPath);
    outputDirPath = MakeAbsolute(outputDirPath);

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

        if (ext.Equals(".slnx", StringComparison.OrdinalIgnoreCase)) {
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
                // update the <PackageReference> versions
                if (projItem.Name.LocalName == "PackageReference") {
                    var packageId = projItem.Attribute("Include").Value;
                    var version = GetSamplePackageVersion(packageId, versionSuffix);
                    if (!string.IsNullOrWhiteSpace(version)) {
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
                    var projectName = System.IO.Path.GetFileNameWithoutExtension(absFilePath.FullPath);
                    var packageId = projectName.Contains(".NativeAssets.")
                        ? projectName
                        : packagingGroup;
                    var version = GetSamplePackageVersion(packagingGroup, versionSuffix);
                    if (!string.IsNullOrWhiteSpace(version)) {
                        Debug($"Substituting project reference {relFilePath} for project {rel}.");
                        var name = projItem.Name.Namespace + "PackageReference";
                        projItem.AddAfterSelf(new XElement(name, new object[] {
                            new XAttribute("Include", packageId),
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

                foreach (var group in xdoc.Root.Elements()
                    .Where(e => (string)e.Attribute("Condition") == $"!Exists('{project}')")) {
                    group.Attribute("Condition").Remove();
                }

                // not inside the samples directory, so needs to be removed
                import.Remove();
            }

            // substitute <SkiaSharpVersion> (used by Uno.Sdk to override the version of its
            // implicitly-referenced SkiaSharp package; not a <PackageReference> so not handled above)
            foreach (var ve in xdoc.Descendants().Where(e => e.Name.LocalName == "SkiaSharpVersion").ToArray()) {
                var skiaVersion = GetSamplePackageVersion("SkiaSharp", versionSuffix);
                if (!string.IsNullOrWhiteSpace(skiaVersion)) {
                    Debug($"Substituting SkiaSharpVersion for {skiaVersion}.");
                    ve.Value = skiaVersion;
                }
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

Task("Default")
    .IsDependentOn("samples");

RunTarget(TARGET);
