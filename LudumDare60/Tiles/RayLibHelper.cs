using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace LudumDare60.Tiles;

public static unsafe class RayLibHelper {
    public static void DrawMeshWithTint(Model model, Int32 mesh, Matrix4x4 transform, Color tint) {
        var material = model.Materials[model.MeshMaterial[mesh]];
        var map = &material.Maps[(Int32)MaterialMapIndex.Albedo];
        var original = map->Color;
        map->Color = new Color(
            (Byte)(original.R * tint.R / 255),
            (Byte)(original.G * tint.G / 255),
            (Byte)(original.B * tint.B / 255),
            (Byte)(original.A * tint.A / 255));
        DrawMesh(model.Meshes[mesh], material, Matrix4x4.Transpose(transform));
        map->Color = original;
    }
}
