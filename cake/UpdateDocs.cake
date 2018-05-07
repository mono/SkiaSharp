
void CreateFrameworks (DirectoryPath docsTempPath)
{
    // download all the versions from nuget so we can generate the docs
    var ids = new Dictionary<string, Version> {
        { "skiasharp",              new Version (1, 57, 0) },
        { "skiasharp.views",        new Version (1, 57, 0) },
        { "skiasharp.views.forms",  new Version (1, 57, 0) },
        { "harfbuzzsharp",          new Version (1, 0, 0) },
        { "skiasharp.harfbuzz",     new Version (1, 57, 0) },
    };
    var metadata = "https://api.nuget.org/v3/registration3/{0}/index.json";
    var download = "https://api.nuget.org/v3-flatcontainer/{0}/{1}/{0}.{1}.nupkg";

    var packagesPath = ROOT_PATH.Combine ("externals/docs_packages");

    // prepare the temp folder
    EnsureDirectoryExists (docsTempPath);
    CleanDirectories (docsTempPath.FullPath);

    var xFrameworks = new XElement ("Frameworks");
    var xFrameworksDoc = new XDocument (xFrameworks);

    foreach (var id in ids.Keys) {
        // get the versions for each nuget
        Information ($"Downloading information for ID: {id}...");
        var md = string.Format (metadata, id);
        var mdFile = DownloadFile (md);
        var mdObj = JObject.Parse (FileReadText (mdFile));
        var page = mdObj ["items"] [0];
        foreach (var package in page ["items"]) {
            var version = (string) package ["catalogEntry"] ["version"];
            // skip pre-release versions
            if (version.Contains("-") || Version.Parse (version) < ids [id])
                continue;

            // download the assemblies
            Information ($"Downloading '{id}' version '{version}'...");
            var dest = packagesPath.Combine (id).Combine (version);
            var destFile = dest.CombineWithFilePath ($"{id}.{version}.nupkg");
            if (!FileExists (destFile)) {
                EnsureDirectoryExists (dest);
                DownloadFile (string.Format (download, id, version), destFile);
                Unzip (destFile, dest);
            }

            // copy the assemblies into the temp folder
            ProcessNuGet (docsTempPath, id, version, dest, xFrameworks);
        }

        // process the generated nugets as well
        var genVersion = GetVersion (id);
        var genDest = ROOT_PATH.Combine ("output").Combine (id).Combine ("nuget");
        ProcessNuGet (docsTempPath, id, genVersion, genDest, xFrameworks);
    }

    xFrameworksDoc.Save ($"{docsTempPath}/frameworks.xml");
}

void ProcessNuGet(DirectoryPath docsTempPath, string id, string version, DirectoryPath dest, XElement xFrameworks)
{
    var xplat = new [] {
        "netstandard1.3",
        "portable-net45%2Bwin8%2Bwpa81%2Bwp8",
        "portable-net45%2Bxamarinmac%2Bxamarinios%2Bmonotouch%2Bmonoandroid%2Bwin8%2Bwpa81%2Bwp8%2Bxamarin.watchos%2Bxamarin.tvos",
    };

    if (id == "skiasharp.views") {
        // copy platform-specific
        foreach (var dir in GetDirectories ($"{dest}/lib/*")) {
            var d = dir.GetDirectoryName ().ToLower ();
            var platform = "";
            if (d.StartsWith ("monoandroid")) {
                platform = "android";
            } else if (d.StartsWith ("net4")) {
                platform = "net";
            } else if (d.StartsWith ("uap")) {
                platform = "uwp";
            } else if (d.StartsWith ("xamarinios") || d.StartsWith ("xamarin.ios")) {
                platform = "ios";
            } else if (d.StartsWith ("xamarinmac") || d.StartsWith ("xamarin.mac")) {
                platform = "macos";
            } else if (d.StartsWith ("xamarintvos") || d.StartsWith ("xamarin.tvos")) {
                platform = "tvos";
            } else if (d.StartsWith ("xamarinwatchos") || d.StartsWith ("xamarin.watchos")) {
                platform = "watchos";
            } else if (d.StartsWith ("tizen")) {
                platform = "tizen";
            } else {
                throw new Exception ($"Unknown platform: {d}");
            }
            var moniker = $"{id.Replace (".", "-")}-{platform}-{version}";
            var o = docsTempPath.Combine (moniker);
            EnsureDirectoryExists (o);
            foreach (var f in GetFiles ($"{dir}/*.dll")) {
                CopyFileToDirectory (f, o);
            }
            xFrameworks.Add (
                new XElement ("Framework",
                    new XAttribute ("Name", o.GetDirectoryName ()),
                    new XAttribute ("Source", o.GetDirectoryName ())));
        }
    } else {
        // copy netstandard/portable
        var moniker = $"{id.Replace (".", "-")}-{version}";
        var o = docsTempPath.Combine (moniker);
        EnsureDirectoryExists (o);
        foreach (var x in xplat) {
            if (DirectoryExists ($"{dest}/lib/{x}")) {
                foreach (var f in GetFiles ($"{dest}/lib/{x}/*.dll")) {
                    CopyFileToDirectory (f, o);
                }
                break;
            }
        }
        xFrameworks.Add (
            new XElement ("Framework",
                new XAttribute ("Name", o.GetDirectoryName ()),
                new XAttribute ("Source", o.GetDirectoryName ())));
    }
}

