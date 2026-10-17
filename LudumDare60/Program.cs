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
        World = new(16);
    }
}
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
    private Camera3D _camera;
    private CountDownCounter _countDownCounter;
    private ScrapCounter _scrapCounter;
    public void Progress(Double deltaTime) {
        var world = app.World!;

        HandleScroll();

        foreach (var tile in world.Tiles) {
            tile.Progress(deltaTime);
        }
        foreach (var enemy in world.Enemies) {
            enemy.Progress(deltaTime);
        }
        _countDownCounter.Progress(deltaTime);
        _scrapCounter.Progress(deltaTime);
    }
    private void HandleScroll() {
        const Single speed = 10f;
        const Single edgeSize = 20f;

        var mouseCoords = GetMousePosition();
        var direction = Vector2.Zero;

        if (IsKeyDown(KeyboardKey.Left) || mouseCoords.X < edgeSize) {
            direction.X -= 1;
        }
        if (IsKeyDown(KeyboardKey.Right) || mouseCoords.X > GetScreenWidth() - edgeSize) {
            direction.X += 1;
        }
        if (IsKeyDown(KeyboardKey.Up) || mouseCoords.Y < edgeSize) {
            direction.Y -= 1;
        }
        if (IsKeyDown(KeyboardKey.Down) || mouseCoords.Y > GetScreenHeight() - edgeSize) {
            direction.Y += 1;
        }

        if (direction == Vector2.Zero) {
            return;
        }

        var offset = Vector2.Normalize(direction) * speed * GetFrameTime();
        var size = app.World!.Size;
        var target = _camera.Target;
        var newX = Math.Clamp(target.X + offset.X, 0, size);
        var newZ = Math.Clamp(target.Z + offset.Y, 0, size);
        var move = new Vector3(newX - target.X, 0, newZ - target.Z);

        _camera.Position += move;
        _camera.Target += move;
    }
    public void Draw() {
        var world = app.World!;

        BeginMode3D(_camera);

        foreach (var tile in world.Tiles) {
            tile.Draw();
        }
        foreach (var enemy in world.Enemies) {
            enemy.Draw();
        }

        EndMode3D();

        //BeginMode2D();
        //DrawBuildMenu
        //DrawScrapCounter
        _countDownCounter.Draw();
        _scrapCounter.Draw();
        //EndMode2D();
    }
    public void Activate() {
        var world = app.World!;
        var centre = world.Size / 2f;
        _camera = new Camera3D {
            Position = new Vector3(centre, 12, world.Size + 2),
            Target = new Vector3(centre, 0, centre),
            Up = Vector3.UnitY,
            FovY = 45,
            Projection = CameraProjection.Perspective,
        };
        _countDownCounter = new(3600);
        _scrapCounter = new(() => app.World.Scraps);
    }
    public void Deactivate() {

    }
}
public sealed class GameOverState : GameState {
    public void Progress(Double deltaTime) => throw new NotImplementedException();
    public void Draw() => throw new NotImplementedException();
    public void Activate() => throw new NotImplementedException();
    public void Deactivate() => throw new NotImplementedException();
}
public sealed class Tile(Int32 x, Int32 y, TileContent content) {
    public Int32 X { get; } = x;
    public Int32 Y { get; } = y;
    public TileContent Content { get; } = content;
    public void Progress(Double deltaTime) => Content.Progress(deltaTime);
    public void Draw() => Content.Draw(new Vector3(X, 0, Y));
}
public interface TileContent {
    void Progress(Double deltaTime);
    void Draw(Vector3 origin);
}
public sealed class SolidTile : TileContent {
    public void Progress(Double deltaTime) { }
    public void Draw(Vector3 origin) {
        var centre = origin + new Vector3(0.5f, 0.5f, 0.5f);
        DrawCube(centre, 1, 1, 1, Color.Gray);
        DrawCubeWires(centre, 1, 1, 1, Color.DarkGray);
    }
}
public sealed class OpenTile : TileContent {
    public OpenTileContent? Content { get; set; }
    public Edge[] Edges { get; } = new Edge[4];
    public void Progress(Double deltaTime) => Content?.Progress(deltaTime);
    public void Draw(Vector3 origin) {
        var centre = origin + new Vector3(0.5f, -0.05f, 0.5f);
        DrawCube(centre, 1, 0.1f, 1, Color.Beige);
        DrawCubeWires(centre, 1, 0.1f, 1, Color.Brown);

        Content?.Draw();
    }
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
    public void Draw() {
        var centre = new Vector3(Position.X, 0.3f, Position.Y);
        DrawCube(centre, 0.4f, 0.6f, 0.4f, Color.Green);
        DrawCubeWires(centre, 0.4f, 0.6f, 0.4f, Color.DarkGreen);

        var facing = new Vector3(MathF.Sin(FacingRotation), 0, MathF.Cos(FacingRotation));
        DrawLine3D(centre, centre + facing * 0.4f, Color.Red);
    }
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
public sealed class CountDownCounter(Int32 duration) : UIElement {
    public Int32 Duration { get; set; } = duration;
    private Double _duration;
    public void Progress(double deltaTime) {
        _duration += deltaTime;
    }
    public void Draw() {
        var fontSize = 20;
        var timeLeft = Math.Max(Duration - _duration, 0);
        var text = $"{(Int32)timeLeft}s";
        var width = GetScreenWidth() - MeasureText(text, fontSize);

        DrawText(text, width / 2, 10, fontSize, Color.Red);
    }
}
public sealed class ScrapCounter(Func<Int32> scraps) : UIElement {
    public void Progress(double deltaTime) {
        
    }
    public void Draw() {
        var fontSize = 20;
        var text = $"{scraps()} scraps";
        var width = GetScreenWidth() - MeasureText(text, fontSize);

        DrawText(text, 10, 10, fontSize, Color.Red);
    }
}