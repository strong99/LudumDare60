using System.Drawing;
using System.Numerics;

namespace LudumDare60.Enemies;

public sealed class ReturnHomeBehaviour(World world, Enemy enemy, Point home) : EnemyBehaviour {
    private const Single _arriveDistance = 0.01f;

    private readonly Pathfinder _pathfinder = new(world);
    private List<Point>? _path;
    private Int32 _pathIndex;

    private void Leave() {
        world.EnemyKnowledge.SyncWith(enemy.Memory);
        world.Enemies.Remove(enemy);
    }

    public void Progress(Double deltaTime) {
        if (enemy.HitPoints <= 0) {
            enemy.Behaviour = new DeadBehaviour(world, enemy);
            return;
        }

        var tile = new Point((Int32)enemy.Position.X, (Int32)enemy.Position.Y);

        if (_path is null) {
            if (tile == home) {
                Leave();
                return;
            }

            _path = _pathfinder.FindPath(tile, home);
            _pathIndex = 0;
            if (_path is null) {
                return;
            }
        }

        if (_pathIndex >= _path.Count) {
            Leave();
            return;
        }

        var target = new Vector2(_path[_pathIndex].X + 0.5f, _path[_pathIndex].Y + 0.5f);
        var offset = target - enemy.Position;
        var distance = offset.Length();
        var step = enemy.Kind.Speed * (Single)deltaTime;

        if (distance > _arriveDistance) {
            enemy.FacingRotation = MathF.Atan2(offset.X, offset.Y);
        }

        if (distance <= step + _arriveDistance) {
            enemy.Position = target;
            _pathIndex++;
            if (_pathIndex >= _path.Count) {
                Leave();
            }
        }
        else {
            enemy.Position += offset / distance * step;
        }
    }
}
