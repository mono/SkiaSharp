# DocsSamplesApp Warnings Inventory

This document catalogs all compiler warnings in the DocsSamplesApp project after the migration to .NET 10 and SkiaSharp 3.119.1.

**Last Updated**: January 2026

---

## Summary

| Category | Status | Count | Notes |
|----------|--------|-------|-------|
| CS8622 - Event handler nullability | ✅ FIXED | 0 | Changed `object sender` to `object? sender` |
| CS0618 - Application.MainPage obsolete | ✅ FIXED | 0 | Migrated to AppShell with CreateWindow |
| CS0618 - Device.StartTimer obsolete | ✅ FIXED | 0 | Changed to `Dispatcher.StartTimer()` |
| CS0618 - SKPaint text APIs | ⏳ PENDING | ~200 | Requires SKFont migration |
| CS0618 - TableView/TextCell obsolete | ⏳ PENDING | ~1700 | Navigation pages use deprecated controls |
| CS0618 - Frame/ListView in Styles.xaml | ⏳ PENDING | ~200 | Default MAUI template styles |
| CS8600 - Null conversion | ⏳ PENDING | ~18 | Minor null handling issues |

**Current Build Status**: 2226 warnings (with XAML SourceGen), 0 errors

> **Note**: Warning count increased significantly after enabling `MauiXamlInflator=SourceGen` because the source generator exposes warnings from XAML that were previously hidden during runtime inflation.

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

### 4. XAML Source Generation
- Enabled `MauiXamlInflator=SourceGen` for 100x faster Debug inflation

---

## ⏳ Remaining Warnings

### Priority 1: Navigation Page Modernization (~1700 warnings)

The app uses a navigation structure with **8 "Home" pages** that use obsolete `TableView` + `TextCell`:

| Page | Purpose | Warnings |
|------|---------|----------|
| `HomePage.xaml` | Main menu → 6 categories | ~100 |
| `BasicsHomePage.xaml` | Basics section menu | ~200 |
| `PathsHomePage.xaml` | Paths section menu | ~100 |
| `TransformsHomePage.xaml` | Transforms section menu | ~250 |
| `CurvesHomePage.xaml` | Curves section menu | ~250 |
| `BitmapsHomePage.xaml` | Bitmaps section menu | ~350 |
| `EffectsHomePage.xaml` | Effects section menu | ~450 |

**Current Pattern** (obsolete):
```xml
<TableView Intent="Menu">
    <TableRoot>
        <TableSection Title="Section Name">
            <TextCell Text="Demo Name"
                      Detail="Description"
                      Command="{Binding NavigateCommand}"
                      CommandParameter="{x:Type local:DemoPage}" />
        </TableSection>
    </TableRoot>
</TableView>
```

**Recommended Fix**: Migrate to `CollectionView` with grouped items:
```xml
<CollectionView ItemsSource="{Binding MenuItems}"
                IsGrouped="True"
                SelectionMode="Single"
                SelectionChanged="OnSelectionChanged">
    <CollectionView.GroupHeaderTemplate>
        <DataTemplate>
            <Label Text="{Binding Name}" FontAttributes="Bold" />
        </DataTemplate>
    </CollectionView.GroupHeaderTemplate>
    <CollectionView.ItemTemplate>
        <DataTemplate>
            <Grid Padding="10">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="Auto" />
                </Grid.RowDefinitions>
                <Label Text="{Binding Title}" FontAttributes="Bold" />
                <Label Grid.Row="1" Text="{Binding Detail}" TextColor="Gray" />
            </Grid>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

**Alternative**: Use Shell navigation with flyout menu (already have AppShell).

---

### Priority 2: Default Styles.xaml (~200 warnings)

The `Resources/Styles/Styles.xaml` file contains default MAUI template styles that reference obsolete `Frame` and `ListView` controls.

**Options**:
1. Remove unused styles for Frame/ListView
2. Replace Frame styles with Border equivalents
3. Suppress warnings for this file only:
   ```xml
   <MauiXaml Update="Resources/Styles/Styles.xaml" Inflator="SourceGen" NoWarn="0618" />
   ```

---

### Priority 3: SKPaint Text APIs (~200 warnings)

Affects ~40 demo page files. Requires migrating from `SKPaint` text properties to `SKFont`.

**This is the largest code change** and affects both samples and documentation.

| Obsolete API | Replacement |
|--------------|-------------|
| `SKPaint.TextSize` | `SKFont.Size` |
| `SKPaint.Typeface` | `SKFont.Typeface` |
| `SKPaint.MeasureText()` | `SKFont.MeasureText()` |
| `SKPaint.GetTextPath()` | `SKFont.GetTextPath()` |
| `SKCanvas.DrawText(s, x, y, paint)` | `SKCanvas.DrawText(s, x, y, align, font, paint)` |

---

### Priority 4: Minor Null Warnings (~18 warnings)

CS8600 null conversion warnings in a few files. Low priority.

---

## Application Structure

```
DocsSamplesApp/
├── App.xaml.cs          → CreateWindow(AppShell)
├── AppShell.xaml        → Shell with HomePage
├── HomePage.xaml        → Main menu (TableView) ⚠️ OBSOLETE
├── HomeBasePage.cs      → Base class for navigation pages
│
├── Basics/              → 6 demo pages + BasicsHomePage ⚠️
├── Paths/               → 5 demo pages + PathsHomePage ⚠️
├── Transforms/          → 15 demo pages + TransformsHomePage ⚠️
├── Curves/              → 13 demo pages + CurvesHomePage ⚠️
├── Bitmaps/             → 16 demo pages + BitmapsHomePage ⚠️
├── Effects/             → 19 demo pages + EffectsHomePage ⚠️
│
└── Resources/
    └── Styles/Styles.xaml  → Default MAUI styles ⚠️
```

**Page Types**:
- **Navigation Pages** (8): Use `TableView`/`TextCell` - OBSOLETE, causing ~1700 warnings
- **Demo Pages** (74): Use `SKCanvasView` for SkiaSharp demos - ~200 SKPaint text warnings

---

## Recommended Next Steps

### Option A: Quick Fix (Suppress Navigation Warnings)
1. Add `NoWarn="0618"` to navigation XAML files
2. Focus on fixing SKPaint text APIs in demo pages
3. Defer navigation modernization

### Option B: Full Modernization (Recommended)
1. **Migrate navigation to Shell flyout** - Replace TableView menus with Shell.FlyoutItems
2. **Clean up Styles.xaml** - Remove Frame/ListView styles or replace with modern equivalents
3. **Migrate SKPaint text** - Update demo pages to use SKFont
4. **Update documentation** - Ensure docs match new code patterns

### Option C: Hybrid Approach
1. Suppress Styles.xaml warnings (template file, low value to fix)
2. Migrate navigation to CollectionView (keeps current UX, removes warnings)
3. Migrate SKPaint text APIs (required for SkiaSharp 3.x best practices)

---

## Commands

```bash
# Build and count warnings
cd samples/DocsSamplesApp
dotnet build -f net10.0-maccatalyst 2>&1 | grep -c "warning CS"

# Build with warnings as errors (goal)
dotnet build -f net10.0-maccatalyst /p:TreatWarningsAsErrors=true

# Suppress specific XAML file warnings
# Add to csproj:
# <MauiXaml Update="HomePage.xaml" Inflator="SourceGen" NoWarn="0618" />
```
