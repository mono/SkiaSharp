using SkiaSharp.Debugger;
using SkiaSharp.Debugger.Models;
using Xunit;

namespace SkiaSharp.Debugger.Tests;

public class DebuggerStateTests
{
    private SkpCommandList CreateSampleCommands()
    {
        return new SkpCommandList
        {
            Version = 1,
            Commands = new()
            {
                new() { Command = "Save", Visible = true },
                new() { Command = "ClipRect", Visible = true, ShortDesc = "rect(0,0,100,200)" },
                new() { Command = "DrawRect", Visible = true, ShortDesc = "rect(10,20,90,180)" },
                new() { Command = "DrawOval", Visible = true },
                new() { Command = "DrawPath", Visible = true },
                new() { Command = "DrawTextBlob", Visible = true, ShortDesc = "Hello" },
                new() { Command = "Restore", Visible = true },
                new() { Command = "Save", Visible = true },
                new() { Command = "Concat", Visible = true },
                new() { Command = "DrawRect", Visible = true, ShortDesc = "rect(0,0,50,50)" },
                new() { Command = "DrawAnnotation", Visible = true, Key = "test" },
                new() { Command = "Restore", Visible = true },
            }
        };
    }

    private SkpCommandList CreateEmptyCommands()
    {
        return new SkpCommandList
        {
            Version = 1,
            Commands = new()
        };
    }

    private SkpCommandList CreateSingleCommand()
    {
        return new SkpCommandList
        {
            Version = 1,
            Commands = new()
            {
                new() { Command = "DrawRect", Visible = true }
            }
        };
    }

