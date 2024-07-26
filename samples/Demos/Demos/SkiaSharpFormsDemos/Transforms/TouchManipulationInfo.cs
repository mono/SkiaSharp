using System;

using SkiaSharp;

namespace SkiaSharpFormsDemos.Transforms
{
    class TouchManipulationInfo
    {
        public SKPoint PreviousPoint { set; get; }

        public SKPoint NewPoint { set; get; }
    }
}
