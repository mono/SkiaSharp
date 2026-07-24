DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath ANGLE_PATH = ROOT_PATH.Combine("externals/angle");
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/uwp"));
 
string ANGLE_VERSION = GetVersion("ANGLE", "release");
 
#load "../../scripts/infra/native/shared/native-shared.cake"
#load "../../scripts/infra/shared/msbuild.cake"
#load "../../scripts/infra/native/windows/windows-shared.cake"

Task("sync-ANGLE")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    // sync ANGLE
    if (!DirectoryExists(ANGLE_PATH)) {
        RunProcess("git", $"clone https://github.com/google/angle.git --branch {ANGLE_VERSION} --depth 1 --single-branch --shallow-submodules {ANGLE_PATH}");
    }
 
    // sync submodules
    var submodules = new[] {
        "build",
        "testing",
        "third_party/zlib",
        "third_party/jsoncpp",
        "third_party/vulkan-deps",
        "third_party/astc-encoder/src",
        "tools/clang",
    };
    foreach (var submodule in submodules) {
        var sub = ANGLE_PATH.Combine(submodule);
        if (FileExists(sub.CombineWithFilePath("BUILD.gn")) || FileExists(sub.CombineWithFilePath(".gitignore")))
            continue;
        RunProcess("git", new ProcessSettings {
            Arguments = $"submodule update --init --recursive --depth 1 --single-branch {submodule}",
            WorkingDirectory = ANGLE_PATH.FullPath,
        });
    }
 
    // patch the output filenames
    {
        var toolchain = ANGLE_PATH.CombineWithFilePath("build/toolchain/win/toolchain.gni");
        var contents = System.IO.File.ReadAllText(toolchain.FullPath);
        var newContents = contents
            .Replace("\"${dllname}.lib\"", "\"{{output_dir}}/{{target_output_name}}.lib\"")
            .Replace("\"${dllname}.pdb\"", "\"{{output_dir}}/{{target_output_name}}.pdb\"");
        if (contents != newContents)
            System.IO.File.WriteAllText(toolchain.FullPath, newContents);
    }
 
    // set build args
    if (!FileExists(ANGLE_PATH.CombineWithFilePath("build/config/gclient_args.gni"))) {
        var lines = new[] {
            "checkout_angle_internal = false",
            "checkout_angle_mesa = false",
            "checkout_angle_restricted_traces = false",
            "generate_location_tags = false"
        };
        System.IO.File.WriteAllLines(ANGLE_PATH.CombineWithFilePath("build/config/gclient_args.gni").FullPath, lines);
    }
 
    // set version numbers
    if (!FileExists(ANGLE_PATH.CombineWithFilePath("build/util/LASTCHANGE"))) {
        var lastchange = ANGLE_PATH.CombineWithFilePath("build/util/LASTCHANGE");
        RunPython(ANGLE_PATH, ANGLE_PATH.CombineWithFilePath("build/util/lastchange.py"), $"-o {lastchange}");
    }
 
    // download rc.exe
    var rc_exe = "build/toolchain/win/rc/win/rc.exe";
    var rcPath = ANGLE_PATH.CombineWithFilePath(rc_exe);
    if (!FileExists(rcPath)) {
        var shaPath = ANGLE_PATH.CombineWithFilePath($"{rc_exe}.sha1");
        var sha = System.IO.File.ReadAllText(shaPath.FullPath);
        var url = $"https://storage.googleapis.com/download/storage/v1/b/chromium-browser-clang/o/rc%2F{sha}?alt=media";
        DownloadFile(url, rcPath);
    }
 
    // download llvm
    if (!FileExists(ANGLE_PATH.CombineWithFilePath("third_party/llvm-build/Release+Asserts/cr_build_revision"))) {
        RunPython(ANGLE_PATH, ANGLE_PATH.CombineWithFilePath("tools/clang/scripts/update.py"));
    }
 
