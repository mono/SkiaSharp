// Shared emdawnwebgpu port sync. Loaded by both native/wasm/build.cake (where
// libSkiaSharp.a compiles against it) and scripts/infra/tests/tests-wasm.cake
// (where the WASM test consumer link inlines the port at emcc link time via
// binding/IncludeNativeAssets.SkiaSharp.targets). Kept next to shared.cake so
// there's one place to update the pinned tag / SHA512 / library patch.

string EMDAWN_TAG = "v20260624.223603";
string EMDAWN_SHA512 = "615257384ad7df17174c5733c17d8ac0473dfdcddeac69e334d7109501954dc42e77ed54deb666bf44581fcf8e69c2365311626786cd267e52a3d48d7a9441c5";
DirectoryPath EMDAWN_ROOT = ROOT_PATH.Combine("externals/emdawnwebgpu_pkg");

void SyncEmdawnwebgpuPort()
{
    // The port ships its own VERSION.txt containing a natural-language string:
    //   "Dawn release v<tag> at revision <sha>."
    // Reusing it as our up-to-date probe keeps the working tree byte-identical
    // to the upstream release (no marker files of ours to `git status`-noise).
    var versionFile = EMDAWN_ROOT.CombineWithFilePath("VERSION.txt");
    var needsExtract = !FileExists(versionFile) ||
        !System.IO.File.ReadAllText(versionFile.FullPath).Contains(EMDAWN_TAG);

    if (needsExtract) {
        var zipName = $"emdawnwebgpu_pkg-{EMDAWN_TAG}.zip";
        var zipUrl = $"https://github.com/google/dawn/releases/download/{EMDAWN_TAG}/{zipName}";
        var cacheDir = ROOT_PATH.Combine("externals/package_cache/emdawnwebgpu");
        EnsureDirectoryExists(cacheDir);
        var zipPath = cacheDir.CombineWithFilePath(zipName);

        if (!FileExists(zipPath)) {
            Information($"Downloading {zipUrl}");
            DownloadFile(zipUrl, zipPath);
        }

        // Verify SHA512 before touching the working tree — a corrupted /
        // tampered zip must never overwrite a good port on disk.
        string actualSha512;
        using (var stream = System.IO.File.OpenRead(zipPath.FullPath))
        using (var sha = System.Security.Cryptography.SHA512.Create()) {
            var hash = sha.ComputeHash(stream);
            var sb = new System.Text.StringBuilder(hash.Length * 2);
            foreach (var b in hash) sb.Append(b.ToString("x2"));
            actualSha512 = sb.ToString();
        }
        if (!string.Equals(actualSha512, EMDAWN_SHA512, StringComparison.OrdinalIgnoreCase)) {
            DeleteFile(zipPath);
            throw new Exception(
                $"emdawnwebgpu port {EMDAWN_TAG} SHA512 mismatch.\n" +
                $"  expected: {EMDAWN_SHA512}\n" +
                $"  actual:   {actualSha512}\n" +
                $"Downloaded zip was deleted; re-run to retry.");
        }

        if (DirectoryExists(EMDAWN_ROOT))
            CleanDirectories(EMDAWN_ROOT.FullPath);
        EnsureDirectoryExists(EMDAWN_ROOT);
        // The zip unpacks with `emdawnwebgpu_pkg/` at its root, so extracting
        // to `externals/` lands the contents directly at EMDAWN_ROOT —
        // VERSION.txt included.
        Unzip(zipPath, ROOT_PATH.Combine("externals"));
    } else {
        Information($"emdawnwebgpu port already at {EMDAWN_TAG}, skipping extract.");
    }

    // emdawnwebgpu's library_webgpu.js declares __deps: ['$stackSave',
    // '$stackRestore', ...] against emscripten's JS library system. That works
    // on newer emsdks where those names exist as `$`-form aliases, but emsdk
    // 3.1.56 (shipped with .NET 10 WASM SDK) has stackSave and stackRestore
    // only as bare WASM_SYSTEM_EXPORTS — no `$` alias — so the ports mechanism
    // resolves them but a bare `--js-library` load bombs at compiler.mjs with
    // "undefined symbol: $stackSave". Rewrite those two to their bare names so
    // consumers on 3.1.56's emsdk can link. Idempotent: applying to already-
    // patched files is a no-op. Runs on every sync (not just first-extract)
    // so a manual re-download can't leave a mismatched library on disk.
    var libJs = EMDAWN_ROOT.CombineWithFilePath("webgpu/src/library_webgpu.js");
    var lib = System.IO.File.ReadAllText(libJs.FullPath);
    var patched = lib
        .Replace("'$stackSave'", "'stackSave'")
        .Replace("'$stackRestore'", "'stackRestore'");
    if (patched != lib) {
        System.IO.File.WriteAllText(libJs.FullPath, patched);
        Information("Patched library_webgpu.js: $stackSave/$stackRestore -> bare names for emsdk 3.1.56.");
    }
}
