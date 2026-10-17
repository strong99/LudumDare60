using System.Numerics;

namespace LudumDare60.Tiles;

public interface Edge {
    void Progress(Double deltaTime) => throw new NotImplementedException();
    void Draw(Vector3 tileCentre, EdgeSide side) => throw new NotImplementedException();
}
