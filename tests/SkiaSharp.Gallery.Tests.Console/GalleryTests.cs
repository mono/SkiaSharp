using System.Text;
using SkiaSharp;
using SkiaSharpSample;
using SkiaSharpSample.Controls;
using SkiaSharpSample.Services;
using Xunit;

namespace SkiaSharp.Gallery.Tests;

public class GalleryTests
{
    public static IEnumerable<object[]> SupportedSamples =>
        new SampleService().GetSamples().Select(sample => new object[] { sample.Title });

    [Fact]
    public void CatalogIncludesEverySampleAndFiltersUnsupportedPlatforms()
    {
        var service = new SampleService();
        var all = service.GetAllSamples().ToArray();
        var expected = typeof(SampleBase).Assembly.DefinedTypes
            .Count(type => !type.IsAbstract && typeof(SampleBase).IsAssignableFrom(type));

        Assert.Equal(expected, all.Length);
        Assert.Equal(all.Length, all.Select(sample => sample.Title).Distinct().Count());
        Assert.Contains(all, sample => sample is CanvasSampleBase);
        Assert.Contains(all, sample => sample is DocumentSampleBase);
        Assert.Equal(all.Where(sample => sample.IsSupported), service.GetSamples());
        Assert.All(all.Where(sample => !sample.IsSupported),
            sample => Assert.Null(service.GetSample(sample.Title)));
    }

    [Fact]
    public void SearchCombinesCategoryTagsAndText()
    {
        var samples = new SampleService().GetAllSamples().ToArray();
        var categories = new HashSet<string> { SampleManager.Shaders };
        var tags = new HashSet<string> { "SKShader", "SKRuntimeEffect" };

        foreach (var matchAll in new[] { false, true })
        {
            var expected = samples.Where(sample =>
                sample.MatchesFilter("shader") &&
                categories.Contains(sample.Category) &&
                (matchAll ? tags.All(sample.ApiTags.Contains) : tags.Any(sample.ApiTags.Contains)));
            var actual = SampleManager.SearchSamples(
                samples, "shader", categories, tags, matchAll, SampleSortOrder.Alphabetical);

            Assert.Equal(expected.OrderBy(sample => sample.IsSupported ? 0 : 1).ThenBy(sample => sample.Title), actual);
        }

        Assert.Empty(SampleManager.SearchSamples(samples, "no-such-gallery-sample"));
        Assert.Equal(samples.Length, SampleManager.SearchSamples(
            samples, "", new HashSet<string>(), new HashSet<string>()).Count());
    }

    [Fact]
    public void SharedCategoriesAreAlphabetical()
    {
        var names = SampleManager.GetCategories().Select(category => category.Name).ToArray();

        Assert.Equal(names.OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(name => name, StringComparer.Ordinal), names);
    }

    [Fact]
    public void SharedApiTagsAreAlphabeticalByTheirDisplayedName()
    {
        var samples = new SampleService().GetAllSamples().ToArray();
        var tags = SampleManager.GetAllTags(samples);
        var expected = tags
            .OrderBy(tag => KnownApis.GetDisplayName(tag.Tag), StringComparer.OrdinalIgnoreCase)
            .ThenBy(tag => tag.Tag, StringComparer.Ordinal);

        Assert.Equal(expected, tags);
        Assert.Equal(tags, SampleManager.GetAllTags(samples.Reverse()));
        Assert.All(tags, tag => Assert.Equal(
            samples.Count(sample => sample.ApiTags.Contains(tag.Tag)), tag.Count));
    }

    [Fact]
    public void TagDisplayNameTiesUseFullApiIdentity()
    {
        var sample = new MetadataSample("Tags", "General",
            "ZType", "Second.Draw", "First.Draw", "Second.Alpha", "aType", "First.alpha");

        var tags = SampleManager.GetAllTags([sample]).Select(tag => tag.Tag);

        Assert.Equal(["First.alpha", "Second.Alpha", "aType", "First.Draw", "Second.Draw", "ZType"], tags);
    }

    [Fact]
    public void CategoryCountsUseOnlyTheCurrentMatches()
    {
        var samples = new SampleService().GetAllSamples().ToArray();
        var matches = SampleManager.SearchSamples(samples, "Lottie Player").ToArray();

        var counts = SampleManager.GetCategoryCounts(matches);

        Assert.Single(matches);
        Assert.Single(counts);
        Assert.Equal(1, counts[SampleManager.General]);
        Assert.False(counts.ContainsKey(SampleManager.Documents));
        Assert.Empty(SampleManager.GetCategoryCounts([]));
    }

