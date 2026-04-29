using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharpSample;

/// <summary>
/// Metadata for a sample category: display name, brand color, and icons.
/// </summary>
public readonly record struct SampleCategory(
	string Name,
	string Color,
	string CssIcon,
	string UnicodeIcon);

/// <summary>
/// Central registry for sample metadata — categories, icons, filtering, sorting, and "new" detection.
/// Shared across Blazor and Uno hosts.
/// </summary>
public static class SampleManager
{
	// Bootstrap-icons font path for Uno/XAML hosts
	public const string IconFontPath = "ms-appx:///Assets/Fonts/bootstrap-icons.ttf#bootstrap-icons";

	// Theme toggle icons (for Uno)
	public const string IconMoonStars = "\uf496"; // bi-moon-stars
	public const string IconSun = "\uf5a2";       // bi-sun

	// ---------------------------------------------------------------
	// Category name constants (used by samples and hosts)
	// ---------------------------------------------------------------

	public const string ImageFilters = "Image & Filters";
	public const string Shaders = "Shaders & Effects";
	public const string Text = "Text & Typography";
	public const string Paths = "Paths & Geometry";
	public const string General = "General";
	public const string BitmapDecoding = "Image Decoding";
	public const string Documents = "Documents";

	// ---------------------------------------------------------------
	// Categories
	// ---------------------------------------------------------------

	private static readonly SampleCategory[] AllCategories =
	[
		new(ImageFilters,    "#E65100", "bi-image",                 "\uf39b"),
		new(Shaders,         "#1565C0", "bi-lightning",             "\uf46f"),
		new(Text,            "#AD1457", "bi-fonts",                 "\uf3da"),
		new(Paths,           "#2E7D32", "bi-pentagon",              "\uf4ce"),
		new(General,         "#1A237E", "bi-grid-3x3",              "\uf3fa"),
		new(BitmapDecoding,  "#00897B", "bi-file-image",            "\uf39b"),
		new(Documents,       "#607D8B", "bi-file-earmark-richtext", "\uf383"),
	];

	private static readonly Dictionary<string, SampleCategory> CategoryMap =
		AllCategories.ToDictionary(c => c.Name);

	public static IReadOnlyList<SampleCategory> GetCategories() => AllCategories;

	public static SampleCategory GetCategoryFor(string categoryName) =>
		CategoryMap.TryGetValue(categoryName, out var cat)
			? cat
			: new SampleCategory(categoryName, "#546E7A", "bi-brush", "\uf1d8");

	// ---------------------------------------------------------------
	// Per-sample icons
	// ---------------------------------------------------------------

	private static readonly Dictionary<string, (string Css, string Unicode)> SampleIcons = new()
	{
		["Blend Modes"]           = ("bi-layers-half",         "\uf45a"),
		["Blur Image Filter"]     = ("bi-droplet-half",        "\uf30c"),
		["2D Transforms"]         = ("bi-arrows-move",         "\uf14e"),
		["3D Transforms"]         = ("bi-box",                 "\uf1c8"),
		["Color Font"]            = ("bi-palette",             "\uf4c3"),
		["Create XPS Document"]   = ("bi-file-earmark-richtext", "\uf383"),
		["Fill Path"]             = ("bi-paint-bucket",        "\uf4ba"),
		["GIF Player"]            = ("bi-film",                "\uf3c3"),
		["Gradient"]              = ("bi-rainbow",             "\uf50d"),
		["Image Decoder"]         = ("bi-file-image",          "\uf39b"),
		["Lottie Player"]         = ("bi-play-circle",         "\uf4f3"),
		["Nine-Patch Scaler"]     = ("bi-grid-3x3",            "\uf3fa"),
		["Path Builder"]          = ("bi-pentagon",            "\uf4ce"),
		["Path Effects Sampler"]  = ("bi-bezier2",             "\uf18c"),
		["PDF Composer"]          = ("bi-file-pdf",            "\uf640"),
		["Perlin Noise Textures"] = ("bi-soundwave",           "\uf57c"),
		["Photo Lab"]             = ("bi-camera",              "\uf220"),
		["Shader Cross-Fade"]     = ("bi-shuffle",             "\uf556"),
		["Shader Playground"]     = ("bi-lightning",           "\uf46f"),
		["Smart Underline"]       = ("bi-type-underline",      "\uf5f8"),
		["Text Lab"]              = ("bi-fonts",               "\uf3da"),
		["Text on Path"]          = ("bi-type",                "\uf5f7"),
		["Text to Path"]          = ("bi-vector-pen",          "\uf604"),
		["Variable Font"]         = ("bi-sliders",             "\uf556"),
		["Vector Art"]            = ("bi-vector-pen",          "\uf604"),
		["Vertex Mesh"]           = ("bi-diagram-3",           "\uf2ee"),
		["Wide-Gamut P3"]         = ("bi-rainbow",             "\uf50d"),
		["World Text"]            = ("bi-globe2",              "\uf3ef"),
	};

	private const string DefaultCssIcon = "bi-brush";
	private const string DefaultUnicodeIcon = "\uf1d8";

	/// <summary>CSS class name for Blazor (e.g. "bi-lightning").</summary>
	public static string GetCssIcon(string sampleTitle) =>
		SampleIcons.TryGetValue(sampleTitle, out var icons) ? icons.Css : DefaultCssIcon;

