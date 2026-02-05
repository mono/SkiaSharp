# MAUI Shape API Comparison: MAUI vs WPF vs UWP vs SKShapeView

This document provides a comprehensive comparison of shape, geometry, brush, and transform APIs across .NET MAUI, WPF, UWP, and the new SKShapeView control in SkiaSharp.

## Table of Contents

1. [Shape Types](#shape-types)
2. [Shape Properties](#shape-properties)
3. [Geometry Types](#geometry-types)
4. [Geometry Properties](#geometry-properties)
5. [Path Segments](#path-segments)
6. [Brush Types](#brush-types)
7. [Brush Properties](#brush-properties)
8. [Transform Types](#transform-types)
9. [Enumerations](#enumerations)
10. [Canvas/Positioning](#canvaspositioning)
11. [Summary](#summary)

---

## Shape Types

| Shape | MAUI | WPF | UWP | SKShapeView |
|-------|:----:|:---:|:---:|:-----------:|
| Rectangle | ✅ | ✅ | ✅ | ✅ |
| RoundRectangle | ✅ | ❌¹ | ❌¹ | ✅ |
| Ellipse | ✅ | ✅ | ✅ | ✅ |
| Line | ✅ | ✅ | ✅ | ✅ |
| Polyline | ✅ | ✅ | ✅ | ✅ |
| Polygon | ✅ | ✅ | ✅ | ✅ |
| Path | ✅ | ✅ | ✅ | ✅ |

¹ WPF/UWP use Rectangle with RadiusX/RadiusY properties for rounded corners

---

## Shape Properties

### Core Shape Properties

| Property | MAUI | WPF | UWP | SKShapeView | Notes |
|----------|:----:|:---:|:---:|:-----------:|-------|
| **Fill** | ✅ | ✅ | ✅ | ✅ | Brush for interior |
| **Stroke** | ✅ | ✅ | ✅ | ✅ | Brush for outline |
| **StrokeThickness** | ✅ | ✅ | ✅ | ✅ | Width of outline |
| **StrokeDashArray** | ✅ | ✅ | ✅ | ✅ | Dash pattern |
| **StrokeDashOffset** | ✅ | ✅ | ✅ | ✅ | Dash start offset |
| **StrokeLineCap** | ✅ | ✅ | ✅ | ✅ | Cap style (Flat, Round, Square) |
| **StrokeLineJoin** | ✅ | ✅ | ✅ | ✅ | Join style (Miter, Bevel, Round) |
| **StrokeMiterLimit** | ✅ | ✅ | ✅ | ✅ | Miter join limit |
| **StrokeStartLineCap** | ❌ | ✅ | ✅ | ❌ | MAUI uses single StrokeLineCap |
| **StrokeEndLineCap** | ❌ | ✅ | ✅ | ❌ | MAUI uses single StrokeLineCap |
| **StrokeDashCap** | ❌ | ✅ | ✅ | ❌ | Cap for dashes |
| **Aspect** | ✅ | ❌ | ❌ | ⚠️ | MAUI-specific, *not yet implemented* |
| **Stretch** | ❌ | ✅ | ✅ | ❌ | Shape stretching mode |
| **GeometryTransform** | ❌ | ✅ | ✅ | ❌ | Transform applied to geometry |

### Shape-Specific Properties

| Property | Shape | MAUI | WPF | UWP | SKShapeView |
|----------|-------|:----:|:---:|:---:|:-----------:|
| **RadiusX/RadiusY** | Rectangle | ❌¹ | ✅ | ✅ | ❌ |
| **CornerRadius** | RoundRectangle | ✅ | ❌ | ❌ | ✅ |
| **X1, Y1, X2, Y2** | Line | ✅ | ✅ | ✅ | ✅ |
| **Points** | Polyline/Polygon | ✅ | ✅ | ✅ | ✅ |
| **FillRule** | Polygon/Path | ✅ | ✅ | ✅ | ✅ |
| **Data** | Path | ✅ | ✅ | ✅ | ✅ |

¹ MAUI uses RoundRectangle shape instead

---

## Geometry Types

| Geometry | MAUI | WPF | UWP | SKShapeView | Notes |
|----------|:----:|:---:|:---:|:-----------:|-------|
| EllipseGeometry | ✅ | ✅ | ✅ | ✅ | |
| RectangleGeometry | ✅ | ✅ | ✅ | ✅ | |
| RoundRectangleGeometry | ✅ | ❌ | ❌ | ✅ | MAUI-specific |
| LineGeometry | ✅ | ✅ | ✅ | ✅ | |
| PathGeometry | ✅ | ✅ | ✅ | ✅ | |
| GeometryGroup | ✅ | ✅ | ✅ | ✅ | |
| CombinedGeometry | ❌ | ✅ | ✅ | ❌ | Boolean operations |
| StreamGeometry | ❌ | ✅ | ❌ | ❌ | Lightweight path |

---

## Geometry Properties

### EllipseGeometry

| Property | MAUI | WPF | UWP | SKShapeView |
|----------|:----:|:---:|:---:|:-----------:|
| Center | ✅ | ✅ | ✅ | ✅ |
| RadiusX | ✅ | ✅ | ✅ | ✅ |
| RadiusY | ✅ | ✅ | ✅ | ✅ |
| Transform | ❌ | ✅ | ✅ | ❌ |

### RectangleGeometry

| Property | MAUI | WPF | UWP | SKShapeView |
|----------|:----:|:---:|:---:|:-----------:|
| Rect | ✅ | ✅ | ✅ | ✅ |
| RadiusX | ❌ | ✅ | ✅ | ❌ |
| RadiusY | ❌ | ✅ | ✅ | ❌ |
| Transform | ❌ | ✅ | ✅ | ❌ |

### RoundRectangleGeometry (MAUI-specific)

| Property | MAUI | WPF | UWP | SKShapeView |
|----------|:----:|:---:|:---:|:-----------:|
| Rect | ✅ | ❌ | ❌ | ✅ |
| CornerRadius | ✅ | ❌ | ❌ | ✅ |

### LineGeometry

| Property | MAUI | WPF | UWP | SKShapeView |
|----------|:----:|:---:|:---:|:-----------:|
| StartPoint | ✅ | ✅ | ✅ | ✅ |
| EndPoint | ✅ | ✅ | ✅ | ✅ |
| Transform | ❌ | ✅ | ✅ | ❌ |

### PathGeometry

| Property | MAUI | WPF | UWP | SKShapeView |
|----------|:----:|:---:|:---:|:-----------:|
| Figures | ✅ | ✅ | ✅ | ✅ |
| FillRule | ✅ | ✅ | ✅ | ✅ |
| Transform | ❌ | ✅ | ✅ | ❌ |

### GeometryGroup

| Property | MAUI | WPF | UWP | SKShapeView |
|----------|:----:|:---:|:---:|:-----------:|
| Children | ✅ | ✅ | ✅ | ✅ |
| FillRule | ✅ | ✅ | ✅ | ✅ |
| Transform | ❌ | ✅ | ✅ | ❌ |

---

## Path Segments

### PathFigure Properties

| Property | MAUI | WPF | UWP | SKShapeView |
|----------|:----:|:---:|:---:|:-----------:|
| StartPoint | ✅ | ✅ | ✅ | ✅ |
| Segments | ✅ | ✅ | ✅ | ✅ |
| IsClosed | ✅ | ✅ | ✅ | ✅ |
| IsFilled | ✅ | ✅ | ✅ | ⚠️ | *Not yet used* |

### Segment Types

| Segment | MAUI | WPF | UWP | SKShapeView | Notes |
|---------|:----:|:---:|:---:|:-----------:|-------|
| LineSegment | ✅ | ✅ | ✅ | ✅ | |
| BezierSegment | ✅ | ✅ | ✅ | ✅ | Cubic bezier |
| QuadraticBezierSegment | ✅ | ✅ | ✅ | ✅ | |
| ArcSegment | ✅ | ✅ | ✅ | ✅ | |
| PolyLineSegment | ✅ | ✅ | ✅ | ✅ | |
| PolyBezierSegment | ✅ | ✅ | ✅ | ✅ | |
| PolyQuadraticBezierSegment | ✅ | ✅ | ✅ | ✅ | |

### Segment Properties

| Segment | Property | MAUI | WPF | UWP | SKShapeView |
|---------|----------|:----:|:---:|:---:|:-----------:|
| LineSegment | Point | ✅ | ✅ | ✅ | ✅ |
| BezierSegment | Point1, Point2, Point3 | ✅ | ✅ | ✅ | ✅ |
| QuadraticBezierSegment | Point1, Point2 | ✅ | ✅ | ✅ | ✅ |
| ArcSegment | Point | ✅ | ✅ | ✅ | ✅ |
| ArcSegment | Size | ✅ | ✅ | ✅ | ✅ |
| ArcSegment | RotationAngle | ✅ | ✅ | ✅ | ✅ |
| ArcSegment | IsLargeArc | ✅ | ✅ | ✅ | ✅ |
| ArcSegment | SweepDirection | ✅ | ✅ | ✅ | ✅ |
| PolyLineSegment | Points | ✅ | ✅ | ✅ | ✅ |
| PolyBezierSegment | Points | ✅ | ✅ | ✅ | ✅ |
| PolyQuadraticBezierSegment | Points | ✅ | ✅ | ✅ | ✅ |
| *All Segments* | IsStroked | ❌ | ✅ | ✅ | ❌ |
| *All Segments* | IsSmoothJoin | ❌ | ✅ | ❌ | ❌ |

---

## Brush Types

| Brush Type | MAUI | WPF | UWP | SKShapeView | Notes |
|------------|:----:|:---:|:---:|:-----------:|-------|
| SolidColorBrush | ✅ | ✅ | ✅ | ✅ | |
| LinearGradientBrush | ✅ | ✅ | ✅ | ✅ | |
| RadialGradientBrush | ✅ | ✅ | ✅ | ✅ | |
| ImageBrush | ❌ | ✅ | ✅ | ❌ | Not in MAUI |
| DrawingBrush | ❌ | ✅ | ❌ | ❌ | WPF-specific |
| VisualBrush | ❌ | ✅ | ❌ | ❌ | WPF-specific |
| TileBrush | ❌ | ✅ | ❌ | ❌ | WPF base class |
| ConicGradientBrush | ❌ | ❌ | ❌ | ❌ | Not in any |
| SweepGradientBrush | ❌ | ❌ | ❌ | ❌ | Not in any |

---

## Brush Properties

### GradientBrush (Base)

| Property | MAUI | WPF | UWP | SKShapeView |
|----------|:----:|:---:|:---:|:-----------:|
| GradientStops | ✅ | ✅ | ✅ | ✅ |

### LinearGradientBrush

| Property | MAUI | WPF | UWP | SKShapeView |
|----------|:----:|:---:|:---:|:-----------:|
| StartPoint | ✅ | ✅ | ✅ | ✅ |
| EndPoint | ✅ | ✅ | ✅ | ✅ |
| SpreadMethod | ❌ | ✅ | ✅ | ⚠️ | *Uses Clamp only* |
| MappingMode | ❌ | ✅ | ✅ | ❌ | Absolute/Relative |
| Transform | ❌ | ✅ | ✅ | ❌ | |
| RelativeTransform | ❌ | ✅ | ✅ | ❌ | |

### RadialGradientBrush

| Property | MAUI | WPF | UWP | SKShapeView |
|----------|:----:|:---:|:---:|:-----------:|
| Center | ✅ | ✅ | ✅ | ✅ |
| Radius | ✅ | ❌¹ | ❌¹ | ✅ |
| RadiusX | ❌ | ✅ | ✅ | ❌ |
| RadiusY | ❌ | ✅ | ✅ | ❌ |
| GradientOrigin | ❌ | ✅ | ✅ | ❌ | Focal point |
| SpreadMethod | ❌ | ✅ | ✅ | ⚠️ | *Uses Clamp only* |
| MappingMode | ❌ | ✅ | ✅ | ❌ | |
| Transform | ❌ | ✅ | ✅ | ❌ | |

¹ WPF/UWP use RadiusX/RadiusY for elliptical gradients

### GradientStop

| Property | MAUI | WPF | UWP | SKShapeView |
|----------|:----:|:---:|:---:|:-----------:|
| Color | ✅ | ✅ | ✅ | ✅ |
| Offset | ✅ | ✅ | ✅ | ✅ |

---

## Transform Types

| Transform | MAUI | WPF | UWP | SKShapeView | Notes |
|-----------|:----:|:---:|:---:|:-----------:|-------|
| TranslateTransform | ✅ | ✅ | ✅ | ✅ | |
| ScaleTransform | ✅ | ✅ | ✅ | ✅ | |
| RotateTransform | ✅ | ✅ | ✅ | ✅ | |
| SkewTransform | ✅ | ✅ | ✅ | ✅ | |
| MatrixTransform | ✅ | ✅ | ✅ | ✅ | |
| CompositeTransform | ✅ | ❌ | ✅ | ✅ | Not in WPF |
| TransformGroup | ✅ | ✅ | ✅ | ✅ | |

### Transform Properties

| Property | Transform | MAUI | WPF | UWP | SKShapeView |
|----------|-----------|:----:|:---:|:---:|:-----------:|
| X, Y | TranslateTransform | ✅ | ✅ | ✅ | ✅ |
| ScaleX, ScaleY | ScaleTransform | ✅ | ✅ | ✅ | ✅ |
| CenterX, CenterY | ScaleTransform | ✅ | ✅ | ✅ | ✅ |
| Angle | RotateTransform | ✅ | ✅ | ✅ | ✅ |
| CenterX, CenterY | RotateTransform | ✅ | ✅ | ✅ | ✅ |
| AngleX, AngleY | SkewTransform | ✅ | ✅ | ✅ | ✅ |
| CenterX, CenterY | SkewTransform | ✅ | ✅ | ✅ | ⚠️ | *Not yet implemented* |
| Matrix | MatrixTransform | ✅ | ✅ | ✅ | ✅ |
| ScaleX, ScaleY, Rotation, etc. | CompositeTransform | ✅ | ❌ | ✅ | ✅ |
| Children | TransformGroup | ✅ | ✅ | ✅ | ✅ |

---

## Enumerations

### PenLineCap

| Value | MAUI | WPF | UWP | SKShapeView |
|-------|:----:|:---:|:---:|:-----------:|
| Flat | ✅ | ✅ | ✅ | ✅ |
| Round | ✅ | ✅ | ✅ | ✅ |
| Square | ✅ | ✅ | ✅ | ✅ |
| Triangle | ❌ | ✅ | ✅ | ❌ |

### PenLineJoin

| Value | MAUI | WPF | UWP | SKShapeView |
|-------|:----:|:---:|:---:|:-----------:|
| Miter | ✅ | ✅ | ✅ | ✅ |
| Bevel | ✅ | ✅ | ✅ | ✅ |
| Round | ✅ | ✅ | ✅ | ✅ |

### FillRule

| Value | MAUI | WPF | UWP | SKShapeView |
|-------|:----:|:---:|:---:|:-----------:|
| EvenOdd | ✅ | ✅ | ✅ | ✅ |
| Nonzero/Winding | ✅ | ✅ | ✅ | ✅ |

### SweepDirection (Arcs)

| Value | MAUI | WPF | UWP | SKShapeView |
|-------|:----:|:---:|:---:|:-----------:|
| Clockwise | ✅ | ✅ | ✅ | ✅ |
| CounterClockwise | ✅ | ✅ | ✅ | ✅ |

### Stretch (Shape scaling)

| Value | MAUI | WPF | UWP | SKShapeView |
|-------|:----:|:---:|:---:|:-----------:|
| None | ❌ | ✅ | ✅ | ❌ |
| Fill | ❌ | ✅ | ✅ | ❌ |
| Uniform | ❌ | ✅ | ✅ | ❌ |
| UniformToFill | ❌ | ✅ | ✅ | ❌ |

---

## Canvas/Positioning

| Feature | MAUI | WPF | UWP | SKShapeView |
|---------|:----:|:---:|:---:|:-----------:|
| Canvas.Left | ❌¹ | ✅ | ✅ | ✅ |
| Canvas.Top | ❌¹ | ✅ | ✅ | ✅ |
| Canvas.Right | ❌ | ✅ | ✅ | ❌ |
| Canvas.Bottom | ❌ | ✅ | ✅ | ❌ |
| Canvas.ZIndex | ❌ | ✅ | ✅ | ❌² |
| RenderTransform | ✅ | ✅ | ✅ | ✅ |
| RenderTransformOrigin | ✅ | ✅ | ✅ | ⚠️ |

¹ MAUI uses AbsoluteLayout for absolute positioning  
² SKShapeView draws children in order added (implicit Z-order)

---

## Summary

### Coverage Statistics

| Category | Total APIs | SKShapeView Supported | Coverage |
|----------|------------|----------------------|----------|
| Shape Types | 7 | 7 | **100%** |
| Core Shape Properties | 13 | 11 | **85%** |
| Geometry Types | 8 | 6 | **75%** |
| Path Segments | 7 | 7 | **100%** |
| Brush Types | 9 | 3 | **33%** |
| Transform Types | 7 | 7 | **100%** |

### Not Yet Implemented in SKShapeView

The following MAUI APIs are not yet supported:

1. **Aspect** - MAUI-specific shape aspect/stretching mode
2. **PathFigure.IsFilled** - Per-figure fill control
3. **Geometry.Transform** - Transform on geometry (vs. shape)
4. **SpreadMethod** - Gradient spread (Pad/Reflect/Repeat)
5. **SkewTransform CenterX/CenterY** - Skew center point
6. **RenderTransformOrigin** - Transform origin point

### Not Available in MAUI (compared to WPF/UWP)

1. **ImageBrush** - Use BackgroundImageSource instead
2. **CombinedGeometry** - Boolean geometry operations
3. **Stretch** - Shape stretching mode
4. **StrokeStartLineCap/StrokeEndLineCap** - Separate start/end caps
5. **StrokeDashCap** - Dash segment caps
6. **GeometryTransform** - Transform applied to geometry

### Fully Supported Features

✅ All MAUI shape types (Rectangle, RoundRectangle, Ellipse, Line, Polyline, Polygon, Path)  
✅ All MAUI geometry types  
✅ All path segment types  
✅ All transform types  
✅ Core stroke properties (thickness, dash patterns, line caps, line joins)  
✅ All MAUI brush types (SolidColorBrush, LinearGradientBrush, RadialGradientBrush)  
✅ Canvas-style positioning with Left/Top attached properties  
✅ Gradient stops with colors and offsets

---

## Legend

| Symbol | Meaning |
|--------|---------|
| ✅ | Fully supported |
| ⚠️ | Partially supported or not yet implemented |
| ❌ | Not supported/available |
