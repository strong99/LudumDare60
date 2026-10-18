using System.Drawing;
using System.Numerics;

namespace LudumDare60.Enemies;

public sealed class ExploreBehaviour(World world, Enemy enemy, Point? home = null) : EnemyBehaviour {
    private const Single _arriveDistance = 0.01f;

    private readonly Pathfinder _pathfinder = new(world);
    private readonly Point _home = home ?? new Point((Int32)enemy.Position.X, (Int32)enemy.Position.Y);
    private List<Point>? _path;
    private Int32 _pathIndex;
    private Point? _lastTile;

    private void Observe(Point tile) {
        MarkOpen(tile);

        if (world.Tiles[tile.X, tile.Y].Content is not OpenTile open) {
            return;
        }

        foreach (var side in Enum.GetValues<EdgeSide>()) {
            if (open.Edges[(Int32)side] is Wall) {
                continue;
            }

            var neighbour = world.GetNeighbour(world.Tiles[tile.X, tile.Y], side);
            if (neighbour?.Content is OpenTile) {
                MarkOpen(new Point(neighbour.X, neighbour.Y));
            }
        }
    }

    private void MarkOpen(Point tile) {
        var position = new Vector2(tile.X, tile.Y);
        var isCore = (world.Tiles[tile.X, tile.Y].Content as OpenTile)?.Content is TowerCore or ReferenceContent { Owner: TowerCore };
        if (isCore) {
            enemy.Memory.Store(position, MemoryTileType.Core);
        }
        else if (enemy.Memory.IsKnown(position) == MemoryTileType.None) {
            enemy.Memory.Store(position, MemoryTileType.Open);
        }
    }

    public void Progress(Double deltaTime) {
        if (enemy.HitPoints <= 0) {
            enemy.Behaviour = new DeadBehaviour(world, enemy);
            return;
        }

        var tile = new Point((Int32)enemy.Position.X, (Int32)enemy.Position.Y);
        if (tile != _lastTile) {
            _lastTile = tile;
            Observe(tile);

            if (enemy.Memory.RetrieveOne(MemoryTileType.Core) is { } core
                && (tile == new Point((Int32)core.X, (Int32)core.Y)
                    || _pathfinder.FindPath(tile, new Point((Int32)core.X, (Int32)core.Y)) is not null)) {
                enemy.Behaviour = new AttackBehaviour(world, enemy, _home);
                return;
            }

            if (enemy.Memory.RetrieveOne(MemoryTileType.Core) is { } blockedCore
                && _pathfinder.FindObstacleToBreak(tile, p => p == new Point((Int32)blockedCore.X, (Int32)blockedCore.Y)) is not null) {
                enemy.Behaviour = new AttackBehaviour(world, enemy, _home);
                return;
            }
        }

        if (_path is null || _pathIndex >= _path.Count) {
            _pathIndex = 0;
            _path = null;

            _path = _pathfinder.FindNearestUnexplored(tile, enemy.Memory);

            if (_path is null) {
                if (_pathfinder.FindObstacleToBreak(tile, p => enemy.Memory.IsKnown(new Vector2(p.X, p.Y)) == MemoryTileType.None) is var (obstaclePath, obstacle)) {
                    enemy.Behaviour = new AttackBehaviour(world, enemy, _home, obstaclePath, obstacle);
                    return;
                }

                if (_pathfinder.FindWallToBreach(tile, enemy.Memory) is var (wallPath, side)) {
                    var wallTile = wallPath.Count > 0 ? wallPath[^1] : tile;
                    enemy.Behaviour = new AttackBehaviour(world, enemy, _home, wallTile, side);
                }
                else {
                    enemy.Behaviour = new ReturnHomeBehaviour(world, enemy, _home);
                }
                return;
            }
        }

        if (_path is null || _path.Count == 0) {
            return;
        }

        var target = new Vector2(_path[_pathIndex].X + 0.5f, _path[_pathIndex].Y + 0.5f);
        var offset = target - enemy.Position;
        var distance = offset.Length();
        var step = enemy.Kind.Speed * (Single)deltaTime;

        if (distance <= step + _arriveDistance) {
            enemy.Position = target;
            _pathIndex++;
        }
        else {
            enemy.Position += offset / distance * step;
        }

        if (distance > _arriveDistance) {
            enemy.FacingRotation = MathF.Atan2(offset.X, offset.Y);
        }
    }
}
