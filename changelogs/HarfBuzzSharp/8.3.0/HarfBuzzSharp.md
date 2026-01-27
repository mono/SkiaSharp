# API diff: HarfBuzzSharp.dll

## HarfBuzzSharp.dll

### Namespace HarfBuzzSharp

#### Type Changed: HarfBuzzSharp.Blob

Removed constructor:

```csharp
[Obsolete ("Use Blob(IntPtr, int, MemoryMode, ReleaseDelegate) instead.")]
public Blob (IntPtr data, uint length, MemoryMode mode, object userData, BlobReleaseDelegate releaseDelegate);
```

Removed method:

```csharp
public System.ReadOnlySpan<byte> AsSpan ();
```

Added method:

```csharp
public System.Span<byte> AsSpan ();
```



