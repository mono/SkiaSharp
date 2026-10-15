DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "./shared/native-shared.cake"

var NATIVE_PLATFORM = Argument("nativePlatform", "linux");
var RUNTIME_LIBC = Argument("nativeLibc", "glibc");

Task("Default")
    .Does(() =>
{
    if (!IsRunningOnLinux())
        throw new Exception("Native HarfBuzz runtime probes require Linux.");
    if (NATIVE_PLATFORM != "linux" && NATIVE_PLATFORM != "alpine")
        throw new Exception("Use --nativePlatform=linux or alpine; each also tests its nodeps variant.");
    if (RUNTIME_LIBC != (NATIVE_PLATFORM == "alpine" ? "musl" : "glibc"))
        throw new Exception("The native platform and expected runtime libc do not match.");

    var arch = ReduceArch(System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString());
    var failures = new List<Exception>();
    foreach (var platform in new[] { NATIVE_PLATFORM, NATIVE_PLATFORM + "nodeps" }) {
        try {
            RunNativeArtifactTests("runtime",
                ROOT_PATH.CombineWithFilePath($"output/native/{platform}/{arch}/libHarfBuzzSharp.so"),
                $"--libc={RUNTIME_LIBC}",
                $"{platform}-{arch}");
        } catch (Exception ex) {
            Error($"Native runtime probe failed for {platform}/{arch}: {ex.Message}");
            failures.Add(ex);
        }
    }
    if (failures.Count > 0)
        throw new AggregateException("Native runtime probes failed.", failures);
});

RunTarget(TARGET);
