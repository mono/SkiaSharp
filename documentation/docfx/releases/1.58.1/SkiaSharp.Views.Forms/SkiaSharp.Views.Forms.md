# API diff: SkiaSharp.Views.Forms.dll

## SkiaSharp.Views.Forms.dll

### Namespace SkiaSharp.Views.Forms

#### Type Changed: SkiaSharp.Views.Forms.SKCanvasView

Added field:

```csharp
public static Xamarin.Forms.BindableProperty EnableTouchEventsProperty;
```

Added property:

```csharp
public bool EnableTouchEvents { get; set; }
```

Added event:

```csharp
public event System.EventHandler<SKTouchEventArgs> Touch;
```

Added method:

```csharp
protected virtual void OnTouch (SKTouchEventArgs e);
```


#### Type Changed: SkiaSharp.Views.Forms.SKGLView

Added field:

```csharp
public static Xamarin.Forms.BindableProperty EnableTouchEventsProperty;
```

Added property:

```csharp
public bool EnableTouchEvents { get; set; }
```

Added event:

```csharp
public event System.EventHandler<SKTouchEventArgs> Touch;
```

Added method:

```csharp
protected virtual void OnTouch (SKTouchEventArgs e);
```


#### New Type: SkiaSharp.Views.Forms.SKMouseButton

```csharp
[Serializable]
public enum SKMouseButton {
	Left = 1,
	Middle = 2,
	Right = 3,
	Unknown = 0,
}
```

#### New Type: SkiaSharp.Views.Forms.SKTouchAction

```csharp
[Serializable]
public enum SKTouchAction {
	Cancelled = 4,
	Entered = 0,
	Exited = 5,
	Moved = 2,
	Pressed = 1,
	Released = 3,
}
```

#### New Type: SkiaSharp.Views.Forms.SKTouchDeviceType

```csharp
[Serializable]
public enum SKTouchDeviceType {
	Mouse = 1,
	Pen = 2,
	Touch = 0,
}
```

#### New Type: SkiaSharp.Views.Forms.SKTouchEventArgs

```csharp
public class SKTouchEventArgs : System.EventArgs {
	// constructors
	public SKTouchEventArgs (long id, SKTouchAction type, SkiaSharp.SKPoint location, bool inContact);
	public SKTouchEventArgs (long id, SKTouchAction type, SKMouseButton mouseButton, SKTouchDeviceType deviceType, SkiaSharp.SKPoint location, bool inContact);
	// properties
	public SKTouchAction ActionType { get; }
	public SKTouchDeviceType DeviceType { get; }
	public bool Handled { get; set; }
	public long Id { get; }
	public bool InContact { get; }
	public SkiaSharp.SKPoint Location { get; }
	public SKMouseButton MouseButton { get; }
	// methods
	public override string ToString ();
}
```


