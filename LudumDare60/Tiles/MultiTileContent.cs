namespace LudumDare60.Tiles;

public interface MultiTileContent : OpenTileContent {
    IReadOnlyList<Tile> Members { get; }
}
