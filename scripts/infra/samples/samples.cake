#addin nuget:?package=Cake.FileHelpers&version=4.0.1

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "../tests/test-shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// SAMPLES TASKS
////////////////////////////////////////////////////////////////////////////////////////////////////

var SAMPLE_FILTER = Argument ("sample", "");
var SAMPLE_SDK_VERSION = Argument ("sampleSdkVersion", "");
var SAMPLE_WORKLOAD_VERSION = Argument ("sampleWorkloadVersion", "");
var CONSUMER_TARGET_FRAMEWORK = Argument ("consumerTargetFramework", "net10.0");
var SAMPLE_TEST_CATEGORIES = Argument ("sampleTestCategories",
    "SampleBuild,PackageOutput,PackageMultiTarget,DockerBuild,SampleRun,RuntimeSmoke,Infrastructure");

Task ("samples-generate")
    .Description ("Generate and zip the samples directory structure.")
    .Does (() =>
{
    EnsureDirectoryExists ($"{ROOT_PATH}/output/");

    // create the samples archive
    CreateSamplesDirectory ($"{ROOT_PATH}/samples/", $"{ROOT_PATH}/output/samples/");
    Zip ($"{ROOT_PATH}/output/samples/", $"{ROOT_PATH}/output/samples.zip");

    // create the preview samples archive
    CreateSamplesDirectory ($"{ROOT_PATH}/samples/", $"{ROOT_PATH}/output/samples-preview/", PREVIEW_NUGET_SUFFIX);
    Zip ($"{ROOT_PATH}/output/samples-preview/", $"{ROOT_PATH}/output/samples-preview.zip");
});

Task ("samples")
    .IsDependentOn ("samples-generate")
    .Description ("Test generated samples and packed NuGets.")
    .Does (() =>
{
    var actualSamples = string.IsNullOrEmpty (PREVIEW_NUGET_SUFFIX)
        ? "samples"
        : "samples-preview";
    var results = ROOT_PATH.Combine($"output/logs/testlogs/samples/{DATE_TIME_STR}");
    var properties = new Dictionary<string, string> {
        { "PackageDirectory", OUTPUT_NUGETS_PATH.FullPath },
        { "SamplesDirectory", ROOT_PATH.Combine("output/" + actualSamples).FullPath },
        { "SampleTestArtifactsDirectory", results.FullPath },
        { "SampleSdkVersion", SAMPLE_SDK_VERSION },
        { "SampleWorkloadVersion", SAMPLE_WORKLOAD_VERSION },
        { "ConsumerTargetFramework", CONSUMER_TARGET_FRAMEWORK },
        { "SampleFilter", SAMPLE_FILTER },
    };
    var categories = SAMPLE_TEST_CATEGORIES.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
    RunDotNetTest(ROOT_PATH.CombineWithFilePath("tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj"),
        results, properties: properties, noBuild: SKIP_BUILD, hangTimeout: "40m", categories: categories);
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// HELPER FUNCTIONS
////////////////////////////////////////////////////////////////////////////////////////////////////

void CreateSamplesDirectory(DirectoryPath samplesDirPath, DirectoryPath outputDirPath, string versionSuffix = "")
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

            // substitute <SkiaSharpVersion> (used by Uno.Sdk to override the version of its
            // implicitly-referenced SkiaSharp package; not a <PackageReference> so not handled above)
            foreach (var ve in xdoc.Descendants().Where(e => e.Name.LocalName == "SkiaSharpVersion").ToArray()) {
                var skiaVersion = GetVersion("SkiaSharp");
                if (!string.IsNullOrWhiteSpace(skiaVersion)) {
                    skiaVersion += string.IsNullOrEmpty(versionSuffix) ? "" : $"-{versionSuffix}";
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
