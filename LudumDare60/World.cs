namespace LudumDare60;

public sealed class World {
    public Int32 Size { get; }
    public Tile[,] Tiles { get; }
    public Enemy[] Enemies { get; } = [];
    public Int32 Scraps { get; }
    public Double Duration { get; }
    public Memory EnemyKnowledge { get; } = new();

    public World(Int32 size, ResourceManager resourceManager) {
        Size = size;
        Tiles = new Tile[size, size];
        for (var x = 0; x < size; x++) {
            for (var y = 0; y < size; y++) {
                var border = x == 0 || y == 0 || x == size - 1 || y == size - 1;
                Tiles[x, y] = new Tile(x, y, border ? new SolidTile() : new OpenTile(resourceManager));
            }
        }
    }

    public void SetEdge(Tile tile, EdgeSide side, Edge? edge) {
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
