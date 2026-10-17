namespace LudumDare60;

public sealed class World {
    public Int32 Size { get; }
    public Tile[,] Tiles { get; }
    public Enemy[] Enemies { get; } = [];
    public Int32 Scraps { get; }
    public Double Duration { get; }
    public Memory EnemyKnowledge { get; } = new();

    public World(Int32 size) {
        Size = size;
        Tiles = new Tile[size, size];
        for (var x = 0; x < size; x++) {
            for (var y = 0; y < size; y++) {
                var border = x == 0 || y == 0 || x == size - 1 || y == size - 1;
                Tiles[x, y] = new Tile(x, y, border ? new SolidTile() : new OpenTile());
            }
        }
    }
}
