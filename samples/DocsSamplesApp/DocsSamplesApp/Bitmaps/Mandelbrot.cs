using System.Numerics;

namespace DocsSamplesApp.Bitmaps
{
    static class Mandelbrot
    {
        public static Task<BitmapInfo> CalculateAsync(Complex center,
                                                      double width, double height,
                                                      int pixelWidth, int pixelHeight,
                                                      int iterations,
                                                      IProgress<double> progress,
                                                      CancellationToken cancelToken)
        {
            return Task.Run(() =>
            {
                int[] iterationCounts = new int[pixelWidth * pixelHeight];
                int index = 0;

                for (int row = 0; row < pixelHeight; row++)
                {
                    progress.Report((double)row / pixelHeight);
                    cancelToken.ThrowIfCancellationRequested();

                    double y = center.Imaginary + height / 2 - row * height / pixelHeight;

                    for (int col = 0; col < pixelWidth; col++)
                    {
                        double x = center.Real - width / 2 + col * width / pixelWidth;
                        Complex c = new Complex(x, y);

                        if ((c - new Complex(-1, 0)).Magnitude < 1.0 / 4)
                        {
                            iterationCounts[index++] = -1;
                        }
                        // http://www.reenigne.org/blog/algorithm-for-mandelbrot-cardioid/
                        else if (c.Magnitude * c.Magnitude * (8 * c.Magnitude * c.Magnitude - 3) < 3.0 / 32 - c.Real)
                        {
                            iterationCounts[index++] = -1;
                        }
                        else
                        {
                            Complex z = 0;
                            int iteration = 0;

                            do
                            {
                                z = z * z + c;
                                iteration++;
                            }
                            while (iteration < iterations && z.Magnitude < 2);

                            if (iteration == iterations)
                            {
                                iterationCounts[index++] = -1;
                            }
                            else
                            {
                                iterationCounts[index++] = iteration;
                            }
                        }
                    }
                }
                return new BitmapInfo(pixelWidth, pixelHeight, iterationCounts);
            }, cancelToken);
        }
    }
}
