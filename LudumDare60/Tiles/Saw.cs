using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.Tiles;

public sealed unsafe class Saw : Trap {
    private const Single _raiseSpeed = 1.0f;
    private const Single _scale = 1.0f;
    private const Single _triggerDistance = 1f;
    private const Single _revolutionsPerSecond = 2f;
    private const String _fileName = "Assets/Saws.glb";

    private const Int32 _firstBladeMesh = 1;

    private readonly ModelHandle _model;
    private readonly Vector3[] _pivots;
    private readonly World _world;
    private readonly Vector2 _position;

    private Boolean _playing;
    private Double _playDuration;
    private Boolean _triggered;
    private Single _height = 0;

    public Saw(ResourceManager resourceManager, World world, Tile tile) {
        _model = resourceManager.GetOrLoadModel(_fileName);

        Model model = _model;
        _pivots = new Vector3[model.MeshCount];
        for (var i = 0; i < model.MeshCount; i++) {
            var bounds = GetMeshBoundingBox(model.Meshes[i]);
            _pivots[i] = (bounds.Min + bounds.Max) / 2f;

            model.Materials[model.MeshMaterial[i]].Maps[(Int32)MaterialMapIndex.Albedo].Color = Color.Beige;
        }

        _world = world;
        _position = new Vector2(tile.X + 0.5f, tile.Y + 0.5f);
    }

    private const Single _maxHeight = 0.3f;
    public void Progress(Double deltaTime) {
        _triggered = _world.Enemies.Any(e => Vector2.DistanceSquared(e.Position, _position) <= _triggerDistance * _triggerDistance);

        if (_triggered && _height < _maxHeight) {
            _height = Math.Min(_maxHeight, _height + (Single)deltaTime * _raiseSpeed);
        }
        else if (!_triggered && _height > 0) {
            _height = Math.Max(0, _height - (Single)deltaTime * _raiseSpeed);
        }

        _playDuration += deltaTime;
    }

    public Boolean RendersOwnFloor { get => true; }

    public void Draw(Vector3 tileCentre) => Draw(tileCentre, Color.White);

    public void Draw(Vector3 tileCentre, Color tint) {
        Model model = _model;
        var angle = (Single)(_playDuration * _revolutionsPerSecond % 1.0 * MathF.Tau);
        var placement = Matrix4x4.CreateScale(_scale) * Matrix4x4.CreateTranslation(tileCentre);
        var sawBob = _triggered ? (Single)Math.Sin(_playDuration) / 10f : 0f;
        var offset = new Vector3(0, _height + sawBob, 0);
         
        for (var i = 0; i < model.MeshCount; i++) {
            var transform = placement;
            if (i >= _firstBladeMesh) {
                var pivot = _pivots[i];
                transform = Matrix4x4.CreateTranslation(-pivot)
                    * Matrix4x4.CreateRotationX(angle)
                    * Matrix4x4.CreateTranslation(pivot + offset)
                    * placement;
            }

            RayLibHelper.DrawMeshWithTint(model, i, transform, tint);
        }
    }
}
