using LudumDare60.Enemies;
using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.Tiles;

public sealed unsafe class Crusher : Trap, MultiTileContent {
    public const String FileName = "Assets/Crusher.glb";

    private const Single _extendSpeed = 2.0f;
    private const Single _maxExtension = 1.0f;
    private const Single _triggerDistance = 1f;
    private const Single _revolutionsPerSecond = 0.5f;
    private const Int32 _damage = 5;

    private const Int32 _crusherMesh = 0;
    private const Int32 _gearMesh = 1;

    private readonly ModelHandle _model;
    private readonly EdgeSide _bodySide;
    private readonly World _world;
    private readonly Vector2 _position;
    private readonly Tile _tile;
    private readonly HashSet<Enemy> _hitThisStroke = [];

    private Double _playDuration;
    private Single _extension = 0;
    private Boolean _damageApplied = false;

    public IReadOnlyList<Tile> Members { get; }

    public Crusher(ResourceManager resourceManager, World world, Tile tile, Tile body, EdgeSide bodySide) {
        Members = [tile, body];
        _bodySide = bodySide;
        _model = resourceManager.GetOrLoadModel(FileName);

        _world = world;
        _tile = tile;
        _position = new Vector2(tile.X + 0.5f, tile.Y + 0.5f);
    }

    public void Progress(Double deltaTime) {
        var triggered = _world.Enemies.Any(e => e.IsAlive && Vector2.DistanceSquared(e.Position, _position) <= _triggerDistance * _triggerDistance);

        if (triggered && _extension < _maxExtension) {
            _extension = Math.Min(_maxExtension, _extension + (Single)deltaTime * _extendSpeed);
        }
        else if (!triggered && _extension > 0) {
            _extension = Math.Max(0, _extension - (Single)deltaTime * _extendSpeed);
        }

        if (!_damageApplied && _extension > _maxExtension - 0.01f) {
            foreach (var enemy in _world.Enemies) {
                if (enemy.IsAlive && IsOnHitArea(enemy.Position) && _hitThisStroke.Add(enemy)) {
                    enemy.HitPoints -= _damage;
                }
            }
        }
        else if (_extension <= 0) {
            _hitThisStroke.Clear();
        }

        _playDuration += deltaTime;
    }

    private Boolean IsOnHitArea(Vector2 p) =>
        p.X >= _tile.X && p.X < _tile.X + 1 && p.Y >= _tile.Y && p.Y < _tile.Y + 1;

    public void Draw(Vector3 tileCentre) {
        var angle = (Single)(_playDuration * _revolutionsPerSecond % 1.0 * MathF.Tau);
        Draw(_model, tileCentre, _bodySide, _extension, angle, Color.White);
    }

    public static void Draw(Model model, Vector3 tileCentre, EdgeSide bodySide, Single extension, Single gearAngle, Color tint) {
        var rotation = Matrix4x4.CreateRotationY(bodySide switch {
            EdgeSide.West => 0f,
            EdgeSide.East => MathF.PI,
            EdgeSide.North => -MathF.PI / 2,
            _ => MathF.PI / 2,
        });
        var bounds = GetMeshBoundingBox(model.Meshes[_gearMesh]);
        var gearPivot = (bounds.Min + bounds.Max) / 2f;

        var crusherTransform = Matrix4x4.CreateTranslation(extension, 0, 0) * rotation * Matrix4x4.CreateTranslation(tileCentre);
        var gearTransform = Matrix4x4.CreateTranslation(-gearPivot)
            * Matrix4x4.CreateRotationZ(gearAngle)
            * Matrix4x4.CreateTranslation(gearPivot)
            * crusherTransform;

        RayLibHelper.DrawMeshWithTint(model, _crusherMesh, crusherTransform, tint);
        RayLibHelper.DrawMeshWithTint(model, _gearMesh, gearTransform, tint);
    }
}
