using Raylib_cs;
using System.Numerics;

namespace LudumDare60.Tiles;

public sealed class Steam : Trap {
    private const Single _triggerDistance = 1f;
    private const Int32 _damage = 1;
    private const Single _steamCost = 0.5f;
    private const Double _hitInterval = 0.5;

    private readonly ModelHandle _model;
    private readonly World _world;
    private readonly Tile _tile;
    private readonly Vector2 _position;

    private const Single _particlesPerSecond = 25f;
    private const Single _particleLifetime = 1.0f;

    private readonly List<Particle> _particles = [];
    private Single _spawnAccumulator;
    public SteamStore Store { get; } = new(2f);

    private Double _hitCooldown;

    public Boolean RendersOwnFloor { get => true; }

    public Steam(ResourceManager resourceManager, World world, Tile tile) {
        _model = resourceManager.GetOrLoadModel("Assets/Steam.glb");
        _world = world;
        _tile = tile;
        _position = new Vector2(tile.X + 0.5f, tile.Y + 0.5f);
    }

    public void Progress(Double deltaTime) {
        Store.Step(deltaTime);
        UpdateParticles((Single)deltaTime);

        var triggered = _world.Enemies.Any(e => e.IsAlive && Vector2.DistanceSquared(e.Position, _position) <= _triggerDistance * _triggerDistance);

        if (!triggered) {
            _hitCooldown = 0;
            return;
        }

        if (Store.Steam >= _steamCost && _world.Enemies.Any(e => e.IsAlive && IsOnHitArea(e.Position))) {
            _spawnAccumulator += (Single)deltaTime * _particlesPerSecond;
            while (_spawnAccumulator >= 1f) {
                _spawnAccumulator -= 1f;
                SpawnParticle();
            }
        }

        _hitCooldown -= deltaTime;
        if (_hitCooldown <= 0 && Store.TryConsume(_steamCost)) {
            _hitCooldown = _hitInterval;
            foreach (var enemy in _world.Enemies) {
                if (enemy.IsAlive && IsOnHitArea(enemy.Position)) {
                    enemy.HitPoints -= _damage;
                }
            }
        }
    }

    private void SpawnParticle() {
        var random = Random.Shared;
        _particles.Add(new Particle {
            Position = new Vector3((random.NextSingle() - 0.5f) * 0.6f, 0.05f, (random.NextSingle() - 0.5f) * 0.6f),
            Velocity = new Vector3((random.NextSingle() - 0.5f) * 0.2f, 0.4f + random.NextSingle() * 0.4f, (random.NextSingle() - 0.5f) * 0.2f),
            Lifetime = _particleLifetime * (0.7f + random.NextSingle() * 0.6f),
        });
    }

    private void UpdateParticles(Single dt) {
        for (var i = _particles.Count - 1; i >= 0; i--) {
            var p = _particles[i];
            p.Age += dt;
            p.Position += p.Velocity * dt;
            if (p.Age >= p.Lifetime) {
                _particles.RemoveAt(i);
            }
        }
    }

    private Boolean IsOnHitArea(Vector2 p) =>
        p.X >= _tile.X && p.X < _tile.X + 1 && p.Y >= _tile.Y && p.Y < _tile.Y + 1;

    public void Draw(Vector3 tileCentre) => Draw(tileCentre, Color.White);

    public void Draw(Vector3 tileCentre, Color tint) {
        Model model = _model;
        var placement = Matrix4x4.CreateTranslation(tileCentre);

        for (var i = 0; i < model.MeshCount; i++) {
            RayLibHelper.DrawMeshWithTint(model, i, placement, tint);
        }

        foreach (var p in _particles) {
            var t = p.Age / p.Lifetime;
            var radius = 0.06f + 0.14f * t;
            var colour = Color.White with { A = (Byte)(150 * (1f - t)) };
            Raylib.DrawSphere(tileCentre + p.Position, radius, colour);
        }
    }

    private sealed class Particle {
        public Vector3 Position;
        public Vector3 Velocity;
        public Single Age;
        public Single Lifetime;
    }
}
