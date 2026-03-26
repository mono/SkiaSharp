using SkiaSharp.Debugger;
using SkiaSharp.Debugger.Models;
using Xunit;

namespace SkiaSharp.Debugger.Tests;

/// <summary>
/// Tests for demo scene patterns and command list rendering behavior.
/// These tests mirror the functionality of DemoScenes from the Blazor app.
/// </summary>
public class DemoScenesTests
{
    private static SkpCommandList CreateShapesScene()
    {
        return new SkpCommandList
        {
            Version = 1,
            Commands = new()
            {
                new() { Command = "Clear", ShortDesc = "white", Visible = true },
                new() { Command = "Save", Visible = true },
                new() { Command = "DrawRect", ShortDesc = "rect(50,50,250,150) Blue", Visible = true },
                new() { Command = "DrawRect", ShortDesc = "rect(100,80,280,200) Red α128", Visible = true },
                new() { Command = "DrawOval", ShortDesc = "oval(300,100,200,150) Green", Visible = true },
                new() { Command = "DrawCircle", ShortDesc = "circle(200,350,80) Purple", Visible = true },
                new() { Command = "DrawLine", ShortDesc = "line(50,450,550,450) Black w=3", Visible = true },
                new() { Command = "DrawRoundRect", ShortDesc = "rrect(350,250,200,150,20,20) Orange", Visible = true },
                new() { Command = "DrawPath", ShortDesc = "star Teal stroke", Visible = true },
                new() { Command = "Restore", Visible = true },
            }
        };
    }

    private static SkpCommandList CreateTransformsScene()
    {
        return new SkpCommandList
        {
            Version = 1,
            Commands = new()
            {
                new() { Command = "Clear", ShortDesc = "white", Visible = true },
                new() { Command = "Save", Visible = true },
                new() { Command = "Concat", ShortDesc = "translate(200,200)", Visible = true },
                new() { Command = "DrawRect", ShortDesc = "rect(−50,−50,100,100) Blue", Visible = true },
                new() { Command = "Save", Visible = true },
                new() { Command = "Concat", ShortDesc = "rotate(45°)", Visible = true },
                new() { Command = "DrawRect", ShortDesc = "rect(−40,−40,80,80) Red α128", Visible = true },
                new() { Command = "Restore", Visible = true },
                new() { Command = "Save", Visible = true },
                new() { Command = "Concat", ShortDesc = "translate(200,0) scale(1.5)", Visible = true },
                new() { Command = "DrawOval", ShortDesc = "oval(−30,−20,60,40) Green", Visible = true },
                new() { Command = "Restore", Visible = true },
                new() { Command = "Save", Visible = true },
                new() { Command = "Concat", ShortDesc = "translate(0,150) skew(0.3,0)", Visible = true },
                new() { Command = "DrawRect", ShortDesc = "rect(−50,−25,100,50) Purple α160", Visible = true },
                new() { Command = "Restore", Visible = true },
                new() { Command = "Restore", Visible = true },
            }
        };
    }

    private static SkpCommandList CreateClippingScene()
    {
        return new SkpCommandList
        {
            Version = 1,
            Commands = new()
            {
                new() { Command = "Clear", ShortDesc = "white", Visible = true },
                new() { Command = "Save", Visible = true },
                new() { Command = "ClipRect", ShortDesc = "rect(50,50,400,350) intersect", Visible = true },
                new() { Command = "DrawRect", ShortDesc = "rect(0,0,600,500) LightBlue (clipped)", Visible = true },
                new() { Command = "Save", Visible = true },
                new() { Command = "ClipPath", ShortDesc = "circle path r=100 intersect", Visible = true },
                new() { Command = "DrawRect", ShortDesc = "rect(0,0,600,500) Coral (double clipped)", Visible = true },
                new() { Command = "DrawCircle", ShortDesc = "circle(225,200,80) DarkRed", Visible = true },
                new() { Command = "Restore", Visible = true },
                new() { Command = "DrawRect", ShortDesc = "rect(200,150,200,100) Green (rect clipped only)", Visible = true },
                new() { Command = "Restore", Visible = true },
            }
        };
    }

