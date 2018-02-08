
var MSBuildNS = (XNamespace) "http://schemas.microsoft.com/developer/msbuild/2003";

var UpdateSkiaSharpVersion = new Action<FilePath, Dictionary<string, string>> ((path, versions) => {
    path = MakeAbsolute (path);
    var fn = System.IO.Path.GetFileName (path.ToString ());
    var ext = System.IO.Path.GetExtension (path.ToString ());

    if (ext == ".nuspec") {
        // NuGet
        var modified = false;
        var xdoc = XDocument.Load (path.ToString ());
        // <dependency>
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
                    oldVersion.Value = version;
                    modified = true;
                }
            }
        }
        // <version>
        var xId = metadata.Elements ("id").FirstOrDefault ();
        var xVersion = metadata.Elements ("version").FirstOrDefault ();
        if (xId != null && xVersion != null) {
            string version;
            if (versions.TryGetValue (xId.Value, out version) && 
                version != xVersion.Value) {
                xVersion.Value = version;
                modified = true;
            }
        }
        if (modified) {
            xdoc.Save (path.ToString ());
        }
    } else if (ext == ".csproj") {
        // project files
        var modified = false;
        var xdoc = XDocument.Load (path.ToString ());
        // <PackageReference>
        var packageReferences = xdoc.Root
            .Elements ("ItemGroup")
            .Elements ("PackageReference")
            .Union (xdoc.Root
                .Elements (MSBuildNS + "ItemGroup")
                .Elements (MSBuildNS + "PackageReference"));
        foreach (var package in packageReferences) {
            var id = package.Attribute ("Include").Value;
            string version;
            if (versions.TryGetValue (id, out version) && 
                version != package.Attribute ("Version").Value) {
                package.Attribute ("Version").Value = version;
                modified = true;
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