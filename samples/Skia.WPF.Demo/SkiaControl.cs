using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using SkiaSharp;

namespace Skia.WPF.Demo
{
    public partial class SkiaControl : FrameworkElement
    {
        private Demos.Sample sample;

        public SkiaControl()
        {
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (sample == null)
                return;

            var m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            var dpiX = m.M11;
            var dpiY = m.M22;

            int width = (int)(ActualWidth * dpiX);
            int height = (int)(ActualHeight * dpiY);

            var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
            bitmap.Lock();
            using (var surface = SKSurface.Create(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul, bitmap.BackBuffer, bitmap.BackBufferStride))
            {
                var skcanvas = surface.Canvas;
                skcanvas.Scale((float)dpiX, (float)dpiY);
                using (new SKAutoCanvasRestore(skcanvas, true))
                {
                    sample.Method(skcanvas, (int)ActualWidth, (int)ActualHeight);
                }
            }
            bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            bitmap.Unlock();

            drawingContext.DrawImage(bitmap, new Rect(0, 0, ActualWidth, ActualHeight));
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            InvalidateVisual();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            sample?.TapMethod?.Invoke();
        }

        public Demos.Sample Sample
        {
            get { return sample; }
            set
            {
                sample = value;
                InvalidateVisual();
            }
        }
    }
}
