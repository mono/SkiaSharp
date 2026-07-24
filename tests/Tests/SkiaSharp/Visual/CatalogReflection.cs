using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Shared reflection helper for the scene and renderer catalogs.
	///
	/// <para>
	/// A renderer (or scene) is discovered no matter which loaded assembly it lives
	/// in, as long as that assembly's simple name starts with <c>SkiaSharp</c>. We
	/// scan:
	/// </para>
	/// <list type="bullet">
	/// <item>the catalog assembly itself (the portable raster/Metal renderers and
	/// the scenes);</item>
	/// <item>the entry assembly, which is how each standalone Console host
	/// (<c>SkiaSharp.Tests.Console</c>, the Vulkan and Direct3D satellites) surfaces
	/// the renderers it compiles in — on desktop the entry assembly is the host;</item>
	/// <item>every other loaded <c>SkiaSharp.*</c> assembly, which is how the
	/// single-process device/MAUI host picks up a satellite renderer library.
	/// <see cref="Assembly.GetEntryAssembly"/> is <see langword="null"/> under
	/// MAUI/Android, so entry-assembly scanning alone would miss those; instead the
	/// host references the satellite library and registers it (e.g. in
	/// <c>MauiProgram</c>'s test-assembly list), which loads it into the
	/// <see cref="AppDomain"/> before discovery runs. Adding a new backend
	/// (Vulkan today, Direct3D next) is then just "reference its library + register
	/// it" — no change here.</item>
	/// </list>
	/// </summary>
	internal static class CatalogReflection
	{
		public static IEnumerable<Type> ConcreteImplementations<T>()
		{
			var seen = new HashSet<Type>();

			foreach (var assembly in CandidateAssemblies())
			{
				Type[] types;
				try
				{
					types = assembly.GetTypes();
				}
				catch (ReflectionTypeLoadException ex)
				{
					types = ex.Types.Where(t => t != null).ToArray();
				}

				foreach (var type in types)
				{
					if (type is null)
						continue;
					if (!type.IsPublic || type.IsAbstract)
						continue;
					if (!typeof(T).IsAssignableFrom(type))
						continue;
					if (type.GetConstructor(Type.EmptyTypes) is null)
						continue;
					if (seen.Add(type))
						yield return type;
				}
			}
		}

		private static IEnumerable<Assembly> CandidateAssemblies()
		{
			var seen = new HashSet<Assembly>();

			// The catalog assembly and the entry assembly are always in scope. The
			// entry assembly is how a standalone Console host contributes the
			// renderers it compiles in; it is null under MAUI/Android, hence the
			// broader scan below.
			void Add(Assembly assembly, List<Assembly> into)
			{
				if (assembly is not null && seen.Add(assembly))
					into.Add(assembly);
			}

			var result = new List<Assembly>();
			Add(typeof(CatalogReflection).Assembly, result);
			Add(Assembly.GetEntryAssembly(), result);

			// Every other loaded SkiaSharp.* assembly. This is how a device/MAUI host
			// finds a satellite renderer library (Vulkan, Direct3D): the host
			// references it and registers it so it is loaded before discovery. The
			// name filter keeps the scan cheap and avoids unrelated assemblies.
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				var name = assembly.GetName().Name;
				if (name is not null && name.StartsWith("SkiaSharp", StringComparison.Ordinal))
					Add(assembly, result);
			}

			return result;
		}
	}
}
