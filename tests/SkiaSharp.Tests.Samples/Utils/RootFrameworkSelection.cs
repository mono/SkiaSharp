using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SkiaSharp.Tests.Samples.Utils;

internal static class RootFrameworkSelection
{
    internal static string Write(SampleWorkspace workspace, string project, string framework, string diagnostics)
    {
        if (!Regex.IsMatch(framework, "^[A-Za-z0-9][A-Za-z0-9.-]+$"))
            throw new ArgumentException("Select one declared target framework.", nameof(framework));
        project = Path.GetFullPath(project);
        var relative = Path.GetRelativePath(workspace.Root, project);
        if (Path.IsPathRooted(relative) || relative == ".." ||
            relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new ArgumentException("The selected root must belong to the owned sample workspace.", nameof(project));
        var path = Path.Combine(diagnostics, "selected-root.targets");
        Directory.CreateDirectory(diagnostics);
        const string root = "'$(MSBuildProjectFullPath)' == '$(RootSampleFullPath)'";
        new XDocument(new XElement("Project", new XAttribute("InitialTargets", "ValidateRootSampleFrameworkSelection"),
            new XElement("PropertyGroup",
                new XElement("RootSampleFullPath", new XAttribute("Condition", "'$(SampleRootProject)' != ''"),
                    "$([System.IO.Path]::GetFullPath('$(SampleRootProject)'))")),
            new XElement("PropertyGroup", new XAttribute("Condition", root),
                new XElement("RootDeclaredFrameworks", "$(TargetFrameworks);$(TargetFramework)"),
                new XElement("TargetFrameworks", new XAttribute("Condition", "'$(TargetFrameworks)' != ''"), "$(SampleRootFramework)")),
            new XElement("Target",
                new XAttribute("Name", "ValidateRootSampleFrameworkSelection"),
                new XAttribute("Condition", root),
                new XElement("Error",
                    new XAttribute("Condition", "'$(SampleRootFramework)' == '' Or !$([System.String]::Copy(';$(RootDeclaredFrameworks);').Contains(';$(SampleRootFramework);'))"),
                    new XAttribute("Text", "Selected framework is not declared by this sample root: $(SampleRootFramework); declared $(RootDeclaredFrameworks).")))))
            .Save(path);
        var targets = Directory.EnumerateFiles(Path.Combine(workspace.Root, "samples"), "Directory.Build.targets",
            SearchOption.AllDirectories).Append(Path.Combine(workspace.Root, "Directory.Build.targets"));
        foreach (var target in targets)
        {
            var document = XDocument.Load(target);
            var import = document.Root!.Elements("Import")
                .SingleOrDefault(element => (string?)element.Attribute("Label") == "SampleRootFrameworkSelection");
            if (import is null)
                document.Root.Add(new XElement("Import", new XAttribute("Project", path),
                    new XAttribute("Label", "SampleRootFrameworkSelection")));
            else
                import.SetAttributeValue("Project", path);
            document.Save(target);
        }
        return path;
    }
}
