using Raylib_cs;
using static Raylib_cs.Raylib;

namespace LudumDare60.UI;

public sealed class TowerCoreCounter(Func<Int32> towerCoreHitpoints) : UIElement {
    public void Progress(Double deltaTime) {

    }
    public void Draw() {
        var fontSize = 20;
        var text = $"{towerCoreHitpoints()} hp";
        var width = GetScreenWidth() - MeasureText(text, fontSize);

        var height = GetScreenHeight();

        DrawText(text, 10, height - 10 - fontSize, fontSize, Color.Red);
    }
}
