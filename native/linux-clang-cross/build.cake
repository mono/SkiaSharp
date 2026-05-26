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
    // When libc++ headers (c++/v1/) exist, clang finds them automatically via
    // -stdlib=libc++ — no explicit -I needed. When only libstdc++ exists (no v1/),
    // we add explicit paths to c++/current (a symlink to the highest GCC version).
    var hasLibcppHeaders = System.IO.Directory.Exists($"{sysroot}/include/c++/v1");
    var includes = hasLibcppHeaders
        ? $"'-I{sysroot}/include', " +
          $"'-I{sysroot}/include/{TOOLCHAIN_ARCH}' "
        : $"'-I{sysroot}/include/c++/current', " +
          $"'-I{sysroot}/include/c++/current/{TOOLCHAIN_ARCH}', " +
          $"'-I{sysroot}/include/{TOOLCHAIN_ARCH}/c++/current', " +
          $"'-I{sysroot}/include', " +
          $"'-I{sysroot}/include/{TOOLCHAIN_ARCH}' ";

    // Detect libc++ availability. The Dockerfile ensures libc++ HEADERS (c++/v1/)
    // exist for all arches (copied from x64 for x86). But libc++.a may not exist
    // for all arches. When libc++.a is missing, we override the linker's stdlib to
    // libstdc++ and link it directly.
    //
    // Three scenarios:
    // 1. libc++.a + libc++abi.a both exist (x86 after Dockerfile builds them):
    //    Let clang link naturally — -stdlib=libc++ adds -lc++ -lc++abi automatically.
    //    No explicit ABI lib needed.
    // 2. libc++.a exists but NOT libc++abi.a (arm64, x64, arm, riscv64, loongarch64):
    //    ABI symbols (operator delete, __cxa_throw) come from GCC's libstdc++.
    //    --whole-archive forces inclusion before objects reference them.
    //    -Wl,-lstdc++ bypasses clang driver's rewrite of -lstdc++ → -lc++.
    // 3. Neither exists: fall back to libstdc++ for both compile and link.
    var sysrootRoot = SYSROOT_ROOT;
    var hasLibcppLib = System.IO.File.Exists($"{sysrootRoot}/usr/lib/libc++.a")
        || System.IO.File.Exists($"{sysroot}/lib/libc++.a");
    var hasLibcppAbi = System.IO.File.Exists($"{sysrootRoot}/usr/lib/libc++abi.a")
        || System.IO.File.Exists($"{sysroot}/lib/libc++abi.a");
    string stdlibLinkOverride;
    string abiLib;
    if (hasLibcppLib && hasLibcppAbi)
    {
        // Full libc++ stack. extra_ldflags appear BEFORE object archives in GN's link
        // command, so --whole-archive forces libc++abi symbols to be included regardless
        // of link ordering. -Wl bypasses clang driver's library name rewriting.
        stdlibLinkOverride = "";
        abiLib = "'-Wl,--whole-archive,-lc++abi,--no-whole-archive'";
    }
    else if (hasLibcppLib)
    {
        // libc++.a but no libc++abi — get ABI symbols from libstdc++ (libsupc++ inside).
        stdlibLinkOverride = "";
        abiLib = "'-Wl,--whole-archive,-lstdc++,--no-whole-archive'";
    }
    else
    {
        // No libc++ at all — pure libstdc++ mode.
        stdlibLinkOverride = "'-stdlib=libstdc++', ";
        abiLib = "'-lstdc++'";
    }

    return
        $"extra_asmflags+=[ {init}, '-no-integrated-as', {bin}, {includes} ] " +
        $"extra_cflags+=[ {init}, {bin}, {includes} ] " +
        $"extra_ldflags+=[ {init}, {bin}, {libs}, {stdlibLinkOverride}{abiLib}, {linker} ] " +
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
