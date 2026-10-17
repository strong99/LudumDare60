public interface Edge {
    void Progress(Double deltaTime) => throw new NotImplementedException();
    void Draw(System.Numerics.Vector3 tileCentre, EdgeSide side) => throw new NotImplementedException();
}
