# DocsSamplesApp Touch Testing Guide

This document lists all pages in the DocsSamplesApp that use touch functionality. Use this guide to verify touch interactions work correctly after the migration from custom `TouchEffect` to the built-in `SKCanvasView.Touch` API.

---

## Test Environment

- **App**: `samples/DocsSamplesApp`
- **Platforms**: iOS, Android, Mac Catalyst, Windows
- **SkiaSharp Version**: 3.119.1
- **.NET Version**: 10.0

---

## Touch-Enabled Pages by Category

### From Main Menu → **SkiaSharp Lines and Paths**

| Section | Page Title | Description | Touch Type |
|---------|-----------|-------------|------------|
| Finger Painting | **Finger Paint** | Draw Lines with Your Fingers | Multi-touch drawing |

---

### From Main Menu → **SkiaSharp Curves and Paths**

| Section | Page Title | Description | Touch Type |
|---------|-----------|-------------|------------|
| Three Ways to Draw an Arc | **Tangent Arc** | Experiment with creating a tangent arc | Drag control points |
| Three Ways to Draw an Arc | **Elliptical Arc** | Experiment with creating an elliptical arc | Drag control points |
| Three Types of Bézier Curves | **Bézier Curve** | Interactively draw a cubic Bézier curve | Drag control points |
| Three Types of Bézier Curves | **Quadratic Curve** | Interactively draw a quadratic Bézier curve | Drag control points |
| Three Types of Bézier Curves | **Conic Curve** | Interactively draw a conic curve | Drag control points |
| Information & Enumeration | **Path Length** | Compute the length of a path | Drag control points |

---

### From Main Menu → **SkiaSharp Transforms**

| Section | Page Title | Description | Touch Type |
|---------|-----------|-------------|------------|
| Matrix Transforms | **Show Affine Matrix** | Display the matrix for affine transforms | Drag 3 corners |
| Touch Manipulations | **Touch Manipulation** ⭐ | Pan, scale, and rotate a bitmap | Multi-touch (2-finger) |
| Touch Manipulations | **Bitmap Scatter View** ⭐ | Pan, scale, and rotate multiple bitmaps | Multi-touch, multiple objects |
| Touch Manipulations | **Single Finger Corner Scale** | Pan and scale a bitmap with one finger | Single touch, hit test |
| Non-Affine Transforms | **Show Non-Affine Matrix** | Display the matrix for non-affine transforms | Drag 4 corners |

---

### From Main Menu → **SkiaSharp Bitmaps**

| Section | Page Title | Description | Touch Type |
|---------|-----------|-------------|------------|
| Bitmap Cropping | **Photo Cropping** ⭐ | Interactively define a bitmap cropping rectangle | Drag crop corners |
| Saving Bitmap Files | **Finger Paint with Save** | Save your finger-painting artworks | Multi-touch drawing |

---

### From Main Menu → **SkiaSharp Effects**

| Section | Page Title | Description | Touch Type |
|---------|-----------|-------------|------------|
| Linear Gradients | **Interactive Linear Gradient** | Interactively experiment with linear gradient points and modes | Drag gradient endpoints |
| Circular Gradients | **Conical Gradient** | Experiment with the Two-Point Conical Gradient | Drag gradient center |

---

## Quick Reference Checklist

Use this checklist when testing:

### Single-Touch Pages
- [ ] **Finger Paint** (Lines and Paths → Finger Painting)
- [ ] **Finger Paint with Save** (Bitmaps → Saving Bitmap Files)
- [ ] **Tangent Arc** (Curves → Three Ways to Draw an Arc)
- [ ] **Elliptical Arc** (Curves → Three Ways to Draw an Arc)
- [ ] **Bézier Curve** (Curves → Three Types of Bézier Curves)
- [ ] **Quadratic Curve** (Curves → Three Types of Bézier Curves)
- [ ] **Conic Curve** (Curves → Three Types of Bézier Curves)
- [ ] **Path Length** (Curves → Information & Enumeration)
- [ ] **Show Affine Matrix** (Transforms → Matrix Transforms)
- [ ] **Show Non-Affine Matrix** (Transforms → Non-Affine Transforms)
- [ ] **Single Finger Corner Scale** (Transforms → Touch Manipulations)
- [ ] **Interactive Linear Gradient** (Effects → Linear Gradients)
- [ ] **Conical Gradient** (Effects → Circular Gradients)

