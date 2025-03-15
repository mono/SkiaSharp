DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));

#load "../../scripts/cake/shared.cake"

if (BUILD_ARCH.Length == 0)
    BUILD_ARCH = new [] { "arm" };

string GetGnArgs(string arch)
{
    var (vendor, abi, sysrootarg, linker) = BUILD_VARIANT switch
    {
        "alpine" or "alpinenodeps" => ("-alpine", "musl", "'--sysroot=/alpine', ", "'-fuse-ld=lld'"),
        _ => ("", "gnu", "", ""),
    };
    var (toolchainArch, targetArch) = arch switch
    {
        "arm" => ($"arm{vendor}-linux-{abi}eabihf", $"armv7a{vendor}-linux-{abi}eabihf"),
        "arm64" => ($"aarch64{vendor}-linux-{abi}", $"aarch64{vendor}-linux-{abi}"),
        _ => ($"{arch}{vendor}-linux-{abi}", $"{arch}{vendor}-linux-{abi}"),
    };

    var sysroot = $"/usr/{toolchainArch}";
    var init = $"{sysrootarg} '--target={targetArch}'";
    var bin = $"'-B{sysroot}/bin/' ";
    var libs = $"'-L{sysroot}/lib/' ";
    var includes = 
        $"'-I{sysroot}/include', " +
        $"'-I{sysroot}/include/c++/current', " +
        $"'-I{sysroot}/include/c++/current/{toolchainArch}' ";

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
            { "variant", BUILD_VARIANT },
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
            { "variant", BUILD_VARIANT },
        });
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
