
var MSBuildNS = (XNamespace) "http://schemas.microsoft.com/developer/msbuild/2003";

var ProcessSolutionProjects = new Action<FilePath, Action<string, FilePath>> ((solutionFilePath, process) => {
    var solutionFile = MakeAbsolute (solutionFilePath).FullPath;
    foreach (var line in FileReadLines (solutionFile)) {
        var match = Regex.Match (line, @"Project\(""(?<type>.*)""\) = ""(?<name>.*)"", ""(?<path>.*)"", "".*""");
        if (match.Success && match.Groups ["type"].Value == "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}") {
            var path = match.Groups["path"].Value;
            var projectFilePath = MakeAbsolute (solutionFilePath.GetDirectory ().CombineWithFilePath (path));
            Information ("Processing project file: " + projectFilePath);
            process (match.Groups["name"].Value, projectFilePath);
        }
    }
});

var SetXValue = new Action<XElement, string, string> ((root, element, value) => {
    var node = root.Element (MSBuildNS + element);
    if (node == null)
        root.Add (new XElement (MSBuildNS + element, value));
    else
        node.Value = value;
});

var AddXValue = new Action<XElement, string, string> ((root, element, value) => {
    var node = root.Element (MSBuildNS + element);
    if (node == null)
        root.Add (new XElement (MSBuildNS + element, value));
    else if (!node.Value.Contains (value))
        node.Value += value;
});

var RemoveXValue = new Action<XElement, string, string> ((root, element, value) => {
    var node = root.Element (MSBuildNS + element);
    if (node != null)
        node.Value = node.Value.Replace (value, string.Empty);
});

var SetXValues = new Action<XElement, string[], string, string> ((root, parents, element, value) => {
    IEnumerable<XElement> nodes = new [] { root };
    foreach (var p in parents) {
        nodes = nodes.Elements (MSBuildNS + p);
    }
    foreach (var n in nodes) {
        SetXValue (n, element, value);
    }
});

var AddXValues = new Action<XElement, string[], string, string> ((root, parents, element, value) => {
    IEnumerable<XElement> nodes = new [] { root };
    foreach (var p in parents) {
        nodes = nodes.Elements (MSBuildNS + p);
    }
    foreach (var n in nodes) {
        AddXValue (n, element, value);
    }
});

var RemoveXValues = new Action<XElement, string[], string, string> ((root, parents, element, value) => {
    IEnumerable<XElement> nodes = new [] { root };
    foreach (var p in parents) {
        nodes = nodes.Elements (MSBuildNS + p);
    }
    foreach (var n in nodes) {
        RemoveXValue (n, element, value);
    }
});

var RemoveFileReference = new Action<XElement, string> ((root, filename) => {
    var element = root
        .Elements (MSBuildNS + "ItemGroup")
        .Elements (MSBuildNS + "ClCompile")
        .Where (e => e.Attribute ("Include") != null)
        .Where (e => e.Attribute ("Include").Value.Contains (filename))
        .FirstOrDefault ();
    if (element != null) {
        element.Remove ();
    }
});

var AddFileReference = new Action<XElement, string> ((root, filename) => {
    var element = root
        .Elements (MSBuildNS + "ItemGroup")
        .Elements (MSBuildNS + "ClCompile")
        .Where (e => e.Attribute ("Include") != null)
        .Where (e => e.Attribute ("Include").Value.Contains (filename))
        .FirstOrDefault ();
    if (element == null) {
        root.Elements (MSBuildNS + "ItemGroup")
            .Elements (MSBuildNS + "ClCompile")
            .Last ()
            .Parent
            .Add (new XElement (MSBuildNS + "ClCompile", new XAttribute ("Include", filename)));
    }
});

