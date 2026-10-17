using System.Numerics;

namespace LudumDare60.Tiles;

public sealed class Tile(Int32 x, Int32 y, TileContent content) {
    public Int32 X { get; } = x;
    public Int32 Y { get; } = y;
    public TileContent Content { get; } = content;
    public void Progress(Double deltaTime) => Content.Progress(deltaTime);
    public void Draw() => Content.Draw(new Vector3(X, 0, Y));
}
