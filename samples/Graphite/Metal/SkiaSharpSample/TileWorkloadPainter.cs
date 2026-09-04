using SkiaSharp;

namespace SkiaSharpSample;

internal sealed class TileWorkloadPainter : IDisposable
{
	private static readonly SKColor[] Palette =
	{
		new(0x57, 0x7B, 0xF9),
		new(0x9B, 0x5D, 0xF5),
		new(0x21, 0xC7, 0xA8),
		new(0xFF, 0x9F, 0x43),
		new(0xFF, 0x5E, 0x78),
		new(0x53, 0xD8, 0xFB),
	};

	private readonly int tileIndex;
	private readonly SKPaint fill = new()
	{
		IsAntialias = true,
		Style = SKPaintStyle.Fill,
	};
	private readonly SKPaint stroke = new()
	{
		IsAntialias = true,
		Style = SKPaintStyle.Stroke,
		StrokeCap = SKStrokeCap.Round,
	};
	private readonly SKFont font = new()
	{
		Hinting = SKFontHinting.Normal,
		Edging = SKFontEdging.Antialias,
	};
	private readonly SKImage atlas;
	private readonly string[] labels;
	private SKRect[] spriteRects = Array.Empty<SKRect>();
	private SKRotationScaleMatrix[] spriteTransforms =
		Array.Empty<SKRotationScaleMatrix>();

	public TileWorkloadPainter(int tileIndex)
	{
		this.tileIndex = tileIndex;
		atlas = CreateAtlas();
		labels = Enumerable.Range(0, 256)
			.Select(index => $"Graphite {index:000}")
			.ToArray();
	}

	public void Draw(
		SKCanvas canvas,
		SKImageInfo info,
		RenderSettings settings,
		double elapsedSeconds)
	{
		var time = settings.Animate ? elapsedSeconds : 0;
		canvas.Clear(new SKColor(
			(byte)(10 + tileIndex * 11 % 24),
			(byte)(14 + tileIndex * 7 % 24),
			(byte)(28 + tileIndex * 13 % 32)));

		switch (settings.Workload)
		{
			case WorkloadKind.UiDashboard:
				DrawUiDashboard(canvas, info, settings.Complexity, time);
				break;
			case WorkloadKind.VectorTiles:
				DrawVectors(canvas, info, settings.Complexity, time);
				break;
			case WorkloadKind.SpriteAtlas:
				DrawSprites(canvas, info, settings.Complexity, time);
				break;
			case WorkloadKind.TextGrid:
				DrawText(canvas, info, settings.Complexity, time);
				break;
		}

		DrawTileBadge(canvas);
	}

	public void Dispose()
	{
		atlas.Dispose();
		font.Dispose();
		stroke.Dispose();
		fill.Dispose();
	}

	private void DrawVectors(
		SKCanvas canvas,
		SKImageInfo info,
		int count,
		double time)
	{
		var width = info.Width;
		var height = info.Height;
		var phase = (float)time * 18f + tileIndex * 31f;

		for (var i = 0; i < count; i++)
		{
			var hash = Mix((uint)(i + tileIndex * 104_729));
			var x = Unit(hash) * Math.Max(1, width - 52);
			var y = Unit(Mix(hash + 1)) * Math.Max(1, height - 52);
			var w = 10f + Unit(Mix(hash + 2)) * 48f;
			var h = 10f + Unit(Mix(hash + 3)) * 48f;
			var drift = MathF.Sin((i % 97) * 0.17f + phase * 0.02f) * 4f;
			var bounds = new SKRect(
				x + drift,
				y - drift,
				Math.Min(width, x + w + drift),
				Math.Min(height, y + h - drift));
			var color = Palette[hash % Palette.Length].WithAlpha(
				(byte)(90 + hash % 150));

			switch (i & 3)
			{
				case 0:
					fill.Color = color;
					var radius = 2f + Unit(Mix(hash + 4)) * 12f;
					canvas.DrawRoundRect(bounds, radius, radius, fill);
					break;
				case 1:
					fill.Color = color;
					canvas.DrawOval(bounds, fill);
					break;
				case 2:
					stroke.Color = color;
					stroke.StrokeWidth = 1f + Unit(Mix(hash + 5)) * 5f;
					canvas.DrawLine(
						bounds.Left,
						bounds.Top,
						bounds.Right,
						bounds.Bottom,
						stroke);
					break;
				default:
					stroke.Color = color;
					stroke.StrokeWidth = 1f + Unit(Mix(hash + 6)) * 4f;
					canvas.DrawArc(
						bounds,
						hash % 180,
						40 + hash % 280,
						false,
						stroke);
					break;
			}
		}
	}

