using Raylib_cs;
using static Raylib_cs.Raylib;

namespace LudumDare60.UI;

public sealed class ScrapCounter(Func<Int32> scraps, TextureHandle texture, TextureHandle icon) : UIElement {
    public TextureHandle Texture { get; } = texture;
    public TextureHandle Icon { get; } = icon;

    public void Progress(Double deltaTime) {

    }
    public void Draw() {
        var fontSize = 20;
        var text = $"{scraps()} scraps";
        var width = GetScreenWidth() - MeasureText(text, fontSize);


        var y = 20;
        DrawTexture(Texture, -180, y - 15, Color.Black);
        DrawTexture(Icon, 0, y - 7, Color.White);
        DrawText(text, 40, y, fontSize, Color.White);
    }
}
