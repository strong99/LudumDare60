using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.Tiles;

public sealed class SolidTile : TileContent {
    public ModelHandle?[] Walls { get; } = new ModelHandle?[4];
    public ModelHandle Top { get; set; }
    public void Progress(Double deltaTime) { }
    public void Draw(Vector3 origin) {
        var centre = origin + new Vector3(0.5f, 0f, 0.5f);
        var offsetTop = new Vector3(0.0f, 1f, 0.0f);

        DrawModelEx(Top, centre + offsetTop, Vector3.UnitY, 0, Vector3.One, Color.White);

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

            DrawModelEx(wall, centre + offset, Vector3.UnitY, -90f * w + 180, Vector3.One, Color.White/*new Color(215, 170, 130, 255)*/);
        }
    }
}
