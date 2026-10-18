using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60;

public sealed class SsaoPass(Single fovY) : IDisposable {
    private const Double _near = 0.1;
    private const Double _far = 100.0;

    private const String _headerDesktop = "#version 330\nout vec4 finalColor;\n";
    // The web build of raylib targets GLES2, so the shader has to be GLSL ES 1.00.
    private const String _headerEs = "#version 100\n#extension GL_OES_standard_derivatives : enable\nprecision highp float;\n#define in varying\n#define texture texture2D\n#define finalColor gl_FragColor\n";

    private const String _fragmentShader = """
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
        uniform vec2 uTanHalfFov;

        float linearDepth(vec2 uv) {
            float z = texture(uDepth, uv).r * 2.0 - 1.0;
            return 2.0 * uNear * uFar / (uFar + uNear - z * (uFar - uNear));
        }

        vec3 viewPos(vec2 uv, float depth) {
            vec2 ndc = uv * 2.0 - 1.0;
            return vec3(ndc * uTanHalfFov * depth, -depth);
        }

        float hash(vec2 p) {
            return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453);
        }

        void main() {
            if (texture(uDepth, fragTexCoord).r >= 1.0) {
                finalColor = vec4(1.0);
                return;
            }

            float centre = linearDepth(fragTexCoord);
            float radiusPixels = clamp(uRadius * uPixelsPerUnit / centre, 2.0, 64.0);
            float angle = hash(gl_FragCoord.xy) * 6.2831853;

            vec3 pos = viewPos(fragTexCoord, centre);
            // Screen-space derivatives blow up at silhouettes and draw crawling outlines, so build the tangents
            // from whichever neighbour on each axis is closer in depth.
            vec3 posR = viewPos(fragTexCoord + vec2(uTexel.x, 0.0), linearDepth(fragTexCoord + vec2(uTexel.x, 0.0)));
            vec3 posL = viewPos(fragTexCoord - vec2(uTexel.x, 0.0), linearDepth(fragTexCoord - vec2(uTexel.x, 0.0)));
            vec3 posU = viewPos(fragTexCoord + vec2(0.0, uTexel.y), linearDepth(fragTexCoord + vec2(0.0, uTexel.y)));
            vec3 posD = viewPos(fragTexCoord - vec2(0.0, uTexel.y), linearDepth(fragTexCoord - vec2(0.0, uTexel.y)));
            vec3 tangentX = abs(posR.z - pos.z) < abs(posL.z - pos.z) ? posR - pos : pos - posL;
            vec3 tangentY = abs(posU.z - pos.z) < abs(posD.z - pos.z) ? posU - pos : pos - posD;
            vec3 normal = normalize(cross(tangentX, tangentY));
            if (dot(normal, pos) > 0.0) {
                normal = -normal;
            }

            const int samples = 16;
            float occlusion = 0.0;
            for (int i = 0; i < samples; i++) {
                float t = (float(i) + 0.5) / float(samples);
                float a = angle + float(i) * 2.399963;
                vec2 offset = vec2(cos(a), sin(a)) * sqrt(t) * radiusPixels * uTexel;
                vec2 sampleUv = fragTexCoord + offset;
                float sampleDepth = linearDepth(sampleUv);
                vec3 toSample = viewPos(sampleUv, sampleDepth) - pos;
                float above = dot(normal, toSample) / max(length(toSample), 0.0001); // 1: directly above the surface
                // Full weight within the radius, fading to nothing by twice the radius, so distant objects don't halo the background.
                float rangeCheck = 1.0 - smoothstep(uRadius, uRadius * 2.0, abs(centre - sampleDepth));
                occlusion += (above > 0.15 ? 1.0 : 0.0) * rangeCheck;
            }

            float ao = 1.0 - uStrength * occlusion / float(samples);
            finalColor = vec4(ao, ao, ao, 1.0);
        }
        """;

    private const String _blurShader = """
        in vec2 fragTexCoord;
        in vec4 fragColor;

        uniform sampler2D texture0;
        uniform sampler2D uDepth;
        uniform sampler2D uAo;
        uniform vec2 uTexel;
        uniform float uNear;
        uniform float uFar;

        float linearDepth(vec2 uv) {
            float z = texture(uDepth, uv).r * 2.0 - 1.0;
            return 2.0 * uNear * uFar / (uFar + uNear - z * (uFar - uNear));
        }

        vec3 steampunk(vec3 c) {
            float l = dot(c, vec3(0.299, 0.587, 0.114));
            vec3 soot = vec3(0.07, 0.05, 0.04);
            vec3 copper = vec3(0.50, 0.28, 0.14);
            vec3 brass = vec3(0.83, 0.62, 0.30);
            vec3 parchment = vec3(1.0, 0.91, 0.72);
            vec3 g = mix(soot, copper, smoothstep(0.0, 0.35, l));
            g = mix(g, brass, smoothstep(0.35, 0.7, l));
            g = mix(g, parchment, smoothstep(0.7, 1.0, l));
            return mix(c, g, 0.85);
        }

        void main() {
            vec4 colour = texture(texture0, fragTexCoord);
            if (texture(uDepth, fragTexCoord).r >= 1.0) {
                finalColor = vec4(steampunk(colour.rgb), colour.a);
                return;
            }

            float centre = linearDepth(fragTexCoord);
            float threshold = 0.05 + 0.02 * centre;
            float sum = 0.0;
            float weights = 0.0;
            for (int y = -1; y <= 1; y++) {
                for (int x = -1; x <= 1; x++) {
                    vec2 uv = fragTexCoord + vec2(float(x), float(y)) * uTexel;
                    float w = 1.0 - smoothstep(0.0, threshold, abs(linearDepth(uv) - centre));
                    sum += texture(uAo, uv).r * w;
                    weights += w;
                }
            }

            finalColor = vec4(steampunk(colour.rgb * (sum / weights)), colour.a);
        }
        """;

