# API diff: HarfBuzzSharp.dll

## HarfBuzzSharp.dll

### Namespace HarfBuzzSharp

#### Type Changed: HarfBuzzSharp.BufferFlags

Added values:

```csharp
ProduceSafeToInsertTatweel = 128,
ProduceUnsafeToConcat = 64,
Verify = 32,
```


#### Type Changed: HarfBuzzSharp.ClusterLevel

Added value:

```csharp
Graphemes = 3,
```


#### Type Changed: HarfBuzzSharp.GlyphFlags

Removed value:

```csharp
Defined = 1,
```

Added values:

```csharp
SafeToInsertTatweel = 4,
UnsafeToConcat = 2,
```


#### Type Changed: HarfBuzzSharp.OpenTypeLayoutBaselineTag

Added values:

```csharp
IdeoEmboxCentral = 1231315813,
IdeoFaceCentral = 1231251043,
```


#### Type Changed: HarfBuzzSharp.UnicodeCombiningClass

Removed value:

```csharp
CCC133 = 132,
```

Added value:

```csharp
CCC132 = 132,
```
