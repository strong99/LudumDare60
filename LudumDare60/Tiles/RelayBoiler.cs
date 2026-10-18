using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.Tiles;

public sealed class RelayBoiler : OpenTileContent, SteamConnected, Damageable {
    private const Single _volume = 5f;
    private const Single _scale = 0.7f;
    private const Int32 _maxDamagePoints = 5;
    public Boolean BlocksMovement { get => true; }
    public Single HitPoints { get; set; } = 20f;
    public Int32 MaxHitPoints { get; set; } = 20;
    public Boolean RendersOwnFloor { get => false; }
    public SteamStore Store { get; } = new(_volume, maxDamagePoints: _maxDamagePoints);
    private readonly ModelHandle _model;
    public RelayBoiler(ResourceManager resourceManager) {
        _model = resourceManager.GetOrLoadModel("Assets/Boiler.glb");
    }
    public void Progress(Double deltaTime) => Store.Step(deltaTime);
    public void Draw(Vector3 tileCentre) => Draw(tileCentre, Color.White);
    public void Draw(Vector3 tileCentre, Color tint) => DrawModelEx(_model, tileCentre, Vector3.UnitY, 0, new Vector3(_scale), tint);
}
