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
	/// renderers and scenes live) and the host test assembly (Console, Vulkan,
	/// Direct3D, Devices, Wasm). This lets host-specific projects contribute their
	/// own renderers (e.g. the Vulkan project adds a Vulkan renderer that depends on
	/// Silk.NET) without the shared catalog needing to reference them. The host is
	/// the entry assembly on desktop, but <see cref="Assembly.GetEntryAssembly"/> is
	/// null on Android/MAUI and WASM, so loaded <c>SkiaSharp.Tests*</c> assemblies
	/// are also scanned by name.
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
			var catalog = typeof(CatalogReflection).Assembly;
			var seen = new HashSet<Assembly> { catalog };
			yield return catalog;

			// The host test project contributes its own renderers (e.g. the Vulkan
			// host adds GaneshVulkanRenderer). On the desktop Console host that host
			// *is* the entry assembly, but Assembly.GetEntryAssembly() returns null
			// on Android/MAUI and WASM, so relying on it alone hides those renderers
			// on device. Every host assembly is named "SkiaSharp.Tests*", so also
			// pick up any that are already loaded.
			var entry = Assembly.GetEntryAssembly();
			if (entry is not null && seen.Add(entry))
				yield return entry;

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				var name = assembly.GetName().Name;
				if (name is not null &&
					name.StartsWith("SkiaSharp.Tests", StringComparison.Ordinal) &&
					seen.Add(assembly))
				{
					yield return assembly;
				}
			}
		}
	}
}
