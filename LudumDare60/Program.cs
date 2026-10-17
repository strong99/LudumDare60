using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

using var app = new App();
app.Run();

public sealed class App : IDisposable {
    private Boolean _active = false;
    private GameState? _nextGameState;
    public World? World { get; set; }
    public GameState GameState { get; private set; }
    public void SetNextGameState(GameState state) {
        _nextGameState = state;
    }
    public void Run() {
        _active = true;
        var screenWidth = 1280;
        var screenHeight = 720;
        var title = "LudumDare60 - BoilerKeep";
        var targetFPS = 60;

        InitWindow(screenWidth, screenHeight, title);
        SetTargetFPS(targetFPS);

        GameState?.Activate();

        var previousTime = GetTime();
        while (!WindowShouldClose() && _active) {
            if (_nextGameState is not null && _nextGameState != GameState) {
                GameState?.Deactivate();
                GameState = _nextGameState;
                GameState?.Activate();
            }

            var currentTime = GetTime();
            var deltaTime = currentTime - previousTime;
            previousTime = currentTime;

            Progress(deltaTime);
            Draw();
        }

        GameState?.Deactivate();

        if (!WindowShouldClose()) {
            CloseWindow();
        }
    }
    private void Progress(Double deltaTime) {
        GameState.Progress(deltaTime);
    }
    private void Draw() {
        BeginDrawing();
        ClearBackground(Raylib_cs.Color.RayWhite);
        GameState.Draw();
        EndDrawing();
    }

    public void Exit() {
        _active = false;
    }

    public void Dispose() {

    }

    public App() {
        GameState = new MenuState(this);
    }

    public void CreateNewWorld() {
        World = new();
    }
}
public sealed class World {
    public Camera3D Camera3D;
    public Tile[,] Tiles { get; }
    public Enemy[] Enemies { get; }
    public Int32 Scraps { get; }
    public Double Duration { get; }
    public Memory EnemyKnowledge { get; }
}
public interface GameState {
    void Progress(Double deltaTime);
    void Draw();
    void Activate();
    void Deactivate();
}
public sealed class MenuState(App app) : GameState {
    private TextButton _newGame;
    private TextButton? _continueGame;
    private TextButton _exit;
    public void Progress(Double deltaTime) {
        _newGame.Progress(deltaTime);
        _continueGame?.Progress(deltaTime);
        _exit.Progress(deltaTime);
    }
    public void Draw() {
        _newGame.Draw();
        _continueGame?.Draw();
        _exit.Draw();
    }
    public void Activate() {
        var width = GetScreenWidth();
        var centreX = width / 2;
        var fontSize = 20;
        var padding = 20;
        var spacing = 10;
        var buttonHeight = fontSize + padding + spacing;
        var y = buttonHeight;
        _newGame = new TextButton("new", new(centreX, y += buttonHeight), _ => {
            app.CreateNewWorld();
            app.SetNextGameState(new PlayState(app));
        }) {
            FontSize = fontSize
        };
        if (app.World is not null) {
            _continueGame = new TextButton("continue", new(centreX, y += buttonHeight), _ => app.SetNextGameState(new PlayState(app))) {
                FontSize = fontSize,
            };
        }
        _exit = new TextButton("exit", new(centreX, y += buttonHeight), _ => app.Exit()) {
            FontSize = fontSize
        };
    }
    public void Deactivate() {

    }
}
public sealed class PlayState(App app) : GameState {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
    public void Activate() => throw new NotImplementedException();
    public void Deactivate() => throw new NotImplementedException();
}
public sealed class GameOverState : GameState {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
    public void Activate() => throw new NotImplementedException();
    public void Deactivate() => throw new NotImplementedException();
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
    public EnemyKind Kind { get; }
    public EnemyBehaviour Behaviour { get; }
    public Memory Memory { get; }
    public Int32 HitPoints { get; set; }
    public Vector2 Position { get; set; }
    public Single FacingRotation { get; set; }
}
public sealed record EnemyKind(String Name, Int32 MaxHitPoints, Single Speed, Single SawResistance, Single SteamResistance);
public interface EnemyBehaviour {
    void Progress(Double deltaTime);
}
public sealed class ReturnHomeBehaviour : EnemyBehaviour {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
}
public sealed class ExploreBehaviour : EnemyBehaviour {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
}
public sealed class AttackBehaviour : EnemyBehaviour {
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
public sealed class TextButton(String text, Vector2 position, Action<TextButton> action) : UIElement {
    public Int32 FontSize {
        get;
        set {
            field = value;
            _isDirty = true;
        }
    } = 12;
    public Int32 Padding {
        get;
        set {
            field = value;
            _isDirty = true;
        }
    } = 12;
    private Boolean _isDirty = true;
    private Int32 _size = 0;
    private Boolean _hover = false;
    public void Progress(Double deltaTime) {
        if (_isDirty) {
            _size = MeasureText(text, FontSize);
            _isDirty = false;
        }

        var mousePosition = GetMousePosition();
        var halfX = _size / 2 + Padding;
        var halfY = FontSize / 2 + Padding;
        var relativeMousePosition = mousePosition - position;
        _hover = relativeMousePosition.X > -halfX && relativeMousePosition.X < halfX &&
                 relativeMousePosition.Y > -halfY && relativeMousePosition.Y < halfY;
        if (_hover) {
            if (IsMouseButtonPressed(MouseButton.Left)) {
                action(this);
            }
        }
    }
    public void Draw() {
        DrawText(text, (Int32)position.X - _size / 2, (Int32)position.Y - FontSize / 2, FontSize, _hover ? Color.Blue : Color.Red);
    }
}