    [Fact]
    public void CategoryAndTagCountsUpdateAtomicallyWithTheSearch()
    {
        var filters = new GalleryFilters(new SampleService().GetAllSamples().ToArray());
        filters.Changed += (_, _) =>
        {
            Assert.Equal(filters.Results.Length, filters.LiveCategoryCounts.Values.Sum());
            Assert.All(filters.LiveCategoryCounts, count => Assert.True(count.Value > 0));
            Assert.Equal(SampleManager.GetCategoryCounts(filters.Results).OrderBy(pair => pair.Key),
                filters.LiveCategoryCounts.OrderBy(pair => pair.Key));
            Assert.Equal(SampleManager.GetTagCounts(filters.Results).OrderBy(pair => pair.Key),
                filters.LiveTagCounts.OrderBy(pair => pair.Key));
        };

        filters.SetSearch("Lottie Player");
        Assert.Single(filters.LiveCategoryCounts);
        Assert.Equal(1, filters.LiveCategoryCounts[SampleManager.General]);
        filters.ToggleTag("Animation");
        filters.SetSearch("__no_match__");
        Assert.Empty(filters.LiveCategoryCounts);
        Assert.Empty(filters.LiveTagCounts);
        Assert.Contains("Animation", filters.Tags);
        filters.SetSearch("");
        Assert.Single(filters.Results);
        filters.Clear();
        Assert.Equal(filters.AllSamples.Length, filters.LiveCategoryCounts.Values.Sum());
    }

    [Fact]
    public void RepeatedSearchDoesNotReenterFilterNotifications()
    {
        var filters = new GalleryFilters(new SampleService().GetAllSamples().ToArray());
        var changes = 0;
        filters.Changed += (_, _) =>
        {
            changes++;
            filters.SetSearch(filters.SearchText);
        };

        filters.SetSearch("Gradient");

        Assert.Equal(1, changes);
        Assert.NotEmpty(filters.Results);
        Assert.All(filters.Results, sample => Assert.True(sample.MatchesFilter("Gradient")));
    }

    [Fact]
    public void ClearingFiltersRestoresTheEntireCatalogAndDefaults()
    {
        var samples = new SampleService().GetAllSamples().ToArray();
        var filters = new GalleryFilters(samples);
        filters.SetSearch("shader");
        filters.SelectCategory(SampleManager.Shaders);
        filters.ToggleTag("SKCanvas");
        filters.SetSort(SampleSortOrder.Alphabetical);
        Assert.Equal(3, filters.ActiveCount);

        filters.Clear();

        Assert.Equal("", filters.SearchText);
        Assert.Null(filters.SelectedCategory);
        Assert.Empty(filters.Tags);
        Assert.Equal(SampleSortOrder.NewestFirst, filters.SortOrder);
        Assert.Equal(0, filters.ActiveCount);
        Assert.Equal(samples.Length, filters.Results.Length);
    }

    [Fact]
    public void GalleryTagSelectionMatchesBlazorAllTagSemantics()
    {
        var samples = new SampleService().GetAllSamples().ToArray();
        var filters = new GalleryFilters(samples);
        filters.ToggleTag("SKRuntimeEffect");
        filters.ToggleTag("SKDocument");

        Assert.Contains(samples, sample => sample.ApiTags.Contains("SKRuntimeEffect"));
        Assert.Contains(samples, sample => sample.ApiTags.Contains("SKDocument"));
        Assert.Equal(
            samples.Where(sample => filters.Tags.All(sample.ApiTags.Contains)).OrderBy(sample => sample.Title),
            filters.Results.OrderBy(sample => sample.Title));
    }

    [Fact]
    public void FilterResultsAndLiveCountsAreReplacedTogether()
    {
        var filters = new GalleryFilters(new SampleService().GetAllSamples().ToArray());
        var original = filters.Results;
        filters.SelectCategory(SampleManager.Text);

        Assert.NotSame(original, filters.Results);
        Assert.All(filters.Results, sample => Assert.Equal(SampleManager.Text, sample.Category));
        var expected = SampleManager.GetTagCounts(filters.Results);
        Assert.Equal(expected.OrderBy(pair => pair.Key), filters.LiveTagCounts.OrderBy(pair => pair.Key));
        filters.SelectCategory(null);
        Assert.Null(filters.SelectedCategory);
        Assert.Equal(filters.AllSamples.Length, filters.Results.Length);
    }

