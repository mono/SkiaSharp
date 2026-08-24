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
            { "DEBUG_INFORMATION_FORMAT", "dwarf-with-dsym" },
        },
    };
    if (properties != null) {
        foreach (var prop in properties) {
            settings.BuildSettings[prop.Key] = prop.Value;
        }
    }

    XCodeBuild(settings);
    ValidateArchiveDsym($"{settings.ArchivePath}.xcarchive", scheme);
}

void ValidateArchiveDsym(DirectoryPath archive, string moduleName)
{
    var runtime = GetArchiveRuntime(archive, moduleName);
    var dwarf = GetArchiveDwarf(archive, runtime, moduleName);
    var runtimeUuids = ReadMachOUuids(runtime);
    var dwarfUuids = ReadMachOUuids(dwarf);
    ValidateMachOUuidSets(runtimeUuids, new[] { dwarfUuids }, $"{archive}/{moduleName}");

    RunProcess("otool", $"-l \"{dwarf}\"", out var loadCommands);
    if (loadCommands.Any(line => line.Trim() == "cmd LC_CODE_SIGNATURE"))
        throw new InvalidOperationException($"The dSYM DWARF must not be code-signed: {dwarf}");
}

FilePath GetArchiveRuntime(DirectoryPath archive, string moduleName)
{
    var dylib = archive.CombineWithFilePath($"Products/@rpath/{moduleName}.dylib");
    var versionedFramework = archive.CombineWithFilePath(
        $"Products/Library/Frameworks/{moduleName}.framework/Versions/A/{moduleName}");
    var framework = archive.CombineWithFilePath(
        $"Products/Library/Frameworks/{moduleName}.framework/{moduleName}");

    if (FileExists(dylib))
        return dylib;
    if (FileExists(versionedFramework))
        return versionedFramework;
    if (FileExists(framework))
        return framework;
    throw new InvalidOperationException($"The expected archived runtime was not produced for {moduleName}: {archive}");
}

FilePath GetArchiveDwarf(DirectoryPath archive, FilePath runtime, string moduleName)
{
    var isDylib = runtime.GetExtension() == ".dylib";
    var dsymName = isDylib ? $"{moduleName}.dylib.dSYM" : $"{moduleName}.framework.dSYM";
    var dwarfName = isDylib ? $"{moduleName}.dylib" : moduleName;
    var dwarf = archive.CombineWithFilePath($"dSYMs/{dsymName}/Contents/Resources/DWARF/{dwarfName}");
    if (!FileExists(dwarf))
        throw new InvalidOperationException($"The expected dSYM DWARF was not produced: {dwarf}");
    return dwarf;
}

HashSet<string> ReadMachOUuids(FilePath file)
{
    RunProcess("dwarfdump", $"--uuid \"{file}\"", out var output);
    var uuids = new HashSet<string>(
        MatchRegex(@"UUID:\s+([0-9A-Fa-f-]+)", output.ToArray()),
        StringComparer.OrdinalIgnoreCase);
    if (uuids.Count == 0)
        throw new InvalidOperationException($"No Mach-O UUIDs were found in {file}.");
    return uuids;
}

void ValidateStagedDsyms(DirectoryPath archives, FilePath runtime)
{
    var moduleName = archives.GetDirectoryName();
    var dwarfUuids = GetDirectories($"{archives}/*.xcarchive")
        .Select(archive => ReadMachOUuids(GetArchiveDwarf(archive, GetArchiveRuntime(archive, moduleName), moduleName)))
        .ToArray();
    if (dwarfUuids.Length == 0)
        throw new InvalidOperationException($"No staged dSYM archives were found in {archives}.");
    ValidateMachOUuidSets(ReadMachOUuids(runtime), dwarfUuids, runtime.FullPath);
}

void ValidateMachOUuidSets(
    HashSet<string> runtimeUuids,
    IEnumerable<HashSet<string>> dwarfUuidSets,
    string description)
{
    var dwarfUuids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var dwarfSet in dwarfUuidSets) {
        foreach (var uuid in dwarfSet) {
            if (!dwarfUuids.Add(uuid))
                throw new InvalidOperationException($"Duplicate dSYM UUID for {description}: {uuid}");
        }
    }
    if (!runtimeUuids.SetEquals(dwarfUuids)) {
        throw new InvalidOperationException(
            $"Runtime and dSYM UUIDs do not match for {description}. " +
            $"Runtime: {string.Join(", ", runtimeUuids)}; dSYM: {string.Join(", ", dwarfUuids)}");
    }
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
    ValidateStagedDsyms(archives, $"{archives}.dylib");
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
    ValidateStagedDsyms(archives, $"{archives}.framework/{libName}");
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
    ValidateStagedDsyms(archives, $"{archives}.framework/Versions/A/{libName}");

    RunZip($"{archives}.framework");
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
