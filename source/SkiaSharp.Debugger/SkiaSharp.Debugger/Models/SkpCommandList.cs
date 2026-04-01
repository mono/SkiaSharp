using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkiaSharp.Debugger.Models
{
    public class SkpCommandList
    {
        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("commands")]
        public List<SkpCommand> Commands { get; set; } = new();
    }

    public class SkpCommand
    {
        [JsonPropertyName("command")]
        public string Command { get; set; } = string.Empty;

        [JsonPropertyName("visible")]
        public bool Visible { get; set; } = true;

        [JsonPropertyName("shortDesc")]
        public string? ShortDesc { get; set; }

        [JsonPropertyName("layerNodeId")]
        public int? LayerNodeId { get; set; }

        [JsonPropertyName("imageIndex")]
        public int? ImageIndex { get; set; }

        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("auditTrail")]
        public SkpAuditTrail? AuditTrail { get; set; }

        // Additional properties are captured as ExtensionData
        [JsonExtensionData]
        public Dictionary<string, object>? AdditionalProperties { get; set; }
    }

    public class SkpAuditTrail
    {
        [JsonPropertyName("Ops")]
        public List<SkpGpuOp>? Ops { get; set; }
    }

    public class SkpGpuOp
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("ClientID")]
        public int ClientID { get; set; }

        [JsonPropertyName("OpsTaskID")]
        public int OpsTaskID { get; set; }

        [JsonPropertyName("ChildID")]
        public int ChildID { get; set; }
    }

    public class MatrixClipInfo
    {
        [JsonPropertyName("ClipRect")]
        public int[] ClipRect { get; set; } = new int[4];

        [JsonPropertyName("ViewMatrix")]
        public float[][] ViewMatrix { get; set; } = new float[3][];
    }
}
