using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.Enemies;

public static class GoblinModel {
    public const String FileName = "Assets/Goblin.glb";

    private const Int32 _attackClip = 0;
    private const Int32 _deathClip = 1;
    private const Int32 _idleClip = 2;
    private const Int32 _walkClip = 3;

    private static readonly Int32[] _bodyMeshes = [0, 1, 2, 3, 4, 7];
    private const Int32 _maceMesh = 5;
    private const Int32 _swordMesh = 6;

    private const Single _walkCyclesPerTile = 1f / 0.9f;
    private const Single _attackDuration = 0.8f;
    private const Single _deathDuration = 0.7f;
    private const Single _idleCycleDuration = 3f;

    private static Model? _model;
    private static ModelAnimation[]? _animations;
    private static Single FrameAt(ModelAnimation clip, Single progress, Boolean loops) =>
        Math.Min(progress * (loops ? clip.KeyFrameCount : clip.KeyFrameCount - 1), clip.KeyFrameCount - 1.001f);

    private static Single Cycle(Single value) => value - MathF.Floor(value);

    public static void Draw(Vector3 position, Single facing, Weapon weapon, Single walkPhase, Boolean walking, Single attackTime, Single deathTime) {
        _model ??= LoadModel(Path.GetFullPath(FileName));
        _animations ??= LoadModelAnimations(Path.GetFullPath(FileName)).ToArray();
        var model = _model.Value;

        Int32 clipIndex;
        Boolean loops = true;
        Single progress;
        if (deathTime >= 0) {
            clipIndex = _deathClip;
            progress = Math.Clamp(deathTime / _deathDuration, 0, 1);
            loops = false;
        }
        else if (attackTime >= 0) {
            clipIndex = _attackClip;
            progress = Cycle(attackTime / _attackDuration);
            loops = false;
        }
        else if (walking) {
            clipIndex = _walkClip;
            progress = Cycle(walkPhase * _walkCyclesPerTile);
        }
        else {
            clipIndex = _idleClip;
            progress = Cycle(walkPhase / _idleCycleDuration);
        }

        var clip = _animations[clipIndex];
        UpdateModelAnimation(model, clip, FrameAt(clip, progress, loops));

        var transform = Matrix4x4.CreateRotationY(facing) * Matrix4x4.CreateTranslation(position);
        foreach (var mesh in _bodyMeshes) {
            RayLibHelper.DrawMeshWithTint(model, mesh, transform, Color.White);
        }
        RayLibHelper.DrawMeshWithTint(model, weapon == Weapon.Sword ? _swordMesh : _maceMesh, transform, Color.White);
    }
}