    private static SkpCommandList CreateTextScene()
    {
        return new SkpCommandList
        {
            Version = 1,
            Commands = new()
            {
                new() { Command = "Clear", ShortDesc = "white", Visible = true },
                new() { Command = "Save", Visible = true },
                new() { Command = "DrawTextBlob", ShortDesc = "\"SkiaSharp Debugger\" (100,60) size=32", Visible = true },
                new() { Command = "DrawTextBlob", ShortDesc = "\"Blazor WASM\" (100,100) size=24 Blue", Visible = true },
                new() { Command = "DrawPath", ShortDesc = "bezier curve Teal w=3", Visible = true },
                new() { Command = "DrawPath", ShortDesc = "rounded rect path DarkOrange filled", Visible = true },
                new() { Command = "DrawTextBlob", ShortDesc = "\"Inside rounded rect\" white (100,330)", Visible = true },
                new() { Command = "Save", Visible = true },
                new() { Command = "Concat", ShortDesc = "translate(400,300)", Visible = true },
                new() { Command = "DrawPath", ShortDesc = "gradient arc MediumPurple w=5", Visible = true },
                new() { Command = "Restore", Visible = true },
                new() { Command = "Restore", Visible = true },
            }
        };
    }

    [Theory]
    [InlineData("shapes")]
    [InlineData("transforms")]
    [InlineData("clipping")]
    [InlineData("text")]
    public void DemoScene_CreatesNonEmptyCommandList(string sceneName)
    {
        var commandList = sceneName switch
        {
            "shapes" => CreateShapesScene(),
            "transforms" => CreateTransformsScene(),
            "clipping" => CreateClippingScene(),
            "text" => CreateTextScene(),
            _ => CreateShapesScene()
        };

        Assert.NotNull(commandList);
        Assert.True(commandList.Commands.Count > 0, $"Scene '{sceneName}' should have commands");
    }

    [Theory]
    [InlineData("shapes")]
    [InlineData("transforms")]
    [InlineData("clipping")]
    [InlineData("text")]
    public void DemoScene_LoadsIntoDebuggerState(string sceneName)
    {
        var commandList = sceneName switch
        {
            "shapes" => CreateShapesScene(),
            "transforms" => CreateTransformsScene(),
            "clipping" => CreateClippingScene(),
            "text" => CreateTextScene(),
            _ => CreateShapesScene()
        };

        var state = new DebuggerState();
        state.LoadCommandList(commandList);

        Assert.Equal(commandList.Commands.Count, state.TotalCommandCount);
        Assert.Equal(commandList.Commands.Count, state.FilteredCommandCount);
    }

    [Theory]
    [InlineData("shapes")]
    [InlineData("transforms")]
    [InlineData("clipping")]
    [InlineData("text")]
    public void DemoScene_AllCommandsHaveValidNames(string sceneName)
    {
        var commandList = sceneName switch
        {
            "shapes" => CreateShapesScene(),
            "transforms" => CreateTransformsScene(),
            "clipping" => CreateClippingScene(),
            "text" => CreateTextScene(),
            _ => CreateShapesScene()
        };

        // Valid Skia command names
        var validCommandNames = new HashSet<string>
        {
            "Clear", "Save", "Restore", "Concat",
            "ClipRect", "ClipPath", "ClipRRect",
            "DrawRect", "DrawOval", "DrawCircle", "DrawLine",
            "DrawPath", "DrawRoundRect", "DrawTextBlob",
            "DrawImage", "DrawImageRect", "DrawAnnotation",
            "DrawPoints", "DrawArc"
        };

        foreach (var cmd in commandList.Commands)
        {
            Assert.False(string.IsNullOrEmpty(cmd.Command), "Command name should not be empty");
            Assert.Contains(cmd.Command, validCommandNames);
        }
    }

    [Fact]
    public void DemoScene_RenderToIndex0_SelectsFirstCommand()
    {
        var commandList = CreateShapesScene();
        var state = new DebuggerState();
        state.LoadCommandList(commandList);

        state.JumpToStart();

        Assert.Equal(0, state.SelectedCommandIndex);
        Assert.Equal(0, state.SelectedUnfilteredIndex);
        
        var cmd = state.GetCommand(state.SelectedUnfilteredIndex);
        Assert.NotNull(cmd);
        Assert.Equal("Clear", cmd!.Command);
    }

