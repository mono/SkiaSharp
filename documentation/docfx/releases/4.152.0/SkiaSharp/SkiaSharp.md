# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 4.152.0.0 vs 4.151.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRContext

Added method:

```csharp
public void CheckAsyncWorkCompletion ();
```


#### Type Changed: SkiaSharp.GRVkExtensions

Added constructor:

```csharp
public GRVkExtensions ();
```


#### Type Changed: SkiaSharp.SKImage

Added methods:

```csharp
public static SKImage FromTexture (SKGraphiteRecorder recorder, SKGraphiteBackendTexture backendTexture, SKColorType colorType, SKAlphaType alphaType);
public static SKImage FromTexture (SKGraphiteRecorder recorder, SKGraphiteBackendTexture backendTexture, SKColorType colorType, SKAlphaType alphaType, SKColorSpace colorSpace);
public static SKImage FromTexture (SKGraphiteRecorder recorder, SKGraphiteBackendTexture backendTexture, SKColorType colorType, SKAlphaType alphaType, SKColorSpace colorSpace, SKGraphiteReleaseDelegate releaseProc);
public void RequestReadPixels (SKImageInfo info, SKRectI srcRect, System.Action<SKImageReadPixelsResult> callback);
public void RequestReadPixels (SKImageInfo info, SKRectI srcRect, SKImageRescaleGamma rescaleGamma, SKImageRescaleMode rescaleMode, System.Action<SKImageReadPixelsResult> callback);
public SKImage ToTextureImage (SKGraphiteRecorder recorder);
public SKImage ToTextureImage (SKGraphiteRecorder recorder, bool mipmapped);
```


#### Type Changed: SkiaSharp.SKSurface

Added methods:

```csharp
public static SKSurface Create (SKGraphiteRecorder recorder, SKImageInfo info);
public static SKSurface Create (SKGraphiteRecorder recorder, SKGraphiteBackendTexture backendTexture, SKColorType colorType);
public static SKSurface Create (SKGraphiteRecorder recorder, SKImageInfo info, SKSurfaceProperties props);
public static SKSurface Create (SKGraphiteRecorder recorder, SKImageInfo info, bool mipmapped);
public static SKSurface Create (SKGraphiteRecorder recorder, SKGraphiteBackendTexture backendTexture, SKColorType colorType, SKColorSpace colorSpace);
public static SKSurface Create (SKGraphiteRecorder recorder, SKImageInfo info, bool mipmapped, SKSurfaceProperties props);
public static SKSurface Create (SKGraphiteRecorder recorder, SKGraphiteBackendTexture backendTexture, SKColorType colorType, SKColorSpace colorSpace, SKSurfaceProperties props);
public static SKSurface Create (SKGraphiteRecorder recorder, SKGraphiteBackendTexture backendTexture, SKColorType colorType, SKColorSpace colorSpace, SKSurfaceProperties props, SKGraphiteReleaseDelegate releaseProc);
public void RequestReadPixels (SKImageInfo info, SKRectI srcRect, System.Action<SKImageReadPixelsResult> callback);
public void RequestReadPixels (SKImageInfo info, SKRectI srcRect, SKImageRescaleGamma rescaleGamma, SKImageRescaleMode rescaleMode, System.Action<SKImageReadPixelsResult> callback);
```


#### New Type: SkiaSharp.SKGraphiteBackend

```csharp
[Serializable]
public enum SKGraphiteBackend {
	Dawn = 0,
	Metal = 1,
	Unknown = -1,
	Vulkan = 2,
}
```

#### New Type: SkiaSharp.SKGraphiteBackendTexture

```csharp
public class SKGraphiteBackendTexture : SkiaSharp.SKObject, System.IDisposable {
	// properties
	public SKGraphiteBackend Backend { get; }
	public SKSizeI Dimensions { get; }
	public bool IsValid { get; }
	// methods
	public static SKGraphiteBackendTexture CreateDawn (IntPtr wgpuTexture);
	public static SKGraphiteBackendTexture CreateMetal (int width, int height, IntPtr mtlTexture);
	public static SKGraphiteBackendTexture CreateVulkan (int width, int height, SKGraphiteVkTextureInfo info, int imageLayout, uint queueFamilyIndex, IntPtr vkImage);
	protected override void DisposeNative ();
}
```

