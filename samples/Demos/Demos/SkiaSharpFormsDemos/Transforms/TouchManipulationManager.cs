using System;
using SkiaSharp;

namespace SkiaSharpFormsDemos.Transforms
{
    class TouchManipulationManager
    {
        public TouchManipulationMode Mode { set; get; }

        public SKMatrix OneFingerManipulate(SKPoint prevPoint, SKPoint newPoint, SKPoint pivotPoint)
        {
            if (Mode == TouchManipulationMode.None)
            {
                return SKMatrix.MakeIdentity();
            }

            SKMatrix touchMatrix = SKMatrix.MakeIdentity();
            SKPoint delta = newPoint - prevPoint;

            if (Mode == TouchManipulationMode.ScaleDualRotate)  // One-finger rotation
            {
                SKPoint oldVector = prevPoint - pivotPoint;
                SKPoint newVector = newPoint - pivotPoint;

                // Avoid rotation if fingers are too close to center
                if (Magnitude(newVector) > 25 && Magnitude(oldVector) > 25)
                {
                    float prevAngle = (float)Math.Atan2(oldVector.Y, oldVector.X);
                    float newAngle = (float)Math.Atan2(newVector.Y, newVector.X);

                    // Calculate rotation matrix
                    float angle = newAngle - prevAngle;
                    touchMatrix = SKMatrix.MakeRotation(angle, pivotPoint.X, pivotPoint.Y);

                    // Effectively rotate the old vector
                    float magnitudeRatio = Magnitude(oldVector) / Magnitude(newVector);
                    oldVector.X = magnitudeRatio * newVector.X;
                    oldVector.Y = magnitudeRatio * newVector.Y;

                    // Recalculate delta
                    delta = newVector - oldVector;
                }
            }

            // Multiply the rotation matrix by a translation matrix
            SKMatrix.PostConcat(ref touchMatrix, SKMatrix.MakeTranslation(delta.X, delta.Y));

            return touchMatrix;
        }

        public SKMatrix TwoFingerManipulate(SKPoint prevPoint, SKPoint newPoint, SKPoint pivotPoint)
        {
            SKMatrix touchMatrix = SKMatrix.MakeIdentity();
            SKPoint oldVector = prevPoint - pivotPoint;
            SKPoint newVector = newPoint - pivotPoint;

            if (Mode == TouchManipulationMode.ScaleRotate ||
                Mode == TouchManipulationMode.ScaleDualRotate)
            {
                // Find angles from pivot point to touch points
                float oldAngle = (float)Math.Atan2(oldVector.Y, oldVector.X);
                float newAngle = (float)Math.Atan2(newVector.Y, newVector.X);

                // Calculate rotation matrix
                float angle = newAngle - oldAngle;
                touchMatrix = SKMatrix.MakeRotation(angle, pivotPoint.X, pivotPoint.Y);

                // Effectively rotate the old vector
                float magnitudeRatio = Magnitude(oldVector) / Magnitude(newVector);
                oldVector.X = magnitudeRatio * newVector.X;
                oldVector.Y = magnitudeRatio * newVector.Y;
            }

            float scaleX = 1;
            float scaleY = 1;

            if (Mode == TouchManipulationMode.AnisotropicScale)
            {
                scaleX = newVector.X / oldVector.X;
                scaleY = newVector.Y / oldVector.Y;

            }
            else if (Mode == TouchManipulationMode.IsotropicScale || 
                     Mode == TouchManipulationMode.ScaleRotate ||
                     Mode == TouchManipulationMode.ScaleDualRotate)
            {
                scaleX = scaleY = Magnitude(newVector) / Magnitude(oldVector);
            }

            if (!float.IsNaN(scaleX) && !float.IsInfinity(scaleX) &&
                !float.IsNaN(scaleY) && !float.IsInfinity(scaleY))
            {
                SKMatrix.PostConcat(ref touchMatrix, 
                    SKMatrix.MakeScale(scaleX, scaleY, pivotPoint.X, pivotPoint.Y));
            }

            return touchMatrix;
        }

        float Magnitude(SKPoint point)
        {
            return (float)Math.Sqrt(Math.Pow(point.X, 2) + Math.Pow(point.Y, 2));
        }
    }
}
