using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60;

public sealed class SsaoPass(Single fovY) : IDisposable {
    private const Double _near = 0.1;
    private const Double _far = 100.0;

    private const String _fragmentShader = """
        #version 330
        in vec2 fragTexCoord;
        in vec4 fragColor;

        uniform sampler2D texture0;
        uniform sampler2D uDepth;
        uniform vec2 uTexel;
        uniform float uNear;
        uniform float uFar;
        uniform float uPixelsPerUnit; 
        uniform float uRadius;
        uniform float uStrength;

        out vec4 finalColor;

        float linearDepth(vec2 uv) {
            float z = texture(uDepth, uv).r * 2.0 - 1.0;
            return 2.0 * uNear * uFar / (uFar + uNear - z * (uFar - uNear));
        }

        float hash(vec2 p) {
            return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453);
        }

        void main() {
            vec4 colour = texture(texture0, fragTexCoord);
            if (texture(uDepth, fragTexCoord).r >= 1.0) {
                finalColor = colour;
                return;
            }

            float centre = linearDepth(fragTexCoord);
            float radiusPixels = clamp(uRadius * uPixelsPerUnit / centre, 2.0, 64.0);
            float angle = hash(gl_FragCoord.xy) * 6.2831853;

            const int samples = 16;
            float occlusion = 0.0;
            for (int i = 0; i < samples; i++) {
                float t = (float(i) + 0.5) / float(samples);
                float a = angle + float(i) * 2.399963; // golden angle spiral
                vec2 offset = vec2(cos(a), sin(a)) * sqrt(t) * radiusPixels * uTexel;
                float sampleDepth = linearDepth(fragTexCoord + offset);
                float diff = centre - sampleDepth; // > 0: sample is nearer the camera
                float rangeCheck = smoothstep(0.0, 1.0, uRadius / max(abs(diff), 0.0001));
                occlusion += (diff > 0.03 ? 1.0 : 0.0) * rangeCheck;
            }

            float ao = 1.0 - uStrength * occlusion / float(samples);
            finalColor = vec4(colour.rgb * ao, colour.a);
        }
        """;

    private RenderTexture2D _target;
    private Texture2D _depth;
    private Shader _shader;
    private Int32 _width, _height;
    private Int32 _depthLoc, _texelLoc, _nearLoc, _farLoc, _pixelsPerUnitLoc, _radiusLoc, _strengthLoc;

    public Single Radius { get; set; } = 0.6f;
    public Single Strength { get; set; } = 1.2f;

    private void EnsureResources() {
        var width = GetScreenWidth();
        var height = GetScreenHeight();

        if (_target.Id != 0 && width == _width && height == _height) {
            return;
        }

        UnloadTarget();
        
        _width = width;
        _height = height;

        var depthId = Rlgl.LoadTextureDepth(width, height, false);

        _target = LoadRenderTexture(width, height);
        
        Rlgl.FramebufferAttach(_target.Id, depthId, FramebufferAttachType.Depth, FramebufferAttachTextureType.Texture2D, 0);
        if (!Rlgl.FramebufferComplete(_target.Id)) {
            throw new Exception("SSAO framebuffer is incomplete");
        }
        _depth = new Texture2D { Id = depthId, Width = width, Height = height, Mipmaps = 1 };

        if (_shader.Id == 0) {
            _shader = LoadShaderFromMemory(null, _fragmentShader);
            if (!IsShaderValid(_shader)) {
                throw new Exception("Failed to compile SSAO shader");
            }
            _depthLoc = GetShaderLocation(_shader, "uDepth");
            _texelLoc = GetShaderLocation(_shader, "uTexel");
            _nearLoc = GetShaderLocation(_shader, "uNear");
            _farLoc = GetShaderLocation(_shader, "uFar");
            _pixelsPerUnitLoc = GetShaderLocation(_shader, "uPixelsPerUnit");
            _radiusLoc = GetShaderLocation(_shader, "uRadius");
            _strengthLoc = GetShaderLocation(_shader, "uStrength");
        }
    }

    public void Begin() {
        EnsureResources();
        Rlgl.SetClipPlanes(_near, _far);
        BeginTextureMode(_target);
        ClearBackground(Color.RayWhite);
    }

    public void End() {
        EndTextureMode();

        var pixelsPerUnit = _height / (MathF.Tan(fovY * MathF.PI / 360f) * 2f);
        SetShaderValue(_shader, _texelLoc, new Vector2(1f / _width, 1f / _height), ShaderUniformDataType.Vec2);
        SetShaderValue(_shader, _nearLoc, (Single)_near, ShaderUniformDataType.Float);
        SetShaderValue(_shader, _farLoc, (Single)_far, ShaderUniformDataType.Float);
        SetShaderValue(_shader, _pixelsPerUnitLoc, pixelsPerUnit, ShaderUniformDataType.Float);
        SetShaderValue(_shader, _radiusLoc, Radius, ShaderUniformDataType.Float);
        SetShaderValue(_shader, _strengthLoc, Strength, ShaderUniformDataType.Float);

        BeginShaderMode(_shader);
        SetShaderValueTexture(_shader, _depthLoc, _depth);
        DrawTextureRec(_target.Texture, new Rectangle(0, 0, _width, -_height), Vector2.Zero, Color.White);
        EndShaderMode();
    }

    private void UnloadTarget() {
        if (_target.Id == 0) {
            return;
        }
        Rlgl.UnloadTexture(_depth.Id);
        UnloadRenderTexture(_target);
        _target = default;
    }

    public void Dispose() {
        UnloadTarget();
        if (_shader.Id != 0) {
            UnloadShader(_shader);
            _shader = default;
        }
    }
}
