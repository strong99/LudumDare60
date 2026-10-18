using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace LudumDare60.Generators;

public sealed class CaveWorldGenerator {

    private enum CaveTileType {
        Solid,
        Open,
        Entrance,
        Core
    }
    private sealed class CaveTile {
        public Int32 DistanceFromEntrance { get; set; } = 0;
        public Single Weight { get; set; } = 0;
        public CaveTileType Type { get; set; } = CaveTileType.Solid;
    }
    public (Tile[,] Tiles, Collection<Decoration> decorations) Generate(Int32 size, ResourceManager resourceManager, Int32 seed = -1) {
        var random = seed == -1 ? new Random() : new Random(seed);

        var tiles = InstantiateTileArray(size);

        var entrances = ApplyEntrancesTo(tiles, size, random);
        var furthestTiles = BFSScoreAndDistance(tiles, entrances, random);

        var towerCore = furthestTiles[random.Next() % furthestTiles.Length];
        tiles[towerCore.X, towerCore.Y].Type = CaveTileType.Core;

        CaveOutRandomRooms(tiles, random, towerCore);

        CarveOutMainPath(entrances, [towerCore], tiles);

        return ConvertToTiles(random, tiles, resourceManager);
    }

    private static (Tile[,] Tiles, Collection<Decoration> Decorations) ConvertToTiles(Random random, CaveTile[,] caveTiles, ResourceManager resourceManager) {
        var width = caveTiles.GetLength(0);
        var height = caveTiles.GetLength(1);

        var decorations = new Collection<Decoration>();
        var tiles = new Tile[width, height];
        for (var x = 0; x < width; x++) {
            for (var y = 0; y < height; y++) {
                TileContent content = caveTiles[x, y].Type switch {
                    CaveTileType.Solid => new SolidTile(),
                    CaveTileType.Open => new OpenTile(resourceManager),
                    CaveTileType.Entrance => new OpenTile(resourceManager) { Content = new TowerEntrance(resourceManager) },
                    CaveTileType.Core => new OpenTile(resourceManager) { Content = new TowerCore(resourceManager) },
                    _ => throw new ArgumentOutOfRangeException(nameof(caveTiles))
                };
                tiles[x, y] = new Tile(x, y, content);

                if (content is OpenTile { Content: TowerEntrance }) {
                    var dx = 0;
                    var dy = 0;
                    var r = 0f;

                    if (x == 0) {
                        dx = -1;
                        r = 180f;
                    }
                    else if (x == width - 1) {
                        dx = 1;
                        r = 0;
                    }
                    else if (y == 0) {
                        dy = -1;
                        r = 90;
                    }
                    else {
                        dy = 1;
                        r = -90;
                    }

                    Vector3 position = new(x + 0.5f + dx, 0, y + 0.5f + dy);
                    Decoration decoration = new StaticDecoration(resourceManager.GetOrLoadModel("Assets/ElevatorShaft.glb"), position, r);
                    decorations.Add(decoration);

                    position = new Vector3(x + 0.5f + dx, 0, y + 0.5f + dy);
                    decoration = new ElevatorDecoration(resourceManager.GetOrLoadModel("Assets/Elevator.glb"), position, r);
                    decorations.Add(decoration);
                }
            }
        }

        DecorateSolidTiles(random, tiles, resourceManager);

        decorations = [
            ..decorations,
            ..GenerateOuterWallDecorations(random, caveTiles, resourceManager)
        ];

        return (tiles, decorations);
    }

    private static void DecorateSolidTiles(Random random, Tile[,] tiles, ResourceManager resourceManager) {
        var width = tiles.GetLength(0);
        var height = tiles.GetLength(1);

        var directions = new Point[] {
            new(0,-1),
            new(1,0),
            new(0,1),
            new(-1,0)
        };

        var wallTypes = new String[] {
            "Assets/SolidWall.01.glb",
            "Assets/SolidWall.02.glb"
        };

        for (var x = 0; x < width; ++x) {
            for (var y = 0; y < height; ++y) {
                var tile = tiles[x, y];
                if (tile.Content is SolidTile solidTile) {
                    for (var d = 0; d < directions.Length; d++) {
                        var direction = directions[d];
                        var sx = x + direction.X;
                        var sy = y + direction.Y;
                        if (sx < 0 || sy < 0 || sx >= width || sy >= height) {
                            continue;
                        }
                        if (tiles[sx, sy].Content is OpenTile) {
                            var wallType = wallTypes[random.Next() % wallTypes.Length];
                            solidTile.Walls[d] = resourceManager.GetOrLoadModel(wallType);
                        }
                    }
                }
            }
        }
    }

