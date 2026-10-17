using System.Numerics;

namespace LudumDare60.Tiles;

public interface TileContent {
    void Progress(Double deltaTime);
    void Draw(Vector3 origin);
}
