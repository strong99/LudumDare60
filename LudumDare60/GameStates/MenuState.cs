using static Raylib_cs.Raylib;

namespace LudumDare60.GameStates;

public sealed class MenuState(App app) : GameState {
    private const Int32 _fontSize = 20;
    private const Int32 _padding = 20;
    private const Int32 _spacing = 10;
    private TextButton _newGame;
    private TextButton? _continueGame;
    private TextButton _exit;
    private Int32 _layoutWidth;
    public void Progress(Double deltaTime) {
        if (GetScreenWidth() != _layoutWidth) {
            UpdateOnResize();
        }
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
        var width = _layoutWidth = GetScreenWidth();
        var centreX = width / 2;
        var buttonHeight = _fontSize + _padding + _spacing;
        var y = buttonHeight;

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

    }
}
