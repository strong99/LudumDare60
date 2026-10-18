using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.Tiles;

public sealed class TowerCore : MultiTileContent, Damageable {
    public const Int32 Length = 3;
    public const Int32 Thickness = 2;

    private static readonly Vector3 _modelOffset = new(0, 0, 0.5f);

    public Boolean RendersOwnFloor { get => true; }
    public Boolean BlocksMovement { get => true; }
    public IReadOnlyList<Tile> Members { get; }

    public EdgeSide Front { get; }

    public Vector2 Centre => new(
        (Members.Min(t => t.X) + Members.Max(t => t.X) + 1) / 2f,
        (Members.Min(t => t.Y) + Members.Max(t => t.Y) + 1) / 2f);
    private readonly ModelHandle _model;
    private Boolean _damageAlarm;
    private SoundHandle _alarmSound;
    public Boolean AlarmActive { get => _damageAlarm; }
    public Single HitPoints { 
        get; 
        set {
            _damageAlarm = value < field;
            field = value;
        }
    } = 100f;
    public Int32 MaxHitPoints { get; set; } = 100;
    public TowerCore(ResourceManager resourceManager, IReadOnlyList<Tile> members, EdgeSide front) {
        Front = front;
        Members = members;
        _model = resourceManager.GetOrLoadModel("Assets/Core.glb");
        _alarmSound = resourceManager.GetOrLoadSound("Assets/Alarm.wav");
    }
    public void Progress(Double deltaTime) {
        if (_damageAlarm && !IsSoundPlaying(_alarmSound)) {
            PlaySound(_alarmSound);
        }
    }
    public void Draw(Vector3 tileCentre) {
        var angle = Front switch {
            EdgeSide.North => 180f,
            EdgeSide.East => 90f,
            EdgeSide.South => 0f,
            _ => -90f
        };
        var centre = new Vector3(Centre.X, tileCentre.Y, Centre.Y);
        var origin = centre + Vector3.Transform(_modelOffset, Matrix4x4.CreateRotationY(angle * MathF.PI / 180f));
        DrawModelEx(_model, origin, Vector3.UnitY, angle, Vector3.One, Color.White);
    }
}
