using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace SkiaSharpSample
{
	public partial class GpuPage : ContentPage
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
            0.5 + 0.4 * cos(t * speed * 0.8 + phase * 1.3) * sin(t * speed * 0.4 + fi * 0.7));
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

		readonly SKRuntimeShaderBuilder builder = SKRuntimeEffect.BuildShader(sksl);
		readonly SKPaint shaderPaint = new();
		readonly Stopwatch stopwatch = Stopwatch.StartNew();

		// FPS tracking
		readonly Queue<long> frameTicks = new();
		long lastFpsUpdate;

		// Touch state
		float touchX, touchY;
		float touchActive;

		public GpuPage()
		{
			InitializeComponent();
		}

		private void OnTouch(object sender, SKTouchEventArgs e)
		{
			switch (e.ActionType)
			{
				case SKTouchAction.Pressed:
					touchActive = 1f;
					touchX = e.Location.X;
					touchY = e.Location.Y;
					break;
				case SKTouchAction.Moved:
					touchX = e.Location.X;
					touchY = e.Location.Y;
					break;
				case SKTouchAction.Released:
				case SKTouchAction.Cancelled:
					touchActive = 0f;
					break;
			}
			e.Handled = true;
		}

		private void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			var info = e.Info;
			var width = (float)info.Width;
			var height = (float)info.Height;
			var elapsed = (float)stopwatch.Elapsed.TotalSeconds;

			builder.Uniforms["iTime"] = elapsed;
			builder.Uniforms["iResolution"] = new float[] { width, height };
			builder.Uniforms["iTouchPos"] = new float[]
			{
				touchActive > 0 ? touchX / width : -1f,
				touchActive > 0 ? touchY / height : -1f
			};
			builder.Uniforms["iTouchActive"] = touchActive;

			shaderPaint.Shader = builder.Build();
			canvas.DrawRect(0, 0, width, height, shaderPaint);

			UpdateFps();
		}

		private void UpdateFps()
		{
			var now = stopwatch.ElapsedMilliseconds;
			frameTicks.Enqueue(now);
			while (frameTicks.Count > 100)
				frameTicks.Dequeue();

			if (now - lastFpsUpdate < 250)
				return;
			lastFpsUpdate = now;

			if (frameTicks.Count < 2)
				return;

			var ticks = frameTicks.ToArray();
			var span = ticks[ticks.Length - 1] - ticks[0];
			if (span <= 0)
				return;

			var fps = (ticks.Length - 1) * 1000.0 / span;

			MainThread.BeginInvokeOnMainThread(() =>
			{
				fpsLabel.Text = $"{fps:F0} FPS";
			});
		}
	}
}
