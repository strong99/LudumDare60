using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.Enemies;

public sealed class Enemy {
    public void Progress(Double deltaTime) => Behaviour?.Progress(deltaTime);
    public void Draw() {
        var centre = new Vector3(Position.X, 0.3f, Position.Y);
        DrawCube(centre, 0.4f, 0.6f, 0.4f, Color.Green);
        DrawCubeWires(centre, 0.4f, 0.6f, 0.4f, Color.DarkGreen);

        var facing = new Vector3(MathF.Sin(FacingRotation), 0, MathF.Cos(FacingRotation));
        DrawLine3D(centre, centre + facing * 0.4f, Color.Red);
    }
    public required EnemyKind Kind { get; init; }
    public EnemyBehaviour? Behaviour { get; set; }
    public required Memory Memory { get; init; }
    public Int32 HitPoints { get; set; }
    public Vector2 Position { get; set; }
    public Single FacingRotation { get; set; }
}
