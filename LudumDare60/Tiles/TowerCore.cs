using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.Tiles;

public sealed class TowerCore : OpenTileContent, Damageable {
    public Boolean RendersOwnFloor { get => true; }
    private readonly ModelHandle _model;
    public Single HitPoints { get; set; } = 100f;
    public Int32 MaxHitPoints { get; set; } = 100;
    public TowerCore(ResourceManager resourceManager) {
        _model = resourceManager.GetOrLoadModel("Assets/Core.glb");
    }
    public void Progress(Double deltaTime) {

    }
    public void Draw(Vector3 tileCentre) => DrawModelEx(_model, tileCentre, Vector3.UnitY, 0, Vector3.One, Color.White);
}
