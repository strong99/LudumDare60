using LudumDare60;
using LudumDare60.Generators;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace LudumDare60;

public sealed class App : IDisposable {
    private Boolean _active = false;
    private GameState? _nextGameState;
    public World? World { get; set; }
    public ResourceManager Resources { get; } = new();
    public GameState GameState { get; private set; }
    public void SetNextGameState(GameState state) {
        _nextGameState = state;
    }
    public void Init() {
        var screenWidth = 1280;
        var screenHeight = 720;
        var title = "LudumDare60 - BoilerKeep";
        var targetFPS = 60;

        SetConfigFlags(ConfigFlags.ResizableWindow);
        SetTargetFPS(targetFPS);
        SetExitKey(KeyboardKey.Null);

        InitWindow(screenWidth, screenHeight, title);
        InitAudioDevice();

        _active = true;
        GameState?.Activate();
    }
    public void Tick(Double deltaTime) {
        if (_nextGameState is not null && _nextGameState != GameState) {
            GameState?.Deactivate();
            GameState = _nextGameState;
            GameState?.Activate();
        }

        Progress(deltaTime);
        Draw();
    }
    public void Run() {
        Init();

        var previousTime = GetTime();
        while (!WindowShouldClose() && _active) {
            var currentTime = GetTime();
            var deltaTime = currentTime - previousTime;
            previousTime = currentTime;

            Tick(deltaTime);
        }

        Deinit();
    }
    
    public void Deinit() {
        _active = false;

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
        var worldGen = new CaveWorldGenerator();

        var (tiles, decorations) = worldGen.Generate(16, Resources, 100);
        World = new(16, Resources, tiles, decorations);
    }
}
