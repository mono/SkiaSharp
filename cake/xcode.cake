#addin nuget:?package=Cake.XCode&version=5.0.0

void RunXCodeBuild(FilePath project, string scheme, string sdk, string arch, string platform = null)
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
    if (platform != null) {
        settings.BuildSettings["PLATFORM"] = platform;
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

void SafeCopy(DirectoryPath src, DirectoryPath dst)
{
    EnsureDirectoryExists(dst);
    DeleteDirectory(dst, new DeleteDirectorySettings { Recursive = true, Force = true });
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
