using Markdig;
using Microsoft.AspNetCore.Components;

namespace SkiaSharp.Triage.Dashboard.Services;

public static class MarkdownHelper
{
    private static readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    /// <summary>
    /// Renders markdown to HTML wrapped in a MarkupString for Blazor rendering.
    /// </summary>
    public static MarkupString ToHtml(string? markdown)
    {
        if (string.IsNullOrEmpty(markdown))
            return new MarkupString(string.Empty);

        var html = Markdown.ToHtml(markdown, _pipeline);
        return new MarkupString(html);
    }
}
