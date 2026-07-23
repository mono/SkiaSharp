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
	/// Scans both the assembly that defines the catalog (where the portable
	/// renderers and scenes live — raster, Metal, and Vulkan) and the entry
	/// assembly, so a host that compiles in its own renderer (the Console host adds
	/// the desktop GL renderer) is covered too, without the shared catalog needing
	/// to reference it.
	/// </para>
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
			yield return typeof(CatalogReflection).Assembly;

			var entry = Assembly.GetEntryAssembly();
			if (entry is not null && entry != typeof(CatalogReflection).Assembly)
				yield return entry;
		}
	}
}
