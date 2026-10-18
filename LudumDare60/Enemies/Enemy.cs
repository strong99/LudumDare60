using System.Numerics;

namespace LudumDare60.Enemies;

public sealed class Enemy {
    private Vector2? _lastPosition;
    private Boolean _attackedThisFrame;
    public Weapon Weapon { get; set; } = Weapon.Sword;
    public Boolean IsWalking { get; private set; }
    public Single WalkPhase { get; private set; }
    public Single AttackTime { get; private set; } = -1;
    public Single DeathTime { get; private set; } = -1;
    public required EnemyKind Kind { get; init; }
    public EnemyBehaviour? Behaviour { get; set; }
    public required Memory Memory { get; init; }
    public Int32 HitPoints { get; set => field = Math.Max(0, value); }
    public Boolean IsAlive { get => HitPoints > 0; }
    public Vector2 Position { get; set; }
    public Single FacingRotation { get; set; }

    public void Progress(Double deltaTime) {
        var before = Position;
        Behaviour?.Progress(deltaTime);

        var moved = (Position - (_lastPosition ?? before)).Length();
        _lastPosition = Position;
        IsWalking = moved > 0.0001f;
        WalkPhase += moved;

        AttackTime = _attackedThisFrame ? Math.Max(0, AttackTime) + (Single)deltaTime : -1;
        _attackedThisFrame = false;

        DeathTime = IsAlive ? -1 : Math.Max(0, DeathTime) + (Single)deltaTime;
    }
    public void Attack() => _attackedThisFrame = true;
    public void Draw() {
        GoblinModel.Draw(Kind, new Vector3(Position.X, 0, Position.Y), FacingRotation, Weapon, WalkPhase, IsWalking, AttackTime, DeathTime);
    }
}
