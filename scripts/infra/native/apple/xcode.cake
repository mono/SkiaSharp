#addin nuget:?package=Cake.XCode&version=5.0.0

void RunXCodeBuild(FilePath project, string scheme, string sdk, string arch, Dictionary<string, string> properties = null)
{
    var dir = project.GetDirectory();

    var settings = new XCodeBuildSettings {
        Project = project.FullPath,
        Scheme = scheme,
        Sdk = sdk,
        Arch = arch,
        Archive = true,
        Configuration = CONFIGURATION,
        DerivedDataPath = dir.Combine($"obj/{CONFIGURATION}/{sdk}/{arch}"),
        ArchivePath = dir.Combine($"bin/{CONFIGURATION}/{sdk}/{arch}"),
        BuildSettings = new Dictionary<string, string> {
            { "SKIP_INSTALL", "NO" },
            { "BUILD_LIBRARIES_FOR_DISTRIBUTION", "YES" },
        },
    };
    if (properties != null) {
        foreach (var prop in properties) {
            settings.BuildSettings[prop.Key] = prop.Value;
        }
    }

    XCodeBuild(settings);
}

void StripSign(FilePath target)
{
    if (!IsRunningOnMacOs())
        throw new InvalidOperationException("strip and codesign are only available on macOS.");

    target = MakeAbsolute(target);
    var archive = target;
    if (target.FullPath.EndsWith(".framework")) {
        archive = $"{target}/{target.GetFilenameWithoutExtension()}";
    }

    // strip anything we can
    RunProcess("strip", new ProcessSettings {
        Arguments = $"-x -S {archive}",
    });

    // re-sign with empty
    RunProcess("codesign", new ProcessSettings {
        Arguments = $"--force --sign - --timestamp=none {target}",
    });
}

void RunLipo(DirectoryPath directory, FilePath output, FilePath[] inputs)
{
    if (!IsRunningOnMacOs())
        throw new InvalidOperationException("lipo is only available on macOS.");

    EnsureDirectoryExists(directory.CombineWithFilePath(output).GetDirectory());

    var inputString = string.Join(" ", inputs.Select(i => string.Format("\"{0}\"", i)));
    RunProcess("lipo", new ProcessSettings {
        Arguments = string.Format("-create -output \"{0}\" {1}", output, inputString),
        WorkingDirectory = directory,
    });
}

void RunLipo(FilePath output, FilePath[] inputs)
{
    if (!IsRunningOnMacOs())
        throw new InvalidOperationException("lipo is only available on macOS.");

    var inputString = string.Join(" ", inputs.Select(i => string.Format("\"{0}\"", i)));
    RunProcess("lipo", new ProcessSettings {
        Arguments = string.Format("-create -output \"{0}\" {1}", output, inputString),
    });
}

void CreateFatDylib(DirectoryPath archives)
{
    var libName = archives.GetDirectoryName();

    var binaries = GetFiles($"{archives}/*.xcarchive/Products/@rpath/{libName}.dylib").ToArray();
    RunLipo($"{archives}.dylib", binaries);

    StripSign($"{archives}.dylib");
}

void CreateFatFramework(DirectoryPath archives)
{
    var libName = archives.GetDirectoryName();

    var frameworks = GetDirectories($"{archives}/*.xcarchive/Products/Library/Frameworks/{libName}.framework").ToArray();
    SafeCopy(frameworks[0], $"{archives}.framework");
    DeleteFile($"{archives}.framework/{libName}");

    var binaries = GetFiles($"{archives}/*.xcarchive/Products/Library/Frameworks/{libName}.framework/{libName}").ToArray();
    RunLipo($"{archives}.framework/{libName}", binaries);

    StripSign($"{archives}.framework");
}