#### New Type: SkiaSharp.SKGraphiteContext

```csharp
public class SKGraphiteContext : SkiaSharp.SKObject, System.IDisposable {
	// properties
	public SKGraphiteBackend Backend { get; }
	public long CurrentBudgetedBytes { get; }
	public bool IsDeviceLost { get; }
	public long MaxBudgetedBytes { get; set; }
	public int MaxTextureSize { get; }
	public bool SupportsProtectedContent { get; }
	// methods
	public void CheckAsyncWorkCompletion ();
	public static SKGraphiteContext CreateDawn (SKGraphiteDawnBackendContext backendContext);
	public static SKGraphiteContext CreateDawn (SKGraphiteDawnBackendContext backendContext, SKGraphiteContextOptions options);
	public static SKGraphiteContext CreateMetal (SKGraphiteMtlBackendContext backendContext);
	public static SKGraphiteContext CreateMetal (SKGraphiteMtlBackendContext backendContext, SKGraphiteContextOptions options);
	public SKGraphiteRecorder CreateRecorder (long recorderBudgetBytes);
	public SKGraphiteRecorder CreateRecorder (long recorderBudgetBytes, SKGraphiteFindOrCreateImageDelegate findOrCreate, System.Action findOrCreateDispose);
	public static SKGraphiteContext CreateVulkan (SKGraphiteVkBackendContext backendContext);
	public static SKGraphiteContext CreateVulkan (SKGraphiteVkBackendContext backendContext, SKGraphiteContextOptions options);
	public void DeleteBackendTexture (SKGraphiteBackendTexture backendTexture);
	protected override void DisposeNative ();
	public void FreeGpuResources ();
	public SKGraphiteInsertStatus InsertRecording (SKGraphiteInsertRecordingInfo info);
	public SKGraphiteInsertStatus InsertRecording (SKGraphiteRecording recording);
	public static bool IsBackendAvailable (SKGraphiteBackend backend);
	public void PerformDeferredCleanup (System.TimeSpan duration);
	public void RequestReadPixels (SKSurface surface, SKImageInfo dstInfo, SKRectI srcRect, System.Action<SKImageReadPixelsResult> callback);
	public void RequestReadPixels (SKSurface surface, SKImageInfo dstInfo, SKRectI srcRect, SKImageRescaleGamma rescaleGamma, SKImageRescaleMode rescaleMode, System.Action<SKImageReadPixelsResult> callback);
	public bool Submit ();
	public bool Submit (SKGraphiteSubmitInfo submitInfo);
}
```

#### New Type: SkiaSharp.SKGraphiteContextOptions

```csharp
public struct SKGraphiteContextOptions, System.IEquatable<SKGraphiteContextOptions> {
	// properties
	public bool DisableDriverCorrectnessWorkarounds { get; set; }
	public long GpuBudgetInBytes { get; set; }
	public int InternalMultisampleCount { get; set; }
	public bool RequireOrderedRecordings { get; set; }
	public bool SetBackendLabels { get; set; }
	// methods
	public virtual bool Equals (SKGraphiteContextOptions obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (SKGraphiteContextOptions left, SKGraphiteContextOptions right);
	public static bool op_Inequality (SKGraphiteContextOptions left, SKGraphiteContextOptions right);
}
```

#### New Type: SkiaSharp.SKGraphiteDawnBackendContext

```csharp
public class SKGraphiteDawnBackendContext : System.IDisposable {
	// constructors
	public SKGraphiteDawnBackendContext ();
	// properties
	public IntPtr WgpuDevice { get; set; }
	public IntPtr WgpuInstance { get; set; }
	public IntPtr WgpuQueue { get; set; }
	// methods
	public virtual void Dispose ();
	protected virtual void Dispose (bool disposing);
}
```

#### New Type: SkiaSharp.SKGraphiteDawnBackendContextInit

```csharp
public struct SKGraphiteDawnBackendContextInit, System.IEquatable<SKGraphiteDawnBackendContextInit> {
	// properties
	public void* Device { get; set; }
	public void* Instance { get; set; }
	public bool NonYielding { get; set; }
	public void* Queue { get; set; }
	// methods
	public virtual bool Equals (SKGraphiteDawnBackendContextInit obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (SKGraphiteDawnBackendContextInit left, SKGraphiteDawnBackendContextInit right);
	public static bool op_Inequality (SKGraphiteDawnBackendContextInit left, SKGraphiteDawnBackendContextInit right);
}
```