    private static Collection<Decoration> GenerateOuterWallDecorations(Random random, CaveTile[,] caveTiles, ResourceManager resourceManager) {
        var width = caveTiles.GetLength(0);
        var height = caveTiles.GetLength(1);

        var decorations = new Collection<Decoration>();
        Dictionary<String, String[]> canChangeTo = new() {
            ["first"] = ["Assets/TowerSide.02b.glb", "Assets/TowerSide.03.glb"],
            ["last"] = ["Assets/TowerSide.02.glb", "Assets/TowerSide.03.glb"],
            ["Assets/TowerSide.01.glb"] = ["Assets/TowerSide.01.glb", "Assets/TowerSide.02.glb"],
            ["Assets/TowerSide.02.glb"] = ["Assets/TowerSide.02b.glb", "Assets/TowerSide.03.glb"],
            ["Assets/TowerSide.02b.glb"] = ["Assets/TowerSide.01.glb", "Assets/TowerSide.02.glb"],
            ["Assets/TowerSide.03.glb"] = ["Assets/TowerSide.03.glb", "Assets/TowerSide.02b.glb"]
        };

        var offset = new Vector3(0.5f, 0, -0.5f);
        var edges = new Vector3[] { new(-1, 0, 0), new(0, 0, 1), new(1, 0, 0), new(0, 0, -1) };
        var orientation = new Int32[] { 180, -90, 0, 90 };
        for (var e = 0; e < edges.Length; e++) {
            var edge = edges[e];
            var rotation = orientation[e];

            var min = 0;
            var max = width;
            var edgeOffset = offset + e switch {
                0 => new Vector3(width - 1, 0, 0),
                1 => new Vector3(0, 0, 1),
                2 => new Vector3(0, 0, height),
                3 => new Vector3(width - 1, 0, height),
                _ => Vector3.Zero
            };

            String previous = "first";
            Console.WriteLine($"Edge {e}");
            for (var x = min; x < max; x++) {
                var last = x == max - 1;
                var nextChoices = canChangeTo[previous];
                if (last) {
                    nextChoices = nextChoices.Where(x => canChangeTo["last"].Contains(x)).ToArray();
                }
                var nextChoice = nextChoices[random.Next() % nextChoices.Length];
                previous = nextChoice;

                decorations.Add(new StaticDecoration(resourceManager.GetOrLoadModel(nextChoice), edge * x + edgeOffset, rotation));
                Console.WriteLine($" - {nextChoice}");
            }
        }

        var corners = new Vector3[] {
            new(0.5f, 0, 0.5f),
            new(width - 0.5f, 0, 0.5f),
            new(width - 0.5f, 0, height - 0.5f),
            new(0.5f, 0, height - 0.5f)
        };
        var cornerOrientation = new Int32[] { 180, 90, 0, -90 };
        var cornerModels = new[] { "Assets/TowerCorner.01.glb", "Assets/TowerCorner.02.glb", "Assets/TowerCorner.03.glb" };
        for (var corner = 0; corner < 4; corner++) {
            var model = cornerModels[random.Next() % cornerModels.Length];
            decorations.Add(new StaticDecoration(resourceManager.GetOrLoadModel(model), corners[corner], cornerOrientation[corner]));
        }

        return decorations;
    }

    private CaveTile[,] InstantiateTileArray(Int32 size) {
        var tiles = new CaveTile[size, size];
        for (var x = 0; x < size; x++) {
            for (var y = 0; y < size; y++) {
                tiles[x, y] = new();
            }
        }
        return tiles;
    }

    private Point[] ApplyEntrancesTo(CaveTile[,] tiles, Int32 size, Random random) {
        var edges = 4;
        var overlappingCorners = 4;
        var totalBorderTiles = edges * size - overlappingCorners;

        var entranceCount = Math.Min(2, totalBorderTiles);

        var entrances = new Collection<Point>();
        var reserved = new HashSet<Int32>();
        var picked = new HashSet<Int32>();
        while (picked.Count < entranceCount) {
            var idx = random.Next(totalBorderTiles);
            if (picked.Contains(idx - 1) || picked.Contains(idx + 1) || !picked.Add(idx)) {
                continue;
            }
            var position = BorderIndexToPosition(idx, size);
            tiles[position.X, position.Y].Type = CaveTileType.Entrance;
            entrances.Add(new(position.X, position.Y));
        }
        return [.. entrances];
    }

    private static Point BorderIndexToPosition(Int32 idx, Int32 size) {
        var last = size - 1;
        var edge = idx / last;
        var offset = idx % last;
        return edge switch {
            0 => new Point(offset, 0),
            1 => new Point(last, offset),
            2 => new Point(last - offset, last),
            _ => new Point(0, last - offset)
        };
    }

