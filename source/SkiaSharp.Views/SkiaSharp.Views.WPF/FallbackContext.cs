using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using SkiaSharp.Views.WPF.Angle;
using SkiaSharp.Views.WPF.OutputImage;

namespace SkiaSharp.Views.WPF
{
	internal class FallbackContext
    {
	    private GameWindow? _gameWindow;
	    private D3DAngleInterop? _angleInterop;
	    private bool _initialized;

		public GRContext? GrContext { get; private set; }

	    public bool IsGpuRendering => GrContext != null;

	    public int SampleCount { get; private set; }
	    public int StencilBits { get; private set; }
	    public GLMode Mode { get; private set; }
	    public SKColorType ColorType { get; private set; }

		public void TryInitialize()
		{
			if (_initialized)
			{
				return;
			}
			_initialized = true;

			TryInitializeAngleContext();

			if (GrContext == null)
			{
				TryInitializeOpenGLContext();
			}

			if (GrContext == null)
			{
				Mode = GLMode.CPU;
				ColorType = SKColorType.Bgra8888;
			}
		}

		private void TryInitializeAngleContext()
		{
			try
			{
				_angleInterop = new D3DAngleInterop();
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

		private void TryInitializeOpenGLContext()
		{
			try
			{
				_gameWindow = new GameWindow();
				_gameWindow.MakeCurrent();
				GrContext = GRContext.CreateGl();

				SampleCount = OpenTK.Graphics.OpenGL.GL.GetInteger(OpenTK.Graphics.OpenGL.GetPName.Samples);
				StencilBits = OpenTK.Graphics.OpenGL.GL.GetInteger(OpenTK.Graphics.OpenGL.GetPName.StencilBits);
				var maxSamples = GrContext.GetMaxSurfaceSampleCount(SKColorType.Bgra8888);
				if (SampleCount > maxSamples)
					SampleCount = maxSamples;

				GC.SuppressFinalize(GrContext);
				Mode = GLMode.OpenGL;
				ColorType = SKColorType.Bgra8888;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}

		public SKSurface CreateSurface(SKImageInfo info)
		{
			if (IsGpuRendering)
			{
				return SKSurface.Create(GrContext, false, info, 1, GRSurfaceOrigin.TopLeft);
			}
			return SKSurface.Create(info);
		}

		public IOutputImage CreateOutputImage(SizeWithDpi size)
		{
			if (Mode == GLMode.Angle)
			{
				return new AngleImage(size, _angleInterop);
			}

			//openGL and CPU will work over CpuImage (WriteableBitmap)
			return new CpuImage(size);
		}

		public void Dispose()
		{
			GrContext?.Dispose();
			GrContext = null;
			_gameWindow?.Dispose();
			_gameWindow = null;
		}
	}
}
