using System;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

using SkiaSharp;

namespace Skia.WindowsDesktop.GLDemo
{
    public class SkiaGLControl : GLControl
    {
        private Demos.Sample sample;
        private GRContext grContext = null;

        public SkiaGLControl()
        {
            //// something else
            //var glInterface = GRGlInterface.CreateNativeInterface();
            //var secondContext = GRContext.Create(GRBackend.OpenGL, glInterface);
            //var secondSurface = SKSurface.Create(secondContext, false, new SKImageInfo(Width, Height));
        }

        public Demos.Sample Sample
        {
            get { return sample; }
            set
            {
                sample = value;
                Invalidate();
            }
        }

        private void Reshape()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, 0, Height, -1, 1);
            GL.Viewport(0, 0, Width, Height);
        }

        protected override void Dispose(bool disposing)
        {
            grContext.Dispose();

            base.Dispose(disposing);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            sample?.TapMethod?.Invoke();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignMode) return;

            Reshape();
            grContext = GRContext.Create(GRBackend.OpenGL);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (grContext != null)
            {
                Reshape();
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (grContext != null)
            {
                var desc = new GRBackendRenderTargetDesc
                {
                    Width = Width,
                    Height = Height,
                    Config = GRPixelConfig.Bgra8888,
                    Origin = GRSurfaceOrigin.TopLeft,
                    SampleCount = 1,
                    StencilBits = 0,
                    RenderTargetHandle = IntPtr.Zero,
                };

                using (var surface = SKSurface.Create(grContext, desc))
                {
                    var skcanvas = surface.Canvas;

                    sample.Method(skcanvas, Width, Height);

                    skcanvas.Flush();
                }

                SwapBuffers();
            }
        }
    }
}
