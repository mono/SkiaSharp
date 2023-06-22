using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Tizen.NUI;
using NGeometry = Tizen.NUI.Geometry;

namespace SkiaSharp.Views.Tizen.NUI
{
    public class SKGLSurfaceView : CustomRenderingView
    {
        static readonly string VERTEX_SHADER =
            "attribute mediump vec2 aPosition;\n" +
            "varying mediump vec2 vTexCoord;\n" +
            "uniform highp mat4 uMvpMatrix;\n" +
            "uniform mediump vec3 uSize;\n" +
            "varying mediump vec2 sTexCoordRect;\n" +
            "void main()\n" +
            "{\n" +
            "   gl_Position = uMvpMatrix * vec4(aPosition * uSize.xy, 0.0, 1.0);\n" +
            "   vTexCoord = aPosition + vec2(0.5);\n" +
            "}\n";

        static readonly string FRAGMENT_SHADER =
            "#extension GL_OES_EGL_image_external:require\n" +
            "uniform lowp vec4 uColor;\n" +
            "varying mediump vec2 vTexCoord;\n" +
            "uniform samplerExternalOES sTexture;\n" +
            "void main()\n" +
            "{\n" +
            "   gl_FragColor = texture2D(sTexture, vTexCoord) * uColor;\n" +
            "}\n";

        NativeImageQueue? _nativeImageSource;

        int _bufferWidth = 0;
        int _bufferHeight = 0;
        int _bufferStride = 0;

        Renderer? _renderer;
        NGeometry _geometry;
        Shader _shader;
        Texture? _texture;
        TextureSet? _textureSet;

        public SKGLSurfaceView()
        {
            _geometry = CreateQuadGeometry();
            _shader = new Shader(VERTEX_SHADER, FRAGMENT_SHADER);
            OnResized();
        }

        protected override void OnDrawFrame()
        {
            if (Size.Width == 0 || Size.Height == 0)
                return;

            if (_nativeImageSource?.CanDequeueBuffer() ?? false)
            {
                var buffer = _nativeImageSource!.DequeueBuffer(ref _bufferWidth, ref _bufferHeight, ref _bufferStride);
                Debug.Assert(buffer != IntPtr.Zero, "AcquireBuffer is faild");

                var info = new SKImageInfo(_bufferWidth, _bufferHeight);
                using (var surface = SKSurface.Create(info, buffer, _bufferStride))
                {
                    // draw using SkiaSharp
                    SendPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
                    surface.Canvas.Flush();
                }
                _nativeImageSource.EnqueueBuffer(buffer);
                Window.Instance.KeepRendering(0);
            }
            else
            {
                Invalidate();
            }
        }

        protected override void OnResized()
        {
            if (Size.Width == 0 || Size.Height == 0)
                return;

            UpdateSurface();
            OnDrawFrame();
            UpdateTexture();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _nativeImageSource?.Dispose();
                _nativeImageSource = null;
            }
            base.Dispose(disposing);
        }

        void UpdateSurface()
        {
            _nativeImageSource?.Dispose();
			_nativeImageSource = new NativeImageQueue((uint)Size.Width, (uint)Size.Height, NativeImageQueue.ColorFormat.BGRA8888);
        }

        void UpdateTexture()
        {
            _texture?.Dispose();
            _textureSet?.Dispose();
            _texture = new Texture(_nativeImageSource);
            _textureSet = new TextureSet();
            _textureSet.SetTexture(0, _texture);
            
            if (_renderer == null)
            {
                _renderer = new Renderer(_geometry, _shader);
                AddRenderer(_renderer);
            }
            _renderer.SetTextures(_textureSet);
        }

        static NGeometry CreateQuadGeometry()
        {
            PropertyBuffer vertexData = CreateVertextBuffer();

            TexturedQuadVertex vertex1 = new TexturedQuadVertex();
            TexturedQuadVertex vertex2 = new TexturedQuadVertex();
            TexturedQuadVertex vertex3 = new TexturedQuadVertex();
            TexturedQuadVertex vertex4 = new TexturedQuadVertex();
            vertex1.position = new Vec2(-0.5f, -0.5f);
            vertex2.position = new Vec2(-0.5f, 0.5f);
            vertex3.position = new Vec2(0.5f, -0.5f);
            vertex4.position = new Vec2(0.5f, 0.5f);


            TexturedQuadVertex[] texturedQuadVertexData = new TexturedQuadVertex[4] { vertex1, vertex2, vertex3, vertex4 };

            int lenght = Marshal.SizeOf(vertex1);
            IntPtr pA = Marshal.AllocHGlobal(lenght * 4);

            for (int i = 0; i < 4; i++)
            {
                Marshal.StructureToPtr(texturedQuadVertexData[i], pA + i * lenght, true);
            }
            vertexData.SetData(pA, 4);


            NGeometry geometry = new NGeometry();
            geometry.AddVertexBuffer(vertexData);
            geometry.SetType(NGeometry.Type.TRIANGLE_STRIP);
            return geometry;
        }

        static PropertyBuffer CreateVertextBuffer()
        {
            PropertyMap vertexFormat = new PropertyMap();
            vertexFormat.Add("aPosition", new PropertyValue((int)PropertyType.Vector2));
            return new PropertyBuffer(vertexFormat);
        }

        struct TexturedQuadVertex
        {
            public Vec2 position;
        };

        [StructLayout(LayoutKind.Sequential)]
        struct Vec2
        {
            float x;
            float y;
            public Vec2(float xIn, float yIn)
            {
                x = xIn;
                y = yIn;
            }
        }

    }
}
