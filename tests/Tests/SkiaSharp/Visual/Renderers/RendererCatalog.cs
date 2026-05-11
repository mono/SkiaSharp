using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Discovers and exposes every renderer this build can talk to. Lazy:
	/// renderers are constructed on first access, never during test discovery.
	/// </summary>
	public static class RendererCatalog
	{
		private static readonly Lazy<IReadOnlyDictionary<string, IRenderer>> _byName =
			new Lazy<IReadOnlyDictionary<string, IRenderer>> (Discover);

		public static IEnumerable<IRenderer> All => _byName.Value.Values;

		public static IEnumerable<string> AllNames => _byName.Value.Keys;

		public static IRenderer Get (string name)
		{
			if (!_byName.Value.TryGetValue (name, out var renderer))
				throw new ArgumentException ($"Unknown renderer '{name}'. Known: {string.Join (", ", _byName.Value.Keys)}");
			return renderer;
		}

		private static IReadOnlyDictionary<string, IRenderer> Discover ()
		{
			// Auto-discovery via reflection. Mirrors SceneCatalog: any concrete
			// parameterless IRenderer in this assembly is registered. Per-host
			// projects (Wasm, Android, iOS) get their renderers by referencing
			// the same renderers source set + recompiling for their TFM.
			var types = typeof (RendererCatalog).Assembly.GetTypes ()
				.Where (t => t.IsPublic
					&& !t.IsAbstract
					&& typeof (IRenderer).IsAssignableFrom (t)
					&& t.GetConstructor (Type.EmptyTypes) != null);

			var dict = new SortedDictionary<string, IRenderer> (StringComparer.Ordinal);
			foreach (var t in types) {
				var renderer = (IRenderer)Activator.CreateInstance (t);
				if (dict.ContainsKey (renderer.Name))
					throw new InvalidOperationException ($"Duplicate renderer name '{renderer.Name}' on {t.FullName} and {dict[renderer.Name].GetType ().FullName}");
				dict[renderer.Name] = renderer;
			}
			return dict;
		}
	}
}
