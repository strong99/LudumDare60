using Raylib_cs;
using static Raylib_cs.Raylib;

namespace LudumDare60.UI;

public sealed class TowerCoreCounter(Func<Int32> towerCoreHitpoints, TextureHandle texture, TextureHandle icon) : UIElement {
    public void Progress(Double deltaTime) {

    }
    public void Draw() {
        var fontSize = 20;
        var text = $"{towerCoreHitpoints()} hp";
        var width = GetScreenWidth() - MeasureText(text, fontSize);

        var y = 70;
        DrawTexture(texture, -180, y - 15, Color.Black);
        DrawTexture(icon, 0, y - 7, Color.White);
        DrawText(text, 40, y, fontSize, Color.White);
    }
}