### Multi-Touch Pages ⭐
- [ ] **Touch Manipulation** (Transforms → Touch Manipulations) - Test pinch-to-zoom and rotate
- [ ] **Bitmap Scatter View** (Transforms → Touch Manipulations) - Test dragging multiple images
- [ ] **Photo Cropping** (Bitmaps → Bitmap Cropping) - Test dragging crop corners

---

## Test Scenarios

### 1. Single-Touch Dragging
- Touch a control point and drag it
- Verify smooth, responsive movement
- Verify the canvas updates in real-time

### 2. Multi-Touch (2-Finger) Gestures
- On **Touch Manipulation** page:
  - Pinch to zoom in/out
  - Rotate with two fingers
  - Try different modes from the picker (PanOnly, IsotropicScale, ScaleRotate, etc.)

### 3. Multi-Object Touch
- On **Bitmap Scatter View** page:
  - Drag one bitmap while another stays still
  - Try dragging two different bitmaps simultaneously (requires two fingers on different images)

### 4. Touch Capture
- Start dragging a control point
- Move finger outside the canvas area
- Return finger to canvas
- Verify tracking continues correctly

### 5. Touch Release
- Drag a control point
- Lift finger
- Verify state resets properly
- Touch again and verify a new drag operation starts

### 6. Photo Cropper
- Navigate to **Photo Cropping**
- Load an image
- Drag each of the 4 crop corners
- Verify the crop rectangle updates correctly

---

## Migration Details

The following changes were made to migrate from custom `TouchEffect` to built-in touch:

### API Changes
| Old (TouchEffect) | New (SKCanvasView.Touch) |
|-------------------|--------------------------|
| `TouchActionType` enum | `SKTouchAction` enum |
| `TouchActionEventArgs` | `SKTouchEventArgs` |
| `args.Type` | `e.ActionType` |
| `args.Location` (view coords) | `e.Location` (already pixels) |
| `Capture="True"` | `e.Handled = true` |

### Files Deleted (8 files)
- `TouchEffect.cs`
- `TouchActionType.cs`
- `TouchActionEventArgs.cs`
- `TouchActionEventHandler.cs`
- `Platforms/Android/TouchEffect.cs`
- `Platforms/iOS/TouchEffect.cs`
- `Platforms/iOS/TouchRecognizer.cs`
- `Platforms/Windows/TouchEffect.cs`

### Code Files Modified
- `TouchPoint.cs` - Updated to use `SKTouchAction`
- `InteractivePage.cs` - Updated to use `SKTouchEventArgs`
- `TouchManipulationBitmap.cs` - Updated to use `SKTouchAction`
- `PhotoCropperCanvasView.cs` - Now uses built-in touch directly
- 17 XAML files - Removed `<tt:TouchEffect>`, added `EnableTouchEvents="True"` and `Touch="OnTouch"`
- 10 code-behind files - Updated event handlers

### Documentation Files Updated (10 files)
- `docs/docs/paths/finger-paint.md` - Finger painting tutorial
- `docs/docs/transforms/touch.md` - Main touch manipulation documentation
- `docs/docs/transforms/matrix.md` - Matrix transforms with TouchPoint
- `docs/docs/curves/beziers.md` - Bézier curve documentation
- `docs/docs/curves/arcs.md` - Arc documentation with InteractivePage
- `docs/docs/curves/information.md` - Path information with touch
- `docs/docs/bitmaps/cropping.md` - Photo cropping documentation
- `docs/docs/bitmaps/saving.md` - Saving bitmaps with finger paint
- `docs/docs/effects/shaders/linear-gradient.md` - Interactive gradient demo
- `docs/docs/effects/shaders/circular-gradients.md` - Conical gradient demo

---

## Known Issues

_None currently identified._

---

## Notes

- The built-in `SKCanvasView.Touch` API provides coordinates already in pixels, so no manual conversion is needed
- Setting `e.Handled = true` in the touch handler enables touch capture (equivalent to old `Capture="True"`)
- The built-in API also provides additional features: `MouseButton`, `DeviceType`, `WheelDelta`, `Pressure`
