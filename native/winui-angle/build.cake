DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath ANGLE_PATH = ROOT_PATH.Combine("externals/angle");
DirectoryPath WINAPPSDK_PATH = ROOT_PATH.Combine("externals/winappsdk");
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/winui"));
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

    // generate Windows App SDK files
    if (!FileExists(WINAPPSDK_PATH.Combine("include").CombineWithFilePath("Microsoft.UI.Dispatching.h"))) {
        var winappsdk_version = GetVersion("Microsoft.WindowsAppSDK", "release");
        var stamp = WINAPPSDK_PATH.CombineWithFilePath($"{winappsdk_version}.stamp");

        // Download and extract the NuGet package using .NET HTTP (works on restricted agents
        // where Python's urllib is blocked by firewall policy)
        if (!FileExists(stamp)) {
            var nugetUrl = $"https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/flat2/microsoft.windowsappsdk/{winappsdk_version}/microsoft.windowsappsdk.{winappsdk_version}.nupkg";
            var nupkgPath = WINAPPSDK_PATH.CombineWithFilePath($"{winappsdk_version}.nupkg");
            EnsureDirectoryExists(WINAPPSDK_PATH);
            DownloadFile(nugetUrl, nupkgPath);
            Unzip(nupkgPath, WINAPPSDK_PATH);
            DeleteFile(nupkgPath);
            System.IO.File.WriteAllText(stamp.FullPath, "");
        }

        // Find Windows SDK tools
        var winSdkDir = EnvironmentVariable("WindowsSdkDir")
            ?? "C:/Program Files (x86)/Windows Kits/10";
        var winSdkBinDir = new DirectoryPath(winSdkDir).Combine("bin");
        var sdkVersions = GetDirectories($"{winSdkBinDir}/10.0.*")
            .OrderByDescending(d => d.GetDirectoryName())
            .First();
        var winSdkBin = sdkVersions.Combine("x64");

        var includePath = WINAPPSDK_PATH.Combine("include");
        var libPath = WINAPPSDK_PATH.Combine("lib");
        var uapVersion = "10.0.18362";
        EnsureDirectoryExists(includePath);

        // Build a batch script that processes all WINMD and IDL files,
        // then run it under vcvarsall.bat so midlrt can find cl.exe
        var winmdFiles = new[] {
            ($"uap{uapVersion}/Microsoft.Foundation.winmd", (string)null),
            ($"uap{uapVersion}/Microsoft.Graphics.winmd", "Microsoft.Graphics.DirectX.idl"),
            ($"uap{uapVersion}/Microsoft.UI.winmd", (string)null),
            ("uap10.0/Microsoft.UI.Text.winmd", (string)null),
            ("uap10.0/Microsoft.UI.Xaml.winmd", (string)null),
            ("uap10.0/Microsoft.Web.WebView2.Core.winmd", (string)null),
        };

        var idlFiles = new[] {
            "Microsoft.Foundation.idl",
            "Microsoft.Graphics.DirectX.idl",
            "Microsoft.UI.Composition.idl",
            "Microsoft.UI.Composition.SystemBackdrops.idl",
            "Microsoft.UI.Dispatching.idl",
            "Microsoft.UI.idl",
            "Microsoft.UI.Input.idl",
            "Microsoft.UI.Text.idl",
            "Microsoft.UI.Windowing.idl",
            "Microsoft.UI.Xaml.Automation.idl",
            "Microsoft.UI.Xaml.Automation.Peers.idl",
            "Microsoft.UI.Xaml.Automation.Provider.idl",
            "Microsoft.UI.Xaml.Automation.Text.idl",
            "Microsoft.UI.Xaml.Controls.idl",
            "Microsoft.UI.Xaml.Controls.Primitives.idl",
            "Microsoft.UI.Xaml.Data.idl",
            "Microsoft.UI.Xaml.Documents.idl",
            "Microsoft.UI.Xaml.idl",
            "Microsoft.UI.Xaml.Input.idl",
            "Microsoft.UI.Xaml.Interop.idl",
            "Microsoft.UI.Xaml.Media.Animation.idl",
            "Microsoft.UI.Xaml.Media.idl",
            "Microsoft.UI.Xaml.Media.Imaging.idl",
            "Microsoft.UI.Xaml.Media.Media3D.idl",
            "Microsoft.UI.Xaml.Navigation.idl",
            "Microsoft.Web.WebView2.Core.idl",
        };

        var winmdidl = winSdkBin.CombineWithFilePath("winmdidl.exe").FullPath;
        var midlrt = winSdkBin.CombineWithFilePath("midlrt.exe").FullPath;
        var includeFullPath = includePath.FullPath;
        var libFullPath = libPath.FullPath;

        // Generate a batch script with all winmdidl + midlrt commands
        var bat = new System.Text.StringBuilder();
        bat.AppendLine("@echo off");
        bat.AppendLine($"cd /d \"{includeFullPath}\"");
        bat.AppendLine("if errorlevel 1 exit /b 1");

        foreach (var (winmd, stampFilename) in winmdFiles) {
            var bat_stamp = stampFilename
                ?? (System.IO.Path.GetFileNameWithoutExtension(winmd) + ".idl");
            bat.AppendLine($"if exist \"{includeFullPath}\\{bat_stamp}\" goto :skip_winmd_{bat_stamp.Replace(".", "_")}");
            bat.AppendLine($"\"{winmdidl}\" \"{libFullPath}\\{winmd.Replace("/", "\\")}\" /metadata_dir:C:\\Windows\\System32\\WinMetadata /metadata_dir:\"{libFullPath}\\uap{uapVersion}\" /metadata_dir:\"{libFullPath}\\uap10.0\" /outdir:\"{includeFullPath}\" /nologo");
            bat.AppendLine("if errorlevel 1 exit /b 1");
            bat.AppendLine($":skip_winmd_{bat_stamp.Replace(".", "_")}");
        }

        foreach (var idl in idlFiles) {
            var noExt = System.IO.Path.GetFileNameWithoutExtension(idl);
            bat.AppendLine($"if exist \"{includeFullPath}\\{noExt}.h\" goto :skip_midl_{noExt.Replace(".", "_")}");
            bat.AppendLine($"\"{midlrt}\" \"{includeFullPath}\\{idl}\" /metadata_dir C:\\Windows\\System32\\WinMetadata /ns_prefix /nomidl /nologo");
            bat.AppendLine("if errorlevel 1 exit /b 1");
            bat.AppendLine($":skip_midl_{noExt.Replace(".", "_")}");
        }

        var batPath = WINAPPSDK_PATH.CombineWithFilePath("_generate_headers.bat");
        System.IO.File.WriteAllText(batPath.FullPath, bat.ToString());

        try {
            // Run the batch script under vcvarsall.bat so cl.exe is on PATH for midlrt
            var vcvarsall = ROOT_PATH.CombineWithFilePath("scripts/infra/native/windows/vcvarsall.bat");
            RunProcess(vcvarsall, $"\"{VS_INSTALL}\" \"x64\" \"{batPath}\"");
        } finally {
            if (FileExists(batPath))
                DeleteFile(batPath);
        }
    }
});