void CreateFatVersionedFramework(DirectoryPath archives)
{
    var libName = archives.GetDirectoryName();

    var frameworks = GetDirectories($"{archives}/*.xcarchive/Products/Library/Frameworks/{libName}.framework").ToArray();
    SafeCopy(frameworks[0], $"{archives}.framework");
    DeleteFile($"{archives}.framework/Versions/A/{libName}");

    var binaries = GetFiles($"{archives}/*.xcarchive/Products/Library/Frameworks/{libName}.framework/Versions/A/{libName}").ToArray();
    RunLipo($"{archives}.framework/Versions/A/{libName}", binaries);

    StripSign($"{archives}.framework");

    RunZip($"{archives}.framework");
}

// mono/skia: build a .framework bundle directly from GN-produced dylibs, replacing the
// xcodebuild-produced dynamic framework. The framework binary is the SAME Mach-O dynamic
// library GN already emits (MH_DYLIB); we only lipo the per-arch slices together, rename
// the binary to the framework convention (no extension), rewrite its install_name to the
// framework-relative @rpath, write a deterministic Info.plist, ad-hoc codesign, and (for
// MacCatalyst) lay out the versioned bundle and zip it. This makes a drop-in replacement
// for the old xcodebuild framework without any hand-maintained xcodeproj.
void CreateFrameworkFromDylibs(
    DirectoryPath frameworkPath,
    FilePath[] dylibs,
    string minOsVersion,
    string[] supportedPlatforms,
    int[] deviceFamily,
    bool versioned = false)
{
    if (!IsRunningOnMacOs())
        throw new InvalidOperationException("framework creation is only available on macOS.");

    var libName = frameworkPath.GetDirectoryName();
    if (libName.EndsWith(".framework"))
        libName = libName.Substring(0, libName.Length - ".framework".Length);

    if (DirectoryExists(frameworkPath))
        DeleteDir(frameworkPath);

    var binDir = versioned ? frameworkPath.Combine("Versions/A") : (DirectoryPath)frameworkPath;
    var resourcesDir = versioned ? frameworkPath.Combine("Versions/A/Resources") : (DirectoryPath)frameworkPath;
    EnsureDirectoryExists(binDir);
    EnsureDirectoryExists(resourcesDir);

    // combine the per-arch GN dylibs into the framework binary (no file extension)
    var binary = binDir.CombineWithFilePath(libName);
    RunLipo(binary, dylibs);

    // GN's solink sets the install name to @rpath/<lib>.dylib; a framework needs the
    // framework-relative install name so the runtime resolves @rpath/<lib>.framework/<lib>.
    var installName = versioned
        ? $"@rpath/{libName}.framework/Versions/A/{libName}"
        : $"@rpath/{libName}.framework/{libName}";
    RunProcess("install_name_tool", $"-id \"{installName}\" \"{binary}\"");

    // deterministic Info.plist with only the stable, contract-relevant keys (the volatile
    // DT*/BuildMachineOSBuild keys xcodebuild emits differ build-to-build and are not shipped contract).
    WriteFrameworkPlist(resourcesDir.CombineWithFilePath("Info.plist"), libName, minOsVersion, supportedPlatforms, deviceFamily);

    if (versioned) {
        RunProcess("ln", $"-sfh A \"{frameworkPath}/Versions/Current\"");
        RunProcess("ln", $"-sfh Versions/Current/{libName} \"{frameworkPath}/{libName}\"");
        RunProcess("ln", $"-sfh Versions/Current/Resources \"{frameworkPath}/Resources\"");
    }

    StripSign($"{frameworkPath}");

    if (versioned)
        RunZip(frameworkPath);
}

