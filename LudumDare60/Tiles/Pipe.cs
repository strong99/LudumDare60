using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.Tiles;

public sealed class Pipe : Edge, SteamConnected {
    private const Single _scale = 1.0f;
    private const Single _volume = 1f;
    private const Int32 _maxDamagePoints = 5;
    private readonly ModelHandle _model;
    public SteamStore Store { get; } = new(_volume, maxDamagePoints: _maxDamagePoints);
    public Pipe(ResourceManager manager) {
        _model = manager.GetOrLoadModel("Assets/Pipe.glb");
    }
    public void Progress(Double deltaTime) => Store.Step(deltaTime);
    public void Draw(Vector3 tileCentre, EdgeSide side) {
        var shade = (Byte)(110 + 145 * Math.Clamp(Store.Pressure, 0f, 1f));
        Draw(tileCentre, side, new Color(shade, shade, shade, (Byte)255));
    }

    public void Draw(Vector3 tileCentre, EdgeSide side, Color tint) {
        var (offset, angle) = side switch {
            EdgeSide.North => (new Vector3(0, 0, -0.5f), 0f),
            EdgeSide.South => (new Vector3(0, 0, 0.5f), 0f),
            EdgeSide.East => (new Vector3(0.5f, 0, 0), 90f),
            _ => (new Vector3(-0.5f, 0, 0), 90f),
        };

        var position = tileCentre + offset;
        DrawModelEx(_model, position, Vector3.UnitY, angle, new Vector3(_scale), tint);
    }
}
