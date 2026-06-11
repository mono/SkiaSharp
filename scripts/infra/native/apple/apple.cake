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

// Combine the per-arch, single-SDK .framework bundles that GN/ninja produced (one per arch out dir)
// into the single shipped framework. GN already owns the bundle: its layout, the framework-relative
// install_name (set at link time), the arm64e thinning, and the provenance-complete Info.plist
// (incl. the DT*/BuildMachineOSBuild keys App Store / notarization validation expects). The only
// thing that cannot live inside a single GN invocation is fusing slices from different arch/SDK out
// dirs, so all this does is:
//   1. copy the base framework (gnFrameworks[0]) - its Info.plist and bundle layout are the shipped
//      ones, so the base must be the framework whose SDK the shipped artifact should advertise (e.g.
//      the iphoneos slice for the device framework, which also carries a legacy simulator slice);
//   2. lipo every slice's binary into the base's universal binary;
//   3. strip and ad-hoc codesign LAST, after lipo (which would otherwise invalidate the signature);
//   4. zip the macOS-style versioned bundle (Mac Catalyst).
// iOS/tvOS/MacCatalyst ship this framework; macOS ships the plain dylib and does not call this.
void CombineFrameworks(DirectoryPath outputFramework, DirectoryPath[] gnFrameworks, bool versioned = false)
{
    if (!IsRunningOnMacOs())
        throw new InvalidOperationException("framework creation is only available on macOS.");

    gnFrameworks = gnFrameworks.Where(f => f != null).ToArray();
    if (gnFrameworks.Length == 0)
        throw new InvalidOperationException($"no GN frameworks to combine into '{outputFramework}'.");

    if (DirectoryExists(outputFramework))
        DeleteDir(outputFramework);
    EnsureDirectoryExists(outputFramework.Combine(".."));

    // The base framework's Info.plist + bundle layout (and symlinks, for versioned bundles) become
    // the shipped ones; -R preserves the symlink structure of versioned frameworks.
    RunProcess("cp", $"-R \"{MakeAbsolute(gnFrameworks[0])}\" \"{MakeAbsolute(outputFramework)}\"");

    // Fuse every per-arch slice into the framework's universal binary.
    var binary = FrameworkBinary(outputFramework, versioned);
    var slices = gnFrameworks.Select(f => FrameworkBinary(f, versioned)).ToArray();
    RunLipo(binary, slices);

    StripSign($"{outputFramework}");

    if (versioned)
        RunZip(outputFramework);
}

// The Mach-O binary inside a .framework is the extension-less file named after the framework, at the
// bundle root (flat iOS/tvOS layout) or under Versions/A (macOS/Mac Catalyst versioned layout).
FilePath FrameworkBinary(DirectoryPath framework, bool versioned)
{
    var name = framework.GetDirectoryName();
    if (name.EndsWith(".framework"))
        name = name.Substring(0, name.Length - ".framework".Length);
    return versioned
        ? framework.Combine("Versions/A").CombineWithFilePath(name)
        : framework.CombineWithFilePath(name);
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

