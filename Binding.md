SkiaSharp provides the same features as the native C++ library through a method of wrapping 
the C++ API with a C API that we P/Invoke into.

Then C# wraps the C API to provide an API similar to the object oriented binding provided by C++.

For example, given this C++ API:

```cpp
class SK_API SkPaint {
public:
  bool isAntiAlias() const;
  void setAntiAlias(bool aa);
};
```

This is then wrapped in a C API:

```cpp
bool sk_paint_is_antialias(const sk_paint_t* cpaint) {
  return AsPaint(cpaint)->isAntiAlias();
}
void sk_paint_set_antialias(sk_paint_t* cpaint, bool aa) {
  AsPaint(cpaint)->setAntiAlias(aa);
}
```

Which is then pulled into the C# project via P/Invoke:

```csharp
public extern static bool sk_paint_is_antialias (sk_paint_t t);
public extern static void sk_paint_set_antialias (sk_paint_t t, bool aa);
```

Finally, this is wrapped into a neat C# class:

```csharp
public class SKPaint : SKObject
{
  public bool IsAntialias {
    get { return SkiaApi.sk_paint_is_antialias (Handle); }
    set { SkiaApi.sk_paint_set_antialias (Handle, value); }
  }
}
```

As a result, the C# API functions and appears the same as the C++ API.

Since the C API is currently a work in progress of the Skia project, we 
maintain a fork in github.com/mono/skia that has our additions.   We intend
to upstream those changes to Google.
