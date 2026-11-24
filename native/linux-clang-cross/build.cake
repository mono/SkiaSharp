DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));

#load "../../scripts/infra/shared/shared.cake"

string TOOLCHAIN_ARCH = Argument("toolchainArch", EnvironmentVariable("TOOLCHAIN_ARCH"));
string TOOLCHAIN_ARCH_SHORT = Argument("toolchainArchShort", EnvironmentVariable("TOOLCHAIN_ARCH_SHORT"));
string TOOLCHAIN_ARCH_TARGET = Argument("toolchainArchTarget", EnvironmentVariable("TOOLCHAIN_ARCH_TARGET"));
string SYSROOT_PATH = Argument("sysrootPath", EnvironmentVariable("SYSROOT_PATH"));
string SYSROOT_ROOT = Argument("sysrootRoot", EnvironmentVariable("SYSROOT_ROOT") ?? "");
string GCC_LIB_DIR = Argument("gccLibDir", EnvironmentVariable("GCC_LIB_DIR") ?? "");

if (string.IsNullOrEmpty(SYSROOT_PATH))
    throw new Exception("FATAL: SYSROOT_PATH is not set. Expected from /etc/skia-env in the cross-compile Docker image.");
if (string.IsNullOrEmpty(SYSROOT_ROOT))
    throw new Exception("FATAL: SYSROOT_ROOT is not set. Expected from /etc/skia-env in the cross-compile Docker image.");
if (string.IsNullOrEmpty(GCC_LIB_DIR))
    throw new Exception("FATAL: GCC_LIB_DIR is not set. Expected from /etc/skia-env in the cross-compile Docker image.");

Information("Toolchain:");
Information($"    Arch:                          {TOOLCHAIN_ARCH} ({TOOLCHAIN_ARCH_SHORT})");
Information($"    Target                         {TOOLCHAIN_ARCH_TARGET}");
Information($"    Sysroot:                       {SYSROOT_PATH}");
Information($"    Sysroot Root:                  {SYSROOT_ROOT}");
Information($"    GCC Lib Dir:                   {GCC_LIB_DIR}");

if (BUILD_ARCH.Length == 0)
    BUILD_ARCH = new [] { "arm" };

string GetGnArgs(string arch)
{
    var sysrootArg = BUILD_VARIANT switch
    {
        _ when !string.IsNullOrEmpty(SYSROOT_ROOT) => $"'--sysroot={SYSROOT_ROOT}', ",
        _ => "",
    };
    var linker = "'-fuse-ld=lld'";

    var sysroot = SYSROOT_PATH;
    var init = $"{sysrootArg} '--target={TOOLCHAIN_ARCH_TARGET}'";
    var bin = $"'-B{sysroot}/bin/', '-B{sysroot}/lib/', '-B{sysroot}/lib64/' ";
    var libs = $"'-L{sysroot}/lib/', '-L{sysroot}/lib64/', '-L{sysroot}/lib/{TOOLCHAIN_ARCH}/' ";
    // Add GCC library directory for CRT files (crtbeginS.o) and libgcc
    if (!string.IsNullOrEmpty(GCC_LIB_DIR))
        libs += $", '-L{GCC_LIB_DIR}/', '-B{GCC_LIB_DIR}/' ";
    // C++ headers MUST come before C headers for #include_next to work correctly.
    // When <cmath> does #include_next <math.h>, the search continues from paths
    // AFTER the directory where cmath was found. If C headers are listed first,
    // they won't be searched by #include_next from C++ headers.
    //
    // All cross-images have libc++ headers installed at c++/v1/. Clang finds them
    // automatically via -stdlib=libc++ — we only need the C system include paths.
    var includes = $"'-I{sysroot}/include', " +
                   $"'-I{sysroot}/include/{TOOLCHAIN_ARCH}' ";

    // All cross-images provide libc++.a built with -DLIBCXX_CXX_ABI=libstdc++.
    // This means libc++ delegates ABI symbols (operator delete, __cxa_throw, etc.)
    // to GCC's libstdc++. We link libstdc++ with --whole-archive so its symbols are
    // available before GN's object archives reference them (extra_ldflags appear first
    // in the link command). -Wl bypasses clang's driver rewrite of -lstdc++ → -lc++.
    var abiLib = "'-Wl,--whole-archive,-lstdc++,--no-whole-archive'";

    return
        $"extra_asmflags+=[ {init}, '-no-integrated-as', {bin}, {includes} ] " +
        $"extra_cflags+=[ {init}, {bin}, {includes} ] " +
        $"extra_ldflags+=[ {init}, {bin}, {libs}, {abiLib}, {linker} ] " +
        ADDITIONAL_GN_ARGS;
}

Task("libSkiaSharp")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    foreach (var arch in BUILD_ARCH) {
        RunCake("../linux/build.cake", "libSkiaSharp", new Dictionary<string, string> {
            { "arch", arch },
            { "gnArgs", GetGnArgs(arch) },
        });
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    foreach (var arch in BUILD_ARCH) {
        RunCake("../linux/build.cake", "libHarfBuzzSharp", new Dictionary<string, string> {
            { "arch", arch },
            { "gnArgs", GetGnArgs(arch) },
        });
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
