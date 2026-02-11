using System.Text.Json;

namespace SkiaSharp.Triage.Models;

/// <summary>
/// Converts enum values to their JSON string representation (kebab-case/snake_case).
/// </summary>
public static class TriageEnumExtensions
{
    private static readonly Dictionary<Type, Dictionary<object, string>> _cache = [];

    /// <summary>
    /// Returns the JSON string name for an enum value (e.g., SuggestedAction.NeedsInvestigation → "needs-investigation").
    /// </summary>
    public static string ToJsonString<T>(this T value) where T : struct, Enum
    {
        var type = typeof(T);
        if (!_cache.TryGetValue(type, out var map))
        {
            map = [];
            foreach (var field in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            {
                var attr = field.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonStringEnumMemberNameAttribute), false)
                    .FirstOrDefault() as System.Text.Json.Serialization.JsonStringEnumMemberNameAttribute;
                var enumVal = (Enum)field.GetValue(null)!;
                map[enumVal] = attr?.Name ?? field.Name;
            }
            _cache[type] = map;
        }
        return map.TryGetValue(value, out var name) ? name : value.ToString();
    }
}
