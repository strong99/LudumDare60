using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.UI;

public sealed class TextButton(String text, Vector2 position, TextureHandle texture, Action<TextButton> action) : UIElement {
    public SoundHandle? HoverSound { get; set; }
    public SoundHandle? ClickSound { get; set; }
    public TextureHandle Texture { get; } = texture;
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
    private Boolean _hovering = false;
    public void Progress(Double deltaTime) {

        if (_isDirty) {
            _textWidth = MeasureText(Text, FontSize);
            _isDirty = false;
        }

        var mousePosition = GetMousePosition();
        var halfX = Texture.Texture.Width / 2;
        var halfY = Texture.Texture.Height / 2;
        var relativeMousePosition = mousePosition - Position;
        _hover = relativeMousePosition.X > -halfX && relativeMousePosition.X < halfX &&
                 relativeMousePosition.Y > -halfY && relativeMousePosition.Y < halfY;
        if (_hover) {
            var newHover = !_hovering;
            if (newHover && HoverSound is { } hoverSound) {
                StopSound(hoverSound);
                PlaySound(hoverSound);
            }

            if (!_hovering) {
                _hovering = true;
            }

            if (IsMouseButtonPressed(MouseButton.Left)) {
                if (ClickSound is { } clickSound) {
                    StopSound(clickSound);
                    PlaySound(clickSound);
                }

                action(this);
            }
        }
        else {
            _hovering = false;
        }
    }
    public void Draw() {
        DrawTexture(Texture, (Int32)Position.X - Texture.Texture.Width / 2, (Int32)Position.Y - Texture.Texture.Height / 2, _hover ? Color.White : Color.Gray);
        DrawText(Text, (Int32)Position.X - _textWidth / 2, (Int32)Position.Y - FontSize / 2, FontSize, _hover ? Color.Black : Color.White);
    }
}
