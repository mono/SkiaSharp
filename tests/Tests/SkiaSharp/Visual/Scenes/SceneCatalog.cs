using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Discovers and exposes every <see cref="ISkiaScene"/> implementation in
	/// this assembly. Tests reference scenes by name; the catalog is what
	/// resolves them back to instances at run-time.
	/// </summary>
	public static class SceneCatalog
	{
		private static readonly Lazy<IReadOnlyDictionary<string, ISkiaScene>> _byName =
			new Lazy<IReadOnlyDictionary<string, ISkiaScene>> (Discover);

		public static IEnumerable<ISkiaScene> All => _byName.Value.Values;

		public static IEnumerable<string> AllNames => _byName.Value.Keys;

		public static ISkiaScene Get (string name)
		{
			if (!_byName.Value.TryGetValue (name, out var scene))
				throw new ArgumentException ($"Unknown scene '{name}'. Known: {string.Join (", ", _byName.Value.Keys)}");
			return scene;
		}

		private static IReadOnlyDictionary<string, ISkiaScene> Discover ()
		{
			// Single-assembly scan. Public, non-nested, parameterless-ctor
			// implementations are registered. Test-local scenes (nested in a
			// test class) stay out of the matrix automatically.
			var types = typeof (SceneCatalog).Assembly.GetTypes ()
				.Where (t => t.IsPublic
					&& !t.IsAbstract
					&& typeof (ISkiaScene).IsAssignableFrom (t)
					&& t.GetConstructor (Type.EmptyTypes) != null);

			var dict = new SortedDictionary<string, ISkiaScene> (StringComparer.Ordinal);
			foreach (var t in types) {
				var scene = (ISkiaScene)Activator.CreateInstance (t);
				if (dict.ContainsKey (scene.Name))
					throw new InvalidOperationException ($"Duplicate scene name '{scene.Name}' on {t.FullName} and {dict[scene.Name].GetType ().FullName}");
				dict[scene.Name] = scene;
			}
			return dict;
		}
	}
}
