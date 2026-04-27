# Sample Scout Report Schema (v1)

## Top-Level Structure

```json
{
  "meta": { ... },
  "summary": { ... },
  "findings": [ ... ]
}
```

## `meta`

| Field | Required | Description |
|-------|----------|-------------|
| `date` | Yes | YYYY-MM-DD |
| `schemaVersion` | Yes | Must be `"1.0"` |
| `source` | Yes | e.g., `"google/skia/gm/"` |
| `totalFiles` | Yes | Number of .cpp files in gm/ |
| `existingGallerySamples` | No | Number of existing SkiaSharp Gallery samples |

## `summary`

| Field | Required | Description |
|-------|----------|-------------|
| `totalFindings` | Yes | Number of GM files analyzed |
| `byInterest` | No | `{"high": N, "medium": N, "low": N}` |
| `byApiAvailability` | No | `{"available": N, "blocked": N}` |
| `bySampleStatus` | No | `{"none": N, "similar": N, "existing": N}` |
| `opportunities` | No | Count of high + APIs ready + no existing sample |

## `findings` — Array of GM Analyses

### Required Fields

| Field | Type | Description |
|-------|------|-------------|
| `file` | string | GM filename (e.g., `"mesh.cpp"`) |
| `name` | string | Human-readable name |
| `description` | string | What the sample demonstrates |
| `interesting` | string | `"high"`, `"medium"`, or `"low"` |
| `apis_available` | boolean | Whether all required APIs exist in SkiaSharp |
| `missing_apis` | string[] | APIs not available (empty if all available) |
| `key_apis` | string[] | Main Skia APIs used |

### Optional Fields

| Field | Type | Description |
|-------|------|-------------|
| `notes` | string | Context (GPU-only, bug regression, etc.) |
| `sampleStatus` | string | `"none"`, `"similar"`, or `"existing"` — Gallery coverage |
| `matchedSample` | string | Name of matching Gallery sample if similar/existing |
