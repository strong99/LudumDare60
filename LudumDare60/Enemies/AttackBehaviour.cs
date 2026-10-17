using System.Drawing;
using System.Numerics;

namespace LudumDare60.Enemies;

public sealed class AttackBehaviour(World world, Enemy enemy, Point home) : EnemyBehaviour {
    private const Single _arriveDistance = 0.01f;
    private const Single _damagePerSecond = 5f;

    private readonly Pathfinder _pathfinder = new(world);
    private (Point Tile, EdgeSide Side)? _wall;
    private List<Point>? _path;
    private Int32 _pathIndex;

    public AttackBehaviour(World world, Enemy enemy, Point home, Point wallTile, EdgeSide wallSide)
        : this(world, enemy, home) {
        _wall = (wallTile, wallSide);
    }

    private void GiveUp() => enemy.Behaviour = new ReturnHomeBehaviour(world, enemy, home);

    private Damageable? FindTarget(Point core) {
        var content = (world.Tiles[core.X, core.Y].Content as OpenTile)?.Content;
        if (content is ReferenceContent reference) {
            content = reference.Owner;
        }
        return content as Damageable;
    }

    private Boolean Follow(Double deltaTime, Int32 count) {
        if (_path is null || _pathIndex >= count) {
            return true;
        }

        var next = _path[_pathIndex];
        var destination = new Vector2(next.X + 0.5f, next.Y + 0.5f);
        var offset = destination - enemy.Position;
        var distance = offset.Length();
        var step = enemy.Kind.Speed * (Single)deltaTime;

        if (distance > _arriveDistance) {
            enemy.FacingRotation = MathF.Atan2(offset.X, offset.Y);
        }

        if (distance <= step + _arriveDistance) {
            enemy.Position = destination;
            _pathIndex++;
        }
        else {
            enemy.Position += offset / distance * step;
        }
        return false;
    }

    private void Face(Vector2 point) {
        var offset = point - enemy.Position;
        if (offset.LengthSquared() > 0) {
            enemy.FacingRotation = MathF.Atan2(offset.X, offset.Y);
        }
    }

    public void Progress(Double deltaTime) {
        if (_wall is { } wall) {
            AttackWall(deltaTime, wall.Tile, wall.Side);
        }
        else {
            AttackCore(deltaTime);
        }
    }

    private void AttackWall(Double deltaTime, Point wallTile, EdgeSide side) {
        var tile = new Point((Int32)enemy.Position.X, (Int32)enemy.Position.Y);

        if ((world.Tiles[wallTile.X, wallTile.Y].Content as OpenTile)?.Edges[(Int32)side] is not Wall wall) {
            enemy.Behaviour = new ExploreBehaviour(world, enemy, home);
            return;
        }

        if (_path is null) {
            _path = tile == wallTile ? [] : _pathfinder.FindPath(tile, wallTile);
            _pathIndex = 0;
            if (_path is null) {
                GiveUp();
                return;
            }
        }

        if (!Follow(deltaTime, _path.Count)) {
            return;
        }

        var offset = side switch {
            EdgeSide.North => new Vector2(0, -1),
            EdgeSide.East => new Vector2(1, 0),
            EdgeSide.South => new Vector2(0, 1),
            _ => new Vector2(-1, 0),
        };
        Face(enemy.Position + offset);

        wall.HitPoints = MathF.Max(0, wall.HitPoints - _damagePerSecond * (Single)deltaTime);
        if (wall.HitPoints <= 0) {
            world.SetEdge(world.Tiles[wallTile.X, wallTile.Y], side, null);
            enemy.Behaviour = new ExploreBehaviour(world, enemy, home);
        }
    }

    private void AttackCore(Double deltaTime) {
        if (enemy.Memory.RetrieveOne(MemoryTileType.Core) is not { } corePosition) {
            GiveUp();
            return;
        }

        var core = new Point((Int32)corePosition.X, (Int32)corePosition.Y);
        var tile = new Point((Int32)enemy.Position.X, (Int32)enemy.Position.Y);

        if (FindTarget(core) is not { } target) {
            enemy.Memory.Store(corePosition, MemoryTileType.None);
            GiveUp();
            return;
        }

        if (_path is null && tile != core) {
            _path = _pathfinder.FindPath(tile, core);
            _pathIndex = 0;
            if (_path is null) {
                enemy.Behaviour = new ExploreBehaviour(world, enemy, home);
                return;
            }
        }

        if (_path is not null && !Follow(deltaTime, _path.Count - 1)) {
            return;
        }

        Face(new Vector2(core.X + 0.5f, core.Y + 0.5f));
        target.HitPoints = MathF.Max(0, target.HitPoints - _damagePerSecond * (Single)deltaTime);
    }
}
