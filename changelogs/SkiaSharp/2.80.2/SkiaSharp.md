# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRContext

Added methods:

```csharp
public void DumpMemoryStatistics (SKTraceMemoryDump dump);
public void PurgeResources ();
public void PurgeUnlockedResources (bool scratchResourcesOnly);
public void PurgeUnlockedResources (long bytesToPurge, bool preferScratchResources);
public void PurgeUnusedResources (long milliseconds);
```


#### Type Changed: SkiaSharp.SKMatrix

Added method:

```csharp
public static SKMatrix CreateScaleTranslation (float sx, float sy, float tx, float ty);
```


#### New Type: SkiaSharp.SKGraphics

```csharp
public static class SKGraphics {
	// methods
	public static void DumpMemoryStatistics (SKTraceMemoryDump dump);
	public static int GetFontCacheCountLimit ();
	public static int GetFontCacheCountUsed ();
	public static long GetFontCacheLimit ();
	public static int GetFontCachePointSizeLimit ();
	public static long GetFontCacheUsed ();
	public static long GetResourceCacheSingleAllocationByteLimit ();
	public static long GetResourceCacheTotalByteLimit ();
	public static long GetResourceCacheTotalBytesUsed ();
	public static void Init ();
	public static void PurgeAllCaches ();
	public static void PurgeFontCache ();
	public static void PurgeResourceCache ();
	public static int SetFontCacheCountLimit (int count);
	public static long SetFontCacheLimit (long bytes);
	public static int SetFontCachePointSizeLimit (int count);
	public static long SetResourceCacheSingleAllocationByteLimit (long bytes);
	public static long SetResourceCacheTotalByteLimit (long bytes);
}
```

#### New Type: SkiaSharp.SKTraceMemoryDump

```csharp
public class SKTraceMemoryDump : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	protected SKTraceMemoryDump (bool detailedDump, bool dumpWrappedObjects);
	// methods
	protected override void DisposeNative ();
	protected virtual void OnDumpNumericValue (string dumpName, string valueName, string units, ulong value);
	protected virtual void OnDumpStringValue (string dumpName, string valueName, string value);
}
```


