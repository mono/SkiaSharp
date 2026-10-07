using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;

namespace SkiaSharpSample;

public partial class ImageSourcesPage : ContentPage
{
	private SKBitmap? bitmap;
	private SKImage? image;
	private SKBitmap? pixmapBacking;
	private SKPixmap? pixmap;
	private SKPicture? picture;
	private byte[]? streamPixels;
	private bool replaced;

	public ImageSourcesPage() => InitializeComponent();

	protected override void OnAppearing()
	{
		base.OnAppearing();
		ReloadSources();
	}

	protected override void OnDisappearing()
	{
		ClearSources();
		base.OnDisappearing();
	}

	private void ReloadSources()
	{
		ClearSources();
		bitmap = CreateBitmap(96, 64, SKColors.Red);
		using (var imageBitmap = CreateBitmap(64, 96, SKColors.DodgerBlue))
			image = SKImage.FromBitmap(imageBitmap);
		pixmapBacking = CreateBitmap(96, 64, SKColors.LimeGreen);
		pixmap = pixmapBacking.PeekPixels();
		using (var recorder = new SKPictureRecorder())
		{
			var canvas = recorder.BeginRecording(new SKRect(0, 0, 96, 64));
			canvas.Clear(SKColors.Gold);
			using var paint = new SKPaint { Color = SKColors.Black };
			canvas.DrawRect(32, 16, 32, 32, paint);
			picture = recorder.EndRecording();
		}
		using (var streamBitmap = CreateBitmap(96, 64, SKColors.Cyan))
		using (var streamImage = SKImage.FromBitmap(streamBitmap))
		using (var encoded = streamImage.Encode(SKEncodedImageFormat.Png, 100))
			streamPixels = encoded.ToArray();

		var bitmapSource = new SKBitmapImageSource { Bitmap = bitmap };
		bitmapImage.Source = bitmapSource;
		imageButton.Source = bitmapSource;
		nativeImage.Source = new SKImageImageSource { Image = image };
		pixmapImage.Source = new SKPixmapImageSource { Pixmap = pixmap };
		pictureImage.Source = new SKPictureImageSource { Picture = picture, Dimensions = new SKSizeI(96, 64) };
		transitionImage.Source = bitmapSource;
		statusLabel.Text = "Loaded: red, blue, green, gold.";
	}

	private static SKBitmap CreateBitmap(int width, int height, SKColor color)
	{
		var result = new SKBitmap(width, height);
		result.Erase(color);
		return result;
	}

	private void OnReplaceClicked(object? sender, EventArgs e)
	{
		if (bitmap is null)
			ReloadSources();

		var previous = bitmap;
		bitmapImage.Source = null;
		imageButton.Source = null;
		transitionImage.Source = null;
		replaced = !replaced;
		bitmap = CreateBitmap(96, 64, replaced ? SKColors.Magenta : SKColors.Red);
		var source = new SKBitmapImageSource { Bitmap = bitmap };
		bitmapImage.Source = source;
		imageButton.Source = source;
		transitionImage.Source = source;
		previous?.Dispose();
		statusLabel.Text = replaced ? "Replaced bitmap: magenta." : "Replaced bitmap: red.";
	}

	private void OnStreamToSkiaClicked(object? sender, EventArgs e)
	{
		if (bitmap is null)
			ReloadSources();

		var bytes = streamPixels!;
		transitionImage.Source = ImageSource.FromStream(async cancellationToken =>
		{
			await Task.Delay(250, cancellationToken);
			return new MemoryStream(bytes, writable: false);
		});
		transitionImage.Source = new SKBitmapImageSource { Bitmap = bitmap };
		statusLabel.Text = "Stream to Skia: the final image must match the red/magenta bitmap, never cyan.";
	}

	private void OnClearClicked(object? sender, EventArgs e)
	{
		ClearSources();
		statusLabel.Text = "Cleared: all images and the ImageButton are empty.";
	}

	private void OnReloadClicked(object? sender, EventArgs e) => ReloadSources();

	private void ClearSources()
	{
		bitmapImage.Source = null;
		nativeImage.Source = null;
		pixmapImage.Source = null;
		pictureImage.Source = null;
		imageButton.Source = null;
		transitionImage.Source = null;
		pixmap?.Dispose();
		pixmap = null;
		pixmapBacking?.Dispose();
		pixmapBacking = null;
		image?.Dispose();
		image = null;
		picture?.Dispose();
		picture = null;
		bitmap?.Dispose();
		bitmap = null;
		streamPixels = null;
		replaced = false;
	}
}