    [Fact]
    public void DemoScene_RenderToEnd_SelectsLastCommand()
    {
        var commandList = CreateShapesScene();
        var state = new DebuggerState();
        state.LoadCommandList(commandList);

        state.JumpToEnd();

        Assert.Equal(commandList.Commands.Count - 1, state.SelectedCommandIndex);
        Assert.Equal(commandList.Commands.Count - 1, state.SelectedUnfilteredIndex);
        
        var cmd = state.GetCommand(state.SelectedUnfilteredIndex);
        Assert.NotNull(cmd);
        Assert.Equal("Restore", cmd!.Command); // Last command in shapes scene
    }

    [Fact]
    public void DemoScene_ToggleCommandVisibility_ExcludesFromIteration()
    {
        var commandList = CreateShapesScene();
        var state = new DebuggerState();
        state.LoadCommandList(commandList);

        // Toggle off all DrawRect commands
        state.ToggleCommandName("DrawRect");

        // Count remaining commands
        var drawRectCount = commandList.Commands.Count(c => c.Command == "DrawRect");
        Assert.Equal(commandList.Commands.Count - drawRectCount, state.FilteredCommandCount);
        
        // Verify no DrawRect in filtered indices
        foreach (var idx in state.FilteredIndices)
        {
            var cmd = state.GetCommand(idx);
            Assert.NotEqual("DrawRect", cmd!.Command);
        }
    }

    [Fact]
    public void DemoScene_CommandVisibleProperty_InitiallyTrue()
    {
        var commandList = CreateShapesScene();

        foreach (var cmd in commandList.Commands)
        {
            Assert.True(cmd.Visible, $"Command {cmd.Command} should be visible by default");
        }
    }

    [Fact]
    public void DemoScene_CommandWithVisibleFalse_StillInDebuggerState()
    {
        var commandList = new SkpCommandList
        {
            Version = 1,
            Commands = new()
            {
                new() { Command = "Clear", Visible = true },
                new() { Command = "DrawRect", Visible = false }, // Hidden
                new() { Command = "DrawOval", Visible = true },
            }
        };

        var state = new DebuggerState();
        state.LoadCommandList(commandList);

        // All commands are still in total count
        Assert.Equal(3, state.TotalCommandCount);
        // Filtered count includes all (visible is separate from filter)
        Assert.Equal(3, state.FilteredCommandCount);

        // Command can still be accessed
        var cmd = state.GetCommand(1);
        Assert.NotNull(cmd);
        Assert.Equal("DrawRect", cmd!.Command);
        Assert.False(cmd.Visible);
    }

    [Fact]
    public void DemoScene_ContainsExpectedCommandTypes_Shapes()
    {
        var commandList = CreateShapesScene();
        var commandNames = commandList.Commands.Select(c => c.Command).ToHashSet();

        Assert.Contains("Clear", commandNames);
        Assert.Contains("Save", commandNames);
        Assert.Contains("Restore", commandNames);
        Assert.Contains("DrawRect", commandNames);
        Assert.Contains("DrawOval", commandNames);
        Assert.Contains("DrawCircle", commandNames);
        Assert.Contains("DrawPath", commandNames);
    }

    [Fact]
    public void DemoScene_ContainsExpectedCommandTypes_Transforms()
    {
        var commandList = CreateTransformsScene();
        var commandNames = commandList.Commands.Select(c => c.Command).ToHashSet();

        Assert.Contains("Concat", commandNames);
        Assert.Contains("Save", commandNames);
        Assert.Contains("Restore", commandNames);
        Assert.Contains("DrawRect", commandNames);
        Assert.Contains("DrawOval", commandNames);
    }

    [Fact]
    public void DemoScene_ContainsExpectedCommandTypes_Clipping()
    {
        var commandList = CreateClippingScene();
        var commandNames = commandList.Commands.Select(c => c.Command).ToHashSet();

        Assert.Contains("ClipRect", commandNames);
        Assert.Contains("ClipPath", commandNames);
        Assert.Contains("DrawRect", commandNames);
        Assert.Contains("DrawCircle", commandNames);
    }

