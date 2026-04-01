using SkiaSharp.Debugger;
using SkiaSharp.Debugger.Models;
using System.Text.Json;
using Xunit;

namespace SkiaSharp.Debugger.Tests;

public class CommandListParsingTests
{
    private const string SampleCommandListJson = @"{
        ""version"": 1,
        ""commands"": [
            { ""command"": ""Save"", ""visible"": true },
            { ""command"": ""ClipRect"", ""visible"": true, ""shortDesc"": ""rect(0,0,100,200)"" },
            { ""command"": ""DrawRect"", ""visible"": true, ""shortDesc"": ""rect(10,20,90,180)"" },
            { ""command"": ""DrawOval"", ""visible"": true, ""shortDesc"": ""oval(30,40,70,160)"" },
            { ""command"": ""DrawPath"", ""visible"": true },
            { ""command"": ""DrawTextBlob"", ""visible"": true, ""shortDesc"": ""Hello World"" },
            { ""command"": ""DrawImage"", ""visible"": true, ""imageIndex"": 0 },
            { ""command"": ""Restore"", ""visible"": true },
            { ""command"": ""Save"", ""visible"": true },
            { ""command"": ""Concat"", ""visible"": true, ""shortDesc"": ""matrix(...)"" },
            { ""command"": ""DrawRect"", ""visible"": true, ""shortDesc"": ""rect(0,0,50,50)"" },
            { ""command"": ""DrawAnnotation"", ""visible"": true, ""key"": ""RenderNode(id=10, name='DecorView')"" },
            { ""command"": ""Restore"", ""visible"": true }
        ]
    }";

    [Fact]
    public void ParsesCommandList()
    {
        var list = JsonSerializer.Deserialize<SkpCommandList>(SampleCommandListJson);

        Assert.NotNull(list);
        Assert.Equal(1, list.Version);
        Assert.Equal(13, list.Commands.Count);
    }

    [Fact]
    public void ParsesCommandNames()
    {
        var list = JsonSerializer.Deserialize<SkpCommandList>(SampleCommandListJson)!;

        Assert.Equal("Save", list.Commands[0].Command);
        Assert.Equal("ClipRect", list.Commands[1].Command);
        Assert.Equal("DrawRect", list.Commands[2].Command);
        Assert.Equal("Restore", list.Commands[7].Command);
    }

    [Fact]
    public void ParsesShortDesc()
    {
        var list = JsonSerializer.Deserialize<SkpCommandList>(SampleCommandListJson)!;

        Assert.Null(list.Commands[0].ShortDesc);
        Assert.Equal("rect(0,0,100,200)", list.Commands[1].ShortDesc);
    }

    [Fact]
    public void ParsesImageIndex()
    {
        var list = JsonSerializer.Deserialize<SkpCommandList>(SampleCommandListJson)!;

        Assert.Null(list.Commands[0].ImageIndex);
        Assert.Equal(0, list.Commands[6].ImageIndex);
    }

    [Fact]
    public void ParsesAnnotationKey()
    {
        var list = JsonSerializer.Deserialize<SkpCommandList>(SampleCommandListJson)!;

        Assert.Equal("RenderNode(id=10, name='DecorView')", list.Commands[11].Key);
    }

    [Fact]
    public void ParsesVisibility()
    {
        var list = JsonSerializer.Deserialize<SkpCommandList>(SampleCommandListJson)!;

        Assert.True(list.Commands[0].Visible);
    }

    [Fact]
    public void ParsesMatrixClipInfo()
    {
        var json = @"{
            ""ClipRect"": [0, 0, 1080, 1920],
            ""ViewMatrix"": [[1, 0, 0], [0, 1, 0], [0, 0, 1]]
        }";

        var info = JsonSerializer.Deserialize<MatrixClipInfo>(json);

        Assert.NotNull(info);
        Assert.Equal(4, info.ClipRect.Length);
        Assert.Equal(0, info.ClipRect[0]);
        Assert.Equal(1920, info.ClipRect[3]);
        Assert.Equal(3, info.ViewMatrix.Length);
        Assert.Equal(1f, info.ViewMatrix[0][0]);
    }

    [Fact]
    public void HandlesEmptyCommandList()
    {
        var json = @"{ ""version"": 1, ""commands"": [] }";
        var list = JsonSerializer.Deserialize<SkpCommandList>(json);

        Assert.NotNull(list);
        Assert.Empty(list.Commands);
    }

    [Fact]
    public void HandlesExtraJsonProperties()
    {
        var json = @"{
            ""version"": 1,
            ""commands"": [
                { ""command"": ""DrawRect"", ""visible"": true, ""coords"": { ""x"": 10, ""y"": 20 }, ""color"": ""#FF0000"" }
            ]
        }";
        var list = JsonSerializer.Deserialize<SkpCommandList>(json);

        Assert.NotNull(list);
        Assert.Single(list.Commands);
        Assert.NotNull(list.Commands[0].AdditionalProperties);
        Assert.True(list.Commands[0].AdditionalProperties!.ContainsKey("coords"));
    }

    // === ADDITIONAL JSON MODEL EDGE CASE TESTS ===

    [Fact]
    public void ParsesJsonWithMissingOptionalFields()
    {
        // Minimal JSON with only required fields
        var json = @"{
            ""version"": 1,
            ""commands"": [
                { ""command"": ""DrawRect"" }
            ]
        }";
        var list = JsonSerializer.Deserialize<SkpCommandList>(json);

        Assert.NotNull(list);
        Assert.Single(list.Commands);
        var cmd = list.Commands[0];
        Assert.Equal("DrawRect", cmd.Command);
        Assert.True(cmd.Visible); // Default value
        Assert.Null(cmd.ShortDesc);
        Assert.Null(cmd.ImageIndex);
        Assert.Null(cmd.Key);
        Assert.Null(cmd.AuditTrail);
        Assert.Null(cmd.LayerNodeId);
    }

    [Fact]
    public void ParsesJsonWithNullShortDesc()
    {
        var json = @"{
            ""version"": 1,
            ""commands"": [
                { ""command"": ""Save"", ""visible"": true, ""shortDesc"": null }
            ]
        }";
        var list = JsonSerializer.Deserialize<SkpCommandList>(json);

        Assert.NotNull(list);
        Assert.Single(list.Commands);
        Assert.Null(list.Commands[0].ShortDesc);
    }

    [Fact]
    public void ParsesCommandWithAuditTrailOpsArray()
    {
        var json = @"{
            ""version"": 1,
            ""commands"": [
                {
                    ""command"": ""DrawRect"",
                    ""visible"": true,
                    ""auditTrail"": {
                        ""Ops"": [
                            { ""Name"": ""GrFillRectOp"", ""ClientID"": 1, ""OpsTaskID"": 2, ""ChildID"": 0 },
                            { ""Name"": ""GrStrokeRectOp"", ""ClientID"": 2, ""OpsTaskID"": 3, ""ChildID"": 1 }
                        ]
                    }
                }
            ]
        }";
        var list = JsonSerializer.Deserialize<SkpCommandList>(json);

        Assert.NotNull(list);
        Assert.Single(list.Commands);
        var cmd = list.Commands[0];
        Assert.NotNull(cmd.AuditTrail);
        Assert.NotNull(cmd.AuditTrail!.Ops);
        Assert.Equal(2, cmd.AuditTrail.Ops!.Count);
        
        Assert.Equal("GrFillRectOp", cmd.AuditTrail.Ops[0].Name);
        Assert.Equal(1, cmd.AuditTrail.Ops[0].ClientID);
        Assert.Equal(2, cmd.AuditTrail.Ops[0].OpsTaskID);
        Assert.Equal(0, cmd.AuditTrail.Ops[0].ChildID);
        
        Assert.Equal("GrStrokeRectOp", cmd.AuditTrail.Ops[1].Name);
    }

    [Fact]
    public void ParsesMatrixClipInfoWith4x4Matrix()
    {
        // 4x4 matrix (perspective transformation)
        var json = @"{
            ""ClipRect"": [0, 0, 1920, 1080],
            ""ViewMatrix"": [
                [1, 0, 0, 0],
                [0, 1, 0, 0],
                [0, 0, 1, 0],
                [100, 200, 0, 1]
            ]
        }";
        var info = JsonSerializer.Deserialize<MatrixClipInfo>(json);

        Assert.NotNull(info);
        Assert.Equal(4, info.ClipRect.Length);
        Assert.Equal(4, info.ViewMatrix.Length);
        Assert.Equal(4, info.ViewMatrix[0].Length);
        Assert.Equal(100f, info.ViewMatrix[3][0]);
        Assert.Equal(200f, info.ViewMatrix[3][1]);
    }

    [Fact]
    public void Roundtrip_SerializeThenDeserialize()
    {
        var original = new SkpCommandList
        {
            Version = 1,
            Commands = new()
            {
                new() { Command = "Save", Visible = true },
                new() { Command = "DrawRect", Visible = true, ShortDesc = "rect(10,20,30,40)" },
                new() { Command = "DrawCircle", Visible = true, ShortDesc = "circle(50,60,70)" },
                new() { Command = "Restore", Visible = true }
            }
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<SkpCommandList>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.Version, deserialized.Version);
        Assert.Equal(original.Commands.Count, deserialized.Commands.Count);
        
        for (int i = 0; i < original.Commands.Count; i++)
        {
            Assert.Equal(original.Commands[i].Command, deserialized.Commands[i].Command);
            Assert.Equal(original.Commands[i].Visible, deserialized.Commands[i].Visible);
            Assert.Equal(original.Commands[i].ShortDesc, deserialized.Commands[i].ShortDesc);
        }
    }

    [Fact]
    public void ParseMalformedJson_Throws()
    {
        var malformedJson = @"{ ""version"": 1, ""commands"": [ { command: ""Save"" } ] }"; // Missing quotes around command

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<SkpCommandList>(malformedJson));
    }

    [Fact]
    public void ParseJsonWhereVisibleIsFalse()
    {
        var json = @"{
            ""version"": 1,
            ""commands"": [
                { ""command"": ""Save"", ""visible"": true },
                { ""command"": ""DrawRect"", ""visible"": false },
                { ""command"": ""Restore"", ""visible"": true }
            ]
        }";
        var list = JsonSerializer.Deserialize<SkpCommandList>(json);

        Assert.NotNull(list);
        Assert.Equal(3, list.Commands.Count);
        Assert.True(list.Commands[0].Visible);
        Assert.False(list.Commands[1].Visible);
        Assert.True(list.Commands[2].Visible);
    }

    [Fact]
    public void ParsesGpuOpModel()
    {
        var json = @"{
            ""Name"": ""GrDrawOp"",
            ""ClientID"": 42,
            ""OpsTaskID"": 7,
            ""ChildID"": 3
        }";
        var op = JsonSerializer.Deserialize<SkpGpuOp>(json);

        Assert.NotNull(op);
        Assert.Equal("GrDrawOp", op.Name);
        Assert.Equal(42, op.ClientID);
        Assert.Equal(7, op.OpsTaskID);
        Assert.Equal(3, op.ChildID);
    }

    [Fact]
    public void ParsesLayerNodeId()
    {
        var json = @"{
            ""version"": 1,
            ""commands"": [
                { ""command"": ""DrawRect"", ""visible"": true, ""layerNodeId"": 123 }
            ]
        }";
        var list = JsonSerializer.Deserialize<SkpCommandList>(json);

        Assert.NotNull(list);
        Assert.Equal(123, list.Commands[0].LayerNodeId);
    }

    [Fact]
    public void ParsesComplexNestedJson()
    {
        var json = @"{
            ""version"": 2,
            ""commands"": [
                {
                    ""command"": ""DrawImage"",
                    ""visible"": true,
                    ""shortDesc"": ""image at (0,0)"",
                    ""imageIndex"": 5,
                    ""layerNodeId"": 42,
                    ""auditTrail"": {
                        ""Ops"": [
                            { ""Name"": ""GrTextureOp"", ""ClientID"": 10, ""OpsTaskID"": 20, ""ChildID"": 30 }
                        ]
                    },
                    ""extraData"": { ""format"": ""RGBA8888"", ""size"": [256, 256] }
                }
            ]
        }";
        var list = JsonSerializer.Deserialize<SkpCommandList>(json);

        Assert.NotNull(list);
        Assert.Equal(2, list.Version);
        var cmd = list.Commands[0];
        Assert.Equal("DrawImage", cmd.Command);
        Assert.Equal(5, cmd.ImageIndex);
        Assert.Equal(42, cmd.LayerNodeId);
        Assert.NotNull(cmd.AuditTrail);
        Assert.Single(cmd.AuditTrail!.Ops!);
        Assert.NotNull(cmd.AdditionalProperties);
        Assert.True(cmd.AdditionalProperties!.ContainsKey("extraData"));
    }

    [Fact]
    public void ParsesMatrixClipInfoWithScaledTransform()
    {
        var json = @"{
            ""ClipRect"": [10, 20, 500, 800],
            ""ViewMatrix"": [[2, 0, 50], [0, 2, 100], [0, 0, 1]]
        }";
        var info = JsonSerializer.Deserialize<MatrixClipInfo>(json);

        Assert.NotNull(info);
        Assert.Equal(10, info.ClipRect[0]);
        Assert.Equal(20, info.ClipRect[1]);
        Assert.Equal(500, info.ClipRect[2]);
        Assert.Equal(800, info.ClipRect[3]);
        Assert.Equal(2f, info.ViewMatrix[0][0]); // ScaleX
        Assert.Equal(2f, info.ViewMatrix[1][1]); // ScaleY
        Assert.Equal(50f, info.ViewMatrix[0][2]); // TranslateX
        Assert.Equal(100f, info.ViewMatrix[1][2]); // TranslateY
    }

    [Fact]
    public void ParsesEmptyAuditTrail()
    {
        var json = @"{
            ""version"": 1,
            ""commands"": [
                {
                    ""command"": ""DrawRect"",
                    ""visible"": true,
                    ""auditTrail"": { ""Ops"": [] }
                }
            ]
        }";
        var list = JsonSerializer.Deserialize<SkpCommandList>(json);

        Assert.NotNull(list);
        var cmd = list.Commands[0];
        Assert.NotNull(cmd.AuditTrail);
        Assert.NotNull(cmd.AuditTrail!.Ops);
        Assert.Empty(cmd.AuditTrail.Ops);
    }

    [Fact]
    public void ParsesAuditTrailWithNullOps()
    {
        var json = @"{
            ""version"": 1,
            ""commands"": [
                {
                    ""command"": ""DrawRect"",
                    ""visible"": true,
                    ""auditTrail"": { ""Ops"": null }
                }
            ]
        }";
        var list = JsonSerializer.Deserialize<SkpCommandList>(json);

        Assert.NotNull(list);
        var cmd = list.Commands[0];
        Assert.NotNull(cmd.AuditTrail);
        Assert.Null(cmd.AuditTrail!.Ops);
    }

    [Fact]
    public void Roundtrip_CommandWithAllProperties()
    {
        var original = new SkpCommand
        {
            Command = "DrawImage",
            Visible = true,
            ShortDesc = "image(100,200)",
            ImageIndex = 3,
            LayerNodeId = 99,
            Key = "test-key",
            AuditTrail = new SkpAuditTrail
            {
                Ops = new List<SkpGpuOp>
                {
                    new() { Name = "Op1", ClientID = 1, OpsTaskID = 2, ChildID = 3 }
                }
            }
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<SkpCommand>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.Command, deserialized.Command);
        Assert.Equal(original.Visible, deserialized.Visible);
        Assert.Equal(original.ShortDesc, deserialized.ShortDesc);
        Assert.Equal(original.ImageIndex, deserialized.ImageIndex);
        Assert.Equal(original.LayerNodeId, deserialized.LayerNodeId);
        Assert.Equal(original.Key, deserialized.Key);
        Assert.NotNull(deserialized.AuditTrail);
        Assert.Single(deserialized.AuditTrail!.Ops!);
        Assert.Equal("Op1", deserialized.AuditTrail.Ops![0].Name);
    }

    [Fact]
    public void ParsesVersionZero()
    {
        var json = @"{ ""version"": 0, ""commands"": [] }";
        var list = JsonSerializer.Deserialize<SkpCommandList>(json);

        Assert.NotNull(list);
        Assert.Equal(0, list.Version);
    }

    [Fact]
    public void ParsesLargeVersion()
    {
        var json = @"{ ""version"": 999, ""commands"": [] }";
        var list = JsonSerializer.Deserialize<SkpCommandList>(json);

        Assert.NotNull(list);
        Assert.Equal(999, list.Version);
    }

    [Fact]
    public void JsonPropertyNames_AreCamelCase()
    {
        var list = new SkpCommandList
        {
            Version = 1,
            Commands = new()
            {
                new() { Command = "Save", Visible = true, ShortDesc = "test" }
            }
        };

        var json = JsonSerializer.Serialize(list);

        Assert.Contains("\"version\"", json);
        Assert.Contains("\"commands\"", json);
        Assert.Contains("\"command\"", json);
        Assert.Contains("\"visible\"", json);
        Assert.Contains("\"shortDesc\"", json);
    }

    [Fact]
    public void ParsesJsonWithWhitespace()
    {
        var json = @"
        
        {
            ""version""   :   1  ,
            ""commands""  :   [
                {   ""command""  :  ""Save""  ,  ""visible""  :  true   }
            ]
        }
        
        ";
        var list = JsonSerializer.Deserialize<SkpCommandList>(json);

        Assert.NotNull(list);
        Assert.Equal(1, list.Version);
        Assert.Single(list.Commands);
    }
}
