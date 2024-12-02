DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath ANGLE_PATH = ROOT_PATH.Combine("externals/angle");
DirectoryPath WINAPPSDK_PATH = ROOT_PATH.Combine("externals/winappsdk");
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/winui"));
string ANGLE_VERSION = GetVersion("ANGLE", "release");

#load "../../scripts/cake/native-shared.cake"
#load "../../scripts/cake/msbuild.cake"

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

    // long the free space
    RunProcess("pwsh", ROOT_PATH.CombineWithFilePath("scripts/get-free-space.ps1").FullPath);

    // generate Windows App SDK files
    if (!FileExists(WINAPPSDK_PATH.CombineWithFilePath("Microsoft.WindowsAppSDK.nuspec"))) {
        var setup = ANGLE_PATH.CombineWithFilePath("scripts/winappsdk_setup.py");
        RunProcess(
            ROOT_PATH.CombineWithFilePath("scripts/vcvarsall.bat"),
            $"\"{VS_INSTALL}\" \"x64\" \"{PYTHON_EXE}\" \"{setup}\" --output \"{WINAPPSDK_PATH}\"");
    }

    // long the free space
    RunProcess("pwsh", ROOT_PATH.CombineWithFilePath("scripts/get-free-space.ps1").FullPath);
});

Task("ANGLE")
    .IsDependentOn("sync-ANGLE")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    Build("x86");
    Build("x64");
    Build("arm64");

    void Build(string arch)
    {
        if (Skip(arch)) return;

        try {
            System.Environment.SetEnvironmentVariable("DEPOT_TOOLS_WIN_TOOLCHAIN", "0");

            RunGn(ANGLE_PATH, $"out/winui/{arch}", 
                $"target_cpu='{arch}' " +
                $"is_component_build=false " +
                $"is_debug=false " +
                $"is_clang=false " +
                $"angle_is_winappsdk=true " +
                $"winappsdk_dir='{WINAPPSDK_PATH}' " +
                $"enable_precompiled_headers=false " +
                $"angle_enable_null=false " +
                $"angle_enable_wgpu=false " +
                $"angle_enable_gl_desktop_backend=false " +
                $"angle_enable_vulkan=false");

            RunNinja(ANGLE_PATH, $"out/winui/{arch}", "libEGL libGLESv2");
        } finally {
            System.Environment.SetEnvironmentVariable("DEPOT_TOOLS_WIN_TOOLCHAIN", "");
        }

        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory(ANGLE_PATH.CombineWithFilePath($"out/winui/{arch}/libEGL.dll"), outDir);
        CopyFileToDirectory(ANGLE_PATH.CombineWithFilePath($"out/winui/{arch}/libEGL.pdb"), outDir);
        CopyFileToDirectory(ANGLE_PATH.CombineWithFilePath($"out/winui/{arch}/libGLESv2.dll"), outDir);
        CopyFileToDirectory(ANGLE_PATH.CombineWithFilePath($"out/winui/{arch}/libGLESv2.pdb"), outDir);
    }
});

Task("Default")
    .IsDependentOn("sync-ANGLE")
    .IsDependentOn("ANGLE");

RunTarget(TARGET);