	private void DrawUiDashboard(
		SKCanvas canvas,
		SKImageInfo info,
		int count,
		double time)
	{
		var width = info.Width;
		var height = info.Height;
		var padding = Math.Max(8f, width * 0.025f);
		var headerHeight = Math.Clamp(height * 0.12f, 34f, 58f);
		var contentTop = headerHeight + padding;
		var contentHeight = Math.Max(1, height - contentTop - padding);
		var cardGap = Math.Max(5f, padding * 0.6f);
		var cardWidth = Math.Max(40f, (width - padding * 2 - cardGap) / 2);
		var cardHeight = Math.Max(30f, (contentHeight - cardGap) / 2);

		fill.Color = new SKColor(0x11, 0x17, 0x2A);
		canvas.DrawRect(0, 0, width, headerHeight, fill);
		font.Size = Math.Clamp(width * 0.045f, 13f, 22f);
		fill.Color = SKColors.White;
		canvas.DrawText(
			$"Workspace {tileIndex + 1}",
			padding,
			headerHeight * 0.67f,
			SKTextAlign.Left,
			font,
			fill);
		font.Size = Math.Clamp(width * 0.025f, 9f, 13f);
		fill.Color = new SKColor(0x8E, 0xA2, 0xC9);
		canvas.DrawText(
			"live visual root",
			width - padding,
			headerHeight * 0.65f,
			SKTextAlign.Right,
			font,
			fill);

		var cards = new[]
		{
			new SKRect(
				padding,
				contentTop,
				padding + cardWidth,
				contentTop + cardHeight),
			new SKRect(
				padding + cardWidth + cardGap,
				contentTop,
				width - padding,
				contentTop + cardHeight),
			new SKRect(
				padding,
				contentTop + cardHeight + cardGap,
				padding + cardWidth,
				height - padding),
			new SKRect(
				padding + cardWidth + cardGap,
				contentTop + cardHeight + cardGap,
				width - padding,
				height - padding),
		};

		for (var i = 0; i < cards.Length; i++)
		{
			fill.Color = new SKColor(
				(byte)(25 + i * 5),
				(byte)(34 + i * 6),
				(byte)(57 + i * 8));
			canvas.DrawRoundRect(cards[i], 12, 12, fill);
		}

		font.Size = Math.Clamp(width * 0.025f, 9f, 13f);
		fill.Color = new SKColor(0xA9, 0xB8, 0xD8);
		canvas.DrawText("CPU LOAD", cards[0].Left + 10, cards[0].Top + 20, SKTextAlign.Left, font, fill);
		canvas.DrawText("EVENTS", cards[1].Left + 10, cards[1].Top + 20, SKTextAlign.Left, font, fill);
		canvas.DrawText("QUEUE", cards[2].Left + 10, cards[2].Top + 20, SKTextAlign.Left, font, fill);
		canvas.DrawText("LATENCY", cards[3].Left + 10, cards[3].Top + 20, SKTextAlign.Left, font, fill);

		var phase = (float)time * 1.8f + tileIndex * 0.7f;
		var samples = Math.Max(1, Math.Min(count, 8_000));
		for (var i = 0; i < samples; i++)
		{
			var hash = Mix((uint)(i + tileIndex * 32_771));
			var card = cards[i & 3];
			var localX = Unit(hash);
			var localY = Unit(Mix(hash + 1));
			var x = card.Left + 8 + localX * Math.Max(1, card.Width - 16);
			var y = card.Top + 28 + localY * Math.Max(1, card.Height - 36);
			var pulse = 0.5f + 0.5f * MathF.Sin(phase + i * 0.013f);
			var color = Palette[(i + tileIndex) % Palette.Length]
				.WithAlpha((byte)(70 + pulse * 130));

			switch (i & 3)
			{
				case 0:
					fill.Color = color;
					canvas.DrawRoundRect(
						new SKRect(x, y, x + 3 + hash % 13, y + 3),
						1.5f,
						1.5f,
						fill);
					break;
				case 1:
					fill.Color = color;
					canvas.DrawCircle(x, y, 1.5f + hash % 4, fill);
					break;
				case 2:
					stroke.Color = color;
					stroke.StrokeWidth = 1f;
					canvas.DrawLine(
						x,
						y,
						Math.Min(card.Right - 6, x + 5 + hash % 18),
						Math.Max(card.Top + 28, y - 3 - hash % 8),
						stroke);
					break;
				default:
					fill.Color = color;
					canvas.DrawRect(
						x,
						y,
						2 + hash % 6,
						2 + Mix(hash + 2) % 9,
						fill);
					break;
			}
		}

		var progress = 0.55f + MathF.Sin(phase) * 0.25f;
		fill.Color = new SKColor(0x32, 0x45, 0x68);
		canvas.DrawRoundRect(
			new SKRect(
				cards[3].Left + 10,
				cards[3].Bottom - 18,
				cards[3].Right - 10,
				cards[3].Bottom - 10),
			4,
			4,
			fill);
		fill.Color = Palette[(tileIndex + 1) % Palette.Length];
		canvas.DrawRoundRect(
			new SKRect(
				cards[3].Left + 10,
				cards[3].Bottom - 18,
				cards[3].Left + 10 + (cards[3].Width - 20) * progress,
				cards[3].Bottom - 10),
			4,
			4,
			fill);
	}

