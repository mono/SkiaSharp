#addin nuget:?package=Cake.FileHelpers&version=4.0.1

#tool nuget:?package=xunit.runner.console&version=2.4.2

////////////////////////////////////////////////////////////////////////////////////////////////////
// TEST UTILITIES — shared by desktop test cakes
////////////////////////////////////////////////////////////////////////////////////////////////////

void RunTests(FilePath testAssembly, DirectoryPath output, bool is32)
{
    var dir = testAssembly.GetDirectory();
    var settings = new XUnit2Settings {
        ReportName = "TestResults",
        XmlReport = true,
        UseX86 = is32,
        NoAppDomain = true,
        Parallelism = ParallelismOption.All,
        OutputDirectory = MakeAbsolute(output).FullPath,
        WorkingDirectory = dir,
        ArgumentCustomization = args => args.Append("-verbose"),
    };
    XUnit2(new [] { testAssembly }, settings);
}

void RunDotNetTest(
    FilePath testProject,
    DirectoryPath output,
    string configuration = null,
    Dictionary<string, string> properties = null)
{
    output = MakeAbsolute(output);
    var dir = testProject.GetDirectory();
    var settings = new DotNetTestSettings {
        Configuration = configuration ?? CONFIGURATION,
        NoBuild = true,
        Loggers = new [] { "xunit" },
        WorkingDirectory = dir,
        ResultsDirectory = output,
        Verbosity = DotNetVerbosity.Normal,
        ArgumentCustomization = args => {
            args = args
                .Append("/p:Platform=\"AnyCPU\"");
            if (COVERAGE)
                args = args
                    .Append("/p:CollectCoverage=true")
                    .Append("/p:CoverletOutputFormat=cobertura")
                    .Append($"/p:CoverletOutput={output.Combine("Coverage").FullPath}/");
            if (properties != null) {
                foreach (var prop in properties) {
                    if (!string.IsNullOrEmpty(prop.Value)) {
                        args = args
                            .Append($"/p:{prop.Key}={prop.Value}");
                    }
                }
            }
            return args;
        },
    };
    DotNetTest(MakeAbsolute(testProject).FullPath, settings);
}

void RunCodeCoverage(string testResultsGlob, DirectoryPath output)
{
    try {
        DotNetTool(
            $"reportgenerator" +
            $"  -reports:{testResultsGlob}" +
            $"  -targetdir:{output}" +
            $"  -reporttypes:HtmlInline_AzurePipelines;Cobertura" +
            $"  -assemblyfilters:-*.Tests");
    } catch (Exception ex) {
        Error("Make sure to install the 'dotnet-reportgenerator-globaltool' .NET Core global tool.");
        Error(ex);
        throw;
    }
    var xml = $"{output}/Cobertura.xml";
    var root = FindRegexMatchGroupsInFile(xml, @"<source>(.*)<\/source>", 0)[1].Value;
    ReplaceTextInFiles(xml, root, "");
}
