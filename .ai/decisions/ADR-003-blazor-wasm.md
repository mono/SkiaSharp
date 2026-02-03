# ADR-003: Blazor WebAssembly Framework

## Status
**Accepted** - 2026-02-03

## Context
We needed to choose a frontend framework for the dashboard application.

## Decision
Use **Blazor WebAssembly** (standalone, no server-side).

## Rationale
- **C# ecosystem**: Consistent with SkiaSharp (a C#/.NET library)
- **No server required**: Runs entirely in the browser, suitable for GitHub Pages
- **Strong typing**: Full C# type safety in the frontend
- **Component model**: Reusable UI components
- **Future potential**: Could use SkiaSharp itself for visualizations

## Consequences

### Positive
- Familiar to .NET developers who contribute to SkiaSharp
- No JavaScript required (though interop is available)
- Strong tooling (Visual Studio, VS Code, Rider)
- Shared models between collectors and UI

### Negative
- Large initial download (~2-3MB for .NET runtime)
- Slower startup than pure JavaScript
- Limited browser support (requires WASM)
- Smaller ecosystem than React/Vue

## Configuration
```xml
<TargetFramework>net10.0</TargetFramework>
<LangVersion>latest</LangVersion>  <!-- C# 14 -->
```

## Alternatives Considered

1. **React/Vue/Angular**: Large ecosystems but JavaScript-based
2. **Blazor Server**: Requires server, not suitable for GitHub Pages
3. **Static HTML + JS**: Simple but lacks component model
4. **MAUI Blazor Hybrid**: Overkill for web-only dashboard

## Related
- Tech Context: `.ai/techContext.md`
