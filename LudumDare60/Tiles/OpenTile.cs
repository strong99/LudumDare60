using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.Tiles;

public sealed class OpenTile : TileContent {
    public OpenTileContent? Content { get; set; }
    public Edge[] Edges { get; } = new Edge[4];
    public void Progress(Double deltaTime) => Content?.Progress(deltaTime);
    public void Draw(Vector3 origin) {
        var centre = origin + new Vector3(0.5f, -0.05f, 0.5f);
        DrawCube(centre, 1, 0.1f, 1, Color.Beige);
        DrawCubeWires(centre, 1, 0.1f, 1, Color.Brown);

        Content?.Draw();

        for (var i = 0; i < Edges.Length; i++) {
            Edges[i]?.Draw(centre, (EdgeSide)i);
        }
    }
}
