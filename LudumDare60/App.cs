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

    public static Action? FullscreenOverride { get; set; }
    public void ToggleFullscreenWindow() {
        if (FullscreenOverride is not null) {
            FullscreenOverride();
        } else {
            ToggleFullscreen();
        }
    }
    public void Init() {
        var screenWidth = 1280;
        var screenHeight = 720;
        var title = "LudumDare60 - BoilerKeep";

        SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.VSyncHint);

        InitWindow(screenWidth, screenHeight, title);
        InitAudioDevice();

        SetExitKey(KeyboardKey.F12);

        _active = true;
        GameState?.Activate();
    }
    public void Tick(Double deltaTime) {
        if (_nextGameState is not null && _nextGameState != GameState) {
            var oldState = GameState;
            GameState = _nextGameState;

            GameState?.Activate();
            oldState?.Deactivate();
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

            if (IsKeyPressed(KeyboardKey.F11)) {
                ToggleFullscreenWindow();
            }

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