    [Fact]
    public void LoadCommandList_SetsCorrectCounts()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        Assert.Equal(12, state.TotalCommandCount);
        Assert.Equal(12, state.FilteredCommandCount);
    }

    [Fact]
    public void LoadCommandList_SelectsLastCommand()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        Assert.Equal(11, state.SelectedCommandIndex);
        Assert.Equal(11, state.SelectedUnfilteredIndex);
    }

    [Fact]
    public void NegativeFilter_ExcludesMatchingCommands()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        state.SetFilter("!Save Restore");

        // Should exclude Save (indices 0, 7) and Restore (indices 6, 11)
        Assert.Equal(8, state.FilteredCommandCount);
        Assert.DoesNotContain(0, state.FilteredIndices);
        Assert.DoesNotContain(6, state.FilteredIndices);
        Assert.DoesNotContain(7, state.FilteredIndices);
        Assert.DoesNotContain(11, state.FilteredIndices);
    }

    [Fact]
    public void PositiveFilter_IncludesOnlyMatchingCommands()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        state.SetFilter("DrawRect");

        // Should include only DrawRect (indices 2, 9)
        Assert.Equal(2, state.FilteredCommandCount);
        Assert.Contains(2, state.FilteredIndices);
        Assert.Contains(9, state.FilteredIndices);
    }

    [Fact]
    public void EmptyFilter_ShowsAllCommands()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        state.SetFilter("DrawRect");
        Assert.Equal(2, state.FilteredCommandCount);

        state.SetFilter("");
        Assert.Equal(12, state.FilteredCommandCount);
    }

    [Fact]
    public void StepForward_AdvancesSelection()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());
        state.JumpToStart();

        Assert.Equal(0, state.SelectedCommandIndex);

        state.StepForward();
        Assert.Equal(1, state.SelectedCommandIndex);

        state.StepForward();
        Assert.Equal(2, state.SelectedCommandIndex);
    }

    [Fact]
    public void StepBackward_ReversesSelection()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        state.StepBackward();
        Assert.Equal(10, state.SelectedCommandIndex);
    }

    [Fact]
    public void StepForward_StopsAtEnd()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());
        // Already at end (index 11)

        state.StepForward();
        Assert.Equal(11, state.SelectedCommandIndex);
    }

    [Fact]
    public void StepBackward_StopsAtStart()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());
        state.JumpToStart();

        state.StepBackward();
        Assert.Equal(0, state.SelectedCommandIndex);
    }

    [Fact]
    public void JumpToUnfilteredIndex_FindsCorrectPosition()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());
        state.SetFilter("!Save Restore");

        // Jump to DrawOval (unfiltered index 3)
        state.JumpToUnfilteredIndex(3);

        Assert.Equal(3, state.SelectedUnfilteredIndex);
    }

    [Fact]
    public void RangeFilter_LimitsCommands()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        state.SetRange(2, 5);

        Assert.Equal(4, state.FilteredCommandCount); // indices 2, 3, 4, 5
        Assert.Equal(2, state.FilteredIndices[0]);
        Assert.Equal(5, state.FilteredIndices[3]);
    }

    [Fact]
    public void ToggleCommandName_ExcludesAndIncludes()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        // Exclude Save
        state.ToggleCommandName("Save");
        Assert.Equal(10, state.FilteredCommandCount);

        // Re-include Save
        state.ToggleCommandName("Save");
        Assert.Equal(12, state.FilteredCommandCount);
    }

    [Fact]
    public void GetHistogram_CountsCommandTypes()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        var histogram = state.GetHistogram();

        Assert.Equal(2, histogram["Save"]);
        Assert.Equal(2, histogram["DrawRect"]);
        Assert.Equal(2, histogram["Restore"]);
        Assert.Equal(1, histogram["DrawOval"]);
        Assert.Equal(1, histogram["ClipRect"]);
    }

    [Fact]
    public void GetCommand_ReturnsCorrectCommand()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        var cmd = state.GetCommand(2);
        Assert.NotNull(cmd);
        Assert.Equal("DrawRect", cmd.Command);
        Assert.Equal("rect(10,20,90,180)", cmd.ShortDesc);
    }

    [Fact]
    public void GetCommand_ReturnsNullForInvalidIndex()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        Assert.Null(state.GetCommand(-1));
        Assert.Null(state.GetCommand(100));
    }

    [Fact]
    public void LoadCommandListJson_ParsesAndAppliesFilters()
    {
        var state = new DebuggerState();
        var json = @"{ ""version"": 1, ""commands"": [
            { ""command"": ""Save"", ""visible"": true },
            { ""command"": ""DrawRect"", ""visible"": true },
            { ""command"": ""Restore"", ""visible"": true }
        ]}";

        state.LoadCommandListJson(json);

        Assert.Equal(3, state.TotalCommandCount);
        Assert.Equal(3, state.FilteredCommandCount);
    }

    [Fact]
    public void StateChanged_FiresOnPropertyChange()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        int changeCount = 0;
        state.StateChanged += () => changeCount++;

        state.StepBackward();
        Assert.True(changeCount > 0);
    }

    [Fact]
    public void UpdateMatrixClipInfo_ParsesJson()
    {
        var state = new DebuggerState();
        var json = @"{
            ""ClipRect"": [0, 0, 500, 800],
            ""ViewMatrix"": [[2, 0, 100], [0, 2, 200], [0, 0, 1]]
        }";

        state.UpdateMatrixClipInfo(json);

        Assert.NotNull(state.MatrixClipInfo);
        Assert.Equal(500, state.MatrixClipInfo!.ClipRect[2]);
        Assert.Equal(2f, state.MatrixClipInfo.ViewMatrix[0][0]);
        Assert.Equal(100f, state.MatrixClipInfo.ViewMatrix[0][2]);
    }

    [Fact]
    public void CombinedFilters_WorkTogether()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        // Apply range (0-6) AND negative filter (!Save Restore)
        state.SetRange(0, 6);
        state.SetFilter("!Save Restore");

        // Range 0-6 has 7 commands, minus Save(0), Restore(6) = 5
        Assert.Equal(5, state.FilteredCommandCount);
    }

    // === EDGE CASE TESTS ===

    [Fact]
    public void EmptyCommandList_AllOperationsAreSafe()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateEmptyCommands());

        Assert.Equal(0, state.TotalCommandCount);
        Assert.Equal(0, state.FilteredCommandCount);
        Assert.Equal(-1, state.SelectedUnfilteredIndex);
        
        // These should not throw
        state.StepForward();
        state.StepBackward();
        state.JumpToStart();
        state.JumpToEnd();
        state.SetFilter("Draw");
        state.SetRange(0, 10);
        state.ToggleCommandName("DrawRect");
        
        Assert.Equal(0, state.FilteredCommandCount);
        Assert.Null(state.GetCommand(0));
    }

    [Fact]
    public void SingleCommandList_AllOperationsWork()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSingleCommand());

        Assert.Equal(1, state.TotalCommandCount);
        Assert.Equal(1, state.FilteredCommandCount);
        Assert.Equal(0, state.SelectedCommandIndex);
        Assert.Equal(0, state.SelectedUnfilteredIndex);
        
        // Step forward - should stay at 0
        state.StepForward();
        Assert.Equal(0, state.SelectedCommandIndex);
        
        // Step backward - should stay at 0
        state.StepBackward();
        Assert.Equal(0, state.SelectedCommandIndex);
        
        var cmd = state.GetCommand(0);
        Assert.NotNull(cmd);
        Assert.Equal("DrawRect", cmd!.Command);
    }

    [Fact]
    public void SetFilter_WithOnlyExclamation_ShowsAllCommands()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        state.SetFilter("!");

        // "!" with no tokens should show all commands
        Assert.Equal(12, state.FilteredCommandCount);
    }

    [Fact]
    public void SetRange_InvertedRange_ClampsCorrectly()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        // Start > End
        state.SetRange(10, 5);

        // Range is clamped: start=10, end=min(5, 11)=5, so empty or weird
        // Actually: _rangeStart = max(0, 10) = 10, _rangeEnd = min(5, 11) = 5
        // Loop: for i=10; i<=5 => no iterations
        Assert.Equal(0, state.FilteredCommandCount);
    }

    [Fact]
    public void SetRange_NegativeStart_ClampedToZero()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        state.SetRange(-5, 3);

        Assert.Equal(4, state.FilteredCommandCount); // 0, 1, 2, 3
        Assert.Equal(0, state.FilteredIndices[0]);
    }

    [Fact]
    public void SetRange_EndBeyondCount_ClampedToMax()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        state.SetRange(0, 100);

        Assert.Equal(12, state.FilteredCommandCount);
    }

    [Fact]
    public void MultipleToggleCommandName_SameName_TogglesCorrectly()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        // Toggle Save off
        state.ToggleCommandName("Save");
        Assert.Equal(10, state.FilteredCommandCount);
        
        // Toggle Save off again (should add back)
        state.ToggleCommandName("Save");
        Assert.Equal(12, state.FilteredCommandCount);
        
        // Toggle Save off
        state.ToggleCommandName("Save");
        Assert.Equal(10, state.FilteredCommandCount);
        
        // Toggle multiple different names
        state.ToggleCommandName("Restore");
        Assert.Equal(8, state.FilteredCommandCount);
        
        state.ToggleCommandName("DrawRect");
        Assert.Equal(6, state.FilteredCommandCount);
    }

    [Fact]
    public void StepForward_WithNoCommandsLoaded_DoesNotThrow()
    {
        var state = new DebuggerState();
        
        // No commands loaded
        state.StepForward();
        state.StepBackward();
        state.JumpToStart();
        state.JumpToEnd();
        
        Assert.Equal(0, state.TotalCommandCount);
    }

    [Fact]
    public void JumpToUnfilteredIndex_IndexNotInFilteredList_DoesNotJump()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());
        state.SetFilter("DrawRect"); // Only indices 2, 9 are included

        var originalIndex = state.SelectedCommandIndex;
        
        // Try to jump to index 0 (Save) which is not in filtered list
        state.JumpToUnfilteredIndex(0);
        
        // Selection should not change since 0 is not in filtered list
        Assert.Equal(originalIndex, state.SelectedCommandIndex);
    }

    [Fact]
    public void LoadCommandList_Twice_ReplacesFirst()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());
        Assert.Equal(12, state.TotalCommandCount);
        
        state.LoadCommandList(CreateSingleCommand());
        Assert.Equal(1, state.TotalCommandCount);
        Assert.Equal(1, state.FilteredCommandCount);
    }

    [Fact]
    public void StateChanged_FiresCorrectNumberOfTimes()
    {
        var state = new DebuggerState();
        int changeCount = 0;
        state.StateChanged += () => changeCount++;

        state.LoadCommandList(CreateSampleCommands());
        var loadChanges = changeCount;
        Assert.True(loadChanges >= 1, "LoadCommandList should fire StateChanged");

        changeCount = 0;
        state.StepBackward();
        Assert.True(changeCount >= 1, "StepBackward should fire StateChanged");

        changeCount = 0;
        state.SetFilter("Draw");
        Assert.True(changeCount >= 1, "SetFilter should fire StateChanged");
    }

    [Fact]
    public void Filter_IsCaseInsensitive()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        state.SetFilter("drawrect");
        var lowerCount = state.FilteredCommandCount;

        state.SetFilter("DRAWRECT");
        var upperCount = state.FilteredCommandCount;

        state.SetFilter("DrawRect");
        var mixedCount = state.FilteredCommandCount;

        Assert.Equal(2, lowerCount);
        Assert.Equal(2, upperCount);
        Assert.Equal(2, mixedCount);
    }

    [Fact]
    public void PartialCommandNameMatch_InPositiveFilter()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        // "Draw" should match DrawRect, DrawOval, DrawPath, DrawTextBlob, DrawAnnotation
        state.SetFilter("Draw");

        Assert.Equal(6, state.FilteredCommandCount);
        foreach (var idx in state.FilteredIndices)
        {
            var cmd = state.GetCommand(idx);
            Assert.StartsWith("Draw", cmd!.Command);
        }
    }

    [Fact]
    public void MultipleStateChangedSubscribers_AllGetNotified()
    {
        var state = new DebuggerState();
        int count1 = 0, count2 = 0, count3 = 0;
        
        state.StateChanged += () => count1++;
        state.StateChanged += () => count2++;
        state.StateChanged += () => count3++;

        state.LoadCommandList(CreateSampleCommands());

        Assert.True(count1 >= 1);
        Assert.True(count2 >= 1);
        Assert.True(count3 >= 1);
        Assert.Equal(count1, count2);
        Assert.Equal(count2, count3);
    }

    [Fact]
    public void CursorPosition_SetterFiresStateChanged()
    {
        var state = new DebuggerState();
        int changeCount = 0;
        state.StateChanged += () => changeCount++;

        state.CursorPosition = new[] { 100, 200 };

        Assert.True(changeCount >= 1);
        Assert.Equal(100, state.CursorPosition[0]);
        Assert.Equal(200, state.CursorPosition[1]);
    }

    [Fact]
    public void CrosshairActive_SetterFiresStateChanged()
    {
        var state = new DebuggerState();
        int changeCount = 0;
        state.StateChanged += () => changeCount++;

        state.CrosshairActive = true;

        Assert.True(changeCount >= 1);
        Assert.True(state.CrosshairActive);
    }

    [Fact]
    public void OverdrawViz_SetterFiresStateChanged()
    {
        var state = new DebuggerState();
        int changeCount = 0;
        state.StateChanged += () => changeCount++;

        state.OverdrawViz = true;

        Assert.True(changeCount >= 1);
        Assert.True(state.OverdrawViz);
    }

    [Fact]
    public void ShowClip_SetterFiresStateChanged()
    {
        var state = new DebuggerState();
        int changeCount = 0;
        state.StateChanged += () => changeCount++;

        state.ShowClip = true;

        Assert.True(changeCount >= 1);
        Assert.True(state.ShowClip);
    }

    [Fact]
    public void ShowOrigin_SetterFiresStateChanged()
    {
        var state = new DebuggerState();
        int changeCount = 0;
        state.StateChanged += () => changeCount++;

        state.ShowOrigin = true;

        Assert.True(changeCount >= 1);
        Assert.True(state.ShowOrigin);
    }

    [Fact]
    public void DarkBackground_SetterFiresStateChanged()
    {
        var state = new DebuggerState();
        int changeCount = 0;
        state.StateChanged += () => changeCount++;

        state.DarkBackground = true;

        Assert.True(changeCount >= 1);
        Assert.True(state.DarkBackground);
    }

    [Fact]
    public void GetHistogram_WithRangeFilter_OnlyCountsInRange()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        // Set range to 0-6 (first 7 commands)
        state.SetRange(0, 6);

        var histogram = state.GetHistogram();

        // In range 0-6: Save(0), ClipRect(1), DrawRect(2), DrawOval(3), DrawPath(4), DrawTextBlob(5), Restore(6)
        Assert.Equal(1, histogram["Save"]);
        Assert.Equal(1, histogram["DrawRect"]);
        Assert.Equal(1, histogram["Restore"]);
        Assert.DoesNotContain("Concat", histogram.Keys);
        Assert.DoesNotContain("DrawAnnotation", histogram.Keys);
    }

    [Fact]
    public void GetCommand_WithNullCommandList_ReturnsNull()
    {
        var state = new DebuggerState();
        // No commands loaded

        Assert.Null(state.GetCommand(0));
        Assert.Null(state.GetCommand(-1));
        Assert.Null(state.GetCommand(100));
    }

    [Fact]
    public void SelectedCommandIndex_Setter_ClampsToValidRange()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        state.SelectedCommandIndex = 100;
        Assert.Equal(11, state.SelectedCommandIndex); // Clamped to max

        state.SelectedCommandIndex = -5;
        Assert.Equal(0, state.SelectedCommandIndex); // Clamped to 0
    }

    [Fact]
    public void FilterText_Property_ReturnsCurrentFilter()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        Assert.Equal("", state.FilterText);

        state.SetFilter("!Save Restore");
        Assert.Equal("!Save Restore", state.FilterText);

        state.SetFilter("");
        Assert.Equal("", state.FilterText);
    }

    [Fact]
    public void MatrixClipInfo_SetterFiresStateChanged()
    {
        var state = new DebuggerState();
        int changeCount = 0;
        state.StateChanged += () => changeCount++;

        var info = new MatrixClipInfo
        {
            ClipRect = new[] { 0, 0, 100, 100 },
            ViewMatrix = new[] { new[] { 1f, 0f, 0f }, new[] { 0f, 1f, 0f }, new[] { 0f, 0f, 1f } }
        };
        state.MatrixClipInfo = info;

        Assert.True(changeCount >= 1);
        Assert.NotNull(state.MatrixClipInfo);
    }

    [Fact]
    public void CommandList_Property_ReturnsLoadedList()
    {
        var state = new DebuggerState();
        Assert.Null(state.CommandList);

        var commands = CreateSampleCommands();
        state.LoadCommandList(commands);

        Assert.NotNull(state.CommandList);
        Assert.Equal(12, state.CommandList!.Commands.Count);
    }

    [Fact]
    public void SetRange_SameStartAndEnd_ShowsSingleCommand()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        state.SetRange(5, 5);

        Assert.Equal(1, state.FilteredCommandCount);
        Assert.Equal(5, state.FilteredIndices[0]);
    }

    [Fact]
    public void CombinedFilters_ExclusionSetAndTextFilter()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        // Use ToggleCommandName to exclude Save
        state.ToggleCommandName("Save");
        // Also apply text filter
        state.SetFilter("!Restore");

        // Should exclude Save (via toggle) and Restore (via filter)
        Assert.DoesNotContain(0, state.FilteredIndices); // Save
        Assert.DoesNotContain(7, state.FilteredIndices); // Save
        Assert.DoesNotContain(6, state.FilteredIndices); // Restore
        Assert.DoesNotContain(11, state.FilteredIndices); // Restore
    }

    [Fact]
    public void JumpToUnfilteredIndex_ValidIndex_UpdatesSelection()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        state.JumpToUnfilteredIndex(5); // DrawTextBlob

        Assert.Equal(5, state.SelectedCommandIndex);
        Assert.Equal(5, state.SelectedUnfilteredIndex);
    }

    [Fact]
    public void GetHistogram_EmptyCommandList_ReturnsEmpty()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateEmptyCommands());

        var histogram = state.GetHistogram();

        Assert.Empty(histogram);
    }

    [Fact]
    public void FilteredIndices_Property_ReturnsReadOnlyList()
    {
        var state = new DebuggerState();
        state.LoadCommandList(CreateSampleCommands());

        var indices = state.FilteredIndices;

        Assert.IsAssignableFrom<IReadOnlyList<int>>(indices);
        Assert.Equal(12, indices.Count);
    }
}
