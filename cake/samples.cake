void CreateSamplesDirectory(DirectoryPath samplesDirPath, DirectoryPath outputDirPath, string versionSuffix = "")
{
    samplesDirPath = MakeAbsolute(samplesDirPath);
    outputDirPath = MakeAbsolute(outputDirPath);

    var solutionProjectRegex = new Regex(@",\s*""(.*?\.\w{2}proj)"", ""(\{.*?\})""");

    EnsureDirectoryExists(outputDirPath);
    CleanDirectory(outputDirPath);

    var ignoreBinObj = new GlobberSettings {
        Predicate = fileSystemInfo => {
            var segments = fileSystemInfo.Path.Segments;
            var keep = segments.All(s =>
                !s.Equals("bin", StringComparison.OrdinalIgnoreCase) &&
                !s.Equals("obj", StringComparison.OrdinalIgnoreCase) &&
                !s.Equals("AppPackages", StringComparison.OrdinalIgnoreCase) &&
                !s.Equals(".vs", StringComparison.OrdinalIgnoreCase));
            return keep;
        }
    };

    var files = GetFiles($"{samplesDirPath}/**/*", ignoreBinObj);
    foreach (var file in files) {
        var rel = samplesDirPath.GetRelativePath(file);
        var dest = outputDirPath.CombineWithFilePath(rel);
        var ext = file.GetExtension() ?? "";

        if (ext.Equals(".sln", StringComparison.OrdinalIgnoreCase)) {
            var lines = FileReadLines(file.FullPath).ToList();
            var guids = new List<string>();

            // remove projects that aren't samples
            for(var i = 0; i < lines.Count; i++) {
                var line = lines [i];
                var m = solutionProjectRegex.Match(line);
                if (!m.Success)
                    continue;

                // get the path of the project relative to the samples directory
                var relProjectPath = (FilePath) m.Groups [1].Value;
                var absProjectPath = GetFullPath(file, relProjectPath);
                var relSamplesPath = samplesDirPath.GetRelativePath(absProjectPath);
                if (!relSamplesPath.FullPath.StartsWith(".."))
                    continue;

                Debug($"Removing the project '{relProjectPath}' for solution '{rel}'.");

                // skip the next line as it is the "EndProject" line
                guids.Add(m.Groups [2].Value.ToLower());
                lines.RemoveAt(i--);
                lines.RemoveAt(i--);
            }

            // remove all the other references to this guid
            if (guids.Count > 0) {
                for(var i = 0; i < lines.Count; i++) {
                    var line = lines [i];
                    foreach (var guid in guids) {
                        if (line.ToLower().Contains(guid)) {
                            lines.RemoveAt(i--);
                        }
                    }
                }
            }

            // save the solution
            EnsureDirectoryExists(dest.GetDirectory());
            FileWriteLines(dest, lines.ToArray());
        } else if (ext.Equals(".csproj", StringComparison.OrdinalIgnoreCase)) {
            var xdoc = XDocument.Load(file.FullPath);

            // process all the files and project references
            var projItems = xdoc.Root
                .Elements().Where(e => e.Name.LocalName == "ItemGroup")
                .Elements().Where(e => !string.IsNullOrWhiteSpace(e.Attribute("Include")?.Value))
                .ToArray();
            foreach (var projItem in projItems) {
                var suffix = string.IsNullOrEmpty(versionSuffix) ? "" : $"-{versionSuffix}";

                // update the <PackageReference> versions
                if (projItem.Name.LocalName == "PackageReference") {
                    var packageId = projItem.Attribute("Include").Value;
                    var version = GetVersion(packageId);
                    if (!string.IsNullOrWhiteSpace(version)) {
                        // only add the suffix for our nugets
                        if (packageId.StartsWith("SkiaSharp") || packageId.StartsWith("HarfBuzzSharp")) {
                            version += suffix;
                        }
                        Debug($"Substituting package version {packageId} for {version}.");
                        projItem.Attribute("Version").Value = version;
                    } else if (packageId.StartsWith("SkiaSharp") || packageId.StartsWith("HarfBuzzSharp")) {
                        Warning($"Unable to find version information for package '{packageId}'.");
                    }
                    continue;
                }

                // get files in the include
                var relFilePath = (FilePath) projItem.Attribute("Include").Value;
                var absFilePath = GetFullPath(file, relFilePath);

                // ignore files in the samples directory
                var relSamplesPath = samplesDirPath.GetRelativePath(absFilePath);
                if (!relSamplesPath.FullPath.StartsWith(".."))
                    continue;

                // substitute <ProjectReference> with <PackageReference>
                if (projItem.Name.LocalName == "ProjectReference" && FileExists(absFilePath)) {
                    var xReference = XDocument.Load(absFilePath.FullPath);
                    var packagingGroup = xReference.Root
                        .Elements().Where(e => e.Name.LocalName == "PropertyGroup")
                        .Elements().Where(e => e.Name.LocalName == "PackagingGroup")
                        .FirstOrDefault()?.Value;
                    var version = GetVersion(packagingGroup);
                    if (!string.IsNullOrWhiteSpace(version)) {
                        Debug($"Substituting project reference {relFilePath} for project {rel}.");
                        var name = projItem.Name.Namespace + "PackageReference";
                        // only add the suffix for our nugets
                        if (packagingGroup.StartsWith("SkiaSharp") || packagingGroup.StartsWith("HarfBuzzSharp")) {
                            version += suffix;
                        }
                        projItem.AddAfterSelf(new XElement(name, new object[] {
                            new XAttribute("Include", packagingGroup),
                            new XAttribute("Version", version),
                        }));
                    } else {
                        Warning($"Unable to find version information for project '{packagingGroup}'.");
                    }
                } else {
                    Debug($"Removing the file '{relFilePath}' for project '{rel}'.");
                }

                // remove files that are outside
                projItem.Remove();
            }

            // process all the imports
            var imports = xdoc.Root
                .Elements().Where(e =>
                    e.Name.LocalName == "Import" &&
                    !string.IsNullOrWhiteSpace(e.Attribute("Project")?.Value))
                .ToArray();
            foreach (var import in imports) {
                var project = import.Attribute("Project").Value;

                // skip files inside the samples directory or do not exist
                var absProject = GetFullPath(file, project);
                var relSamplesPath = samplesDirPath.GetRelativePath(absProject);
                if (!relSamplesPath.FullPath.StartsWith(".."))
                    continue;

                Debug($"Removing import '{project}' for project '{rel}'.");

                // not inside the samples directory, so needs to be removed
                import.Remove();
            }

            // save the project
            EnsureDirectoryExists(dest.GetDirectory());
            xdoc.Save(dest.FullPath);
        } else {
            EnsureDirectoryExists(dest.GetDirectory());
            CopyFile(file, dest);
        }
    }

    DeleteFiles($"{outputDirPath}/README.md");
    MoveFile($"{outputDirPath}/README.zip.md", $"{outputDirPath}/README.md");
}

FilePath GetFullPath(FilePath root, FilePath path)
{
    path = path.FullPath.Replace("*", "_");
    path = root.GetDirectory().CombineWithFilePath(path);
    return (FilePath) System.IO.Path.GetFullPath(path.FullPath);
}
