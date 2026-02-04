using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkiaSharp.Collector.Services;

/// <summary>
/// Service for writing JSON output files.
/// </summary>
public static class OutputService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static async Task WriteJsonAsync<T>(string path, T data)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(data, JsonOptions);
        await File.WriteAllTextAsync(path, json);
    }

    public static string GetOutputPath(string outputDir, string filename)
    {
        return Path.Combine(outputDir, filename);
    }
}
