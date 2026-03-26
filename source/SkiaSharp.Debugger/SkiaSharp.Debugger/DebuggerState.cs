using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using SkiaSharp.Debugger.Models;

namespace SkiaSharp.Debugger
{
    public class DebuggerState
    {
        private SkpCommandList? _commandList;
        private List<int> _filteredIndices = new();
        private HashSet<string> _excludedCommandNames = new(StringComparer.OrdinalIgnoreCase);
        private string _filterText = string.Empty;
        private int _selectedCommandIndex;
        private int _rangeStart;
        private int _rangeEnd;
        private bool _overdrawViz;
        private bool _showClip;
        private bool _showOrigin;
        private bool _darkBackground;
        private int[] _cursorPosition = new int[] { 0, 0 };
        private bool _crosshairActive;
        private MatrixClipInfo? _matrixClipInfo;

        public event Action? StateChanged;

        // Properties
        public SkpCommandList? CommandList => _commandList;
        public IReadOnlyList<int> FilteredIndices => _filteredIndices;
        public int SelectedCommandIndex
        {
            get => _selectedCommandIndex;
            set
            {
                _selectedCommandIndex = Math.Max(0, Math.Min(value, _filteredIndices.Count - 1));
                OnStateChanged();
            }
        }

        /// <summary>
        /// The actual command index in the unfiltered list (what to pass to drawTo).
        /// </summary>
        public int SelectedUnfilteredIndex =>
            _filteredIndices.Count > 0 && _selectedCommandIndex < _filteredIndices.Count
                ? _filteredIndices[_selectedCommandIndex]
                : -1;

        public int TotalCommandCount => _commandList?.Commands.Count ?? 0;
        public int FilteredCommandCount => _filteredIndices.Count;
        public string FilterText => _filterText;
        public bool OverdrawViz { get => _overdrawViz; set { _overdrawViz = value; OnStateChanged(); } }
        public bool ShowClip { get => _showClip; set { _showClip = value; OnStateChanged(); } }
        public bool ShowOrigin { get => _showOrigin; set { _showOrigin = value; OnStateChanged(); } }
        public bool DarkBackground { get => _darkBackground; set { _darkBackground = value; OnStateChanged(); } }
        public int[] CursorPosition { get => _cursorPosition; set { _cursorPosition = value; OnStateChanged(); } }
        public bool CrosshairActive { get => _crosshairActive; set { _crosshairActive = value; OnStateChanged(); } }
        public MatrixClipInfo? MatrixClipInfo { get => _matrixClipInfo; set { _matrixClipInfo = value; OnStateChanged(); } }

        /// <summary>
        /// Pixel color at cursor position (RGBA). Updated during paint — no event fired to avoid loops.
        /// </summary>
        public int[]? CursorPixelRGBA { get; set; }

        /// <summary>
        /// Load a command list from JSON (as returned by SKDebugCanvas.GetCommandListJson).
        /// </summary>
        public void LoadCommandListJson(string json)
        {
            _commandList = JsonSerializer.Deserialize<SkpCommandList>(json);
            _rangeStart = 0;
            _rangeEnd = Math.Max(0, (_commandList?.Commands.Count ?? 1) - 1);
            _selectedCommandIndex = _rangeEnd;
            ApplyFilters();
        }

        /// <summary>
        /// Load a command list directly (for testing).
        /// </summary>
        public void LoadCommandList(SkpCommandList commands)
        {
            _commandList = commands;
            _rangeStart = 0;
            _rangeEnd = Math.Max(0, commands.Commands.Count - 1);
            _selectedCommandIndex = _rangeEnd;
            ApplyFilters();
        }

        /// <summary>
        /// Set the text filter. Leading ! negates the entire filter.
        /// Space-separated tokens filter by command name.
        /// </summary>
        public void SetFilter(string filter)
        {
            _filterText = filter;
            ApplyFilters();
        }