#### New Type: SkiaSharp.SKGraphiteFindOrCreateImageDelegate

```csharp
public sealed delegate SKGraphiteFindOrCreateImageDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public SKGraphiteFindOrCreateImageDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (SKGraphiteRecorder recorder, SKImage image, bool mipmapped, System.AsyncCallback callback, object object);
	public virtual SKImage EndInvoke (System.IAsyncResult result);
	public virtual SKImage Invoke (SKGraphiteRecorder recorder, SKImage image, bool mipmapped);
}
```

#### New Type: SkiaSharp.SKGraphiteImageCache

```csharp
public sealed class SKGraphiteImageCache : System.IDisposable {
	// constructors
	public SKGraphiteImageCache ();
	// methods
	public virtual void Dispose ();
	public SKImage FindOrCreate (SKGraphiteRecorder recorder, SKImage image, bool mipmapped);
}
```

#### New Type: SkiaSharp.SKGraphiteInsertRecordingInfo

```csharp
public struct SKGraphiteInsertRecordingInfo, System.IEquatable<SKGraphiteInsertRecordingInfo> {
	// properties
	public IntPtr Recording { get; set; }
	public SKRectI TargetClip { get; set; }
	public IntPtr TargetSurface { get; set; }
	public int TargetTranslationX { get; set; }
	public int TargetTranslationY { get; set; }
	// methods
	public virtual bool Equals (SKGraphiteInsertRecordingInfo obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (SKGraphiteInsertRecordingInfo left, SKGraphiteInsertRecordingInfo right);
	public static bool op_Inequality (SKGraphiteInsertRecordingInfo left, SKGraphiteInsertRecordingInfo right);
}
```

#### New Type: SkiaSharp.SKGraphiteInsertStatus

```csharp
[Serializable]
public enum SKGraphiteInsertStatus {
	AddCommandsFailed = 3,
	AsyncShaderCompilesFailed = 4,
	InvalidRecording = 1,
	OutOfOrderRecording = 5,
	PromiseInstantiationFailed = 2,
	Success = 0,
}
```

#### New Type: SkiaSharp.SKGraphiteMtlBackendContext

```csharp
public class SKGraphiteMtlBackendContext : System.IDisposable {
	// constructors
	public SKGraphiteMtlBackendContext ();
	// properties
	public IntPtr MtlDevice { get; set; }
	public IntPtr MtlQueue { get; set; }
	// methods
	public virtual void Dispose ();
	protected virtual void Dispose (bool disposing);
}
```

#### New Type: SkiaSharp.SKGraphiteMtlBackendContextInit

```csharp
public struct SKGraphiteMtlBackendContextInit, System.IEquatable<SKGraphiteMtlBackendContextInit> {
	// properties
	public void* Device { get; set; }
	public void* Queue { get; set; }
	// methods
	public virtual bool Equals (SKGraphiteMtlBackendContextInit obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (SKGraphiteMtlBackendContextInit left, SKGraphiteMtlBackendContextInit right);
	public static bool op_Inequality (SKGraphiteMtlBackendContextInit left, SKGraphiteMtlBackendContextInit right);
}
```

#### New Type: SkiaSharp.SKGraphiteRecorder

```csharp
public class SKGraphiteRecorder : SkiaSharp.SKObject, System.IDisposable {
	// properties
	public SKGraphiteBackend Backend { get; }
	public int MaxTextureSize { get; }
	// methods
	public SKGraphiteBackendTexture CreateBackendTexture (int width, int height, SKGraphiteTextureInfo info);
	public void DeleteBackendTexture (SKGraphiteBackendTexture backendTexture);
	protected override void DisposeNative ();
	public SKGraphiteRecording Snap ();
}
```

#### New Type: SkiaSharp.SKGraphiteRecording

```csharp
public class SKGraphiteRecording : SkiaSharp.SKObject, System.IDisposable {
	// methods
	protected override void DisposeNative ();
}
```

#### New Type: SkiaSharp.SKGraphiteReleaseDelegate

```csharp
public sealed delegate SKGraphiteReleaseDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public SKGraphiteReleaseDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (System.AsyncCallback callback, object object);
	public virtual void EndInvoke (System.IAsyncResult result);
	public virtual void Invoke ();
}
```

