DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));

#load "../../scripts/infra/shared/shared.cake"

string TOOLCHAIN_ARCH = Argument("toolchainArch", EnvironmentVariable("TOOLCHAIN_ARCH"));
string TOOLCHAIN_ARCH_SHORT = Argument("toolchainArchShort", EnvironmentVariable("TOOLCHAIN_ARCH_SHORT"));
string TOOLCHAIN_ARCH_TARGET = Argument("toolchainArchTarget", EnvironmentVariable("TOOLCHAIN_ARCH_TARGET"));
string SYSROOT_PATH = Argument("sysrootPath", EnvironmentVariable("SYSROOT_PATH"));
string SYSROOT_ROOT = Argument("sysrootRoot", EnvironmentVariable("SYSROOT_ROOT") ?? "");
string GCC_LIB_DIR = Argument("gccLibDir", EnvironmentVariable("GCC_LIB_DIR") ?? "");

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
    // The Dockerfile creates: include/c++/current -> VERSION
    // Arch-specific headers vary by layout:
    //   Debootstrap:     {sysroot}/include/{ARCH}/c++/current/
    //   Cross-packages:  {sysroot}/include/c++/current/{ARCH}/
    // We include both — non-existent paths are harmless to clang.
    var includes =
        $"'-I{sysroot}/include/c++/current', " +
        $"'-I{sysroot}/include/c++/current/{TOOLCHAIN_ARCH}', " +
        $"'-I{sysroot}/include/{TOOLCHAIN_ARCH}/c++/current', " +
        $"'-I{sysroot}/include', " +
        $"'-I{sysroot}/include/{TOOLCHAIN_ARCH}' ";

    // The sysroot provides libc++.a (static) but NOT libc++abi. The C++ ABI
    // symbols (__cxa_throw, __cxa_begin_catch, operator new/delete, etc.) and
    // std::runtime_error come from GCC's libstdc++.a (which includes libsupc++).
    //
    // Two complications with the link command:
    // 1. extra_ldflags appears BEFORE the object archives in GN's link command,
    //    so we need --whole-archive to force inclusion regardless of ordering.
    // 2. -stdlib=libc++ causes clang's driver to REWRITE -lstdc++ into -lc++.
    //    We bypass this by using -Wl,-lstdc++ to pass directly to the linker.
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
