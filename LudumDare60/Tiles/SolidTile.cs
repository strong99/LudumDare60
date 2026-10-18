using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.Tiles;

public sealed class SolidTile : TileContent {
    public ModelHandle?[] Walls { get; } = new ModelHandle?[4];
    public void Progress(Double deltaTime) { }
    public void Draw(Vector3 origin) {
        var centre = origin + new Vector3(0.5f, -0.05f, 0.5f);
        var offsetTop = new Vector3(0.5f, 1f, 0.5f);

        DrawPlane(origin + offsetTop, new(1, 1), Color.Black);

        for(var w = 0; w < Walls.Length; w++) {
            if (Walls[w] is not { } wall) {
                continue;
            }

            var offset = (EdgeSide)w switch {
                EdgeSide.North => new Vector3(0, 0, -0.5f),
                EdgeSide.East => new Vector3(0.5f, 0, 0),
                EdgeSide.South => new Vector3(0, 0, 0.5f),
                _ => new Vector3(-0.5f, 0, 0),
            };

            DrawModelEx(wall, centre + offset, Vector3.UnitY, -90f * w + 180, Vector3.One, Color.White);
        }
    }
}