	private void DrawSprites(
		SKCanvas canvas,
		SKImageInfo info,
		int count,
		double time)
	{
		EnsureSpriteArrays(count);
		var width = info.Width;
		var height = info.Height;

		for (var i = 0; i < count; i++)
		{
			var hash = Mix((uint)(i + tileIndex * 65_537));
			var sprite = (int)(hash % 16);
			var cellX = sprite % 4;
			var cellY = sprite / 4;
			spriteRects[i] = new SKRect(
				cellX * 32,
				cellY * 32,
				cellX * 32 + 32,
				cellY * 32 + 32);

			var angle = (float)(time * (18 + i % 7) + hash % 360);
			var x = Unit(Mix(hash + 1)) * width;
			var y = Unit(Mix(hash + 2)) * height;
			var scale = 0.35f + Unit(Mix(hash + 3)) * 0.9f;
			spriteTransforms[i] = SKRotationScaleMatrix.CreateDegrees(
				scale,
				angle,
				x,
				y,
				16,
				16);
		}

		canvas.DrawAtlas(
			atlas,
			spriteRects,
			spriteTransforms,
			SKSamplingOptions.Default,
			fill);
	}

	private void DrawText(
		SKCanvas canvas,
		SKImageInfo info,
		int count,
		double time)
	{
		var fontSize = Math.Clamp(info.Width / 24f, 10f, 18f);
		font.Size = fontSize;
		var lineHeight = fontSize * 1.25f;
		var columns = Math.Max(1, info.Width / 112);
		var rows = Math.Max(1, (int)(info.Height / lineHeight));
		var phase = (float)time * 11f;

		for (var i = 0; i < count; i++)
		{
			var cell = i % (columns * rows);
			var column = cell % columns;
			var row = cell / columns;
			var pass = i / Math.Max(1, columns * rows);
			fill.Color = Palette[(i + tileIndex) % Palette.Length]
				.WithAlpha((byte)Math.Max(36, 220 - pass * 18));
			var x = 6 + column * 112 + MathF.Sin(phase + row * 0.3f) * 2f;
			var y = 20 + row * lineHeight;
			canvas.DrawText(
				labels[(i + tileIndex * 17) % labels.Length],
				x,
				y,
				SKTextAlign.Left,
				font,
				fill);
		}
	}

	private void DrawTileBadge(SKCanvas canvas)
	{
		fill.Color = new SKColor(0, 0, 0, 170);
		canvas.DrawRoundRect(new SKRect(8, 8, 48, 34), 10, 10, fill);
		font.Size = 14;
		fill.Color = SKColors.White;
		canvas.DrawText($"{tileIndex + 1}", 28, 27, SKTextAlign.Center, font, fill);
	}

	private void EnsureSpriteArrays(int count)
	{
		if (spriteRects.Length == count)
			return;

		spriteRects = new SKRect[count];
		spriteTransforms = new SKRotationScaleMatrix[count];
	}

	private static SKImage CreateAtlas()
	{
		using var surface = SKSurface.Create(new SKImageInfo(128, 128))
			?? throw new InvalidOperationException("Unable to create the sprite atlas.");
		var canvas = surface.Canvas;
		canvas.Clear(SKColors.Transparent);
		using var paint = new SKPaint { IsAntialias = true };

		for (var i = 0; i < 16; i++)
		{
			var x = i % 4 * 32;
			var y = i / 4 * 32;
			paint.Color = Palette[i % Palette.Length];
			canvas.DrawRoundRect(new SKRect(x + 2, y + 2, x + 30, y + 30), 7, 7, paint);
			paint.Color = SKColors.White.WithAlpha(180);
			canvas.DrawCircle(x + 16, y + 16, 4 + i % 6, paint);
		}

		return surface.Snapshot()
			?? throw new InvalidOperationException("Unable to snapshot the sprite atlas.");
	}

	private static uint Mix(uint value)
	{
		value ^= value >> 16;
		value *= 0x7FEB352D;
		value ^= value >> 15;
		value *= 0x846CA68B;
		return value ^ (value >> 16);
	}

	private static float Unit(uint value) =>
		(value & 0x00FF_FFFF) / 16_777_215f;
}
