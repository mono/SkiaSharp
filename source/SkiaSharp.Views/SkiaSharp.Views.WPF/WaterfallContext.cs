using System;
using System.Diagnostics;
using OpenTK;
using SkiaSharp.Views.WPF.Angle;
using SkiaSharp.Views.WPF.OutputImage;

namespace SkiaSharp.Views.WPF
{
	/// <summary>
	/// Tryes to initialize ANGLE, if failed - uses CPU rendering.
	///
	/// ANGLE can be not itialized if ANGLE libs are missing or GPU device is missing.
	/// </summary>
	internal class WaterfallContext
	{
		private bool disposed;
	    private GameWindow? gameWindow;
	    private D3DAngleInterop? angleInterop;

		public GRContext? GrContext { get; private set; }

	    public bool IsGpuRendering => GrContext != null;

	    public int SampleCount { get; private set; }
	    public int StencilBits { get; private set; }
	    public GLMode Mode { get; private set; }
	    public SKColorType ColorType { get; private set; }

		public void Initialize()
		{
			TryInitializeAngleContext();

			if (!IsGpuRendering)
			{
				Mode = GLMode.CPU;
				ColorType = SKColorType.Bgra8888;
			}
		}

		private void TryInitializeAngleContext()
		{
			try
			{
				angleInterop = new D3DAngleInterop();
				var glInterface = GRGlInterface.CreateNativeAngleInterface();
				GrContext = GRContext.CreateGl(glInterface);

				SampleCount = OpenTK.Graphics.ES20.GL.GetInteger(OpenTK.Graphics.ES20.GetPName.Samples);
				StencilBits = OpenTK.Graphics.ES20.GL.GetInteger(OpenTK.Graphics.ES20.GetPName.StencilBits);
				var maxSamples = GrContext.GetMaxSurfaceSampleCount(SKColorType.Rgba8888);
				if (SampleCount > maxSamples)
					SampleCount = maxSamples;

				GC.SuppressFinalize(GrContext);
				Mode = GLMode.Angle;
				ColorType = SKColorType.Rgba8888;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}

		public IOutputImage CreateOutputImage(SizeWithDpi size)
		{
			if (Mode == GLMode.Angle)
			{
				return new AngleImage(size, angleInterop, this);
			}

			//openGL and CPU will work over CpuImage (WriteableBitmap)
			return new CpuImage(size, ColorType);
		}

		~WaterfallContext()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposeManaged)
		{
			if (disposed)
				return;

			if (disposeManaged)
			{
				angleInterop?.Dispose();
				gameWindow?.Dispose();
				GrContext?.Dispose();
			}
			disposed = true;
		}
	}
}
