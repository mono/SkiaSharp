# API diff: HarfBuzzSharp.dll

## HarfBuzzSharp.dll

### Namespace HarfBuzzSharp

### New Namespace HarfBuzzSharp.Internals

#### New Type: HarfBuzzSharp.Internals.PlatformConfiguration

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