	/// <summary>Unicode glyph for the bootstrap-icons TTF font (Uno).</summary>
	public static string GetUnicodeIcon(string sampleTitle) =>
		SampleIcons.TryGetValue(sampleTitle, out var icons) ? icons.Unicode : DefaultUnicodeIcon;

	// ---------------------------------------------------------------
	// "New" detection — samples within 10 days of the newest
	// ---------------------------------------------------------------

	private static DateOnly? cachedCutoff;

	/// <summary>
	/// Returns true if this sample's DateAdded is within the "new" window
	/// (the newest sample's date minus 10 days).
	/// </summary>
	public static bool IsNew(SampleBase sample, IEnumerable<SampleBase> allSamples)
	{
		if (sample.DateAdded is not { } added)
			return false;

		cachedCutoff ??= ComputeCutoff(allSamples);
		return added >= cachedCutoff.Value;
	}

	/// <summary>Reset the cached cutoff (call if the sample set changes).</summary>
	public static void ResetNewCache() => cachedCutoff = null;

	private static DateOnly ComputeCutoff(IEnumerable<SampleBase> allSamples)
	{
		var newest = allSamples
			.Where(s => s.DateAdded.HasValue)
			.Max(s => s.DateAdded!.Value);
		return newest.AddDays(-10);
	}

	// ---------------------------------------------------------------
	// Card sizing for masonry layout
	// ---------------------------------------------------------------

	/// <summary>
	/// Determines the card size for masonry layout.
	/// Feature: new or animated samples. Compact: old (pre-2026). Standard: everything else.
	/// </summary>
	public static SampleCardSize GetCardSize(SampleBase sample, IEnumerable<SampleBase> allSamples)
	{
		if (IsNew(sample, allSamples) || (sample is CanvasSampleBase cs && cs.IsAnimated))
			return SampleCardSize.Feature;

		if (sample.DateAdded is { } date && date < new DateOnly(2026, 1, 1))
			return SampleCardSize.Compact;

		return SampleCardSize.Standard;
	}

	/// <summary>Returns the number of samples in a given category.</summary>
	public static int GetSampleCount(string categoryName, IEnumerable<SampleBase> allSamples) =>
		allSamples.Count(s => s.Category == categoryName);

	// ---------------------------------------------------------------
	// Search, filter, sort
	// ---------------------------------------------------------------

	/// <summary>
	/// Filter and sort samples. Default sort: NEW first, then day-seeded shuffle.
	/// Unsupported samples are always pushed to the end.
	/// </summary>
	public static IEnumerable<SampleBase> SearchSamples(
		IEnumerable<SampleBase> samples,
		string? searchText = null,
		ISet<string>? categories = null,
		SampleSortOrder sort = SampleSortOrder.NewestFirst)
	{
		var query = samples.ToList().AsEnumerable();

		if (!string.IsNullOrWhiteSpace(searchText))
			query = query.Where(s => s.MatchesFilter(searchText));

		if (categories is { Count: > 0 })
			query = query.Where(s => categories.Contains(s.Category));

		var list = query.ToList();

		return sort switch
		{
			SampleSortOrder.NewestFirst => SortNewFirstThenDayShuffle(list, samples),
			SampleSortOrder.OldestFirst => list
				.OrderBy(s => s.IsSupported ? 0 : 1)
				.ThenBy(s => s.DateAdded ?? DateOnly.MaxValue)
				.ThenBy(s => s.Title),
			SampleSortOrder.Alphabetical => list
				.OrderBy(s => s.IsSupported ? 0 : 1)
				.ThenBy(s => s.Title),
			SampleSortOrder.Category => list
				.OrderBy(s => s.IsSupported ? 0 : 1)
				.ThenBy(s => s.Category)
				.ThenBy(s => s.Title),
			_ => list
				.OrderBy(s => s.IsSupported ? 0 : 1)
				.ThenBy(s => s.Title),
		};
	}

	/// <summary>
	/// NEW items first (shuffled), then remaining items shuffled.
	/// Both groups use day-based seed so the gallery feels fresh each day.
	/// </summary>
	private static IEnumerable<SampleBase> SortNewFirstThenDayShuffle(
		List<SampleBase> items, IEnumerable<SampleBase> allSamples)
	{
		var unsupported = items.Where(s => !s.IsSupported).ToList();
		var supported = items.Where(s => s.IsSupported).ToList();

		var newItems = supported.Where(s => IsNew(s, allSamples)).ToList();
		var rest = supported.Where(s => !IsNew(s, allSamples)).ToList();

		// Day-based seed: changes once per day
		var daySeed = DateTime.UtcNow.DayOfYear * 1000 + DateTime.UtcNow.Year;

		Shuffle(newItems, new Random(daySeed));
		Shuffle(rest, new Random(daySeed + 7919)); // different seed for rest

		return newItems.Concat(rest).Concat(unsupported);
	}

	private static void Shuffle<T>(List<T> list, Random rng)
	{
		for (var i = list.Count - 1; i > 0; i--)
		{
			var j = rng.Next(i + 1);
			(list[i], list[j]) = (list[j], list[i]);
		}
	}
}

public enum SampleSortOrder
{
	NewestFirst,
	OldestFirst,
	Alphabetical,
	Category,
}

public enum SampleCardSize
{
	Feature,
	Standard,
	Compact,
}
