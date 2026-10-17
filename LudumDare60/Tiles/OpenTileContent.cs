using System.Numerics;

namespace LudumDare60.Tiles;

public interface OpenTileContent {
    void Progress(Double deltaTime) => throw new NotImplementedException();
    void Draw(Vector3 tileCentre) => throw new NotImplementedException();
}
