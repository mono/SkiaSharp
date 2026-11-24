# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKSize

Modified methods:

```diff
-public bool op_Equality (SKSize sz1, SKSize sz2---right---)
+public bool op_Equality (SKSize left, SKSize +++sz2+++right)
-public bool op_Inequality (SKSize sz1, SKSize sz2---right---)
+public bool op_Inequality (SKSize left, SKSize +++sz2+++right)
```


#### Type Changed: SkiaSharp.SKSizeI

Modified methods:

```diff
-public bool op_Equality (SKSizeI sz1, SKSizeI sz2---right---)
+public bool op_Equality (SKSizeI left, SKSizeI +++sz2+++right)
-public bool op_Inequality (SKSizeI sz1, SKSizeI sz2---right---)
+public bool op_Inequality (SKSizeI left, SKSizeI +++sz2+++right)
```



