DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));

#load "../../scripts/cake/shared.cake"

string TOOLCHAIN_ARCH = Argument("toolchainArch", EnvironmentVariable("TOOLCHAIN_ARCH"));
string TOOLCHAIN_ARCH_SHORT = Argument("toolchainArchShort", EnvironmentVariable("TOOLCHAIN_ARCH_SHORT"));
string TOOLCHAIN_ARCH_TARGET = Argument("toolchainArchTarget", EnvironmentVariable("TOOLCHAIN_ARCH_TARGET"));

Information("Toolchain:");
Information($"    Arch:                          {TOOLCHAIN_ARCH} ({TOOLCHAIN_ARCH_SHORT})");
Information($"    Target                         {TOOLCHAIN_ARCH_TARGET}");

if (BUILD_ARCH.Length == 0)
    BUILD_ARCH = new [] { "arm" };

string GetGnArgs(string arch)
{
    var (sysrootArg, linker) = BUILD_VARIANT switch
    {
        "alpine" or "alpinenodeps" => ("'--sysroot=/alpine', ", "'-fuse-ld=lld'"),
        _ => ("", ""),
    };

    var sysroot = $"/usr/{TOOLCHAIN_ARCH}";
    var init = $"{sysrootArg} '--target={TOOLCHAIN_ARCH_TARGET}'";
    var bin = $"'-B{sysroot}/bin/' ";
    var libs = $"'-L{sysroot}/lib/' ";
    var includes = 
        $"'-I{sysroot}/include', " +
        $"'-I{sysroot}/include/c++/current', " +
        $"'-I{sysroot}/include/c++/current/{TOOLCHAIN_ARCH}' ";

    return
        $"extra_asmflags+=[ {init}, '-no-integrated-as', {bin}, {includes} ] " +
        $"extra_cflags+=[ {init}, {bin}, {includes} ] " +
        $"extra_ldflags+=[ {init}, {bin}, {libs}, {linker} ] " +
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
