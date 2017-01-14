
var TransformToUWP = new Action<FilePath, string> ((projectFilePath, platform) => {
    var projectFile = MakeAbsolute (projectFilePath).FullPath;
    var xdoc = XDocument.Load (projectFile);
    
    var configType = xdoc.Root
        .Elements (MSBuildNS + "PropertyGroup")
        .Elements (MSBuildNS + "ConfigurationType")
        .Select (e => e.Value)
        .FirstOrDefault ();
    if (configType != "StaticLibrary") {
        // skip over "Utility" projects as they aren't actually 
        // library projects, but intermediate build steps.
        return;
    } else {
        // special case for ARM, gyp does not yet have ARM, 
        // so it defaults to Win32
        // update and reload
        if (platform.ToUpper () == "ARM") {
            ReplaceTextInFiles (projectFile, "Win32", "ARM");
            xdoc = XDocument.Load (projectFile);
        }
    }
    
    var rootNamespace = xdoc.Root
        .Elements (MSBuildNS + "PropertyGroup")
        .Elements (MSBuildNS + "RootNamespace")
        .Select (e => e.Value)
        .FirstOrDefault ();
    var globals = xdoc.Root
        .Elements (MSBuildNS + "PropertyGroup")
        .Where (e => e.Attribute ("Label") != null && e.Attribute ("Label").Value == "Globals")
        .Single ();
        
    globals.Elements (MSBuildNS + "WindowsTargetPlatformVersion").Remove ();
    SetXValue (globals, "Keyword", "StaticLibrary");
    SetXValue (globals, "AppContainerApplication", "true");
    SetXValue (globals, "ApplicationType", "Windows Store");
    SetXValue (globals, "WindowsTargetPlatformVersion", "10.0.10586.0");
    SetXValue (globals, "WindowsTargetPlatformMinVersion", "10.0.10240.0");
    SetXValue (globals, "ApplicationTypeRevision", "10.0");
    SetXValue (globals, "DefaultLanguage", "en-US");

    var properties = xdoc.Root
        .Elements (MSBuildNS + "PropertyGroup")
        .Elements (MSBuildNS + "LinkIncremental")
        .First ()
        .Parent;
    SetXValue (properties, "GenerateManifest","false");
    SetXValue (properties, "IgnoreImportLibrary","false");

    SetXValues (xdoc.Root, new [] { "ItemDefinitionGroup", "ClCompile" }, "CompileAsWinRT", "false");
    SetXValues (xdoc.Root, new [] { "ItemDefinitionGroup", "ClCompile" }, "DebugInformationFormat", "ProgramDatabase");
    AddXValues (xdoc.Root, new [] { "ItemDefinitionGroup", "ClCompile" }, "PreprocessorDefinitions", ";SK_BUILD_FOR_WINRT;WINAPI_FAMILY=WINAPI_FAMILY_APP;");
    // if (platform.ToUpper () == "ARM") {
    //     AddXValues (xdoc.Root, new [] { "ItemDefinitionGroup", "ClCompile" }, "PreprocessorDefinitions", ";__ARM_NEON;__ARM_NEON__;");
    // }
    AddXValues (xdoc.Root, new [] { "ItemDefinitionGroup", "ClCompile" }, "DisableSpecificWarnings", ";4146;4703;");
    SetXValues (xdoc.Root, new [] { "ItemDefinitionGroup", "Link" }, "SubSystem", "Console");
    SetXValues (xdoc.Root, new [] { "ItemDefinitionGroup", "Link" }, "IgnoreAllDefaultLibraries", "false");
    SetXValues (xdoc.Root, new [] { "ItemDefinitionGroup", "Link" }, "GenerateWindowsMetadata", "false");

    xdoc.Root
        .Elements (MSBuildNS + "ItemDefinitionGroup")
        .Elements (MSBuildNS + "Link")
        .Elements (MSBuildNS + "AdditionalDependencies")
        .Remove ();
        
    if (rootNamespace == "pdf") {
        // remove sfntly as this is not supported for winrt
        RemoveXValues (xdoc.Root, new [] { "ItemDefinitionGroup", "ClCompile" }, "PreprocessorDefinitions", "SK_PDF_USE_SFNTLY");
    } else if (rootNamespace == "ports") {
        RemoveFileReference (xdoc.Root, "SkFontHost_win.cpp");
        AddXValues (xdoc.Root, new [] { "ItemDefinitionGroup", "ClCompile" }, "PreprocessorDefinitions", ";SK_HAS_DWRITE_1_H;SK_HAS_DWRITE_2_H;");
    } else if (rootNamespace == "skgpu" ) {
        // GL is not available to WinRT
        RemoveFileReference (xdoc.Root, "GrGLCreateNativeInterface_none.cpp");
        AddFileReference (xdoc.Root, @"..\..\src\gpu\gl\GrGLCreateNativeInterface_none.cpp");
        RemoveFileReference (xdoc.Root, "GrGLCreateNativeInterface_win.cpp");
        RemoveFileReference (xdoc.Root, "SkCreatePlatformGLContext_win.cpp");
    } else if (rootNamespace == "utils" ) {
        // GL is not available to WinRT
        RemoveFileReference (xdoc.Root, "SkWGL.h");
        RemoveFileReference (xdoc.Root, "SkWGL_win.cpp");
        AddXValues (xdoc.Root, new [] { "ItemDefinitionGroup", "ClCompile" }, "PreprocessorDefinitions", ";SK_HAS_DWRITE_1_H;SK_HAS_DWRITE_2_H;");
    } 

    xdoc.Save (projectFile);
});
