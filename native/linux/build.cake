DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../scripts/cake/native-shared.cake"

string SUPPORT_GPU_VAR = Argument("supportGpu", EnvironmentVariable("SUPPORT_GPU") ?? "true").ToLower();
bool SUPPORT_GPU = SUPPORT_GPU_VAR == "1" || SUPPORT_GPU_VAR == "true";

string SUPPORT_VULKAN_VAR = Argument("supportVulkan", EnvironmentVariable("SUPPORT_VULKAN") ?? "true");
bool SUPPORT_VULKAN = SUPPORT_VULKAN_VAR == "1" || SUPPORT_VULKAN_VAR.ToLower() == "true";

var VERIFY_EXCLUDED = Argument("verifyExcluded", Argument("verifyexcluded", ""))
    .ToLower().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

var VERIFY_GLIBC_MAX_VAR = Argument("verifyGlibcMax", Argument("verifyglibcmax", "2.28"));
var VERIFY_GLIBC_MAX = string.IsNullOrEmpty(VERIFY_GLIBC_MAX_VAR) ? null : System.Version.Parse(VERIFY_GLIBC_MAX_VAR);

string CC = Argument("cc", EnvironmentVariable("CC"));
string CXX = Argument("cxx", EnvironmentVariable("CXX"));
string AR = Argument("ar", EnvironmentVariable("AR"));

string VARIANT = string.IsNullOrEmpty(BUILD_VARIANT) ? "linux" : BUILD_VARIANT?.Trim();

if (BUILD_ARCH.Length == 0)
    BUILD_ARCH = new [] { "x64" };

var COMPILERS = "";
if (!string.IsNullOrEmpty(CC))
    COMPILERS += $"cc='{CC}' ";
if (!string.IsNullOrEmpty(CXX))
    COMPILERS += $"cxx='{CXX}' ";
if (!string.IsNullOrEmpty(AR))
    COMPILERS += $"ar='{AR}' ";

void CheckDeps(FilePath so)
{
    Information($"Making sure that there are no dependencies on: {string.Join(", ", VERIFY_EXCLUDED)}");

    RunProcess("readelf", $"-dV {so}", out var stdoutEnum);
    var stdout = stdoutEnum.ToArray();

    var needed = MatchRegex(@"\(NEEDED\).+\[(.+)\]", stdout).ToList();

    Information("Dependencies:");
    foreach (var need in needed) {
        Information($"    {need}");
    }

    foreach (var exclude in VERIFY_EXCLUDED) {
        if (needed.Any(o => o.Contains(exclude.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new Exception($"{so} contained a dependency on {exclude}.");
    }

    var glibcs = MatchRegex(@"GLIBC_([\w\.\d]+)", stdout).Distinct().ToList();
    glibcs.Sort();

    Information("GLIBC:");
    foreach (var glibc in glibcs) {
        Information($"    {glibc}");
    }
    
    if (VERIFY_GLIBC_MAX != null) {
        foreach (var glibc in glibcs) {
            var version = System.Version.Parse(glibc);
            if (version > VERIFY_GLIBC_MAX)
                throw new Exception($"{so} contained a dependency on GLIBC {glibc} which is greater than the expected GLIBC {VERIFY_GLIBC_MAX}.");
        }
    }
}

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    // patch the gclient_paths.py for Python 3.7
    {
        var gclient = DEPOT_PATH.CombineWithFilePath("gclient_paths.py");
        var contents = System.IO.File.ReadAllText(gclient.FullPath);
        var newContents = contents
            .Replace("@functools.lru_cache", "@functools.lru_cache()")
            .Replace("@functools.lru_cache()()", "@functools.lru_cache()");
        if (contents != newContents)
            System.IO.File.WriteAllText(gclient.FullPath, newContents);
    }

    foreach (var arch in BUILD_ARCH) {
        if (Skip(arch)) return;

        var soname = GetVersion("libSkiaSharp", "soname");
        var map = MakeAbsolute((FilePath)"libSkiaSharp/libSkiaSharp.map");

        // This is terrible! But, Alpine (musl) does not define this
        // so we are forced to for dng_sdk. If this ever becomes a problem
        // for other libraries, we will need to find a better solution.
        var wordSize = ReduceArch(arch).EndsWith("64") ? "64" : "32";
        var wordSizeDefine = VARIANT.ToLower().StartsWith("alpine")
            ? $", '-D__WORDSIZE={wordSize}'"
            : $"";

        GnNinja($"{VARIANT}/{arch}", "SkiaSharp",
            $"target_os='linux' " +
            $"target_cpu='{arch}' " +
            $"skia_enable_ganesh={(SUPPORT_GPU ? "true" : "false")} " +
            $"skia_use_harfbuzz=false " +
            $"skia_use_icu=false " +
            $"skia_use_piex=true " +
            $"skia_use_sfntly=false " +
            $"skia_use_system_expat=false " +
            $"skia_use_system_freetype2=false " +
            $"skia_use_system_libjpeg_turbo=false " +
            $"skia_use_system_libpng=false " +
            $"skia_use_system_libwebp=false " +
            $"skia_use_system_zlib=false " +
            $"skia_enable_skottie=true " +
            $"skia_use_vulkan={SUPPORT_VULKAN} ".ToLower() +
            $"extra_asmflags=[] " +
            $"extra_cflags=[ '-DSKIA_C_DLL', '-DHAVE_SYSCALL_GETRANDOM', '-DXML_DEV_URANDOM' {wordSizeDefine} ] " +
            $"extra_ldflags=[ '-static-libstdc++', '-static-libgcc', '-Wl,--version-script={map}' ] " +
            COMPILERS +
            $"linux_soname_version='{soname}' " +
            ADDITIONAL_GN_ARGS);

        var outDir = OUTPUT_PATH.Combine($"{VARIANT}/{arch}");
        EnsureDirectoryExists(outDir);
        var so = SKIA_PATH.CombineWithFilePath($"out/{VARIANT}/{arch}/libSkiaSharp.so.{soname}");
        CopyFileToDirectory(so, outDir);
        CopyFile(so, outDir.CombineWithFilePath("libSkiaSharp.so"));

        CheckDeps(so);
    }
});

Task("libHarfBuzzSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    foreach (var arch in BUILD_ARCH) {
        if (Skip(arch)) return;

        var soname = GetVersion("HarfBuzz", "soname");
        var map = MakeAbsolute((FilePath)"libHarfBuzzSharp/libHarfBuzzSharp.map");

        GnNinja($"{VARIANT}/{arch}", "HarfBuzzSharp",
            $"target_os='linux' " +
            $"target_cpu='{arch}' " +
            $"visibility_hidden=false " +
            $"extra_asmflags=[] " +
            $"extra_cflags=[] " +
            $"extra_ldflags=[ '-static-libstdc++', '-static-libgcc', '-Wl,--version-script={map}' ] " +
            COMPILERS +
            $"linux_soname_version='{soname}' " +
            ADDITIONAL_GN_ARGS);

        var outDir = OUTPUT_PATH.Combine($"{VARIANT}/{arch}");
        EnsureDirectoryExists(outDir);
        var so = SKIA_PATH.CombineWithFilePath($"out/{VARIANT}/{arch}/libHarfBuzzSharp.so.{soname}");
        CopyFileToDirectory(so, outDir);
        CopyFile(so, outDir.CombineWithFilePath("libHarfBuzzSharp.so"));

        CheckDeps(so);
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