    private RenderTexture2D _target;
    private RenderTexture2D _aoTarget;
    private Texture2D _depth;
    private Shader _shader;
    private Shader _blur;
    private Int32 _blurDepthLoc, _blurAoLoc, _blurTexelLoc, _blurNearLoc, _blurFarLoc;
    private Int32 _width, _height;
    private Int32 _depthLoc, _texelLoc, _nearLoc, _farLoc, _pixelsPerUnitLoc, _radiusLoc, _strengthLoc, _tanHalfFovLoc;

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
        _aoTarget = LoadRenderTexture(width, height);

        if (_shader.Id == 0) {
            _shader = LoadShaderFromMemory(null, (OperatingSystem.IsBrowser() ? _headerEs : _headerDesktop) + _fragmentShader);
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
            _tanHalfFovLoc = GetShaderLocation(_shader, "uTanHalfFov");
        }

        if (_blur.Id == 0) {
            _blur = LoadShaderFromMemory(null, (OperatingSystem.IsBrowser() ? _headerEs : _headerDesktop) + _blurShader);
            if (!IsShaderValid(_blur)) {
                throw new Exception("Failed to compile SSAO blur shader");
            }
            _blurDepthLoc = GetShaderLocation(_blur, "uDepth");
            _blurAoLoc = GetShaderLocation(_blur, "uAo");
            _blurTexelLoc = GetShaderLocation(_blur, "uTexel");
            _blurNearLoc = GetShaderLocation(_blur, "uNear");
            _blurFarLoc = GetShaderLocation(_blur, "uFar");
        }
    }

    public void Begin() {
        EnsureResources();
        Rlgl.SetClipPlanes(_near, _far);
        BeginTextureMode(_target);
        ClearBackground(new Color(200, 170, 130, 255));
    }

    public void End() {
        EndTextureMode();

        var pixelsPerUnit = _height / (MathF.Tan(fovY * MathF.PI / 360f) * 2f);
        var tanHalfFovY = MathF.Tan(fovY * MathF.PI / 360f);
        SetShaderValue(_shader, _tanHalfFovLoc, new Vector2(tanHalfFovY * _width / _height, tanHalfFovY), ShaderUniformDataType.Vec2);
        SetShaderValue(_shader, _texelLoc, new Vector2(1f / _width, 1f / _height), ShaderUniformDataType.Vec2);
        SetShaderValue(_shader, _nearLoc, (Single)_near, ShaderUniformDataType.Float);
        SetShaderValue(_shader, _farLoc, (Single)_far, ShaderUniformDataType.Float);
        SetShaderValue(_shader, _pixelsPerUnitLoc, pixelsPerUnit, ShaderUniformDataType.Float);
        SetShaderValue(_shader, _radiusLoc, Radius, ShaderUniformDataType.Float);
        SetShaderValue(_shader, _strengthLoc, Strength, ShaderUniformDataType.Float);

        var source = new Rectangle(0, 0, _width, -_height);

        BeginTextureMode(_aoTarget);
        BeginShaderMode(_shader);
        SetShaderValueTexture(_shader, _depthLoc, _depth);
        DrawTextureRec(_target.Texture, source, Vector2.Zero, Color.White);
        EndShaderMode();
        EndTextureMode();

        SetShaderValue(_blur, _blurTexelLoc, new Vector2(1f / _width, 1f / _height), ShaderUniformDataType.Vec2);
        SetShaderValue(_blur, _blurNearLoc, (Single)_near, ShaderUniformDataType.Float);
        SetShaderValue(_blur, _blurFarLoc, (Single)_far, ShaderUniformDataType.Float);

        BeginShaderMode(_blur);
        SetShaderValueTexture(_blur, _blurDepthLoc, _depth);
        SetShaderValueTexture(_blur, _blurAoLoc, _aoTarget.Texture);
        DrawTextureRec(_target.Texture, source, Vector2.Zero, Color.White);
        EndShaderMode();
    }

    private void UnloadTarget() {
        if (_target.Id == 0) {
            return;
        }
        Rlgl.UnloadTexture(_depth.Id);
        UnloadRenderTexture(_target);
        UnloadRenderTexture(_aoTarget);
        _target = default;
        _aoTarget = default;
    }

    public void Dispose() {
        UnloadTarget();
        if (_shader.Id != 0) {
            UnloadShader(_shader);
            _shader = default;
        }
        if (_blur.Id != 0) {
            UnloadShader(_blur);
            _blur = default;
        }
    }
}
