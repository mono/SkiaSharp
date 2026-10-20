#addin nuget:?package=Cake.FileHelpers&version=4.0.1

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "tests-shared.cake"
#load "tests-shared-apple.cake"
#load "tests-shared-android.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// SAMPLES TASKS
////////////////////////////////////////////////////////////////////////////////////////////////////

var RUN_GROUP = Argument("runGroup", "");
var RUN_GROUPS = new[] { "Build", "Host", "Docker", "Android", "Ios", "Mac", "Windows" };
var APPIUM_RUN_GROUPS = new[] { "Ios" };
if (!string.IsNullOrEmpty(RUN_GROUP) && !RUN_GROUPS.Contains(RUN_GROUP))
    throw new ArgumentException($"Unknown sample run group '{RUN_GROUP}'. Expected {string.Join(", ", RUN_GROUPS)}.");

var APPIUM_MODE = Argument("appiumMode", EnvironmentVariable("TEST_APPIUM_MODE") ?? "all");
if (!new[] { "all", "build", "run" }.Contains(APPIUM_MODE))
    throw new ArgumentException($"Unknown Appium mode '{APPIUM_MODE}'. Expected all, build or run.");
if (APPIUM_MODE != "all" && !APPIUM_RUN_GROUPS.Contains(RUN_GROUP))
    throw new ArgumentException($"Select an Appium run group ({string.Join(", ", APPIUM_RUN_GROUPS)}) with an explicit phase.");

string OWNED_IOS_SIMULATOR = null;
string OWNED_ANDROID_AVD = null;
string OWNED_ANDROID_SERIAL = null;

void SetupSampleDevices()
{
    if (APPIUM_RUN_GROUPS.Contains("Ios") && (string.IsNullOrEmpty(RUN_GROUP) || RUN_GROUP == "Ios")) {
        SetupIosSimulator(ref OWNED_IOS_SIMULATOR);

        Environment.SetEnvironmentVariable("TEST_IOS_SIMULATOR_VERSION", TEST_IOS_SIMULATOR_VERSION);
        Environment.SetEnvironmentVariable("TEST_IOS_SIMULATOR_MODEL", TEST_IOS_SIMULATOR_MODEL);
        Environment.SetEnvironmentVariable("TEST_IOS_SIMULATOR_UDID", OWNED_IOS_SIMULATOR);
    }
    if (APPIUM_RUN_GROUPS.Contains("Android") && (string.IsNullOrEmpty(RUN_GROUP) || RUN_GROUP == "Android")) {
        SetupAndroidEmulator(ref OWNED_ANDROID_AVD, ref OWNED_ANDROID_SERIAL);
        Environment.SetEnvironmentVariable("TEST_ANDROID_IMAGE", TEST_ANDROID_IMAGE);
        Environment.SetEnvironmentVariable("TEST_ANDROID_MODEL", TEST_ANDROID_MODEL);
        Environment.SetEnvironmentVariable("TEST_ANDROID_UDID", OWNED_ANDROID_SERIAL);
    }
}

Teardown(context =>
{
    var failures = new List<Exception>();
    if (string.IsNullOrEmpty(RUN_GROUP) || RUN_GROUP == "Android") {
        try {
            TeardownAndroidEmulator(ref OWNED_ANDROID_AVD, ref OWNED_ANDROID_SERIAL, context.ThrownException);
        } catch (Exception error) {
            failures.Add(error);
        }
    }

    if (string.IsNullOrEmpty(RUN_GROUP) || RUN_GROUP == "Ios") {
        try {
            TeardownIosSimulator(ref OWNED_IOS_SIMULATOR, context.ThrownException);
        } catch (Exception error) {
            failures.Add(error);
        }
    }
    if (failures.Count != 0)
        throw new AggregateException("Owned sample device cleanup failed.", failures);
});

Task ("samples-generate")
    .WithCriteria(() => APPIUM_MODE != "run" || TARGET.Equals("samples-generate", StringComparison.OrdinalIgnoreCase))
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
    Environment.SetEnvironmentVariable("TEST_SAMPLE_REPOSITORY", ROOT_PATH.FullPath);
    var mobile = APPIUM_RUN_GROUPS.Contains(RUN_GROUP);
    var project = ROOT_PATH.CombineWithFilePath("tests/SkiaSharp.Tests.Samples/SkiaSharp.Tests.Samples.csproj");

    // Consumer commands own their timeouts; MTP's activity monitor misdetects these process trees.
    if (mobile || string.IsNullOrEmpty(RUN_GROUP)) {
        if (APPIUM_MODE != "run") {
            // Finish every selected app build/export before any owned device is booted.
            foreach (var group in mobile ? new[] { RUN_GROUP } : APPIUM_RUN_GROUPS) {
                Environment.SetEnvironmentVariable("TEST_APPIUM_MODE", "build");
                RunDotNetTest(project, results.Combine($"build-{group}"), noBuild: SKIP_BUILD, hangTimeout: null,
                    properties: SampleTestProperties(group),
                    runnerArguments: new[] { "--filter-trait", $"RunGroup={group}" });
                ExportAppiumRunner(group);
            }
        }
        if (APPIUM_MODE == "build")
            return;
        Environment.SetEnvironmentVariable("TEST_APPIUM_MODE", "run");
        if (mobile) {
            // Acquire/extract the exact host runner before boot, never evaluate or compile a sample project.
            RunPreparedAppiumTests(RUN_GROUP, results.Combine($"run-{RUN_GROUP}"));
            return;
        }
        SetupSampleDevices();
    }
    RunDotNetTest(project, results.Combine("tests"), noBuild: SKIP_BUILD, hangTimeout: null,
        properties: SampleTestProperties(RUN_GROUP),
        runnerArguments: string.IsNullOrEmpty(RUN_GROUP) ? null : new[] { "--filter-trait", $"RunGroup={RUN_GROUP}" });
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// HELPER FUNCTIONS
////////////////////////////////////////////////////////////////////////////////////////////////////

