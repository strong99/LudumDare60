using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.Tiles;

public sealed class TowerCore : OpenTileContent, Damageable {
    public Boolean RendersOwnFloor { get => true; }
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
    public TowerCore(ResourceManager resourceManager) {
        _model = resourceManager.GetOrLoadModel("Assets/Core.glb");
        _alarmSound = resourceManager.GetOrLoadSound("Assets/Alarm.wav");
    }
    public void Progress(Double deltaTime) {
        if (_damageAlarm && !IsSoundPlaying(_alarmSound)) {
            PlaySound(_alarmSound);
        }
    }
    public void Draw(Vector3 tileCentre) => DrawModelEx(_model, tileCentre, Vector3.UnitY, 0, Vector3.One, Color.White);
}
