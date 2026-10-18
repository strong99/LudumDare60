using Raylib_cs;
using System.Numerics;

namespace LudumDare60.Tiles;

public sealed class Spikes : Trap {
    private const Single _raiseSpeed = 1.0f;
    private const Single _maxHeight = 0.3f;
    private const Single _triggerDistance = 1f;
    private const Int32 _damage = 3;
    private const Single _steamCost = 0.5f;
    public const Single SteamCost = _steamCost;
    private const Double _hitInterval = 0.5;
    private const Int32 _spikesMesh = 1;

    private readonly ModelHandle _model;
    private readonly World _world;
    private readonly Tile _tile;
    private readonly Vector2 _position;

    public SteamStore Store { get; } = new(2f);

    private Double _hitCooldown;
    private Single _height = 0;

    public Boolean RendersOwnFloor { get => true; }

    public Spikes(ResourceManager resourceManager, World world, Tile tile) {
        _model = resourceManager.GetOrLoadModel("Assets/Spikes.glb");
        _world = world;
        _tile = tile;
        _position = new Vector2(tile.X + 0.5f, tile.Y + 0.5f);
    }

    public void Progress(Double deltaTime) {
        Store.Step(deltaTime);
        var triggered = _world.Enemies.Any(e => e.IsAlive && Vector2.DistanceSquared(e.Position, _position) <= _triggerDistance * _triggerDistance);

        if (triggered && _height < _maxHeight) {
            _height = Math.Min(_maxHeight, _height + (Single)deltaTime * _raiseSpeed);
        }
        else if (!triggered && _height > 0) {
            _height = Math.Max(0, _height - (Single)deltaTime * _raiseSpeed);
        }

        if (_height >= _maxHeight) {
            _hitCooldown -= deltaTime;
            if (_hitCooldown <= 0 && _world.Enemies.Any(e => e.IsAlive && IsOnHitArea(e.Position)) && Store.TryConsume(_steamCost)) {
                _hitCooldown = _hitInterval;
                foreach (var enemy in _world.Enemies) {
                    if (enemy.IsAlive && IsOnHitArea(enemy.Position)) {
                        enemy.HitPoints -= _damage;
                    }
                }
            }
        }
        else {
            _hitCooldown = 0;
        }
    }

    private Boolean IsOnHitArea(Vector2 p) =>
        p.X >= _tile.X && p.X < _tile.X + 1 && p.Y >= _tile.Y && p.Y < _tile.Y + 1;

    public void Draw(Vector3 tileCentre) => Draw(tileCentre, Color.White);

    public void Draw(Vector3 tileCentre, Color tint) {
        Model model = _model;
        var placement = Matrix4x4.CreateTranslation(tileCentre);
        var spikesTransform = Matrix4x4.CreateTranslation(0, _height - _maxHeight, 0) * placement;

        for (var i = 0; i < model.MeshCount; i++) {
            RayLibHelper.DrawMeshWithTint(model, i, i == _spikesMesh ? spikesTransform : placement, tint);
        }
    }
}
