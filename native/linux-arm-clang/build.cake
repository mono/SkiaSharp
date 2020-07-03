DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../cake/shared.cake"

var BUILD_VARIANT = Argument("variant", EnvironmentVariable("BUILD_VARIANT") ?? "linux");
OUTPUT_PATH = OUTPUT_PATH.Combine(BUILD_VARIANT);

Task("libSkiaSharp")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    // RunCake("../linux/build.cake", "libSkiaSharp", new Dictionary<string, string> {
    //     { "variant", BUILD_VARIANT },
    //     { "arch", "arm" },
    // });

    var sysroot = "/usr/arm-linux-gnueabihf";
    var target = "armv7a-linux-gnueabihf";
    var includes = 
        "'-I/usr/arm-linux-gnueabihf/include', " +
        "'-I/usr/arm-linux-gnueabihf/include/c++/4.8.5', " +
        "'-I/usr/arm-linux-gnueabihf/include/c++/4.8.5/arm-linux-gnueabihf'";
    RunCake("../linux/build.cake", "libSkiaSharp", new Dictionary<string, string> {
        { "variant", BUILD_VARIANT },
        { "arch", "arm" },
        { "gnArgs",
            $"extra_asmflags+=[ '--sysroot={sysroot}', '--target={target}',  '-mfloat-abi=hard', '-march=armv7-a', '-mfpu=neon', '-mthumb', {includes} ] " +
            $"extra_cflags+=[   '--sysroot={sysroot}', '--target={target}',  '-mfloat-abi=hard', '-march=armv7-a', '-mfpu=neon', '-mthumb', {includes} ] " +
            $"extra_ldflags+=[  '--sysroot={sysroot}', '--target={target}' , '-mfloat-abi=hard', '-march=armv7-a', '-mfpu=neon', '-mthumb'] "
        },
    });

    RunProcess("ldd", OUTPUT_PATH.CombineWithFilePath($"x64/libSkiaSharp.so").FullPath, out var stdout);

    if (stdout.Any(o => o.Contains("fontconfig")))
        throw new Exception("libSkiaSharp.so contained a dependency on fontconfig.");
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    RunCake("../linux/build.cake", "libHarfBuzzSharp", new Dictionary<string, string> {
        { "variant", BUILD_VARIANT },
    });

    RunProcess("ldd", OUTPUT_PATH.CombineWithFilePath($"x64/libHarfBuzzSharp.so").FullPath, out var stdout);

    if (stdout.Any(o => o.Contains("fontconfig")))
        throw new Exception("libHarfBuzzSharp.so contained a dependency on fontconfig.");
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