Task ("docs-update-frameworks")
    .Does (() => 
{
    var docsTempPath = MakeAbsolute (ROOT_PATH.Combine ("output/docs/temp"));

    // create the frameworks folder from the released NuGets
    CreateFrameworks (docsTempPath);

    // the reference folders to locate assemblies
    var refs = new List<DirectoryPath> ();
    if (IsRunningOnWindows ()) {
        var refAssemblies = "C:/Program Files (x86)/Microsoft Visual Studio/*/*/Common7/IDE/ReferenceAssemblies/Microsoft/Framework";
        refs.AddRange (GetDirectories ($"{refAssemblies}/MonoAndroid/v1.0"));
        refs.AddRange (GetDirectories ($"{refAssemblies}/MonoAndroid/v4.0.3"));
        refs.AddRange (GetDirectories ($"{refAssemblies}/Xamarin.iOS/v1.0"));
        refs.AddRange (GetDirectories ($"{refAssemblies}/Xamarin.TVOS/v1.0"));
        refs.AddRange (GetDirectories ($"{refAssemblies}/Xamarin.WatchOS/v1.0"));
        refs.AddRange (GetDirectories ($"{refAssemblies}/Xamarin.Mac/v2.0"));
        refs.AddRange (GetDirectories ("C:/Program Files (x86)/Windows Kits/10/References/Windows.Foundation.UniversalApiContract/1.0.0.0"));
        refs.AddRange (GetDirectories ($"{NUGET_PACKAGES}/xamarin.forms/{GetVersion ("Xamarin.Forms", "release")}/lib/*"));
        refs.AddRange (GetDirectories ($"{NUGET_PACKAGES}/tizen.net/{GetVersion ("Tizen.NET", "release")}/lib/*"));
        refs.AddRange (GetDirectories ($"{NUGET_PACKAGES}/opentk.glcontrol/{GetVersion ("OpenTK.GLControl", "release")}/lib/*"));
    }

    // generate doc files
    var refArgs = string.Join (" ", refs.Select (r => $"--lib=\"{r}\""));
    var fw = MakeAbsolute (docsTempPath.CombineWithFilePath ("frameworks.xml"));
    RunProcess (MDocPath, new ProcessSettings {
        Arguments = $"update --delete --out=\"{DOCS_PATH}\" -lang=DocId --frameworks={fw} {refArgs}",
        WorkingDirectory = docsTempPath
    });

    // clean up after working
    CleanDirectories (docsTempPath.FullPath);
});

Task ("docs-format-docs")
    .Does (() => 
{
    // process the generated docs
    var docFiles = GetFiles ("./docs/**/*.xml");
    float typeCount = 0;
    float memberCount = 0;
    float totalTypes = 0;
    float totalMembers = 0;
    foreach (var file in docFiles) {
        var xdoc = XDocument.Load (file.FullPath);

        // remove IComponent docs as this is just designer
        if (xdoc.Root.Name == "Type") {
            xdoc.Root
                .Elements ("Members")
                .Elements ("Member")
                .Where (e => e.Attribute ("MemberName")?.Value?.StartsWith ("System.ComponentModel.IComponent.") == true)
                .Remove ();
        }

        // remove any duplicate public keys
        if (xdoc.Root.Name == "Overview") {
            var multiKey = xdoc.Root
                .Elements ("Assemblies")
                .Elements ("Assembly")
                .Where (e => e.Elements ("AssemblyPublicKey").Count () > 1);
            foreach (var mass in multiKey) {
                mass.Elements ("AssemblyPublicKey")
                    .Skip (1)
                    .Remove ();
            }
        }

        // Fix the type rename from SkPath1DPathEffectStyle to SKPath1DPathEffectStyle
        // this breaks linux as it is just a case change and that OS is case sensitive
        if (xdoc.Root.Name == "Overview") {
            xdoc.Root
                .Elements ("Types")
                .Elements ("Namespace")
                .Elements ("Type")
                .Where (e => e.Attribute ("Name")?.Value == "SkPath1DPathEffectStyle")
                .Remove ();
        }

        // count the types without docs
        var typesWithDocs = xdoc.Root
            .Elements ("Docs");
        totalTypes += typesWithDocs.Count ();
        var currentTypeCount = typesWithDocs.Count (m => m.Value?.IndexOf ("To be added.") >= 0);
        typeCount += currentTypeCount;

        // count the members without docs
        var membersWithDocs = xdoc.Root
            .Elements ("Members")
            .Elements ("Member")
            .Where (m => m.Attribute ("MemberName")?.Value != "Dispose" && m.Attribute ("MemberName")?.Value != "Finalize")
            .Elements ("Docs");
        totalMembers += membersWithDocs.Count ();
        var currentMemberCount = membersWithDocs.Count (m => m.Value?.IndexOf ("To be added.") >= 0);
        memberCount += currentMemberCount;

        // log if either type or member has missing docs
        currentMemberCount += currentTypeCount;
        if (currentMemberCount > 0) {
            var fullName = xdoc.Root.Attribute ("FullName");
            if (fullName != null)
                Information ("Docs missing on {0} = {1}", fullName.Value, currentMemberCount);
        }

        // get the whitespaces right
        var settings = new XmlWriterSettings {
            Encoding = new UTF8Encoding (),
            Indent = true,
            NewLineChars = "\n",
            OmitXmlDeclaration = true,
        };
        using (var writer = XmlWriter.Create (file.ToString (), settings)) {
            xdoc.Save (writer);
            writer.Flush ();
        }

        // empty line at the end
        System.IO.File.AppendAllText (file.ToString (), "\n");
    }

    // log summary
    Information (
        "Documentation missing in {0}/{1} ({2:0.0%}) types and {3}/{4} ({5:0.0%}) members.", 
        typeCount, totalTypes, typeCount / totalTypes, 
        memberCount, totalMembers, memberCount / totalMembers);
});

Task ("update-docs")
    .IsDependentOn ("docs-update-frameworks")
    .IsDependentOn ("docs-format-docs")
    .Does (() => 
{
});
