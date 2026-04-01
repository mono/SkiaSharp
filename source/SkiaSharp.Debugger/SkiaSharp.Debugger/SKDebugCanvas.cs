using System;
using System.Runtime.InteropServices;

namespace SkiaSharp.Debugger
{
    public class SKDebugCanvas : IDisposable
    {
        private IntPtr _handle;
        private bool _disposed;
        private static volatile bool _nativeAvailable = true;

        public SKDebugCanvas(int width, int height)
        {
            if (!_nativeAvailable)
                throw new NotSupportedException("Native debugger support is not available. Build with SKIA_DEBUGGER=true.");
            try
            {
                _handle = DebugCanvasApi.sk_debug_canvas_new(width, height);
            }
            catch (Exception ex) when (ex is EntryPointNotFoundException or DllNotFoundException)
            {
                _nativeAvailable = false;
                throw new NotSupportedException("Native debugger support is not available. Build with SKIA_DEBUGGER=true.", ex);
            }
        }

        public static bool IsNativeAvailable
        {
            get
            {
                if (!_nativeAvailable) return false;
                try
                {
                    var h = DebugCanvasApi.sk_debug_canvas_new(1, 1);
                    DebugCanvasApi.sk_debug_canvas_destroy(h);
                    return true;
                }
                catch (Exception ex) when (ex is EntryPointNotFoundException or DllNotFoundException)
                {
                    _nativeAvailable = false;
                    return false;
                }
            }
        }

        public IntPtr Handle
        {
            get
            {
                ThrowIfDisposed();
                return _handle;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SKDebugCanvas));
        }

        /// <summary>
        /// Load an SKP file from raw bytes. Returns the number of commands recorded, or -1 on failure.
        /// Note: For best results, create a new SKDebugCanvas for each SKP file. Calling LoadSkp
        /// again on the same instance clears previous commands but canvas state (matrix/clip) may
        /// carry over from the previous picture.
        /// </summary>
        public int LoadSkp(ReadOnlySpan<byte> data)
        {
            ThrowIfDisposed();
            unsafe
            {
                fixed (byte* ptr = data)
                {
                    return DebugCanvasApi.sk_debug_canvas_load_skp(_handle, (IntPtr)ptr, (IntPtr)data.Length);
                }
            }
        }

        /// <summary>
        /// Load an SKP file from a byte array.
        /// </summary>
        public int LoadSkp(byte[] data)
        {
            return LoadSkp(data.AsSpan());
        }

        public int CommandCount
        {
            get
            {
                ThrowIfDisposed();
                return DebugCanvasApi.sk_debug_canvas_get_command_count(_handle);
            }
        }

        /// <summary>
        /// Draw all commands to the target canvas.
        /// </summary>
        public void Draw(SKCanvas target)
        {
            ThrowIfDisposed();
            DebugCanvasApi.sk_debug_canvas_draw(_handle, target.Handle);
        }

        /// <summary>
        /// Draw commands up to and including the given index.
        /// </summary>
        public void DrawTo(SKCanvas target, int index)
        {
            ThrowIfDisposed();
            DebugCanvasApi.sk_debug_canvas_draw_to(_handle, target.Handle, index);
        }

        /// <summary>
        /// Get the JSON command list. Requires a target canvas for GPU audit trail info.
        /// </summary>
        public string GetCommandListJson(SKCanvas target)
        {
            ThrowIfDisposed();
            var str = DebugCanvasApi.sk_string_new_empty();
            try
            {
                DebugCanvasApi.sk_debug_canvas_get_command_list_json(_handle, target.Handle, str);
                return StringFromSkString(str);
            }
            finally
            {
                DebugCanvasApi.sk_string_destructor(str);
            }
        }

        /// <summary>
        /// Get the current matrix and clip info as JSON after the last drawTo call.
        /// </summary>
        public string GetCommandInfoJson()
        {
            ThrowIfDisposed();
            var str = DebugCanvasApi.sk_string_new_empty();
            try
            {
                DebugCanvasApi.sk_debug_canvas_get_command_info_json(_handle, str);
                return StringFromSkString(str);
            }
            finally
            {
                DebugCanvasApi.sk_string_destructor(str);
            }
        }

        private static unsafe string StringFromSkString(IntPtr skString)
        {
            var cstr = DebugCanvasApi.sk_string_get_c_str(skString);
            var size = (int)DebugCanvasApi.sk_string_get_size(skString);
            if (cstr == null || size == 0)
                return string.Empty;
            // Skia strings are UTF-8; use UTF-8 decoding (not ANSI)
            return System.Text.Encoding.UTF8.GetString((byte*)cstr, size);
        }

        public void SetCommandVisibility(int index, bool visible)
        {
            ThrowIfDisposed();
            DebugCanvasApi.sk_debug_canvas_set_command_visibility(_handle, index, visible);
        }

        public void DeleteCommand(int index)
        {
            ThrowIfDisposed();
            DebugCanvasApi.sk_debug_canvas_delete_command(_handle, index);
        }

        public void SetOverdrawVis(bool enabled)
        {
            ThrowIfDisposed();
            DebugCanvasApi.sk_debug_canvas_set_overdraw_vis(_handle, enabled);
        }

        public void SetClipVizColor(SKColor color)
        {
            ThrowIfDisposed();
            DebugCanvasApi.sk_debug_canvas_set_clip_viz_color(_handle, (uint)color);
        }

        public void SetOriginVisible(bool visible)
        {
            ThrowIfDisposed();
            DebugCanvasApi.sk_debug_canvas_set_origin_visible(_handle, visible);
        }

        public SKRectI GetBounds()
        {
            ThrowIfDisposed();
            DebugCanvasApi.sk_debug_canvas_get_bounds(_handle, out var bounds);
            return bounds;
        }

        public void Dispose()
        {
            if (!_disposed && _handle != IntPtr.Zero)
            {
                DebugCanvasApi.sk_debug_canvas_destroy(_handle);
                _handle = IntPtr.Zero;
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        ~SKDebugCanvas() => Dispose();
    }

    internal static class DebugCanvasApi
    {
        private const string SKIA = "libSkiaSharp";

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr sk_debug_canvas_new(int width, int height);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sk_debug_canvas_destroy(IntPtr canvas);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sk_debug_canvas_load_skp(IntPtr canvas, IntPtr data, IntPtr length);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sk_debug_canvas_get_command_count(IntPtr canvas);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sk_debug_canvas_draw(IntPtr canvas, IntPtr target);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sk_debug_canvas_draw_to(IntPtr canvas, IntPtr target, int index);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sk_debug_canvas_get_command_list_json(IntPtr canvas, IntPtr target, IntPtr result);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sk_debug_canvas_get_command_info_json(IntPtr canvas, IntPtr result);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sk_debug_canvas_set_command_visibility(IntPtr canvas, int index, [MarshalAs(UnmanagedType.I1)] bool visible);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sk_debug_canvas_delete_command(IntPtr canvas, int index);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sk_debug_canvas_set_overdraw_vis(IntPtr canvas, [MarshalAs(UnmanagedType.I1)] bool enabled);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sk_debug_canvas_set_clip_viz_color(IntPtr canvas, uint color);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sk_debug_canvas_set_origin_visible(IntPtr canvas, [MarshalAs(UnmanagedType.I1)] bool visible);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sk_debug_canvas_get_bounds(IntPtr canvas, out SKRectI bounds);

        // sk_string helpers (since SKString is internal to SkiaSharp)

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr sk_string_new_empty();

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sk_string_destructor(IntPtr str);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void* sk_string_get_c_str(IntPtr str);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr sk_string_get_size(IntPtr str);
    }
}
