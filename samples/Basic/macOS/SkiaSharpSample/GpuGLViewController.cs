using System;
using AppKit;
using Foundation;
using SkiaSharp;
using SkiaSharp.Views.Mac;

namespace SkiaSharpSample
{
	[Register("GpuGLViewController")]
	public class GpuGLViewController : NSViewController
	{
		const string sksl = @"
			uniform float iTime;
			uniform float2 iResolution;
			uniform float2 iTouchPos;
			uniform float iTouchActive;

			half4 main(float2 fragCoord) {
				float2 uv = fragCoord / iResolution;
				float aspect = iResolution.x / iResolution.y;
				float2 st = float2(uv.x * aspect, uv.y);
				float t = iTime;

				float field = 0.0;
				float3 weighted = float3(0.0);

				float3 colors[6];
				colors[0] = float3(1.0, 0.3, 0.4);
				colors[1] = float3(0.3, 0.7, 1.0);
				colors[2] = float3(1.0, 0.6, 0.1);
				colors[3] = float3(0.4, 1.0, 0.7);
				colors[4] = float3(0.7, 0.3, 1.0);
				colors[5] = float3(1.0, 0.9, 0.2);

				for (int i = 0; i < 6; i++) {
					float fi = float(i);
					float phase = fi * 1.047;
					float speed = 0.3 + fi * 0.07;

					float2 center = float2(
						aspect * 0.5 + 0.4 * sin(t * speed + phase) * cos(t * speed * 0.6 + fi),
						0.5 + 0.4 * cos(t * speed * 0.8 + phase * 1.3) * sin(t * speed * 0.4 + fi * 0.7)
					);

					float2 d = st - center;
					float r = length(d);
					float strength = 0.030 / (r * r + 0.002);
					field += strength;
					weighted += colors[i] * strength;
				}

				if (iTouchActive > 0.5) {
					float2 touchSt = float2(iTouchPos.x * aspect, iTouchPos.y);
					float2 d = st - touchSt;
					float r = length(d);
					float strength = 0.050 / (r * r + 0.002);
					field += strength;
					weighted += float3(1.0, 0.95, 0.9) * strength;
				}

				float3 blobColor = weighted / max(field, 0.001);

				float edge = smoothstep(5.0, 8.0, field);
				float innerGlow = smoothstep(8.0, 20.0, field) * 0.3;

				float3 bg = float3(0.03, 0.02, 0.08);
				bg += float3(0.02, 0.01, 0.03) * sin(t * 0.2 + uv.y * 3.0);

				float halo = smoothstep(3.0, 5.0, field) * (1.0 - edge);

				float3 result = bg;
				result += blobColor * halo * 0.4;
				result = mix(result, blobColor * (1.0 + innerGlow), edge);

				float2 vc = uv - 0.5;
				float vignette = 1.0 - dot(vc, vc) * 0.8;
				result *= vignette;

				return half4(clamp(result, 0.0, 1.0), 1.0);
			}
		";

		SKGLView? skiaView;

		NSTimer? timer;
		readonly SKRuntimeShaderBuilder builder = SKRuntimeEffect.BuildShader(sksl);
		readonly long startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		readonly SKPaint shaderPaint = new();

		float touchX = -1f, touchY = -1f;
		float touchActive;

		int tickIndex;
		long tickSum;
		readonly long[] tickList = new long[100];
		long lastTick = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

		public override void LoadView()
		{
			skiaView = new SKGLView();
			View = skiaView;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			if (skiaView != null)
				skiaView.PaintSurface += OnPaintSurface;
		}

		public override void ViewDidAppear()
		{
			base.ViewDidAppear();
			timer = NSTimer.CreateRepeatingScheduledTimer(1.0 / 60.0, _ =>
			{
				if (skiaView != null)
					skiaView.NeedsDisplay = true;
			});
		}

		public override void ViewWillDisappear()
		{
			base.ViewWillDisappear();
			if (skiaView != null)
				skiaView.PaintSurface -= OnPaintSurface;
			timer?.Invalidate();
			timer = null;
		}

		public override void MouseDown(NSEvent theEvent) => UpdateTouch(theEvent, true);
		public override void MouseDragged(NSEvent theEvent) => UpdateTouch(theEvent, true);
		public override void MouseUp(NSEvent theEvent) => UpdateTouch(theEvent, false);

		void UpdateTouch(NSEvent theEvent, bool active)
		{
			touchActive = active ? 1f : 0f;
			if (!active || View == null) return;
			var loc = View.ConvertPointFromView(theEvent.LocationInWindow, null);
			touchX = (float)(loc.X / View.Bounds.Width);
			touchY = (float)(1.0 - loc.Y / View.Bounds.Height);
		}

		void OnPaintSurface(object? sender, SKPaintGLSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			var width = e.Info.Width;
			var height = e.Info.Height;

			var elapsed = (float)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime) / 1000f;
			builder.Uniforms["iTime"] = elapsed;
			builder.Uniforms["iResolution"] = new float[] { width, height };
			builder.Uniforms["iTouchPos"] = new float[] {
				touchActive > 0 ? touchX : -1f,
				touchActive > 0 ? touchY : -1f
			};
			builder.Uniforms["iTouchActive"] = touchActive;

			using var shader = builder.Build();
			shaderPaint.Shader = shader;
			canvas.DrawRect(0, 0, width, height, shaderPaint);

			// FPS overlay
			var fps = GetCurrentFPS();
			using var bgPaint = new SKPaint { Color = new SKColor(0, 0, 0, 128) };
			canvas.DrawRect(8, 8, 90, 22, bgPaint);
			using var fpsPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
			using var fpsFont = new SKFont { Size = 14 };
			canvas.DrawText($"FPS: {fps:F0} (GL)", new SKPoint(12, 24), SKTextAlign.Left, fpsFont, fpsPaint);
		}

		double GetCurrentFPS()
		{
			var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			var delta = now - lastTick;
			lastTick = now;
			tickSum -= tickList[tickIndex];
			tickSum += delta;
			tickList[tickIndex] = delta;
			if (++tickIndex == tickList.Length)
				tickIndex = 0;
			return tickList.Length * 1000.0 / Math.Max(tickSum, 1);
		}
	}
}