    private Point[] BFSScoreAndDistance(CaveTile[,] tiles, Point[] entrances, Random random, Int32 topFurthestCount = 10) {
        var width = tiles.GetLength(0);
        var height = tiles.GetLength(1);

        var visited = new Boolean[width, height];
        var queue = new Queue<Point>();

        foreach (var entrance in entrances) {
            tiles[entrance.X, entrance.Y].DistanceFromEntrance = 0;
            tiles[entrance.X, entrance.Y].Weight = random.NextSingle();
            visited[entrance.X, entrance.Y] = true;
            queue.Enqueue(entrance);
        }

        var furthest = new Queue<Point>(topFurthestCount + 1);

        Point[] directions = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1)];
        while (queue.Count > 0) {
            var current = queue.Dequeue();
            var distance = tiles[current.X, current.Y].DistanceFromEntrance;
            foreach (var direction in directions) {
                var nx = current.X + direction.X;
                var ny = current.Y + direction.Y;
                if (nx < 0 || ny < 0 || nx >= width || ny >= height || visited[nx, ny]) {
                    continue;
                }

                var nonBorder = nx > 0 && nx < width - 1 &&
                                ny > 0 && ny < height - 1;

                visited[nx, ny] = true;
                tiles[nx, ny].DistanceFromEntrance = distance + 1;
                tiles[nx, ny].Weight = nonBorder ? random.NextSingle() : 10f;
                queue.Enqueue(new Point(nx, ny));
            }

            var nonBorderTiles = current.X > 0 && current.X < width - 1 &&
                                 current.Y > 0 && current.Y < height - 1;
            if (nonBorderTiles) {
                furthest.Enqueue(current);
                if (furthest.Count > topFurthestCount) {
                    furthest.Dequeue();
                }
            }
        }

        return [.. furthest];
    }

    private void CaveOutRandomRooms(
        CaveTile[,] tiles,
        Random random,
        Point core,
        Int32 minRooms = 3,
        Int32 maxRooms = 6,
        Int32 minSize = 3,
        Int32 maxSize = 5
    ) {
        var size = tiles.GetLength(0);
        var rooms = new List<Rectangle>();
        var target = random.Next(minRooms, maxRooms + 1);
        var maxAttempts = target * 20;

        for (var attempt = 0; attempt < maxAttempts && rooms.Count < target; attempt++) {
            var width = random.Next(minSize, maxSize + 1);
            var height = random.Next(minSize, maxSize + 1);
            if (width > size - 2 || height > size - 2) {
                continue;
            }

            var room = new Rectangle(random.Next(1, size - width), random.Next(1, size - height), width, height);

            var padded = Rectangle.Inflate(room, 1, 1);
            if (rooms.Any(r => r.IntersectsWith(padded))) {
                continue;
            }

            for (var x = room.Left; x < room.Right; x++) {
                for (var y = room.Top; y < room.Bottom; y++) {
                    if (tiles[x, y].Type == CaveTileType.Solid) {
                        tiles[x, y].Type = CaveTileType.Open;
                        tiles[x, y].Weight = 0;
                    }
                }
            }

            rooms.Add(room);
        }

        foreach (var room in rooms) {
            CarveOutMainPath([new Point(room.X + room.Width / 2, room.Y + room.Height / 2)], [core], tiles);
        }
    }

    private void CarveOutMainPath(Point[] from, Point[] to, CaveTile[,] tiles) {
        foreach (var start in from) {
            foreach (var tile in AStar(tiles, start, to)) {
                if (tiles[tile.X, tile.Y].Type == CaveTileType.Solid) {
                    tiles[tile.X, tile.Y].Type = CaveTileType.Open;
                }
            }
        }
    }

    private static List<Point> AStar(CaveTile[,] tiles, Point start, Point[] goals) {
        var width = tiles.GetLength(0);
        var height = tiles.GetLength(1);

        var goalSet = new HashSet<Point>(goals);

        Int32 Heuristic(Point p) {
            var best = Int32.MaxValue;
            foreach (var goal in goals) {
                best = Math.Min(best, Math.Abs(p.X - goal.X) + Math.Abs(p.Y - goal.Y));
            }
            return best;
        }

        var cost = new Single[width, height];
        for (var x = 0; x < width; x++) {
            for (var y = 0; y < height; y++) {
                cost[x, y] = Single.MaxValue;
            }
        }
        var cameFrom = new Point?[width, height];
        var closed = new Boolean[width, height];
        var open = new PriorityQueue<Point, Single>();

        cost[start.X, start.Y] = 0;
        open.Enqueue(start, Heuristic(start));

        Point[] directions = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1)];
        while (open.TryDequeue(out var current, out _)) {
            if (closed[current.X, current.Y]) {
                continue;
            }
            closed[current.X, current.Y] = true;

            if (goalSet.Contains(current)) {
                var path = new List<Point> { current };
                while (cameFrom[current.X, current.Y] is { } previous) {
                    path.Add(previous);
                    current = previous;
                }
                path.Reverse();
                return path;
            }

            foreach (var direction in directions) {
                var nx = current.X + direction.X;
                var ny = current.Y + direction.Y;
                if (nx < 0 || ny < 0 || nx >= width || ny >= height || closed[nx, ny]) {
                    continue;
                }

                var newCost = cost[current.X, current.Y] + 1 + tiles[nx, ny].Weight;
                if (newCost >= cost[nx, ny]) {
                    continue;
                }
                cost[nx, ny] = newCost;
                cameFrom[nx, ny] = current;
                var next = new Point(nx, ny);
                open.Enqueue(next, newCost + Heuristic(next));
            }
        }

        return [];
    }
}
