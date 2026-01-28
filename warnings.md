# DocsSamplesApp Warnings Inventory

This document catalogs compiler warnings in the DocsSamplesApp project after the migration to .NET 10 and SkiaSharp 3.119.1.

**Last Updated**: January 2026  
**Current Build**: 275 warnings, 0 errors

---

## Summary

| Category | Status | Count | Notes |
|----------|--------|-------|-------|
| CS8622 - Event handler nullability | ✅ FIXED | 0 | Changed `object sender` to `object? sender` |
| CS0618 - Application.MainPage | ✅ FIXED | 0 | Migrated to AppShell with CreateWindow |
| CS0618 - Device.StartTimer | ✅ FIXED | 0 | Changed to `Dispatcher.StartTimer()` |
| CS0618 - TableView/TextCell | ✅ FIXED | 0 | Migrated to CollectionView |
| CS0618 - SKPaint text APIs | ⏳ PENDING | ~200 | 40 files need SKFont migration |
| CS0618 - Frame in Styles.xaml | ⏳ PENDING | ~30 | Default MAUI template styles |
| CS0618 - ListView in Styles.xaml | ⏳ PENDING | ~10 | Default MAUI template styles |
| CS8600 - Null conversion | ⏳ PENDING | ~16 | Minor null handling issues |

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

---

## ⏳ Remaining Warnings (275)

### Priority 1: SKPaint Text APIs (~200 warnings, 40 files)

Affects demo pages that render text. Requires migrating from `SKPaint` text properties to `SKFont`.

| Obsolete API | Replacement |
|--------------|-------------|
| `SKPaint.TextSize` | `SKFont.Size` |
| `SKPaint.Typeface` | `SKFont.Typeface` |
| `SKPaint.MeasureText()` | `SKFont.MeasureText()` |
| `SKPaint.GetTextPath()` | `SKFont.GetTextPath()` |
| `SKPaint.TextAlign` | Pass `SKTextAlign` to DrawText |
| `SKCanvas.DrawText(s, x, y, paint)` | `SKCanvas.DrawText(s, x, y, align, font, paint)` |

**Fix Pattern:**
```csharp
// Before (SkiaSharp 2.x)
using var paint = new SKPaint { TextSize = 40 };
canvas.DrawText("Hello", x, y, paint);

// After (SkiaSharp 3.x)
using var paint = new SKPaint();
using var font = new SKFont { Size = 40 };
canvas.DrawText("Hello", x, y, SKTextAlign.Left, font, paint);
```

### Priority 2: Styles.xaml (~40 warnings)

The default MAUI Styles.xaml contains styles for obsolete Frame and ListView.

**Options:**
1. Remove unused Frame/ListView styles
2. Suppress warnings for this file:
   ```xml
   <MauiXaml Update="Resources/Styles/Styles.xaml" Inflator="SourceGen" NoWarn="0618" />
   ```

### Priority 3: Null Conversion (~16 warnings)

Minor CS8600 warnings in a few files. Low priority.

---

## Build History

| Date | Warnings | Errors | Notes |
|------|----------|--------|-------|
| Initial | 2226 | 0 | After XAML SourceGen enabled |
| After TableView→CollectionView | 275 | 0 | 88% reduction |

---

## Commands

```bash
# Force rebuild and count warnings
cd samples/DocsSamplesApp
touch DocsSamplesApp/DocsSamplesApp.csproj
dotnet build -f net10.0-maccatalyst 2>&1 | tail -5

# Build with warnings as errors (goal)
dotnet build -f net10.0-maccatalyst /p:TreatWarningsAsErrors=true
```