void WriteFrameworkPlist(FilePath path, string libName, string minOsVersion, string[] supportedPlatforms, int[] deviceFamily)
{
    var sb = new System.Text.StringBuilder();
    sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
    sb.AppendLine("<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">");
    sb.AppendLine("<plist version=\"1.0\">");
    sb.AppendLine("<dict>");
    sb.AppendLine("\t<key>CFBundleDevelopmentRegion</key>");
    sb.AppendLine("\t<string>en</string>");
    sb.AppendLine("\t<key>CFBundleExecutable</key>");
    sb.AppendLine($"\t<string>{libName}</string>");
    sb.AppendLine("\t<key>CFBundleIdentifier</key>");
    sb.AppendLine($"\t<string>com.microsoft.{libName}</string>");
    sb.AppendLine("\t<key>CFBundleInfoDictionaryVersion</key>");
    sb.AppendLine("\t<string>6.0</string>");
    sb.AppendLine("\t<key>CFBundleName</key>");
    sb.AppendLine($"\t<string>{libName}</string>");
    sb.AppendLine("\t<key>CFBundlePackageType</key>");
    sb.AppendLine("\t<string>FMWK</string>");
    sb.AppendLine("\t<key>CFBundleShortVersionString</key>");
    sb.AppendLine("\t<string>1.0</string>");
    sb.AppendLine("\t<key>CFBundleSignature</key>");
    sb.AppendLine("\t<string>????</string>");
    sb.AppendLine("\t<key>CFBundleVersion</key>");
    sb.AppendLine("\t<string>1</string>");
    if (supportedPlatforms != null && supportedPlatforms.Length > 0) {
        sb.AppendLine("\t<key>CFBundleSupportedPlatforms</key>");
        sb.AppendLine("\t<array>");
        foreach (var p in supportedPlatforms)
            sb.AppendLine($"\t\t<string>{p}</string>");
        sb.AppendLine("\t</array>");
    }
    if (!string.IsNullOrEmpty(minOsVersion)) {
        // MacCatalyst frameworks express their minimum as the macOS-form LSMinimumSystemVersion;
        // iOS/tvOS frameworks use the iOS-form MinimumOSVersion.
        var isMacCatalyst = supportedPlatforms != null && System.Array.IndexOf(supportedPlatforms, "MacOSX") >= 0;
        sb.AppendLine(isMacCatalyst ? "\t<key>LSMinimumSystemVersion</key>" : "\t<key>MinimumOSVersion</key>");
        sb.AppendLine($"\t<string>{minOsVersion}</string>");
    }
    if (deviceFamily != null && deviceFamily.Length > 0) {
        sb.AppendLine("\t<key>UIDeviceFamily</key>");
        sb.AppendLine("\t<array>");
        foreach (var d in deviceFamily)
            sb.AppendLine($"\t\t<integer>{d}</integer>");
        sb.AppendLine("\t</array>");
    }
    sb.AppendLine("</dict>");
    sb.AppendLine("</plist>");

    EnsureDirectoryExists(path.GetDirectory());
    System.IO.File.WriteAllText(path.FullPath, sb.ToString());
}

void SafeCopy(DirectoryPath src, DirectoryPath dst)
{
    EnsureDirectoryExists(dst);
    DeleteDir(dst);
    RunProcess("cp", $"-R {src} {dst}");
}

void RunZip(DirectoryPath src)
{
    var dir = src.Combine("..");
    var dst = (FilePath)(src.FullPath + ".zip");
    if (FileExists(dst))
        DeleteFile(dst);
    RunProcess("zip", new ProcessSettings {
        Arguments = $"-yr {dst} {src.GetDirectoryName()}",
        WorkingDirectory = dir.FullPath,
    });
}

// mono/skia: GN's is_ios config force-adds an "arm64e" slice for non-simulator arm64
// builds. The shipped frameworks have only the plain arm64 slice, so thin any GN dylib
// down to the single requested arch to keep the produced binaries drop-in identical.
void EnsureSingleArch(FilePath dylib, string machArch)
{
    if (!IsRunningOnMacOs())
        throw new InvalidOperationException("lipo is only available on macOS.");

    RunProcess("lipo", $"-archs \"{dylib}\"", out var lines);
    var archs = string.Join(" ", lines).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    if (archs.Length <= 1)
        return;

    var tmp = dylib.GetDirectory().CombineWithFilePath($"{dylib.GetFilename()}.thin");
    RunProcess("lipo", $"\"{dylib}\" -thin {machArch} -output \"{tmp}\"");
    DeleteFile(dylib);
    MoveFile(tmp, dylib);
}

