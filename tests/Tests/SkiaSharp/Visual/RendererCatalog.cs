using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Discovers every <see cref="IRenderer"/> this build can talk to. Renderers
	/// are constructed once, lazily, on first access — never during test
	/// discovery — and must be cheap to construct (see <see cref="IRenderer"/>).
	///
	/// <para>
	/// Add a new backend by creating a public, non-abstract
	/// <see cref="IRenderer"/> with a parameterless constructor. Portable
	/// renderers live under <c>tests/Tests/SkiaSharp/Visual/Renderers/</c>;
	/// desktop-only renderers (which depend on the GL/Vulkan/Metal context
	/// abstractions) live under <c>Renderers/Desktop/</c> and are compiled only
	/// into the desktop host projects. This is the single seam the Graphite
	/// backend plugs into: it adds renderer classes and golden images, nothing
	/// else.
	/// </para>
	/// </summary>
	public static class RendererCatalog
	{
		private static readonly Lazy<IReadOnlyDictionary<string, IRenderer>> byName =
			new(Discover);

		public static IEnumerable<IRenderer> All => byName.Value.Values;

		public static IEnumerable<string> AllNames => byName.Value.Keys;

		public static IRenderer Get(string name)
		{
			if (!byName.Value.TryGetValue(name, out var renderer))
				throw new ArgumentException($"Unknown renderer '{name}'. Known: {string.Join(", ", byName.Value.Keys)}");
			return renderer;
		}

		private static IReadOnlyDictionary<string, IRenderer> Discover()
		{
			var dict = new SortedDictionary<string, IRenderer>(StringComparer.Ordinal);

			foreach (var type in CatalogReflection.ConcreteImplementations<IRenderer>())
			{
				var renderer = (IRenderer)Activator.CreateInstance(type);
				if (dict.TryGetValue(renderer.Name, out var existing))
					throw new InvalidOperationException(
						$"Duplicate renderer name '{renderer.Name}' on {type.FullName} and {existing.GetType().FullName}");
				dict[renderer.Name] = renderer;
			}

			return dict;
		}
	}
}
