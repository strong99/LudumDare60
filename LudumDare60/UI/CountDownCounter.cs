using Raylib_cs;
using static Raylib_cs.Raylib;

namespace LudumDare60.UI;

public sealed class CountDownCounter(Int32 duration, TextureHandle texture, TextureHandle icon) : UIElement {
    public Int32 Duration { get; set; } = duration;
    private Double _duration;
    public Boolean TimeUp { get => Duration < _duration; }
    public void Progress(Double deltaTime) {
        _duration += deltaTime;
    }
    public void Draw() {
        var fontSize = 20;
        var timeLeft = (Int32)Math.Max(Duration - _duration, 0);
        var minutes = timeLeft / 60;
        var seconds = timeLeft % 60;
        var text = $"{minutes}m{seconds}s";

        var y = 120;
        DrawTexture(texture, -180, y - 15, Color.Black);
        DrawTexture(icon, 0, y - 7, Color.White);
        DrawText(text, 40, y, fontSize, Color.White);
    }
}