#### New Type: SkiaSharp.SKGraphiteSubmitInfo

```csharp
public struct SKGraphiteSubmitInfo, System.IEquatable<SKGraphiteSubmitInfo> {
	// properties
	public ulong FrameID { get; set; }
	public bool MarkBoundary { get; set; }
	public bool Sync { get; set; }
	// methods
	public virtual bool Equals (SKGraphiteSubmitInfo obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (SKGraphiteSubmitInfo left, SKGraphiteSubmitInfo right);
	public static bool op_Inequality (SKGraphiteSubmitInfo left, SKGraphiteSubmitInfo right);
}
```

#### New Type: SkiaSharp.SKGraphiteTextureInfo

```csharp
public class SKGraphiteTextureInfo : SkiaSharp.SKObject, System.IDisposable {
	// properties
	public SKGraphiteBackend Backend { get; }
	public bool IsValid { get; }
	public bool Mipmapped { get; }
	public int SampleCount { get; }
	// methods
	public static SKGraphiteTextureInfo CreateVulkan (SKGraphiteVkTextureInfo info);
	protected override void DisposeNative ();
}
```

#### New Type: SkiaSharp.SKGraphiteVkBackendContext

```csharp
public class SKGraphiteVkBackendContext : System.IDisposable {
	// constructors
	public SKGraphiteVkBackendContext ();
	// properties
	public SKGraphiteVkGetProcedureAddressDelegate GetProcedureAddress { get; set; }
	public uint GraphicsQueueIndex { get; set; }
	public uint MaxApiVersion { get; set; }
	public bool ProtectedContext { get; set; }
	public IntPtr VkDevice { get; set; }
	public IntPtr VkInstance { get; set; }
	public IntPtr VkPhysicalDevice { get; set; }
	public IntPtr VkQueue { get; set; }
	// methods
	public virtual void Dispose ();
	protected override void ~SKGraphiteVkBackendContext ();
}
```

#### New Type: SkiaSharp.SKGraphiteVkGetProcedureAddressDelegate

```csharp
public sealed delegate SKGraphiteVkGetProcedureAddressDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public SKGraphiteVkGetProcedureAddressDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (string name, IntPtr instance, IntPtr device, System.AsyncCallback callback, object object);
	public virtual IntPtr EndInvoke (System.IAsyncResult result);
	public virtual IntPtr Invoke (string name, IntPtr instance, IntPtr device);
}
```

#### New Type: SkiaSharp.SKGraphiteVkTextureInfo

```csharp
public struct SKGraphiteVkTextureInfo, System.IEquatable<SKGraphiteVkTextureInfo> {
	// properties
	public uint AspectMask { get; set; }
	public uint Flags { get; set; }
	public int Format { get; set; }
	public int ImageTiling { get; set; }
	public uint ImageUsageFlags { get; set; }
	public bool Mipmapped { get; set; }
	public int SampleCount { get; set; }
	public int SharingMode { get; set; }
	// methods
	public virtual bool Equals (SKGraphiteVkTextureInfo obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (SKGraphiteVkTextureInfo left, SKGraphiteVkTextureInfo right);
	public static bool op_Inequality (SKGraphiteVkTextureInfo left, SKGraphiteVkTextureInfo right);
}
```

#### New Type: SkiaSharp.SKImageReadPixelsResult

```csharp
public sealed class SKImageReadPixelsResult : System.IDisposable {
	// properties
	public int PlaneCount { get; }
	// methods
	public void CopyPlaneTo (int planeIndex, System.Span<byte> destination);
	public virtual void Dispose ();
	public System.ReadOnlySpan<byte> GetPlaneData (int planeIndex);
	public int GetPlaneRowBytes (int planeIndex);
	public byte[] ToArray (int planeIndex);
	public SKBitmap ToBitmap ();
	public SKImage ToImage ();
}
```

#### New Type: SkiaSharp.SKImageRescaleGamma

```csharp
[Serializable]
public enum SKImageRescaleGamma {
	Linear = 1,
	Src = 0,
}
```

#### New Type: SkiaSharp.SKImageRescaleMode

```csharp
[Serializable]
public enum SKImageRescaleMode {
	Linear = 1,
	Nearest = 0,
	RepeatedCubic = 3,
	RepeatedLinear = 2,
}
```
