using System;
using System.Reflection;
using System.Resources;

[assembly: AssemblyTitle("SkiaSharp.HarfBuzz")]
[assembly: AssemblyDescription("This package adds text shaping support to SkiaSharp via HarfBuzz.")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyProduct("SkiaSharp.HarfBuzz")]
[assembly: AssemblyCopyright("© Microsoft Corporation. All rights reserved.")]
[assembly: NeutralResourcesLanguage("en")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
