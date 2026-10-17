using static Raylib_cs.Raylib;

namespace LudumDare60.GameStates;

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