var RedirectBuildOutputs = new Action<FilePath> ((projectFilePath) => {
    var projectFile = MakeAbsolute (projectFilePath).FullPath;
    var xdoc = XDocument.Load (projectFile);

    var properties = xdoc.Root
        .Elements (MSBuildNS + "PropertyGroup")
        .Elements (MSBuildNS + "LinkIncremental")
        .First ()
        .Parent;
    SetXValue (properties, "OutDir",@"$(SolutionDir)\bin\$(Platform)\$(Configuration)\");
    SetXValue (properties, "IntDir",@"$(SolutionDir)\obj\$(Platform)\$(Configuration)\$(ProjectName)\");

    xdoc.Save (projectFile);
});

var UpdateSkiaSharpVersion = new Action<FilePath, Dictionary<string, string>> ((path, versions) => {
    path = MakeAbsolute (path);
    var fn = System.IO.Path.GetFileName (path.ToString ());
    var ext = System.IO.Path.GetExtension (path.ToString ());

    var ProjectJsonRegex = new Regex (@"(?<pre>^\s*\"")(?<id>.*)(?<mid>\""\:\s*\"")(?<version>.*)(?<post>\"".*$)");
    var TargetsRegex = new Regex (@"(?<pre>^.*\$\(NuGetPackageRoot\)\\)(?<id>.*?)(?<mid>\\)(?<version>.*?)(?<post>\\.*)");
    var HintRegex = new Regex (@"(?<pre>^.*packages\\)(?<id>.*?)(?<mid>\.)(?<version>[\d\.]+(\-.*?){0,1})(?<post>\\.*)");

    if (fn == "project.json") {
        // replace the NuGet v3 project.json

        var modified = false;
        var lines = FileReadLines (path);
        // regex for `"id": "version",`
        for (var i = 0; i < lines.Length; i++) {
            // check if this line matches anything
            var match = ProjectJsonRegex.Match (lines [i]);
            if (match.Success) {
                var id = match.Groups ["id"].Value;
                // check to see what it matches
                string version;
                if (versions.TryGetValue (id, out version) && 
                    version != match.Groups ["version"].Value) {
                    // replace with the new version
                    lines[i] = match.Result ("${pre}" + id + "${mid}" + version + "${post}");
                    modified = true;
                }
            }
        }
        if (modified) {
            FileWriteLines (path, lines);
        }
    } else if (fn == "packages.config") {
        // replace the NuGet v2 packages.config

        var modified = false;
        var xdoc = XDocument.Load (path.ToString ());
        foreach (var package in xdoc.Root.Elements ("package")) {
            var id = package.Attribute ("id");
            var oldVersion = package.Attribute ("version");
            if (id != null && oldVersion != null) {
                string version;
                if (versions.TryGetValue (id.Value, out version) && 
                    version != oldVersion.Value) {
                    // replace with the new version
                    oldVersion.Value = version;
                    modified = true;
                }
            }
        }
        if (modified) {
            xdoc.Save (path.ToString ());
        }
    } else if (ext == ".nuspec") {
        // replace NuSpec

        var modified = false;
        var xdoc = XDocument.Load (path.ToString ());
        var metadata = xdoc.Root.Elements ("metadata");
        var dependencies = metadata
            .Elements ("dependencies")
            .Elements ("dependency");
        var groupDependencies = metadata
            .Elements ("dependencies")
            .Elements ("group")
            .Elements ("dependency");
        foreach (var package in dependencies.Union (groupDependencies)) {
            var id = package.Attribute ("id");
            var oldVersion = package.Attribute ("version");
            if (id != null && oldVersion != null) {
                string version;
                if (versions.TryGetValue (id.Value, out version) && 
                    version != oldVersion.Value) {
                    // replace with the new version
                    oldVersion.Value = version;
                    modified = true;
                }
            }
        }
        var xId = metadata.Elements ("id").FirstOrDefault ();
        var xVersion = metadata.Elements ("version").FirstOrDefault ();
        if (xId != null && xVersion != null) {
            string version;
            if (versions.TryGetValue (xId.Value, out version) && 
                version != xVersion.Value) {
                // replace with the new version
                xVersion.Value = version;
                modified = true;
            }
        }
        if (modified) {
            xdoc.Save (path.ToString ());
        }
    } else if (fn.EndsWith (".nuget.targets")) {
        // replace NuGet v3 targets

        var modified = false;
        var xdoc = XDocument.Load (path.ToString ());
        var imports = xdoc.Root
            .Elements (MSBuildNS + "ImportGroup")
            .Elements (MSBuildNS + "Import");
        foreach (var package in imports) {
            var proj = package.Attribute ("Project");
            var cond = package.Attribute ("Condition");
            if (proj != null) {
                var projMatch = TargetsRegex.Match (proj.Value);
                var id = projMatch.Groups ["id"].Value;
                var oldVersion = projMatch.Groups ["version"].Value;
                string version;
                if (versions.TryGetValue (id, out version) && 
                    version != oldVersion) {
                    // replace with the new version
                    proj.Value = projMatch.Result ("${pre}" + id + "${mid}" + version + "${post}");
                    if (cond != null) {
                        var condMatch = TargetsRegex.Match (cond.Value);
                        cond.Value = condMatch.Result ("${pre}" + id + "${mid}" + version + "${post}");
                    }
                    modified = true;
                }
            }
        }
        if (modified) {
            xdoc.Save (path.ToString ());
        }
    } else if (ext == ".csproj") {
        // replace C# projects

        var modified = false;
        var replace = new Func<string, string> ((text) => {
            var match = HintRegex.Match (text);
            var id = match.Groups ["id"].Value;
            var oldVersion = match.Groups ["version"].Value;
            string version;
            if (versions.TryGetValue (id, out version) && 
                version != oldVersion) {
                // replace with the new version
                modified = true;
                text = match.Result ("${pre}" + id + "${mid}" + version + "${post}");
            }
            return text;
        });
        var xdoc = XDocument.Load (path.ToString ());
        var references = xdoc.Root
            .Elements (MSBuildNS + "ItemGroup")
            .Elements (MSBuildNS + "Reference");
        foreach (var package in references) {
            var hint = package.Element (MSBuildNS + "HintPath");
            if (hint != null) {
                hint.Value = replace (hint.Value);
            }
        }
        var packageReferences = xdoc.Root
            .Elements ("ItemGroup")
            .Elements ("PackageReference");
        foreach (var package in packageReferences) {
            var id = package.Attribute ("Include").Value;
            // check to see what it matches
            string version;
            if (versions.TryGetValue (id, out version) && 
                version != package.Attribute ("Version").Value) {
                // replace with the new version
                package.Attribute ("Version").Value = version;
                modified = true;
            }
        }
        var imports = xdoc.Root
            .Elements (MSBuildNS + "Import");
        foreach (var package in imports) {
            var proj = package.Attribute ("Project");
            if (proj != null) {
                proj.Value = replace (proj.Value);
            }
            var cond = package.Attribute ("Condition");
            if (cond != null) {
                cond.Value = replace (cond.Value);
            }
        }
        var errors = xdoc.Root
            .Elements (MSBuildNS + "Target")
            .Elements (MSBuildNS + "Error");
        foreach (var package in errors) {
            var cond = package.Attribute ("Condition");
            if (cond != null) {
                cond.Value = replace (cond.Value);
            }
            var text = package.Attribute ("Text");
            if (text != null) {
                text.Value = replace (text.Value);
            }
        }
        if (modified) {
            xdoc.Save (path.ToString ());
        }
    }
});

var UpdateAssemblyInfo = new Action<FilePath, string, string, string> ((path, assembly, version, sha) => {
    var info = ParseAssemblyInfo (path);
    var settings = new AssemblyInfoSettings {
        Version = assembly,
        FileVersion = version,
        InformationalVersion = version + "-" + sha,
        Company = info.Company,
        Copyright = info.Copyright,
        Description = info.Description,
        Product = info.Product,
        Title = info.Title,
        Trademark = info.Trademark,
    };
    CreateAssemblyInfo (path, settings);
});