using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.Tiles;

public sealed class OpenTile : TileContent {
    public OpenTileContent? Content { get; set; }
    public Edge[] Edges { get; } = new Edge[4];
    private ModelHandle _model;
    public OpenTile(ResourceManager resourceManager) {
        _model = resourceManager.GetOrLoadModel("Assets/Floor.glb");
    }
    public void Progress(Double deltaTime) => Content?.Progress(deltaTime);
    public void Draw(Vector3 origin) {
        var centre = origin + new Vector3(0.5f, -0.05f, 0.5f);
        DrawModelEx(_model, centre, new(), 0, new(1f, 1f, 1f), Color.White);
        DrawCubeWires(centre, 1, 0.1f, 1, Color.Brown);

        Content?.Draw(centre);

        for (var i = 0; i < Edges.Length; i++) {
            Edges[i]?.Draw(centre, (EdgeSide)i);
        }
    }
}
