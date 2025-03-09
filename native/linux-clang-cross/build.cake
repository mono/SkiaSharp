DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));

#load "../../scripts/cake/shared.cake"

if (BUILD_ARCH.Length == 0)
    BUILD_ARCH = new [] { "arm" };

string GetGnArgs(string arch)
{
    var (toolchainArch, targetArch) = arch switch
    {
        "arm" => ("arm-linux-gnueabihf", "armv7a-linux-gnueabihf"),
        "arm64" or "riscv64" => ($"{arch}-linux-gnu", $"{arch}-linux-gnu"),
        _ => throw new ArgumentException($"Unknown architecture: {arch}")
    };

    var sysroot = $"/usr/{toolchainArch}";
    var init = $"'--target={targetArch}'";
    var bin = $"'-B{sysroot}/bin/' ";
    var libs = $"'-L{sysroot}/lib/' ";
    var includes = 
        $"'-I{sysroot}/include', " +
        $"'-I{sysroot}/include/c++/current', " +
        $"'-I{sysroot}/include/c++/current/{toolchainArch}' ";

    return
        $"extra_asmflags+=[ {init}, '-no-integrated-as', {bin}, {includes} ] " +
        $"extra_cflags+=[ {init}, {bin}, {includes} ] " +
        $"extra_ldflags+=[ {init}, {bin}, {libs} ] " +
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