        /// <summary>
        /// Set the command range filter.
        /// </summary>
        public void SetRange(int start, int end)
        {
            _rangeStart = Math.Max(0, start);
            _rangeEnd = Math.Min(end, TotalCommandCount - 1);
            ApplyFilters();
        }

        /// <summary>
        /// Toggle a command name in the exclusion set.
        /// </summary>
        public void ToggleCommandName(string name)
        {
            if (_excludedCommandNames.Contains(name))
                _excludedCommandNames.Remove(name);
            else
                _excludedCommandNames.Add(name);
            ApplyFilters();
        }

        /// <summary>
        /// Step forward by one in the filtered list.
        /// </summary>
        public void StepForward()
        {
            if (_selectedCommandIndex < _filteredIndices.Count - 1)
                SelectedCommandIndex++;
        }

        /// <summary>
        /// Step backward by one in the filtered list.
        /// </summary>
        public void StepBackward()
        {
            if (_selectedCommandIndex > 0)
                SelectedCommandIndex--;
        }

        /// <summary>
        /// Jump to the start.
        /// </summary>
        public void JumpToStart() => SelectedCommandIndex = 0;

        /// <summary>
        /// Jump to the end.
        /// </summary>
        public void JumpToEnd() => SelectedCommandIndex = _filteredIndices.Count - 1;

        /// <summary>
        /// Jump to a specific unfiltered command index.
        /// </summary>
        public void JumpToUnfilteredIndex(int unfilteredIndex)
        {
            var filteredPos = _filteredIndices.IndexOf(unfilteredIndex);
            if (filteredPos >= 0)
                SelectedCommandIndex = filteredPos;
        }

        /// <summary>
        /// Get a histogram of command types in the current range.
        /// </summary>
        public Dictionary<string, int> GetHistogram()
        {
            if (_commandList == null) return new();

            var histogram = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = _rangeStart; i <= _rangeEnd && i < _commandList.Commands.Count; i++)
            {
                var name = _commandList.Commands[i].Command;
                if (!histogram.ContainsKey(name))
                    histogram[name] = 0;
                histogram[name]++;
            }
            return histogram.OrderByDescending(kv => kv.Value)
                           .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        /// <summary>
        /// Get the command at a given unfiltered index.
        /// </summary>
        public SkpCommand? GetCommand(int unfilteredIndex)
        {
            if (_commandList == null || unfilteredIndex < 0 || unfilteredIndex >= _commandList.Commands.Count)
                return null;
            return _commandList.Commands[unfilteredIndex];
        }

        /// <summary>
        /// Parse matrix/clip info JSON.
        /// </summary>
        public void UpdateMatrixClipInfo(string json)
        {
            MatrixClipInfo = JsonSerializer.Deserialize<MatrixClipInfo>(json);
        }

        private void ApplyFilters()
        {
            if (_commandList == null)
            {
                _filteredIndices.Clear();
                OnStateChanged();
                return;
            }

            var filtered = new List<int>();
            bool isNegativeFilter = _filterText.StartsWith("!");
            var tokens = (isNegativeFilter ? _filterText.Substring(1) : _filterText)
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = _rangeStart; i <= _rangeEnd && i < _commandList.Commands.Count; i++)
            {
                var cmd = _commandList.Commands[i];
                var name = cmd.Command;

                // Check exclusion set
                if (_excludedCommandNames.Contains(name))
                    continue;

                // Check text filter
                if (tokens.Length > 0)
                {
                    bool matchesAnyToken = tokens.Any(t =>
                        name.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0);

                    if (isNegativeFilter && matchesAnyToken)
                        continue;
                    if (!isNegativeFilter && !matchesAnyToken)
                        continue;
                }

                filtered.Add(i);
            }

            _filteredIndices = filtered;

            // Clamp selection
            if (_selectedCommandIndex >= _filteredIndices.Count)
                _selectedCommandIndex = Math.Max(0, _filteredIndices.Count - 1);

            OnStateChanged();
        }

        private void OnStateChanged() => StateChanged?.Invoke();
    }
}
