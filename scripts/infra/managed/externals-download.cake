#addin nuget:?package=NuGet.Packaging&version=6.9.1
#addin nuget:?package=Mono.ApiTools.NuGetDiff&version=1.4.1

using System.Xml.Linq;
using NuGet.Packaging;
using NuGet.Versioning;

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS DOWNLOAD — download pre-built native binaries from CI feed
////////////////////////////////////////////////////////////////////////////////////////////////////

async Task DownloadPackageAsync(string id, DirectoryPath outputDirectory)
{
    var version = "0.0.0-";
    if (!string.IsNullOrEmpty(PREVIEW_LABEL) && PREVIEW_LABEL.StartsWith("pr."))
        version += PREVIEW_LABEL.ToLower();
    else if (!string.IsNullOrEmpty(GIT_SHA))
        version += "commit." + GIT_SHA.ToLower();
    else if (!string.IsNullOrEmpty(GIT_BRANCH_NAME))
        version += "branch." + GIT_BRANCH_NAME.Replace("/", ".").ToLower();
    else
        version += "branch.main";
    version += ".*";

    var filter = new NuGetVersions.Filter {
        IncludePrerelease = true,
        SourceUrl = CI_ARTIFACTS_FEED_URL,
        VersionRange = VersionRange.Parse(version),
    };

    var latestVersion = await NuGetVersions.GetLatestAsync(id, filter);

    var comparer = new NuGetDiff(CI_ARTIFACTS_FEED_URL);
    comparer.PackageCache = PACKAGE_CACHE_PATH.FullPath;

    var queue = new Queue<(string id, NuGetVersion version)>();
    var discovered = new HashSet<string>();
    var completed = new List<string>();
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

    queue.Enqueue((id, latestVersion));
    discovered.Add(id.ToLower());

    Information($"Starting download of native packages...");
    Information($"");

    while (queue.Count > 0)
    {
        var pkg = queue.Dequeue();
        var currentId = pkg.id.ToLower();
        var currentVersion = pkg.version;

        var total = completed.Count + queue.Count + 1;
        var progress = (completed.Count * 100) / total;
        var remaining = queue.Count;

        Information($"[{progress,3}%] ({completed.Count + 1}/{total}) Downloading: {currentId}" +
                   (remaining > 0 ? $"  ({remaining} more in queue)" : ""));

        var root = await comparer.ExtractCachedPackageAsync(currentId, currentVersion);
        var toolsDir = $"{root}/tools/";
        if (DirectoryExists(toolsDir)) {
            var allFiles = GetFiles(toolsDir + "**/*");
            foreach (var file in allFiles) {
                var relative = MakeAbsolute(Directory(toolsDir)).GetRelativePath(file);
                var dir = $"{outputDirectory}/{relative.GetDirectory()}";
                EnsureDirectoryExists(dir);
                CopyFileToDirectory(file, dir);
            }
        }

        var nuspec = $"{root}/{currentId}.nuspec";
        var xdoc = XDocument.Load(nuspec);
        var xmlns = xdoc.Root.Name.Namespace;
        var dependencies = xdoc.Root.Descendants(xmlns + "dependency").ToArray();

        var newDeps = 0;
        foreach (var dep in dependencies)
        {
            var depId = dep.Attribute("id").Value.ToLower();
            var depVersion = dep.Attribute("version").Value;

            if (!discovered.Contains(depId))
            {
                discovered.Add(depId);
                queue.Enqueue((depId, NuGetVersion.Parse(depVersion)));
                newDeps++;
            }
        }

        if (newDeps > 0)
            Information($"         └─ Found {newDeps} new dependencies");

        completed.Add(currentId);
    }

    stopwatch.Stop();
    Information($"");
    Information($"========================================");
    Information($"Download complete!");
    Information($"  Packages: {completed.Count}");
    Information($"  Duration: {stopwatch.Elapsed.TotalSeconds:F1}s");
    Information($"========================================");
}

Task("Default")
    .Does(async () =>
{
    EnsureDirectoryExists ("./output");
    CleanDirectories ("./output");

    await DownloadPackageAsync("_nativeassets", "./output/native");
});

RunTarget(TARGET);
