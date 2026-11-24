using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Discovers every <see cref="ISkiaScene"/> in the test assemblies. Tests
	/// reference scenes by name; the catalog resolves them back to instances.
	///
	/// <para>
	/// Add a new scene by creating a public, non-abstract
	/// <see cref="ISkiaScene"/> with a parameterless constructor under
	/// <c>tests/Tests/SkiaSharp/Visual/Scenes/</c>. It appears in every renderer's
	/// column automatically.
	/// </para>
	/// </summary>
	public static class SceneCatalog
	{
		private static readonly Lazy<IReadOnlyDictionary<string, ISkiaScene>> byName =
			new(Discover);

		public static IEnumerable<ISkiaScene> All => byName.Value.Values;

		public static IEnumerable<string> AllNames => byName.Value.Keys;

		public static ISkiaScene Get(string name)
		{
			if (!byName.Value.TryGetValue(name, out var scene))
				throw new ArgumentException($"Unknown scene '{name}'. Known: {string.Join(", ", byName.Value.Keys)}");
			return scene;
		}

		private static IReadOnlyDictionary<string, ISkiaScene> Discover()
		{
			var dict = new SortedDictionary<string, ISkiaScene>(StringComparer.Ordinal);

			foreach (var type in CatalogReflection.ConcreteImplementations<ISkiaScene>())
			{
				var scene = (ISkiaScene)Activator.CreateInstance(type);
				if (dict.TryGetValue(scene.Name, out var existing))
					throw new InvalidOperationException(
						$"Duplicate scene name '{scene.Name}' on {type.FullName} and {existing.GetType().FullName}");
				dict[scene.Name] = scene;
			}

			return dict;
		}
	}
}
