using System.Numerics;

namespace LudumDare60.Tiles;

public sealed class ReferenceContent(MultiTileContent owner) : OpenTileContent {
    public MultiTileContent Owner { get; } = owner;
    public Boolean BlocksMovement { get => true; }
    public void Progress(Double deltaTime) {
        // Owner tile draws the content
    }
    public void Draw(Vector3 tileCentre) {
        // Owner tile draws the content
    }
}
