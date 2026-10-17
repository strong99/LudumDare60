using Raylib_cs;
using static Raylib_cs.Raylib;

namespace LudumDare60.UI;

public sealed class CountDownCounter(Int32 duration) : UIElement {
    public Int32 Duration { get; set; } = duration;
    private Double _duration;
    public Boolean TimeUp { get => Duration < _duration; }
    public void Progress(double deltaTime) {
        _duration += deltaTime;
    }
    public void Draw() {
        var fontSize = 20;
        var timeLeft = (Int32)Math.Max(Duration - _duration, 0);
        var minutes = timeLeft / 60;
        var seconds = timeLeft % 60;
        var text = $"{minutes}m{seconds}s";
        var width = GetScreenWidth() - MeasureText(text, fontSize);

        DrawText(text, width / 2, 10, fontSize, Color.Red);
    }
}
