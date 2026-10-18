using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.UI;

public sealed class TextButton(String text, Vector2 position, TextureHandle texture, Action<TextButton> action) : UIElement {
    public Int32 FontSize {
        get;
        set {
            field = value;
            _isDirty = true;
        }
    } = 16;
    public Int32 Padding { get; set; } = 12;
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
    private Boolean _hover = false;
    private TextureHandle _texture = texture;
    public void Progress(Double deltaTime) {

        if (_isDirty) {
            _textWidth = MeasureText(Text, FontSize);
            _isDirty = false;
        }

        var mousePosition = GetMousePosition();
        var halfX = _texture.Texture.Width / 2;
        var halfY = _texture.Texture.Height / 2;
        var relativeMousePosition = mousePosition - Position;
        _hover = relativeMousePosition.X > -halfX && relativeMousePosition.X < halfX &&
                 relativeMousePosition.Y > -halfY && relativeMousePosition.Y < halfY;
        if (_hover) {
            if (IsMouseButtonPressed(MouseButton.Left)) {
                action(this);
            }
        }
    }
    public void Draw() {
        DrawTexture(_texture, (Int32)Position.X - _texture.Texture.Width / 2, (Int32)Position.Y - _texture.Texture.Height / 2, _hover ? Color.White : Color.Gray);
        DrawText(Text, (Int32)Position.X - _textWidth / 2, (Int32)Position.Y - FontSize / 2, FontSize, _hover ? Color.Black : Color.White);
    }
}
