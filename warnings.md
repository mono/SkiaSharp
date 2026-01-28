# DocsSamplesApp Warnings Inventory

This document catalogs compiler warnings in the DocsSamplesApp project after the migration to .NET 10 and SkiaSharp 3.119.1.

**Last Updated**: January 2026  
**Current Build**: 181 warnings, 0 errors (Android target framework)

---

## Summary

| Category | Status | Count | Notes |
|----------|--------|-------|-------|
| CS8622 - Event handler nullability | ✅ FIXED | 0 | Changed `object sender` to `object? sender` |
| CS0618 - Application.MainPage | ✅ FIXED | 0 | Migrated to AppShell with CreateWindow |
| CS0618 - Device.StartTimer | ✅ FIXED | 0 | Changed to `Dispatcher.StartTimer()` |
| CS0618 - TableView/TextCell | ✅ FIXED | 0 | Migrated to CollectionView |
| CS0618 - SKPaint text APIs | ✅ FIXED | 0 | Migrated to SKFont APIs in code and docs |
| CS0618 - Frame in Styles.xaml | ✅ FIXED | 0 | Replaced with Border style |
| CS0618 - ListView in Styles.xaml | ✅ FIXED | 0 | Replaced with CollectionView style |
| CS0618 - LayoutOptions.*AndExpand | ⏳ PENDING | ~15 | Various XAML pages |
| CS0618 - Device.GetNamedSize | ⏳ PENDING | ~3 | PhotoPuzzle pages |
| CS0618 - LayoutTo obsolete | ⏳ PENDING | ~1 | PhotoPuzzlePage4.xaml.cs |
| CS8600/CS8602/CS8625 - Null handling | ⏳ PENDING | ~6 | ColorAdjustment, PhotoPuzzle pages |

---

## ✅ Completed Fixes

### 1. CS8622: Event Handler Nullability
- Changed `object sender` → `object? sender` in all event handlers
- Fixed 97 code files and 45 documentation files

### 2. CS0618: Application.MainPage  
- Migrated to AppShell pattern with `CreateWindow()`
- Deleted unused MainPage.xaml template files

### 3. CS0618: Device.StartTimer
- Replaced `Device.StartTimer` → `Dispatcher.StartTimer` in 15 files

### 4. CS0618: TableView/TextCell Navigation
- Migrated all 8 navigation pages from TableView to CollectionView
- Created MenuItem.cs and MenuSection.cs classes
- Added reusable templates in Styles.xaml
- Reduced warnings from 2226 to 275

### 5. XAML Source Generation
- Enabled `MauiXamlInflator=SourceGen` for faster inflation

### 6. SKPaint Text APIs → SKFont
- Migrated all code files from `SKPaint` text properties to `SKFont`
- Updated all documentation to match code changes
- Verified code and docs are in sync

| Old API | New API |
|---------|---------|
| `SKPaint.TextSize` | `SKFont.Size` |
| `SKPaint.Typeface` | `SKFont.Typeface` |
| `SKPaint.MeasureText(str, ref bounds)` | `SKFont.MeasureText(str, out bounds)` |
| `SKPaint.GetTextPath(s, x, y)` | `SKFont.GetTextPath(s, new SKPoint(x, y))` |
| `SKPaint.TextAlign` | `SKTextAlign` parameter in DrawText |
| `canvas.DrawText(text, x, y, paint)` | `canvas.DrawText(text, x, y, align, font, paint)` |
| `canvas.DrawTextOnPath(text, path, x, y, paint)` | `canvas.DrawTextOnPath(text, path, x, y, font, paint)` |

### 7. Frame/ListView Styles
- Replaced `TargetType="Frame"` style with `TargetType="Border"` style
- Replaced `TargetType="ListView"` style with `TargetType="CollectionView"` style

---

## ⏳ Remaining Warnings (~25 unique issues)

### Priority 1: LayoutOptions.*AndExpand (~15 warnings)

Several XAML pages use deprecated expansion options:
- `FillAndExpand`
- `CenterAndExpand`

**Files affected:**
- Bitmaps/PhotoPuzzlePage2.xaml
- Bitmaps/PhotoPuzzlePage4.xaml
- Bitmaps/SaveFileFormatsPage.xaml
- Bitmaps/FingerPaintSavePage.xaml
- Bitmaps/BitmapRotatorPage.xaml
- Bitmaps/ColorAdjustmentPage.xaml
- Effects/DistanceLightExperimentPage.xaml
- Effects/LightenAndDarkenPage.xaml
- Effects/ImageBlurExperimentPage.xaml
- Effects/GradientTransitionsPage.xaml
- Effects/DropShadowExperimentPage.xaml

**Fix:** Replace StackLayout with Grid for proper layout expansion.

### Priority 2: PhotoPuzzle Pages (~4 warnings)

- `Device.GetNamedSize` / `NamedSize` obsolete (PhotoPuzzlePage2.xaml)
- `LayoutTo` obsolete → use `LayoutToAsync` (PhotoPuzzlePage4.xaml.cs)

### Priority 3: Null handling (~6 warnings)

Minor CS8600/CS8602/CS8625 warnings in:
- ColorAdjustmentPage.xaml.cs
- PhotoPuzzlePage4.xaml.cs

---

## Build History

| Date | Warnings | Errors | Notes |
|------|----------|--------|-------|
| Initial | 2226 | 0 | After XAML SourceGen enabled |
| After TableView→CollectionView | 275 | 0 | 88% reduction |
| After SKFont migration | 340 | 0 | Verified code/docs match |
| After event handler fix | 292 | 0 | Fixed 12 remaining object sender |
| After Frame/ListView removal | 262 | 0 | Replaced with Border/CollectionView styles |
| After gesture cleanup | 181 | 0 | Removed redundant TapGestureRecognizer |

---

## Commands

```bash
# Force rebuild and count warnings
cd samples/DocsSamplesApp
touch DocsSamplesApp/DocsSamplesApp.csproj
dotnet build DocsSamplesApp --no-restore -v q 2>&1 | tail -5

# Build with warnings as errors (goal)
dotnet build -f net10.0-maccatalyst /p:TreatWarningsAsErrors=true
```
