using LudumDare60.Generators;
using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.GameStates;

public sealed class MenuState(App app) : GameState {
    private Camera3D _camera;
    private SsaoPass _ssao = null!;
    private const Int32 _fontSize = 20;
    private const Int32 _padding = 20;
    private const Int32 _spacing = 10;
    private TextButton _newGame;
    private TextButton? _continueGame;
    private TextButton _exit;
    private Int32 _layoutWidth;
    private ModelHandle _towerModel;
    private ModelHandle _cityModel;
    private ModelHandle _zeppelinModel;
    private MusicGenerator<MenuMusicGeneratorStrategy>? _music;
    private Double _time = 0;
    private const Single _zeppelinSpeed = 0.25f;

    private readonly List<(Vector3 Start, Vector3 End, Single Offset)> _zeppelins = [
        (new(-10f, 3f, -1.5f), new(10f, 3f, -1.5f), 10),
        (new(-8f, 3f, -3.5f), new(8f, 3f, -3.5f), 30),
        (new(-8f, 2f, -3.5f), new(8f, 2f, -3.5f), 15),
        (new(-6f, 3f, -4f), new(2f, 3f, 7f), 10),
        (new(-6f, 3f, 3f), new(2f, 3f, -6f), 20),
        (new(-6f, 2f, 3f), new(2f, 2f, -6f), 15)
    ];

    public void Progress(Double deltaTime) {
        _time += deltaTime;
        _music?.Progress();
        if (GetScreenWidth() != _layoutWidth) {
            UpdateOnResize();
        }
        _newGame.Progress(deltaTime);
        _continueGame?.Progress(deltaTime);
        _exit.Progress(deltaTime);

        _camera.Position = new Vector3(MathF.Sin((Single)_time / 10f) * 5f, 4.5f, MathF.Cos((Single)_time / 10f) * 5f);
    }
    public void Draw() {
        _ssao.Begin();
        BeginMode3D(_camera);

        DrawModel(_towerModel, Vector3.Zero, 1, Color.White);
        DrawModel(_cityModel, Vector3.Zero, 1, Color.White);

        var scale = Vector3.One / 20f;
        foreach (var (start, end, offset) in _zeppelins) {
            var displacement = end - start;
            var distance = displacement.Length();
            var travelled = (_time + offset) * _zeppelinSpeed % distance;
            var position = start + displacement * (Single)(travelled / distance);
            var yaw = MathF.Atan2(-displacement.Z, displacement.X) * (180f / MathF.PI) + 90;
            DrawModelEx(_zeppelinModel, position, Vector3.UnitY, yaw, scale, Color.White);
        }

        EndMode3D();
        _ssao.End();

        _newGame.Draw();
        _continueGame?.Draw();
        _exit.Draw();
    }
    public void Activate() {
        _camera = new Camera3D {
            Position = new (5f, 4.5f, 4.5f),
            Target = new (0f, 2f, 0f),
            Up = Vector3.UnitY,
            FovY = 45,
            Projection = CameraProjection.Perspective,
        };
        _ssao = new SsaoPass(_camera.FovY);

        var width = _layoutWidth = GetScreenWidth();
        var centreX = width / 2;
        var buttonHeight = _fontSize + _padding + _spacing;
        var y = buttonHeight;

        _towerModel = app.Resources.GetOrLoadModel("Assets/Menu.glb");
        _cityModel = app.Resources.GetOrLoadModel("Assets/CityScape.glb");
        _zeppelinModel = app.Resources.GetOrLoadModel("Assets/Zeppelin.glb");

        _newGame = new TextButton("new", new(centreX, y += buttonHeight), _ => {
            app.CreateNewWorld();
            app.SetNextGameState(new PlayState(app));
        }) {
            FontSize = _fontSize
        };
        if (app.World is not null) {
            _continueGame = new TextButton("continue", new(centreX, y += buttonHeight), _ => app.SetNextGameState(new PlayState(app))) {
                FontSize = _fontSize,
            };
        }
        _exit = new TextButton("exit", new(centreX, y += buttonHeight), _ => app.Exit()) {
            FontSize = _fontSize
        };

        _music = new(new MenuMusicGeneratorStrategy());
        _music.Play();
    }
    private void UpdateOnResize() {
        var width = _layoutWidth = GetScreenWidth();
        var centreX = width / 2;
        var buttonHeight = _fontSize + _padding + _spacing;
        var y = buttonHeight;

        _newGame.Position = new(centreX, y += buttonHeight);
        if (_continueGame is not null) {
            _continueGame.Position = new(centreX, y += buttonHeight);
        }
        _exit.Position = new(centreX, y += buttonHeight);
    }
    public void Deactivate() {
        _music?.Dispose();
        _music = null;
    }
}
