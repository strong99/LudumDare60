using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.GameStates;

public sealed class GameOverState(App app, Boolean win) : GameState {
    private Camera3D _camera;
    private SsaoPass _ssao = null!;
    private ModelHandle _cityScape;
    public void Progress(Double deltaTime) {
        if (IsKeyPressed(KeyboardKey.Escape)) {
            app.SetNextGameState(new MenuState(app));
        }
    }
    public void Draw() {
        var world = app.World!;

        _ssao.Begin();
        BeginMode3D(_camera);
        foreach (var decoration in world.Decorations) {
            decoration.Draw();
        }
        foreach (var tile in world.Tiles) {
            tile.Draw();
        }
        foreach (var enemy in world.Enemies) {
            enemy.Draw();
        }

        var backgroundScale = 8;
        var backGroundOffset = 8;
        DrawModel(_cityScape, new Vector3(backGroundOffset, -12, backGroundOffset), backgroundScale, new Color(190, 150, 120, 255));

        EndMode3D();
        _ssao.End();

        var width = GetScreenWidth();
        var height = GetScreenHeight();
        DrawRectangle(0, 0, width, height, new Color(0, 0, 0, 120));

        var title = win ? "Game won" : "Tower core destroyed";
        var titleSize = 60;
        DrawText(title, (width - MeasureText(title, titleSize)) / 2, height / 2 - titleSize, titleSize, win ? Color.Gold : Color.Red);

        var hint = "Press ESC to return to the main menu";
        var hintSize = 20;
        DrawText(hint, (width - MeasureText(hint, hintSize)) / 2, height / 2 + hintSize, hintSize, Color.White);
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
        _ssao = new SsaoPass(_camera.FovY);
        _cityScape = app.Resources.GetOrLoadModel("Assets/CityScape.glb");
    }
    public void Deactivate() {
        _ssao.Dispose();
    }
}
