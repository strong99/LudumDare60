using System.Numerics;
using static Raylib_cs.Raylib;

public sealed class App {
    public World? World { get; }
    public GameState GameState { get; }
    public void Run() => throw new NotImplementedException();
    public void Draw() {
        var screenWidth = 1280;
        var screenHeight = 720;
        var title = "LudumDare60 - BoilerKeep";
        InitWindow(screenWidth, screenHeight, title);
    }
    public void Exit() => throw new NotImplementedException();
    public App() {
        GameState = new MenuState(this);
    }
}
public sealed class World {
    public Tile[,] Tiles { get; }
    public Enemy[] Enemies { get; } 
    public Int32 Scraps { get; }
    public Double Duration { get; }
    public Memory EnemyKnowledge { get; }
}
public interface GameState {
    void Progress(Double deltaTime);
    void Draw();
}
public sealed class MenuState(App app) : GameState {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
}
public sealed class PlayState : GameState {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
}
public sealed class GameOverState : GameState {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
}
public sealed class Tile {
    public TileContent Content { get; }
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
}
public interface TileContent {
    void Progress(Double deltaTime);
    void Draw();
}
public sealed class SolidTile : TileContent {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
}
public sealed class OpenTile : TileContent {
    public OpenTileContent Content { get; set; }
    public Edge[] Edges { get; } = new Edge[4];
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
}
public interface Edge {
    void Progress(Double deltaTime) => throw new NotImplementedException();
    void Draw() => throw new NotImplementedException();
}
public sealed class Pipe : Edge {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
}
public sealed class Wall : Edge {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
}
public interface OpenTileContent {
    void Progress(Double deltaTime) => throw new NotImplementedException();
    void Draw() => throw new NotImplementedException();
}
public sealed class Loot : OpenTileContent {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
}
public sealed class TowerCore : OpenTileContent {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
}
public sealed class Boiler : OpenTileContent {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
}
public sealed class RelayBoiler : OpenTileContent {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
}
public interface Trap : OpenTileContent;
public sealed class Crusher : Trap {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
}
public sealed class Nails : Trap {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
}
public sealed class Saw : Trap {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
}
public sealed class Steam : Trap {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
}
public sealed class WaveSpawner {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
}
public sealed class Enemy {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
    public EnemyBehaviour Behaviour { get; }
    public Int32 HitPoints { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Rotation { get; set; }
}
public interface EnemyBehaviour {
    void Progress(Double deltaTime);
}
public sealed class GoblinScraper : EnemyBehaviour {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
}
public sealed class GoblinEngineer : EnemyBehaviour {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
}
public sealed class GoblinFigher : EnemyBehaviour {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
}
public sealed class Pathfinder {
    public Vector2 GetPath(Vector2 destination) => throw new NotImplementedException();
}
public sealed class SteamNetwork {
    public Vector2 CalculateForeachConnection() => throw new NotImplementedException();
}
public sealed class Memory {
    public Boolean IsKnown(OpenTileContent tile) => throw new NotImplementedException();
    public Boolean SyncWith(Memory memory) => throw new NotImplementedException();
}
public interface UIElement {
    void Progress(Double deltaTime);
    void Draw();
}
public sealed class Button : UIElement {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
}