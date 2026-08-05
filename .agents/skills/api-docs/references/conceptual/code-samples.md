# Code samples in conceptual articles

Readers copy code even when an article calls it illustrative. Make each block explicit about its purpose
and honest about what it omits.

## Choose a snippet or a sample

| Form | Use for | Quality bar |
|---|---|---|
| Focused snippet | One API call, decision, or local pattern | Short, exact, declares the relevant values, and names omitted setup |
| Complete sample | A workflow the reader is expected to run | Buildable, all consequential control flow present, expected result stated |
| Pseudocode | Host- or API-specific scaffolding that cannot be portable | Labeled as pseudocode; do not use a `csharp` fence if it is not valid C# |

Prefer a repository sample or test as the source for complete code. This docset currently uses inline
fenced code rather than Microsoft Learn's `:::code source=...:::` extraction, so inline examples can
drift. Compare them with the nearest test/renderer and compile a focused complete sample when practical.

## Verify from source

For every C# block:

1. Confirm type names, property capitalization, overloads, argument order, and return types in current
   source.
2. Confirm every variable is declared or explicitly identified as host-provided.
3. Check [`../obsolete-api-map.md`](../obsolete-api-map.md) and source attributes.
4. Confirm platform-only APIs are shown in the correct target context.
5. State whether the block is complete or what setup it intentionally omits.

Do not use ellipses where omitted code controls ownership, failure handling, synchronization, or cleanup.

## Model failure honestly

A complete workflow should:

- Check nullable factories before dereferencing the result.
- Check `bool`, enum, and status results when failure changes the outcome.
- Catch only specific exceptions the example can recover from.
- Surface unrecoverable failures instead of silently returning success-shaped data.
- Show an expected result or verification so readers know the code worked.

Avoid broad `catch (Exception)` and `catch (SystemException)` examples. They hide the actual contract and
teach readers to discard actionable failures.

## Model SkiaSharp ownership and lifetime

Use the authoritative caller-owned versus parent-owned table in
[`../skia-patterns.md`](../skia-patterns.md); do not infer ownership from whether a managed type implements
`IDisposable`.

- Keep backing memory, native devices/contexts, and delegates alive for the full native use.
- Never pass a managed pointer beyond its pinning scope.
- Drain or synchronize queued GPU work before releasing resources when the backend contract requires it.
- Check same-instance returns before disposing an input that may also be the result.
- Keep a graphics context current for calls and cleanup only on backends that require it; do not
  generalize an OpenGL rule to Vulkan, Metal, or Direct3D.

When the lifecycle is the lesson, prefer a slightly longer correct example over a short example that
leaks or races.

## Model asynchronous and callback-only results

Show:

1. Who initiates the operation.
2. What drives completion.
3. The lifetime of callback parameters.
4. What data must be copied before the callback returns.
5. How cancellation, timeout, or failure is reported.
6. When resources can be released.

Do not imply that polling a fixed number of times guarantees completion unless the API contract says so.
For production loops, explain the host's scheduling or timeout policy rather than presenting a magic
iteration count as universal.

## Show intentionally incorrect code safely

When a troubleshooting or migration article needs a bad example:

- Introduce it in prose as incorrect.
- Mark it inside the code block with a comment such as `// Incorrect: ...`.
- Keep dangerous or non-compiling lines commented out when readers may copy the block wholesale.
- Follow it immediately with the corrected pattern and explain the behavioral difference.

Never rely on a heading such as "Before" alone to signal that code is unsafe or obsolete.

## Validate

Follow [`validation.md`](validation.md). At minimum:

- Compile complete examples or match them line by line to a compiling repository sample/test.
- Verify illustrative SkiaSharp calls against source.
- Run the example when the article promises runtime output and the current host supports it.
- State when platform hardware prevents execution and what evidence substituted for it.
