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

void RunLipo(FilePath output, FilePath[] inputs)
{
    if (!IsRunningOnMacOs())
        throw new InvalidOperationException("lipo is only available on macOS.");

    var inputString = string.Join(" ", inputs.Select(i => string.Format("\"{0}\"", i)));
    RunProcess("lipo", new ProcessSettings {
        Arguments = string.Format("-create -output \"{0}\" {1}", output, inputString),
    });
}

// Assemble a .framework bundle from the GN-produced dylibs. A framework is just a Mach-O dynamic
// library (MH_DYLIB) plus an Info.plist and a code signature in a bundle directory, so:
//   1. lipo the per-arch GN dylibs into the single universal framework binary (GN/ninja builds one
//      arch per out dir, so combining the slices into a fat binary is intrinsic and unavoidable -
//      Xcode itself does the same). The dylib's install_name was already set to the
//      framework-relative @rpath at LINK time by GN (skiasharp_apple_framework), so there is no
//      post-link install_name_tool surgery here;
//   2. generate the Info.plist with make-framework-plist.sh, which reproduces exactly what Xcode's
//      PROCESS_INFOPLIST emitted - including the DT*/BuildMachineOSBuild build-provenance keys that
//      Apple's App Store / notarization validation expects on embedded frameworks;
//   3. lay out the macOS-style versioned bundle (Mac Catalyst) and zip it;
//   4. strip and ad-hoc codesign LAST, after the universal binary and plist are in place.
// iOS/tvOS/MacCatalyst ship this framework; macOS ships the plain dylib and does not call this.
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

    // combine the per-arch GN dylibs into the universal framework binary (no file extension)
    var binary = binDir.CombineWithFilePath(libName);
    RunLipo(binary, dylibs);

    // Info.plist generated the way Xcode did (incl. DT*/BuildMachineOSBuild provenance keys).
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

// Map the framework's primary CFBundleSupportedPlatforms entry to the SDK whose build/version
// provenance the Info.plist should record (matching the SDK Xcode processed the plist under).
string SupportedPlatformToSdk(string platform)
{
    switch (platform) {
        case "iPhoneOS": return "iphoneos";
        case "iPhoneSimulator": return "iphonesimulator";
        case "AppleTVOS": return "appletvos";
        case "AppleTVSimulator": return "appletvsimulator";
        case "MacOSX": return "macosx";
        default: throw new InvalidOperationException($"Unknown framework platform '{platform}'.");
    }
}

// Generate the framework Info.plist by delegating to make-framework-plist.sh, which uses
// first-party Apple tools only (plutil + xcrun/xcodebuild/sw_vers) to reproduce Xcode's
// PROCESS_INFOPLIST output - including the DT*/BuildMachineOSBuild keys required for App Store /
// notarization validation.
void WriteFrameworkPlist(FilePath path, string libName, string minOsVersion, string[] supportedPlatforms, int[] deviceFamily)
{
    EnsureDirectoryExists(path.GetDirectory());

    var isMacCatalyst = supportedPlatforms != null && System.Array.IndexOf(supportedPlatforms, "MacOSX") >= 0;
    var sdk = SupportedPlatformToSdk(supportedPlatforms[0]);
    var script = MakeAbsolute(ROOT_PATH.CombineWithFilePath("scripts/infra/native/apple/make-framework-plist.sh"));

    var args = new System.Collections.Generic.List<string> {
        "--output", $"\"{path}\"",
        "--name", libName,
        "--identifier", $"com.microsoft.{libName}",
        "--sdk", sdk,
        "--min-os", minOsVersion,
        "--supported-platforms",
    };
    args.AddRange(supportedPlatforms);
    if (deviceFamily != null && deviceFamily.Length > 0) {
        args.Add("--device-family");
        args.AddRange(deviceFamily.Select(d => d.ToString()));
    }
    if (isMacCatalyst)
        args.Add("--catalyst");

    RunProcess("bash", $"\"{script}\" {string.Join(" ", args)}");
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

