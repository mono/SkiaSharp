using SkiaSharp;
using SkiaSharp.Views.Blazor;

namespace SkiaSharpSample.Pages;

/// <summary>
/// GPU Canvas Demo — pure SkSL lava lamp with 6 metaballs, touch interaction,
/// and SKRuntimeEffect.BuildShader() + Uniforms + Build() caching pattern.
/// </summary>
public partial class GPU
{
    // SkSL shader source: a "lava lamp" metaball effect.
    // 6 colored blobs orbit on Lissajous curves. When the user touches,
    // an extra white-hot blob appears at the touch position and merges
    // with the others. All rendering is per-pixel in the shader.
    const string sksl = """
        // Uniforms passed from C# each frame
        uniform float iTime;          // elapsed seconds
        uniform float2 iResolution;   // canvas size in pixels
        uniform float2 iTouchPos;     // touch position normalized 0..1
        uniform float iTouchActive;   // 1.0 when touching, 0.0 otherwise

        half4 main(float2 fragCoord) {
            // Convert pixel coords to normalized UV and aspect-corrected coords
            float2 uv = fragCoord / iResolution;
            float aspect = iResolution.x / iResolution.y;
            float2 st = float2(uv.x * aspect, uv.y);
            float t = iTime;

            // Metaball field: accumulate 1/r² contributions from each blob.
            // 'weighted' tracks color contribution weighted by field strength.
            float field = 0.0;
            float3 weighted = float3(0.0);

            // Color palette for the 6 orbiting blobs
            float3 colors[6];
            colors[0] = float3(1.0, 0.3, 0.4);    // hot pink
            colors[1] = float3(0.3, 0.7, 1.0);    // sky blue
            colors[2] = float3(1.0, 0.6, 0.1);    // orange
            colors[3] = float3(0.4, 1.0, 0.7);    // mint
            colors[4] = float3(0.7, 0.3, 1.0);    // purple
            colors[5] = float3(1.0, 0.9, 0.2);    // yellow

            // Each blob orbits on a Lissajous curve with unique phase and speed
            for (int i = 0; i < 6; i++) {
                float fi = float(i);
                float phase = fi * 1.047;   // evenly spaced: 2*pi/6
                float speed = 0.3 + fi * 0.07;

                // Lissajous orbit center
                float2 center = float2(
                    aspect * 0.5 + 0.4 * sin(t * speed + phase) * cos(t * speed * 0.6 + fi),
                    0.5 + 0.4 * cos(t * speed * 0.8 + phase * 1.3) * sin(t * speed * 0.4 + fi * 0.7)
                );

                // Metaball field: strength falls off as 1/r², creating smooth merging
                float2 d = st - center;
                float r = length(d);
                float strength = 0.030 / (r * r + 0.002);
                field += strength;
                weighted += colors[i] * strength;
            }

            // Touch interaction: add an extra, larger blob at the touch position
            if (iTouchActive > 0.5) {
                float2 touchSt = float2(iTouchPos.x * aspect, iTouchPos.y);
                float2 d = st - touchSt;
                float r = length(d);
                float strength = 0.050 / (r * r + 0.002);
                field += strength;
                // white-hot touch blob
                weighted += float3(1.0, 0.95, 0.9) * strength;
            }

            // Normalize accumulated color by field strength for smooth blending
            float3 blobColor = weighted / max(field, 0.001);

            // Threshold the field into blob edges with smooth falloff
            float edge = smoothstep(5.0, 8.0, field);
            float innerGlow = smoothstep(8.0, 20.0, field) * 0.3;

            // Subtly animated dark background
            float3 bg = float3(0.03, 0.02, 0.08);
            bg += float3(0.02, 0.01, 0.03) * sin(t * 0.2 + uv.y * 3.0);

            // Outer glow: visible between the halo and solid edge
            float halo = smoothstep(3.0, 5.0, field) * (1.0 - edge);

            // Composite: background + halo + solid blobs with inner glow
            float3 result = bg;
            result += blobColor * halo * 0.4;
            result = mix(result, blobColor * (1.0 + innerGlow), edge);

            // Vignette: darken corners for depth
            float2 vc = uv - 0.5;
            float vignette = 1.0 - dot(vc, vc) * 0.8;
            result *= vignette;

            return half4(clamp(result, 0.0, 1.0), 1.0);
        }
        """;

    // SKRuntimeShaderBuilder: compiled once via BuildShader(), then reused.
    // Each frame we update uniforms and call Build() to get a new SKShader.
    SKRuntimeShaderBuilder builder = SKRuntimeEffect.BuildShader(sksl);
    long startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    // Touch state passed to the shader as uniforms
    float touchX = -1f, touchY = -1f;
    float touchActive = 0f;
    double displayFps;
    long lastUiUpdate;

    // Rolling average FPS calculation
    int tickIndex = 0;
    long tickSum = 0;
    long[] tickList = new long[100];
    long lastTick = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    // Handle touch/mouse: track position while pressed, clear on release
    void OnTouch(SKTouchEventArgs e)
    {
        if (e.ActionType == SKTouchAction.Pressed || e.ActionType == SKTouchAction.Moved)
        {
            touchX = e.Location.X;
            touchY = e.Location.Y;
            touchActive = 1f;
            e.Handled = true;
        }
        else if (e.ActionType == SKTouchAction.Released || e.ActionType == SKTouchAction.Cancelled)
        {
            touchActive = 0f;
            e.Handled = true;
        }
    }

    // Called every frame by SKGLView's render loop
    void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var width = e.Info.Width;
        var height = e.Info.Height;

        // Update uniforms — no allocations, reuses the builder's internal storage
        var elapsed = (float)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime) / 1000f;
        builder.Uniforms["iTime"] = elapsed;
        builder.Uniforms["iResolution"] = new float[] { width, height };
        builder.Uniforms["iTouchPos"] = new float[] {
            touchActive > 0 ? touchX / width : -1f,
            touchActive > 0 ? touchY / height : -1f
        };
        builder.Uniforms["iTouchActive"] = touchActive;

        // Build() creates a shader from cached effect + current uniforms
        // Draw a full-screen quad to run the shader on every pixel
        using var shader = builder.Build();
        using var paint = new SKPaint();
        paint.Shader = shader;
        canvas.DrawRect(0, 0, width, height, paint);

        displayFps = GetCurrentFPS();

        // Throttle Blazor UI updates to ~4Hz for the FPS counter
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (now - lastUiUpdate > 250)
        {
            lastUiUpdate = now;
            _ = InvokeAsync(StateHasChanged);
        }
    }

    double GetCurrentFPS()
    {
        var newTick = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var delta = newTick - lastTick;
        lastTick = newTick;
        tickSum -= tickList[tickIndex];
        tickSum += delta;
        tickList[tickIndex] = delta;
        if (++tickIndex == tickList.Length) tickIndex = 0;
        return 1000.0 / ((double)tickSum / tickList.Length);
    }
}
