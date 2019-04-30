# API diff: HarfBuzzSharp.dll

## HarfBuzzSharp.dll

### Namespace HarfBuzzSharp

#### Type Changed: HarfBuzzSharp.Buffer

Modified properties:

```diff
-public uint Length { get; set; }
+public int Length { get; set; }
```


#### Type Changed: HarfBuzzSharp.Face

Modified properties:

```diff
-public uint Index { get; set; }
+public int Index { get; set; }
-public uint UnitsPerEm { get; set; }
+public int UnitsPerEm { get; set; }
```


#### Type Changed: HarfBuzzSharp.Feature

Modified properties:

```diff
-public uint Tag { get; set; }
+public Tag Tag { get; set; }
```