// patch platform_helpers.cpp for UWP ARM64:
// the SDK only declares VerSetConditionMask / VerifyVersionInfoW for the
// desktop partition on ARM64 (x86/x64 get an inline VerSetConditionMask)
{
    var helpers = ANGLE_PATH.CombineWithFilePath("src/common/platform_helpers.cpp");
    var contents = System.IO.File.ReadAllText(helpers.FullPath);
    if (!contents.Contains("UWP ARM64 fix")) {
        var fix = @"
// UWP ARM64 fix: on ARM64 the Windows SDK declares these APIs only for the
// desktop partition, so under WINAPI_FAMILY_PC_APP they are missing.
// VerifyVersionInfoW links fine in UWP; VerSetConditionMask is reimplemented
// locally (it is pure bit math, mirroring the inline x86/x64 SDK version).
#if defined(_M_ARM64) && defined(WINAPI_FAMILY) && (WINAPI_FAMILY != WINAPI_FAMILY_DESKTOP_APP)
extern ""C"" WINBASEAPI BOOL WINAPI VerifyVersionInfoW(LPOSVERSIONINFOEXW lpVersionInformation,
                                                       DWORD dwTypeMask,
                                                       DWORDLONG dwlConditionMask);
static ULONGLONG VerSetConditionMask(ULONGLONG conditionMask, DWORD typeMask, BYTE condition)
{
    condition &= 0x7;  // VER_CONDITION_MASK
    if (typeMask == 0 || condition == 0)
        return conditionMask;

    ULONGLONG shift = 0;
    if (typeMask & VER_PRODUCT_TYPE)          shift = 7;
    else if (typeMask & VER_SUITENAME)        shift = 6;
    else if (typeMask & VER_SERVICEPACKMAJOR) shift = 5;
    else if (typeMask & VER_SERVICEPACKMINOR) shift = 4;
    else if (typeMask & VER_PLATFORMID)       shift = 3;
    else if (typeMask & VER_BUILDNUMBER)      shift = 2;
    else if (typeMask & VER_MINORVERSION)     shift = 1;
    else                                      shift = 0;  // VER_MAJORVERSION

    return conditionMask | ((ULONGLONG)condition << (shift * 3 /* VER_NUM_BITS_PER_CONDITION_MASK */));
}
#endif

";
        var idx = contents.IndexOf("namespace angle");
        var newContents = contents.Insert(idx, fix);
        System.IO.File.WriteAllText(helpers.FullPath, newContents);
    }
}


    // NOTE: unlike the WinUI (winui-angle) build, the UWP build does NOT
    // use the Windows App SDK. Modern .NET UWP apps use the in-box
    // Windows.UI.Xaml stack, so no WinAppSDK download or header
    // generation is needed here (angle_is_winappsdk stays false).
});
 
Task("ANGLE")
    .IsDependentOn("sync-ANGLE")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    foreach (var arch in new[] { "x64", "x86", "arm64" })
    {
        Build(arch, "libEGL");
        Build(arch, "libGLESv2");
    }
 
    void Build(string arch, string target)
    {
        if (Skip(arch)) return;
 
        var spectreLibPath = GetSpectreLibPath(arch);
 
        try
        {
            System.Environment.SetEnvironmentVariable("DEPOT_TOOLS_WIN_TOOLCHAIN", "0");
            RunGn(ANGLE_PATH, $"out/winuwp/{arch}",
                $"target_os='winuwp' " +
                $"target_cpu='{arch}' " +
                $"is_component_build=false " +
                $"is_debug=false " +
                $"is_clang=false " +
                $"angle_is_winappsdk=false " +
                $"enable_precompiled_headers=false " +
                $"angle_enable_null=false " +
                $"angle_enable_wgpu=false " +
                $"angle_enable_gl_desktop_backend=false " +
                $"angle_enable_vulkan=false " +
                $"extra_cflags=[ '/guard:cf', '/GS' ] " +
                $"extra_ldflags=[ '/guard:cf', '/LIBPATH:{spectreLibPath}' ]");
            RunNinja(ANGLE_PATH, $"out/winuwp/{arch}", target);
        }
        finally
        {
            System.Environment.SetEnvironmentVariable("DEPOT_TOOLS_WIN_TOOLCHAIN", "");
        }
 
        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory(ANGLE_PATH.CombineWithFilePath($"out/winuwp/{arch}/{target}.dll"), outDir);
        CopyFileToDirectory(ANGLE_PATH.CombineWithFilePath($"out/winuwp/{arch}/{target}.pdb"), outDir);
    }
});
 
Task("Default")
    .IsDependentOn("sync-ANGLE")
    .IsDependentOn("ANGLE");
 
RunTarget(TARGET);