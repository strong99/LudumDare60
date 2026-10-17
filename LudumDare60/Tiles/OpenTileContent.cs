using System.Numerics;

namespace LudumDare60.Tiles;

public interface OpenTileContent {
    Boolean RendersOwnFloor { get => false; }
    Boolean BlocksMovement { get => false; }
    void Progress(Double deltaTime);
    void Draw(Vector3 tileCentre);
}
