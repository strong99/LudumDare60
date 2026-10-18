using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.Tiles;

public sealed class Boiler : OpenTileContent, SteamConnected, Damageable {
    private const Single _volume = 10f;
    private const Single _production = 10f;
    private const Int32 _maxDamagePoints = 5;
    public Boolean BlocksMovement { get => true; }
    public Single HitPoints { get; set; } = 10f;
    public Int32 MaxHitPoints { get; set; } = 10;
    public Boolean RendersOwnFloor { get => true; }
    public SteamStore Store { get; } = new(_volume, _production, _maxDamagePoints);
    private readonly ModelHandle _model;
    public Boiler(ResourceManager resourceManager) {
        _model = resourceManager.GetOrLoadModel("Assets/Boiler.glb");
    }
    public void Progress(Double deltaTime) => Store.Step(deltaTime);
    public void Draw(Vector3 tileCentre) => Draw(tileCentre, Color.White);
    public void Draw(Vector3 tileCentre, Color tint) => DrawModelEx(_model, tileCentre, Vector3.UnitY, 0, Vector3.One, tint);
}
