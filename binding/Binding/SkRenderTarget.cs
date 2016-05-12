using System;
using System.Collections.Generic;
using System.Text;
using static SkiaSharp.SkiaApi;

namespace SkiaSharp
{
    public class SkRenderTarget : SKObject
    {
        public SkRenderTarget (IntPtr handle) : base (handle)
        {
        }

        protected override void Dispose (bool disposing)
        {
            if (Handle != IntPtr.Zero)
                sk_render_target_delete (Handle);
            base.Dispose (disposing);
        }

        public SkRenderContext CreateRenderContext ()
            => GetObject<SkRenderContext> (sk_render_target_create_context (Handle));

    }

    public class SkRenderContext : SKObject
    {
        public SkRenderContext (IntPtr handle) : base (handle)
        {
        }

        public SKCanvas Canvas => GetObject<SKCanvas> (sk_render_context_get_canvas (Handle));

        protected override void Dispose (bool disposing)
        {
            if (Handle != IntPtr.Zero)
                sk_render_context_delete (Handle);
            base.Dispose(disposing);
        }
    }

    public class SkOsWindowRenderTarget : SkRenderTarget
    {
        public SkOsWindowRenderTarget (IntPtr hWnd, bool softwareOnly) : base(sk_os_window_render_target_new (hWnd, softwareOnly))
        {
            
        }
    }
}
