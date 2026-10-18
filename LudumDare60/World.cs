using System.Collections.ObjectModel;

namespace LudumDare60;

public sealed class World {
    public Int32 Size { get; }
    public Tile[,] Tiles { get; }
    public Collection<Decoration> Decorations { get; }
    public Collection<Enemy> Enemies { get; } = [];
    public Int32 Scraps { get; private set; } = 100;
    public Double Duration { get; }
    public Memory EnemyKnowledge { get; } = new();

    public event Action<Enemy>? EnemyDied;

    public void RaiseEnemyDied(Enemy enemy) => EnemyDied?.Invoke(enemy);

    public World(Int32 size, ResourceManager resourceManager, Tile[,]? tiles = null, IEnumerable<Decoration>? decorations =null) {
        Size = size;
        Tiles = tiles ?? new Tile[size, size];
        Decorations = decorations is null ? [] : [..decorations];

        if (tiles is null) {
            var toPlaceSpawners = 2;
            var baseSpawnChance = 0.2;
            var borderCount = 4 * size - 4; //4 sides - 4 corners
            var borderIndex = 0;

            var random = new Random();
            for (var x = 0; x < size; x++) {
                for (var y = 0; y < size; y++) {
                    var border = x == 0 || y == 0 || x == size - 1 || y == size - 1;

                    TileContent content = border ? new SolidTile() : new OpenTile(resourceManager);
                    if (border) {
                        if (toPlaceSpawners > 0) {
                            var lastChanceIndex = borderCount - toPlaceSpawners;
                            var chance = borderIndex >= lastChanceIndex
                                ? 1.0
                                : baseSpawnChance + (1.0 - baseSpawnChance) * borderIndex / lastChanceIndex;
                            if (random.NextDouble() < chance) {
                                var spawner = new OpenTile(resourceManager) {
                                    Content = new TowerEntrance(resourceManager)
                                };
                                content = spawner;
                                toPlaceSpawners--;
                            }
                        }

                        borderIndex++;
                    }

                    Tiles[x, y] = new Tile(x, y, content);
                }
            }
        }
    }

    public Boolean CanAfford(Int32 cost) => Scraps >= cost;

    public void Spend(Int32 cost) => Scraps -= cost;

    public Tile? GetNeighbour(Tile tile, EdgeSide side) {
        var (dx, dy) = side switch {
            EdgeSide.North => (0, -1),
            EdgeSide.East => (1, 0),
            EdgeSide.South => (0, 1),
            _ => (-1, 0),
        };

        var nx = tile.X + dx;
        var ny = tile.Y + dy;
        return nx < 0 || ny < 0 || nx >= Size || ny >= Size ? null : Tiles[nx, ny];
    }

    public Boolean IsFree(Tile tile) => tile.Content is OpenTile { Content: null };

    public void SetContent(Tile tile, OpenTileContent? content) {
        ClearContentCore(tile);
        if (tile.Content is OpenTile open) {
            open.Content = content;
        }

        SteamLinks.Connect(this);
    }

    public Boolean TryPlace(MultiTileContent content) {
        if (content.Members.Skip(1).Any(t => !IsFree(t))) {
            return false;
        }

        ClearContentCore(content.Members[0]);
        ((OpenTile)content.Members[0].Content).Content = content;
        foreach (var tile in content.Members.Skip(1)) {
            ((OpenTile)tile.Content).Content = new ReferenceContent(content);
        }

        SteamLinks.Connect(this);
        return true;
    }

    public void ClearContent(Tile tile) {
        ClearContentCore(tile);
        SteamLinks.Connect(this);
    }

    private void ClearContentCore(Tile tile) {
        if (tile.Content is not OpenTile { Content: { } content } open) {
            return;
        }

        if (content is ReferenceContent reference) {
            content = reference.Owner;
        }

        if (content is MultiTileContent multiTileContent) {
            foreach (var t in multiTileContent.Members) {
                if (t.Content is OpenTile o) {
                    o.Content = null;
                }
            }
        }
        else {
            open.Content = null;
        }
    }

    public void SetEdge(Tile tile, EdgeSide side, Edge? edge) {
        SetEdgeCore(tile, side, edge);
        SteamLinks.Connect(this);
    }

    private void SetEdgeCore(Tile tile, EdgeSide side, Edge? edge) {
        if (tile.Content is OpenTile open) {
            open.Edges[(Int32)side] = edge;
        }

        var (dx, dy) = side switch {
            EdgeSide.North => (0, -1),
            EdgeSide.East => (1, 0),
            EdgeSide.South => (0, 1),
            _ => (-1, 0),
        };

        var nx = tile.X + dx;
        var ny = tile.Y + dy;
        if (nx < 0 || ny < 0 || nx >= Size || ny >= Size) {
            return;
        }

        if (Tiles[nx, ny].Content is OpenTile neighbour) {
            neighbour.Edges[((Int32)side + 2) % 4] = edge;
        }
    }
}