    [Fact]
    public void GalleryUsesOneCategoryLikeBlazor()
    {
        var filters = new GalleryFilters(new SampleService().GetAllSamples().ToArray());
        filters.SelectCategory(SampleManager.Text);
        filters.SelectCategory(SampleManager.Shaders);

        Assert.Equal(SampleManager.Shaders, filters.SelectedCategory);
        Assert.Equal(1, filters.ActiveCount);
        Assert.NotEmpty(filters.Results);
        Assert.All(filters.Results, sample => Assert.Equal(SampleManager.Shaders, sample.Category));
        var current = filters.Results;
        filters.SelectCategory(SampleManager.Shaders);
        Assert.Same(current, filters.Results);
    }

    [Fact]
    public void SharedControlsHaveValidValuesAndUniqueQualifiedIds()
    {
        foreach (var sample in new SampleService().GetAllSamples())
        {
            var ids = new HashSet<string>();
            ValidateControls(sample.Controls, "", ids);
        }
    }

    [Fact]
    public async Task CancelledAnimationWorkerCannotJoinAReopenedSample()
    {
        var sample = new PausedAnimationSample();
        var cancellationToken = TestContext.Current.CancellationToken;
        try
        {
            await sample.InitAsync();
            await sample.FirstUpdate.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
            sample.Destroy();

            await sample.InitAsync();
            await sample.SecondUpdate.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
            sample.ReleaseFirstUpdate.TrySetResult();

            await Task.WhenAny(sample.StaleWorker.Task, Task.Delay(250, cancellationToken));
            Assert.False(sample.StaleWorker.Task.IsCompleted,
                "The cancelled worker resumed using the reopened sample's cancellation token.");
        }
        finally
        {
            sample.ReleaseFirstUpdate.TrySetResult();
            sample.Destroy();
        }
    }

    [Fact]
    public void EachGalleryVisitHasIndependentSampleState()
    {
        var service = new SampleService();
        var first = Assert.IsAssignableFrom<SampleBase>(service.CreateSample("Gradient"));
        var second = Assert.IsAssignableFrom<SampleBase>(service.CreateSample("Gradient"));
        var original = second.Controls.OfType<SliderControl>().Single(control => control.Id == "angle").Value;

        Assert.NotSame(first, second);
        Assert.NotSame(first, service.GetSample("Gradient"));
        first.UpdateControl("angle", original + 30);
        Assert.Equal(original, second.Controls.OfType<SliderControl>().Single(control => control.Id == "angle").Value);
        Assert.Null(service.CreateSample("unknown sample"));
    }

    [Fact]
    public async Task CancelledAnimationErrorsCannotStopAReopenedSample()
    {
        var sample = new PausedAnimationSample(failFirstUpdate: true);
        var failure = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
        sample.AnimationFailed += (_, error) => failure.TrySetResult(error);
        var cancellationToken = TestContext.Current.CancellationToken;
        try
        {
            await sample.InitAsync();
            await sample.FirstUpdate.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
            sample.Destroy();
            await sample.InitAsync();
            await sample.SecondUpdate.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
            sample.ReleaseFirstUpdate.TrySetResult();

            await Task.WhenAny(failure.Task, Task.Delay(250, cancellationToken));
            Assert.False(failure.Task.IsCompleted, "The old worker reported its error to a new activation.");
        }
        finally
        {
            sample.ReleaseFirstUpdate.TrySetResult();
            sample.Destroy();
        }
    }

    [Fact]
    public async Task FailedInitializationReleasesPartialResources()
    {
        var sample = new FailingSample(failDuringInitialization: true);
        var error = await Assert.ThrowsAsync<InvalidOperationException>(sample.InitAsync);

        Assert.Same(sample.Error, error);
        Assert.True(sample.Destroyed);
        Assert.False(sample.IsInitialized);
    }

    [Fact]
    public async Task AnimationErrorsAreReportedToTheHost()
    {
        var sample = new FailingSample(failDuringInitialization: false);
        var failure = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
        sample.AnimationFailed += (_, error) => failure.TrySetResult(error);
        try
        {
            await sample.InitAsync();
            var error = await failure.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            Assert.Same(sample.Error, error);
        }
        finally
        {
            sample.Destroy();
        }
    }

