using System.Numerics;

public interface TileContent {
    void Progress(Double deltaTime);
    void Draw(Vector3 origin);
}
