using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

public sealed class SolidTile : TileContent {
    public void Progress(Double deltaTime) { }
    public void Draw(Vector3 origin) {
        var centre = origin + new Vector3(0.5f, 0.5f, 0.5f);
        DrawCube(centre, 1, 1, 1, Color.Gray);
        DrawCubeWires(centre, 1, 1, 1, Color.DarkGray);
    }
}