Dictionary<string, string> SampleTestProperties(string group) => new Dictionary<string, string> {
    ["SampleRunGroup"] = group,
    ["TargetFramework"] = "net10.0",
    ["TargetFrameworks"] = "net10.0",
};

DirectoryPath AppiumRunnerDirectory(string group)
{
    var architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
    if (architecture != "arm64" && architecture != "x64")
        throw new Exception("Appium runners require an arm64 or x64 host.");
    return ROOT_OUTPUT_PATH.Combine($"appium-runner/{(group == "Ios" ? "ios" : "android")}/{architecture}");
}

string AppiumRunnerHost() =>
    IsRunningOnWindows() ? "windows" : IsRunningOnMacOs() ? "macos" : IsRunningOnLinux() ? "linux" :
        throw new Exception("Unsupported Appium runner host OS.");

void AppiumTar(params string[] arguments)
{
    var args = new ProcessArgumentBuilder();
    foreach (var argument in arguments)
        args.AppendQuoted(argument);
    if (StartProcess("tar", new ProcessSettings { Arguments = args, WorkingDirectory = ROOT_PATH }) != 0)
        throw new Exception("Could not archive/extract the prepared Appium test runner.");
}

string AppiumRunnerHash(string archive)
{
    using var stream = System.IO.File.OpenRead(archive);
    return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream));
}

void ExportAppiumRunner(string group)
{
    var source = ROOT_PATH.Combine($"tests/SkiaSharp.Tests.Samples/bin/{CONFIGURATION}/net10.0");
    if (!System.IO.File.Exists(source.CombineWithFilePath("SkiaSharp.Tests.Samples.dll").FullPath))
        throw new Exception($"The source-backed sample test runner is missing: {source}");
    var directory = AppiumRunnerDirectory(group);
    EnsureDirectoryExists(directory);
    var archive = directory.CombineWithFilePath("runner.tar").FullPath;
    AppiumTar("-cf", archive, "-C", source.FullPath, ".");
    System.IO.File.WriteAllText(directory.CombineWithFilePath("runner.sha256").FullPath, AppiumRunnerHash(archive));
    System.IO.File.WriteAllText(directory.CombineWithFilePath("runner.host").FullPath, AppiumRunnerHost());
}

void RunPreparedAppiumTests(string group, DirectoryPath results)
{
    var directory = AppiumRunnerDirectory(group);
    var archive = directory.CombineWithFilePath("runner.tar").FullPath;
    var checksum = directory.CombineWithFilePath("runner.sha256").FullPath;
    var host = directory.CombineWithFilePath("runner.host").FullPath;
    if (!System.IO.File.Exists(archive) || !System.IO.File.Exists(checksum) ||
        !System.IO.File.Exists(host) || System.IO.File.ReadAllText(host) != AppiumRunnerHost() ||
        System.IO.File.ReadAllText(checksum) != AppiumRunnerHash(archive))
        throw new Exception($"Acquire the exact platform/host-architecture Appium runner from its build job: {directory}");
    var runtime = ROOT_OUTPUT_PATH.Combine($"appium-runner-workspaces/{Guid.NewGuid():N}");
    EnsureDirectoryExists(runtime);
    EnsureDirectoryExists(results);
    try {
        AppiumTar("-xf", archive, "-C", runtime.FullPath);
        var assembly = runtime.CombineWithFilePath("SkiaSharp.Tests.Samples.dll");
        if (!System.IO.File.Exists(assembly.FullPath))
            throw new Exception($"The acquired source-backed runner assembly is missing: {assembly}");
        SetupSampleDevices();
        var args = new ProcessArgumentBuilder().Append("exec").AppendQuoted(assembly.FullPath)
            .Append("--show-live-output on --no-ansi --results-directory").AppendQuoted(results.FullPath)
            .Append("--report-trx --report-trx-filename TestResults.trx")
            .Append("--filter-trait").AppendQuoted($"RunGroup={group}");
        if (StartProcess("dotnet", new ProcessSettings { Arguments = args, WorkingDirectory = ROOT_PATH }) != 0)
            throw new Exception($"Prepared Appium {group} tests failed.");
    } finally {
        DeleteDirectory(runtime, new DeleteDirectorySettings { Recursive = true, Force = true });
    }
}

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