    [Fact]
    public void DemoScene_ContainsExpectedCommandTypes_Text()
    {
        var commandList = CreateTextScene();
        var commandNames = commandList.Commands.Select(c => c.Command).ToHashSet();

        Assert.Contains("DrawTextBlob", commandNames);
        Assert.Contains("DrawPath", commandNames);
        Assert.Contains("Concat", commandNames);
    }

    [Fact]
    public void DemoScene_Histogram_CountsCommandTypes()
    {
        var commandList = CreateShapesScene();
        var state = new DebuggerState();
        state.LoadCommandList(commandList);

        var histogram = state.GetHistogram();

        Assert.Equal(2, histogram["DrawRect"]);
        Assert.Equal(1, histogram["Clear"]);
        Assert.Equal(1, histogram["Save"]);
        Assert.Equal(1, histogram["Restore"]);
    }

    [Fact]
    public void DemoScene_Stepping_IteratesThroughCommands()
    {
        var commandList = CreateShapesScene();
        var state = new DebuggerState();
        state.LoadCommandList(commandList);
        state.JumpToStart();

        var visitedCommands = new List<string>();
        for (int i = 0; i < commandList.Commands.Count; i++)
        {
            var cmd = state.GetCommand(state.SelectedUnfilteredIndex);
            visitedCommands.Add(cmd!.Command);
            state.StepForward();
        }

        Assert.Equal(commandList.Commands.Count, visitedCommands.Count);
        Assert.Equal("Clear", visitedCommands[0]);
        Assert.Equal("Restore", visitedCommands[^1]);
    }

    [Fact]
    public void DemoScene_FilterByDraw_ShowsOnlyDrawCommands()
    {
        var commandList = CreateShapesScene();
        var state = new DebuggerState();
        state.LoadCommandList(commandList);

        state.SetFilter("Draw");

        // All filtered commands should contain "Draw"
        foreach (var idx in state.FilteredIndices)
        {
            var cmd = state.GetCommand(idx);
            Assert.StartsWith("Draw", cmd!.Command);
        }
    }

    [Fact]
    public void DemoScene_NegativeFilter_ExcludesSaveRestore()
    {
        var commandList = CreateShapesScene();
        var state = new DebuggerState();
        state.LoadCommandList(commandList);

        state.SetFilter("!Save Restore");

        // No filtered command should be Save or Restore
        foreach (var idx in state.FilteredIndices)
        {
            var cmd = state.GetCommand(idx);
            Assert.NotEqual("Save", cmd!.Command);
            Assert.NotEqual("Restore", cmd!.Command);
        }
    }

    [Fact]
    public void DemoScene_ShortDescriptions_ProvideContext()
    {
        var commandList = CreateShapesScene();

        var commandsWithDesc = commandList.Commands.Where(c => !string.IsNullOrEmpty(c.ShortDesc)).ToList();
        Assert.True(commandsWithDesc.Count > 0, "Demo scene should have commands with descriptions");

        // Check that DrawRect has rect info
        var drawRect = commandList.Commands.First(c => c.Command == "DrawRect");
        Assert.NotNull(drawRect.ShortDesc);
        Assert.Contains("rect", drawRect.ShortDesc, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DemoScene_CanApplyRangeFilter()
    {
        var commandList = CreateShapesScene();
        var state = new DebuggerState();
        state.LoadCommandList(commandList);

        // Apply range to show only commands 2-5
        state.SetRange(2, 5);

        Assert.Equal(4, state.FilteredCommandCount);
        Assert.Equal(2, state.FilteredIndices[0]);
        Assert.Equal(5, state.FilteredIndices[3]);
    }

    [Fact]
    public void DemoScene_CombinedFilters_RangeAndText()
    {
        var commandList = CreateShapesScene();
        var state = new DebuggerState();
        state.LoadCommandList(commandList);

        // Range 0-6 and exclude Save/Restore
        state.SetRange(0, 6);
        state.SetFilter("!Save Restore");

        // Should exclude Save(1) and not include Restore (which is at index 9)
        foreach (var idx in state.FilteredIndices)
        {
            var cmd = state.GetCommand(idx);
            Assert.NotEqual("Save", cmd!.Command);
            Assert.True(idx >= 0 && idx <= 6);
        }
    }
}
