using System;
using System.Reflection;
using System.Resources;

[assembly: AssemblyTitle("SkiaSharp.DotNet.Interactive")]
[assembly: AssemblyDescription("SkiaSharp.DotNet.Interactive adds functionality for SkiaSharp to .NET Interactive notebooks.")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyProduct("SkiaSharp.DotNet.Interactive")]
[assembly: AssemblyCopyright("© Microsoft Corporation. All rights reserved.")]
[assembly: NeutralResourcesLanguage("en")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
