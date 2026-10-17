using System.Drawing;
using System.Numerics;

namespace LudumDare60.Enemies;

public sealed class Pathfinder(World world) {
    private static readonly (EdgeSide Side, Int32 Dx, Int32 Dy)[] _directions = [
        (EdgeSide.North, 0, -1),
        (EdgeSide.East, 1, 0),
        (EdgeSide.South, 0, 1),
        (EdgeSide.West, -1, 0),
    ];

    /// <summary>
    /// Breadth first search over open tiles, not crossing walls, to the nearest tile the memory doesn't know yet.
    /// Returns the tiles to walk (excluding the start), or null when nothing unexplored is reachable.
    /// </summary>
    public List<Point>? FindNearestUnexplored(Point start, Memory memory) =>
        Search(start, p => memory.IsKnown(new Vector2(p.X, p.Y)) == MemoryTileType.None);

    /// <summary>
    /// Finds the nearest reachable tile that has a wall on a side facing an open tile the memory doesn't know yet.
    /// Returns the tiles to walk to get there (empty when already there) and the side of the wall.
    /// </summary>
    public (List<Point> Path, EdgeSide Side)? FindWallToBreach(Point start, Memory memory) {
        if (HasUnknownBehindWall(start, memory, out var startSide)) {
            return ([], startSide);
        }

        var path = Search(start, p => HasUnknownBehindWall(p, memory, out _));
        if (path is null) {
            return null;
        }

        HasUnknownBehindWall(path[^1], memory, out var side);
        return (path, side);
    }

    private Boolean HasUnknownBehindWall(Point tile, Memory memory, out EdgeSide side) {
        side = default;
        if (world.Tiles[tile.X, tile.Y].Content is not OpenTile open) {
            return false;
        }

        foreach (var (s, dx, dy) in _directions) {
            var next = new Point(tile.X + dx, tile.Y + dy);
            if (open.Edges[(Int32)s] is Wall
                && next.X >= 0 && next.Y >= 0 && next.X < world.Size && next.Y < world.Size
                && world.Tiles[next.X, next.Y].Content is OpenTile
                && memory.IsKnown(new Vector2(next.X, next.Y)) == MemoryTileType.None) {
                side = s;
                return true;
            }
        }

        return false;
    }

    /// <summary>Shortest path to the given tile, or null when it is unreachable.</summary>
    public List<Point>? FindPath(Point start, Point goal) => Search(start, p => p == goal);

    private List<Point>? Search(Point start, Func<Point, Boolean> isGoal) {
        var previous = new Dictionary<Point, Point> { [start] = start };
        var queue = new Queue<Point>();
        queue.Enqueue(start);

        while (queue.TryDequeue(out var current)) {
            if (current != start && isGoal(current)) {
                var path = new List<Point>();
                for (var p = current; p != start; p = previous[p]) {
                    path.Add(p);
                }
                path.Reverse();
                return path;
            }

            if (world.Tiles[current.X, current.Y].Content is not OpenTile open) {
                continue;
            }

            foreach (var (side, dx, dy) in _directions) {
                var next = new Point(current.X + dx, current.Y + dy);
                if (next.X < 0 || next.Y < 0 || next.X >= world.Size || next.Y >= world.Size
                    || previous.ContainsKey(next)
                    || open.Edges[(Int32)side] is Wall
                    || world.Tiles[next.X, next.Y].Content is not OpenTile) {
                    continue;
                }

                previous[next] = current;
                queue.Enqueue(next);
            }
        }

        return null;
    }
}
