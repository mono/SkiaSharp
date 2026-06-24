# API diff: SkiaSharp.Views.Forms.dll

## SkiaSharp.Views.Forms.dll

### Namespace SkiaSharp.Views.Forms

#### New Type: SkiaSharp.Views.Forms.SKBitmapImageSource

```csharp
public sealed class SKBitmapImageSource : Xamarin.Forms.ImageSource, System.ComponentModel.INotifyPropertyChanged, Xamarin.Forms.IElementController, Xamarin.Forms.Internals.IDynamicResourceHandler, Xamarin.Forms.Internals.INameScope {
	// constructors
	public SKBitmapImageSource ();
	// fields
	public static Xamarin.Forms.BindableProperty BitmapProperty;
	// properties
	public SkiaSharp.SKBitmap Bitmap { get; set; }
	// methods
	public override System.Threading.Tasks.Task<bool> Cancel ();
	protected override void OnPropertyChanged (string propertyName);
	public static SKBitmapImageSource op_Implicit (SkiaSharp.SKBitmap bitmap);
	public static SkiaSharp.SKBitmap op_Implicit (SKBitmapImageSource source);
}
```

#### New Type: SkiaSharp.Views.Forms.SKImageImageSource

```csharp
public sealed class SKImageImageSource : Xamarin.Forms.ImageSource, System.ComponentModel.INotifyPropertyChanged, Xamarin.Forms.IElementController, Xamarin.Forms.Internals.IDynamicResourceHandler, Xamarin.Forms.Internals.INameScope {
	// constructors
	public SKImageImageSource ();
	// fields
	public static Xamarin.Forms.BindableProperty ImageProperty;
	// properties
	public SkiaSharp.SKImage Image { get; set; }
	// methods
	public override System.Threading.Tasks.Task<bool> Cancel ();
	protected override void OnPropertyChanged (string propertyName);
	public static SKImageImageSource op_Implicit (SkiaSharp.SKImage image);
	public static SkiaSharp.SKImage op_Implicit (SKImageImageSource source);
}
```

#### New Type: SkiaSharp.Views.Forms.SKPictureImageSource

```csharp
public sealed class SKPictureImageSource : Xamarin.Forms.ImageSource, System.ComponentModel.INotifyPropertyChanged, Xamarin.Forms.IElementController, Xamarin.Forms.Internals.IDynamicResourceHandler, Xamarin.Forms.Internals.INameScope {
	// constructors
	public SKPictureImageSource ();
	// fields
	public static Xamarin.Forms.BindableProperty DimensionsProperty;
	public static Xamarin.Forms.BindableProperty PictureProperty;
	// properties
	public SkiaSharp.SKSizeI Dimensions { get; set; }
	public SkiaSharp.SKPicture Picture { get; set; }
	// methods
	public override System.Threading.Tasks.Task<bool> Cancel ();
	protected override void OnPropertyChanged (string propertyName);
	public static SkiaSharp.SKPicture op_Explicit (SKPictureImageSource source);
}
```

#### New Type: SkiaSharp.Views.Forms.SKPixmapImageSource

```csharp
public sealed class SKPixmapImageSource : Xamarin.Forms.ImageSource, System.ComponentModel.INotifyPropertyChanged, Xamarin.Forms.IElementController, Xamarin.Forms.Internals.IDynamicResourceHandler, Xamarin.Forms.Internals.INameScope {
	// constructors
	public SKPixmapImageSource ();
	// fields
	public static Xamarin.Forms.BindableProperty PixmapProperty;
	// properties
	public SkiaSharp.SKPixmap Pixmap { get; set; }
	// methods
	public override System.Threading.Tasks.Task<bool> Cancel ();
	protected override void OnPropertyChanged (string propertyName);
	public static SKPixmapImageSource op_Implicit (SkiaSharp.SKPixmap pixmap);
	public static SkiaSharp.SKPixmap op_Implicit (SKPixmapImageSource source);
}
```


