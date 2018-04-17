using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("SkiaSharp")]
[assembly: AssemblyDescription("SkiaSharp is a cross-platform 2D graphics API for .NET platforms that can be used across mobile, server and desktop models to render images.")]
[assembly: AssemblyCompany("Xamarin Inc.")]
[assembly: AssemblyProduct("SkiaSharp")]
[assembly: AssemblyCopyright("© Microsoft Corporation. All rights reserved.")]
[assembly: NeutralResourcesLanguage("en")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: InternalsVisibleTo("SkiaSharp.Tests, PublicKey=" +
	"002400000480000094000000060200000024000052534131000400000100010079159977d2d03a" +
	"8e6bea7a2e74e8d1afcc93e8851974952bb480a12c9134474d04062447c37e0e68c080536fcf3c" +
	"3fbe2ff9c979ce998475e506e8ce82dd5b0f350dc10e93bf2eeecf874b24770c5081dbea7447fd" +
	"dafa277b22de47d6ffea449674a4f9fccf84d15069089380284dbdd35f46cdff12a1bd78e4ef00" +
	"65d016df")]
