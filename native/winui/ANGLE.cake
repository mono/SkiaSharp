void InitializeAngle(string branch, DirectoryPath ANGLE_PATH, DirectoryPath WINAPPSDK_PATH)
{
    if (!DirectoryExists(ANGLE_PATH)) {
        RunProcess("git", $"clone https://github.com/google/angle.git --branch {branch} --depth 1 --single-branch --shallow-submodules {ANGLE_PATH}");
    }

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

    {
        var toolchain = ANGLE_PATH.CombineWithFilePath("build/toolchain/win/toolchain.gni");
        var contents = System.IO.File.ReadAllText(toolchain.FullPath);
        var newContents = contents
            .Replace("\"${dllname}.lib\"", "\"{{output_dir}}/{{target_output_name}}.lib\"")
            .Replace("\"${dllname}.pdb\"", "\"{{output_dir}}/{{target_output_name}}.pdb\"");
        if (contents != newContents)
            System.IO.File.WriteAllText(toolchain.FullPath, newContents);
    }

    if (!FileExists(ANGLE_PATH.CombineWithFilePath("build/config/gclient_args.gni"))) {
        var lines = new[] {
            "checkout_angle_internal = false",
            "checkout_angle_mesa = false",
            "checkout_angle_restricted_traces = false",
            "generate_location_tags = false"
        };
        System.IO.File.WriteAllLines(ANGLE_PATH.CombineWithFilePath("build/config/gclient_args.gni").FullPath, lines);
    }

    if (!FileExists(ANGLE_PATH.CombineWithFilePath("build/util/LASTCHANGE"))) {
        var lastchange = ANGLE_PATH.CombineWithFilePath("build/util/LASTCHANGE");
        RunPython(ANGLE_PATH, ANGLE_PATH.CombineWithFilePath("build/util/lastchange.py"), $"-o {lastchange}");
    }

    if (!FileExists(ANGLE_PATH.CombineWithFilePath("build/toolchain/win/rc/win/rc.exe"))) {
        var oldPath = EnvironmentVariable("PATH");
        try {
            System.Environment.SetEnvironmentVariable("PATH", DEPOT_PATH.FullPath + System.IO.Path.PathSeparator + oldPath);

            RunPython(ANGLE_PATH,
                DEPOT_PATH.CombineWithFilePath("download_from_google_storage.py"),
                $"--no_resume --no_auth --bucket chromium-browser-clang/rc -s build/toolchain/win/rc/win/rc.exe.sha1");
        } finally {
            System.Environment.SetEnvironmentVariable("PATH", oldPath);
        }
    }

    if (!FileExists(ANGLE_PATH.CombineWithFilePath("third_party/llvm-build/Release+Asserts/cr_build_revision"))) {
        RunPython(ANGLE_PATH, ANGLE_PATH.CombineWithFilePath("tools/clang/scripts/update.py"));
    }

    if (!FileExists(WINAPPSDK_PATH.CombineWithFilePath("Microsoft.WindowsAppSDK.nuspec"))) {
        var setup = ANGLE_PATH.CombineWithFilePath("scripts/winappsdk_setup.py");
        RunProcess(
            ROOT_PATH.CombineWithFilePath("scripts/vcvarsall.bat"),
            $"\"{VS_INSTALL}\" \"x64\" \"{PYTHON_EXE}\" \"{setup}\" --output \"{WINAPPSDK_PATH}\"");
    }
}
