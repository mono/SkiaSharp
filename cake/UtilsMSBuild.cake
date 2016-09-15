
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
