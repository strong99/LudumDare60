using Raylib_cs;
using static Raylib_cs.Raylib;

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
