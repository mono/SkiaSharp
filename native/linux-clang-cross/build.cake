DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));

#load "../../scripts/cake/shared.cake"

if (BUILD_ARCH.Length == 0)
    BUILD_ARCH = new [] { "arm" };

string VARIANT = string.IsNullOrEmpty(BUILD_VARIANT) ? "linux" : BUILD_VARIANT?.Trim();

string GetGnArgs(string arch)
{
    var toolchainArch = arch == "arm"
        ? VARIANT == "alpine" ? "arm-none-eabi" : "arm-linux-gnueabihf"
        : VARIANT == "alpine" ? "aarch64-none-elf" : "aarch64-linux-gnu";
    var targetArch = arch == "arm"
        ? VARIANT == "alpine" ? "arm-none-eabi" : "armv7a-linux-gnueabihf"
        : VARIANT == "alpine" ? "aarch64-none-elf" : "aarch64-linux-gnu";

    var sysroot = $"/usr/{toolchainArch}";
    var init = $"'--sysroot={sysroot}', '--target={targetArch}'";
    var bin = $"'-B{sysroot}/bin/' ";
    var libs = $"'-L{sysroot}/lib/' ";
    var includes = 
        $"'-I{sysroot}/include/', " +
        $"'-I{sysroot}/include/c++/current/' , " +
        $"'-I{sysroot}/include/c++/current/{toolchainArch}/' ";

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
