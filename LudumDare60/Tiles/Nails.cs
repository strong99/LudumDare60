using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.Tiles;

public sealed class Nails : Trap {
    public Boolean RendersOwnFloor { get => true; }
    private readonly ModelHandle _model;
    public Nails(ResourceManager resourceManager) {
        _model = resourceManager.GetOrLoadModel("Assets/Spikes.glb");
    }
    public void Progress(Double deltaTime) { }
    public void Draw(Vector3 tileCentre) => Draw(tileCentre, Color.White);
    public void Draw(Vector3 tileCentre, Color tint) => DrawModelEx(_model, tileCentre, Vector3.UnitY, 0, Vector3.One, tint);
}
