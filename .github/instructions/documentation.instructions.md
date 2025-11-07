---
applyTo: "design/**/*.md,*.md,!node_modules/**,!externals/**"
---

# Documentation Instructions

You are working on project documentation.

## Documentation Standards

- Use clear, concise language
- Include code examples where helpful
- Document memory management and ownership
- Explain pointer type implications
- Cover error handling patterns
- Optimize for AI readability

## Code Examples Best Practices

### Always Show Disposal
```csharp
// ✅ Good - proper disposal
using (var paint = new SKPaint())
{
    paint.Color = SKColors.Red;
}
```

### Include Error Handling
```csharp
if (string.IsNullOrEmpty(path))
    throw new ArgumentException("Path cannot be null or empty");
```

### Show Complete Context
Include all necessary using statements and complete, runnable examples.

## Structure Guidelines

- Use clear headings
- Include diagrams where helpful (ASCII, Mermaid)
- Provide complete examples through all layers
- Cross-reference related documents

## What NOT to Document

- Exhaustive API lists (use XML comments instead)
- Implementation details (focus on concepts)
- Temporary workarounds
- Platform-specific details (unless critical)
