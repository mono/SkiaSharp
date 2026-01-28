# DocsSamplesApp Warnings Inventory

This document catalogs compiler warnings in the DocsSamplesApp project after the migration to .NET 10 and SkiaSharp 3.119.1.

**Last Updated**: January 2026  
**Current Build**: 80 warnings, 0 errors (per target framework, ~240 total across 3 frameworks)

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
| CS0618 - Touch handling | ✅ FIXED | 0 | Only mark handled when actually processing |
| CS0618 - LayoutOptions.*AndExpand | ✅ FIXED | 0 | Converted StackLayout → Grid |
| CS0618 - DisplayAlert/LayoutTo | ✅ FIXED | 0 | Changed to Async versions |
| CS0618 - Device.Idiom | ✅ FIXED | 0 | Changed to DeviceInfo.Idiom |
| CS0618 - IPhotoLibrary | ✅ FIXED | 0 | Removed, using MAUI MediaPicker + MCT FileSaver |
| CS0618 - TranslateTo | ✅ FIXED | 0 | Changed to TranslateToAsync |
| CS0612 - Device.GetNamedSize | ⏳ PENDING | 12 | PhotoPuzzle pages (XAML SourceGen) |
| CS8600/CS8602 - Null dereference | ⏳ PENDING | 48 | Various pages |
| CS8625 - Null literal conversion | ⏳ PENDING | 10 | Various pages |
| CS8618 - Non-nullable field | ⏳ PENDING | 6 | Uninitialized fields |
| CS9191 - ref vs in parameter | ⏳ PENDING | 2 | TouchManipulationBitmap.cs |
| CS8604 - Null argument | ⏳ PENDING | 2 | BitmapExtensions |

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

### 5. XAML Source Generation
- Enabled `MauiXamlInflator=SourceGen` for faster inflation

### 6. SKPaint Text APIs → SKFont
- Migrated all code files from `SKPaint` text properties to `SKFont`
- Updated all documentation to match code changes

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
- Replaced `TargetType="Frame"` with `TargetType="Border"` style
- Replaced `TargetType="ListView"` with `TargetType="CollectionView"` style

### 8. Shell URI-Based Navigation
- Migrated from `Type PageType` to `string Route` in MenuItem
- Routes use hierarchical kebab-case: `/basics/simple-circle`
- Use `Shell.Current.GoToAsync()` instead of `Activator.CreateInstance`
- Removed redundant TapGestureRecognizer (CollectionView handles selection)

### 9. Touch Handling
- Fixed `e.Handled = true` blocking Shell back navigation
- Now only set `Handled` when actually processing a touch point

### 10. LayoutOptions.*AndExpand
- Converted 25 XAML files from StackLayout to Grid
- For correct axis usage: Grid with `*` rows/columns
- For wrong axis usage: Simplified to `Center` or `Fill`

### 11. Animation/Display APIs
- `DisplayAlert` → `DisplayAlertAsync`
- `LayoutTo` → Replaced with `TranslateToAsync` (calc delta, reset after)
- `TranslateTo` → `TranslateToAsync`

### 12. Device APIs
- `Device.Idiom` → `DeviceInfo.Idiom`

### 13. IPhotoLibrary → MAUI APIs + Community Toolkit
- Deleted `IPhotoLibrary.cs` interface
- Deleted platform-specific implementations:
  - `Platforms/Android/PhotoLibrary.cs`
  - `Platforms/iOS/PhotoLibrary.cs`
  - `Platforms/Windows/PhotoLibrary.cs`
- **Pick photo**: Now uses `MediaPicker.Default.PickPhotosAsync()`
- **Save photo**: Now uses `FileSaver.Default.SaveAsync()` from Community Toolkit
- Added `CommunityToolkit.Maui` v13.0.0 package
- Updated `MauiProgram.cs` with `.UseMauiCommunityToolkit()`

---

## ⏳ Remaining Warnings (86 per framework)

### CS0612: Device.GetNamedSize (12 warnings)

PhotoPuzzle XAML pages use obsolete `Device.GetNamedSize` in FontSize markup:
- PhotoPuzzlePage2.xaml - `{OnPlatform iOS={OnIdiom Phone=10...}}`
- PhotoPuzzlePage3.xaml - `{OnPlatform iOS={OnIdiom Phone=10...}}`

**Note:** These come from XAML SourceGen compilation. Need to replace with explicit font sizes.

### CS8600/CS8602: Null Dereference (52 warnings)

Possible null reference warnings in:
- Transforms/TouchManipulationPage.xaml.cs
- Transforms/TestPerspectivePage.xaml.cs
- Bitmaps/PhotoPuzzlePage4.xaml.cs
- Bitmaps/ColorAdjustmentPage.xaml.cs
- Effects/LightenAndDarkenPage.xaml.cs
- Platforms/Android/PhotoLibrary.cs

### CS8625: Null Literal Conversion (10 warnings)

Cannot convert null literal to non-nullable type in various pages.

### CS8618: Non-nullable Field (6 warnings)

Uninitialized non-nullable fields - need to add `= null!` or make nullable.

### CS9191: ref vs in Parameter (2 warnings)

- TouchManipulationBitmap.cs: `ref` should be `in` for readonly parameter

### CS8604: Null Argument (2 warnings)

- PhotoCroppingPage.xaml.cs: Possible null argument for parameter

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
| After Shell routing | 262 | 0 | Migrated to URI-based Shell navigation |
| After touch handling fix | 170 | 0 | Fixed e.Handled blocking back gesture |
| After LayoutOptions fix | 88 | 0 | Converted StackLayout to Grid |
| After LayoutToAsync fix | 86 | 0 | Use TranslateToAsync instead |
| After IPhotoLibrary removal | 80 | 0 | Use MAUI MediaPicker + MCT FileSaver |
| After touch handling fix | 170 | 0 | Fixed e.Handled blocking back gesture |
| After LayoutOptions fix | 88 | 0 | Converted StackLayout to Grid |
| After LayoutToAsync fix | 86 | 0 | Use TranslateToAsync instead |

---

## Commands

```bash
# Force rebuild and count warnings
cd samples/DocsSamplesApp
dotnet clean DocsSamplesApp -v q
dotnet build DocsSamplesApp -f net10.0-android -v q 2>&1 | tail -5

# Count warnings by type
dotnet build DocsSamplesApp -f net10.0-android -v q 2>&1 | grep "warning CS" | sed 's/.*warning \(CS[0-9]*\).*/\1/' | sort | uniq -c | sort -rn

# Build with warnings as errors (goal)
dotnet build -f net10.0-android /p:TreatWarningsAsErrors=true
```
