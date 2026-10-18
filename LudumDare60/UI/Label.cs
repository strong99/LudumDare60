using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.UI;

public sealed class Label(String text, Vector2 position, TextureHandle texture) : UIElement {
    public TextureHandle Texture { get; } = texture;
    public Int32 FontSize {
        get;
        set {
            field = value;
            _isDirty = true;
        }
    } = 20;
    public Vector2 Position { get; set; } = position;
    public String Text { 
        get; 
        set {
            field = value;
            _isDirty = true;
        }
    } = text;
    private Boolean _isDirty = true;
    private Int32 _textWidth = 0;
    public void Progress(Double deltaTime) {
        if (_isDirty) {
            _textWidth = MeasureText(Text, FontSize);
            _isDirty = false;
        }
    }
    public void Draw() {
        DrawTexture(Texture, (Int32)Position.X - Texture.Texture.Width / 2, (Int32)Position.Y - Texture.Texture.Height / 2, Color.Black);
        DrawText(Text, (Int32)Position.X - _textWidth / 2, (Int32)Position.Y - FontSize / 2, FontSize, Color.White);
    }
}