    [Theory]
    [MemberData(nameof(SupportedSamples))]
    public async Task EverySupportedSampleInitializesRendersAndCanBeReopened(string title)
    {
        var sample = Assert.IsAssignableFrom<SampleBase>(new SampleService().GetSample(title));

        for (var visit = 0; visit < 2; visit++)
        {
            try
            {
                await sample.InitAsync();
                Assert.True(sample.IsInitialized);
                ValidateControls(sample.Controls, "", new HashSet<string>());
                using var surface = SKSurface.Create(new SKImageInfo(640, 480));
                Assert.NotNull(surface);

                switch (sample)
                {
                    case CanvasSampleBase canvas:
                        canvas.DrawSample(surface.Canvas, 640, 480);
                        break;
                    case DocumentSampleBase document:
                        document.DrawSample(surface.Canvas, 640, 480);
                        Assert.True(document.HasDownload);
                        Assert.NotEmpty(document.DocumentBytes!);
                        if (document.DocumentMimeType == "application/pdf")
                            Assert.Equal("%PDF-", Encoding.ASCII.GetString(document.DocumentBytes!, 0, 5));
                        break;
                    default:
                        Assert.Fail($"No gallery renderer for {sample.GetType().Name}.");
                        break;
                }

                using var image = surface.Snapshot();
                using var encoded = image.Encode(SKEncodedImageFormat.Png, 100);
                Assert.NotNull(encoded);
                Assert.True(encoded.Size > 0);
            }
            finally
            {
                sample.Destroy();
            }

            Assert.False(sample.IsInitialized);
        }
    }

    private static void ValidateControls(IReadOnlyList<SampleControl> controls, string prefix, HashSet<string> ids)
    {
        foreach (var control in controls)
        {
            var id = string.IsNullOrEmpty(prefix) ? control.Id : $"{prefix}.{control.Id}";
            Assert.True(ids.Add(id), $"Duplicate control ID: {id}");
            Assert.False(string.IsNullOrWhiteSpace(control.Label));

            switch (control)
            {
                case SliderControl slider:
                    Assert.True(float.IsFinite(slider.Min) && float.IsFinite(slider.Max));
                    Assert.True(slider.Min <= slider.Max, $"Invalid slider range: {id}");
                    Assert.InRange(slider.Value, slider.Min, slider.Max);
                    Assert.True(float.IsFinite(slider.Step) && slider.Step >= 0);
                    break;
                case PickerControl picker:
                    Assert.NotEmpty(picker.Options);
                    Assert.InRange(picker.SelectedIndex, 0, picker.Options.Length - 1);
                    break;
                case GroupControl group:
                    ValidateControls(group.Children, id, ids);
                    break;
                case ToggleControl:
                    break;
                default:
                    Assert.Fail($"Unsupported gallery control: {control.GetType().Name}");
                    break;
            }
        }

    }

    private sealed class MetadataSample(string title, string category, params string[] tags) : SampleBase
    {
        public override string Title => title;
        public override string Category => category;
        public override IReadOnlyList<string> ApiTags => tags;
    }

    private sealed class FailingSample(bool failDuringInitialization) : CanvasSampleBase
    {
        public override string Title => "Failing sample";
        public override IReadOnlyList<string> ApiTags => [];
        public override bool IsAnimated => !failDuringInitialization;
        public InvalidOperationException Error { get; } = new("Expected sample failure");
        public bool Destroyed { get; private set; }

        protected override Task OnInit() =>
            failDuringInitialization ? Task.FromException(Error) : base.OnInit();

        protected override Task OnUpdate(CancellationToken token) => Task.FromException(Error);

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroyed = true;
        }

        protected override void OnDrawSample(SKCanvas canvas, int width, int height)
        {
        }
    }

    private sealed class PausedAnimationSample(bool failFirstUpdate = false) : CanvasSampleBase
    {
        private int updateCount;

        public override string Title => "Paused animation";
        public override IReadOnlyList<string> ApiTags => [];
        public override bool IsAnimated => true;
        public TaskCompletionSource FirstUpdate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource SecondUpdate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ReleaseFirstUpdate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource StaleWorker { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override async Task OnUpdate(CancellationToken token)
        {
            switch (Interlocked.Increment(ref updateCount))
            {
                case 1:
                    FirstUpdate.TrySetResult();
                    await ReleaseFirstUpdate.Task;
                    if (failFirstUpdate)
                        throw new InvalidOperationException("The cancelled worker failed.");
                    break;
                case 2:
                    SecondUpdate.TrySetResult();
                    await Task.Delay(Timeout.Infinite, token);
                    break;
                default:
                    StaleWorker.TrySetResult();
                    await Task.Delay(Timeout.Infinite, token);
                    break;
            }
        }

        protected override void OnDrawSample(SKCanvas canvas, int width, int height)
        {
        }
    }
}
