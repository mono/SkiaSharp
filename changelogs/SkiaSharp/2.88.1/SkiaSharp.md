# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKEncodedImageFormat

Added value:

```csharp
Avif = 12,
```


#### Removed Type SkiaSharp.Resource

### Namespace SkiaSharp.Internals

#### New Type: SkiaSharp.Internals.PlatformConfiguration

```csharp
public static class PlatformConfiguration {
	// properties
	public static bool Is64Bit { get; }
	public static bool IsArm { get; }
	public static bool IsGlibc { get; }
	public static bool IsLinux { get; }
	public static bool IsMac { get; }
	public static bool IsUnix { get; }
	public static bool IsWindows { get; }
	public static string LinuxFlavor { get; set; }
}
```


