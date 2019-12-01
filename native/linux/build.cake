DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../cake/native-shared.cake"

string SUPPORT_GPU_VAR = Argument("supportGpu", EnvironmentVariable("SUPPORT_GPU") ?? "true").ToLower();
bool SUPPORT_GPU = SUPPORT_GPU_VAR == "1" || SUPPORT_GPU_VAR == "true";

string CC = Argument("cc", EnvironmentVariable("CC"));
string CXX = Argument("ccx", EnvironmentVariable("CXX"));
string AR = Argument("ar", EnvironmentVariable("AR"));

string VARIANT = Argument("variant", EnvironmentVariable("LINUX_VARIANT") ?? "linux");

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    Build("x64", "x64", "x64");

    void Build(string arch, string skiaArch, string dir)
    {
        if (Skip(arch)) return;

        var compilers = "";
        if (!string.IsNullOrEmpty(CC))
            compilers += $"cc='{CC}' ";
        if (!string.IsNullOrEmpty(CXX))
            compilers += $"cxx='{CXX}' ";
        if (!string.IsNullOrEmpty(AR))
            compilers += $"ar='{AR}' ";

        var soname = GetVersion("libSkiaSharp", "soname");
        var map = MakeAbsolute((FilePath)"libSkiaSharp/libSkiaSharp.map");

        GnNinja($"{VARIANT}/{arch}", "SkiaSharp",
            $"is_official_build=true skia_enable_tools=false " +
            $"target_os='linux' target_cpu='{arch}' " +
            $"skia_use_icu=false skia_use_sfntly=false skia_use_piex=true " +
            $"skia_use_system_expat=false skia_use_system_freetype2=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false " +
            $"skia_enable_gpu={(SUPPORT_GPU ? "true" : "false")} " +
            $"extra_cflags=[ '-DSKIA_C_DLL' ] " +
            $"extra_ldflags=[ '-static-libstdc++', '-static-libgcc', '-Wl,--version-script={map}' ] " +
            compilers +
            $"linux_soname_version='{soname}'");

        var outDir = OUTPUT_PATH.Combine($"{VARIANT}/{dir}");
        EnsureDirectoryExists(outDir);
        var so = SKIA_PATH.CombineWithFilePath($"out/{VARIANT}/{arch}/libSkiaSharp.so.{soname}");
        CopyFileToDirectory(so, outDir);
        CopyFile(so, outDir.CombineWithFilePath("libSkiaSharp.so"));
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    Build("x64", "x64");

    void Build(string arch, string dir)
    {
        if (Skip(arch)) return;

        var soname = GetVersion("HarfBuzz", "soname");

        RunProcess("make", new ProcessSettings {
            Arguments = $"ARCH={arch} SONAME_VERSION={soname} VARIANT={VARIANT} LDFLAGS=-static-libstdc++",
            WorkingDirectory = "libHarfBuzzSharp",
        });

        var outDir = OUTPUT_PATH.Combine($"{VARIANT}/{dir}");
        EnsureDirectoryExists(outDir);
        var so = $"libHarfBuzzSharp/bin/{VARIANT}/{arch}/libHarfBuzzSharp.so.{soname}";
        CopyFileToDirectory(so, outDir);
        CopyFile(so, outDir.CombineWithFilePath("libHarfBuzzSharp.so"));
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
