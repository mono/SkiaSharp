using System;

using SkiaSharp;

namespace DocsSamplesApp.Transforms
{
    class TouchManipulationInfo
    {
        public SKPoint PreviousPoint { set; get; }

        public SKPoint NewPoint { set; get; }
    }
}
