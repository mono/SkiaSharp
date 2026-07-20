DirectoryPath ANDROID_NDK_HOME = Argument("ndk", EnvironmentVariable("ANDROID_NDK_HOME") ?? EnvironmentVariable("ANDROID_NDK_ROOT") ?? PROFILE_PATH.Combine("android-ndk").FullPath);


void CheckAlignment(FilePath so)
{
    Information($"Making sure that everything is 16 KB aligned...");

    var prebuilt = ANDROID_NDK_HOME.CombineWithFilePath("toolchains/llvm/prebuilt").FullPath;
    var objdump = GetFiles($"{prebuilt}/*/bin/llvm-objdump*").FirstOrDefault() ?? throw new Exception("Could not find llvm-objdump");
    RunProcess(objdump.FullPath, $"-p {so}", out var stdout);

    var loads = stdout
        .Where(l => l.Trim().StartsWith("LOAD"))
        .ToList();

    if (loads.Any(l => !l.Trim().EndsWith("align 2**14"))) {
        Information(String.Join(Environment.NewLine + "    ", stdout));
        throw new Exception($"{so} contained a LOAD that was not 16 KB aligned.");
    } else {
        Information("Everything is 16 KB aligned:");
        Information(String.Join(Environment.NewLine, loads));
    }
}

void StripCopy(FilePath so, DirectoryPath outDir)
{
    Information($"Stripping and copying {so} to {outDir}...");

    EnsureDirectoryExists(outDir);

    CopyFileToDirectory(so, outDir);
    so = outDir.CombineWithFilePath(so.GetFilename());
    CheckAlignment(so);

    ExtractAndStripSymbols(so);
}

FilePath ExtractAndStripSymbols(FilePath so)
{
    Information($"Extracting debug symbols from {so}...");

    var prebuilt = ANDROID_NDK_HOME.Combine("toolchains/llvm/prebuilt");
    var objcopy = GetFiles($"{prebuilt}/*/bin/llvm-objcopy*").FirstOrDefault() ?? throw new Exception("Could not find llvm-objcopy");

    var symbolsFile = so.ChangeExtension(".so.dbg");

    // Record original size before stripping
    var originalSize = new System.IO.FileInfo(so.FullPath).Length;

    // Step 1: Extract debug symbols to separate file
    Information($"Creating symbols file: {symbolsFile}");
    RunProcess(objcopy, $"--only-keep-debug \"{so}\" \"{symbolsFile}\"");

    // Step 2: Strip debug symbols from main binary (keep essential symbols)
    Information($"Stripping debug symbols from main binary: {so}");
    RunProcess(objcopy, $"--strip-debug --strip-unneeded \"{so}\"");

    // Step 3: Add debug link to connect stripped binary with symbols file
    Information($"Linking stripped binary to symbols file...");
    RunProcess(objcopy.FullPath, $"--add-gnu-debuglink=\"{symbolsFile}\" \"{so}\"");

    // Report final sizes
    var strippedSize = new System.IO.FileInfo(so.FullPath).Length;
    var symbolsSize = new System.IO.FileInfo(symbolsFile.FullPath).Length;
    var savedBytes = originalSize - strippedSize;
    var savedPercent = savedBytes / (double)originalSize * 100.0;

    Information($"Symbol extraction complete:");
    Information($"  Original size:   {originalSize / 1024.0 / 1024.0:F1} MB ({originalSize:N0} bytes)");
    Information($"  Stripped binary: {strippedSize / 1024.0 / 1024.0:F1} MB ({strippedSize:N0} bytes)");
    Information($"  Symbols file:    {symbolsSize / 1024.0 / 1024.0:F1} MB ({symbolsSize:N0} bytes)");
    Information($"  Space saved:     {savedBytes / 1024.0 / 1024.0:F1} MB ({savedBytes:N0} bytes, {savedPercent:F1}%)");

    return symbolsFile;
}

void RunNdkBuild(string arch, DirectoryPath working)
{
    var cmd = IsRunningOnWindows() ? ".cmd" : "";
    var ndkbuild = ANDROID_NDK_HOME.CombineWithFilePath($"ndk-build{cmd}").FullPath;

    RunProcess(ndkbuild, new ProcessSettings {
        Arguments = $"APP_ABI={arch}",
        WorkingDirectory = working.FullPath,
    });
}