Task("ANGLE")
    .IsDependentOn("sync-ANGLE")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    foreach (var arch in new[] { "x86", "x64", "arm64" })
    {
        Build(arch, "libEGL", wasdk: false);
        Build(arch, "libGLESv2", wasdk: true);
    }

    void Build(string arch, string target, bool wasdk)
    {
        if (Skip(arch)) return;

        var suffix = wasdk ? "_wasdk" : "";
        var spectreLibPath = GetSpectreLibPath(arch);

        try
        {
            System.Environment.SetEnvironmentVariable("DEPOT_TOOLS_WIN_TOOLCHAIN", "0");

            RunGn(ANGLE_PATH, $"out/winui{suffix}/{arch}",
                $"target_cpu='{arch}' " +
                $"is_component_build=false " +
                $"is_debug=false " +
                $"is_clang=false " +
                $"angle_is_winappsdk={wasdk} ".ToLower() +
                $"winappsdk_dir='{WINAPPSDK_PATH}' " +
                $"enable_precompiled_headers=false " +
                $"angle_enable_null=false " +
                $"angle_enable_wgpu=false " +
                $"angle_enable_gl_desktop_backend=false " +
                $"angle_enable_vulkan=false " +
                $"extra_cflags=[ '/guard:cf', '/GS' ] " +
                $"extra_ldflags=[ '/guard:cf', '/LIBPATH:{spectreLibPath}' ]");

            RunNinja(ANGLE_PATH, $"out/winui{suffix}/{arch}", target);
        }
        finally
        {
            System.Environment.SetEnvironmentVariable("DEPOT_TOOLS_WIN_TOOLCHAIN", "");
        }

        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory(ANGLE_PATH.CombineWithFilePath($"out/winui{suffix}/{arch}/{target}.dll"), outDir);
        CopyFileToDirectory(ANGLE_PATH.CombineWithFilePath($"out/winui{suffix}/{arch}/{target}.pdb"), outDir);
        CheckWindowsDependencies($"{outDir}/{target}.dll", excluded: VERIFY_EXCLUDED);
    }
});

Task("Default")
    .IsDependentOn("sync-ANGLE")
    .IsDependentOn("ANGLE");

RunTarget(TARGET);
