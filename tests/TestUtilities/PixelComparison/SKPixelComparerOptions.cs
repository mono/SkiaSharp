// Based on SkiaSharp.Extended's SKPixelComparerOptions at
// https://github.com/mono/SkiaSharp.Extended/tree/579c974196199962dc1cb7c22bc68fe32e9a5b64
// Copyright (c) 2015-2016 Xamarin, Inc.
// Copyright (c) 2017-2020 Microsoft Corporation. Licensed under the MIT license.
#nullable enable

namespace SkiaSharp.Testing;

/// <summary>Comparison settings, snapshotted at the start of each operation.</summary>
internal sealed class SKPixelComparerOptions
{
	/// <summary>Apply tolerance independently per selected channel, rather than to their sum.</summary>
	public bool TolerancePerChannel { get; set; } = true;

	/// <summary>Include alpha in comparisons and rejection decisions (but not raw RGB delta images).</summary>
	public bool CompareAlpha { get; set; }

	/// <summary>Normalize source pixels as unpremultiplied or premultiplied BGRA8888.</summary>
	public SKAlphaType AlphaType { get; set; } = SKAlphaType.Unpremul;
}
