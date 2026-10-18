using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.UI;

public sealed class IconButton(Vector2 position, TextureHandle back, TextureHandle icon, Action<IconButton> action) : UIElement {
    public SoundHandle? HoverSound { get; set; }
    public SoundHandle? ClickSound { get; set; }
    public TextureHandle Back { get; } = back;
    public TextureHandle Icon { get; } = icon;
    public Int32 Padding { get; set; } = 12;
    public Vector2 Position { get; set; } = position;
    private Boolean _hover = false;
    private Boolean _hovering = false;
    public void Progress(Double deltaTime) {

        var mousePosition = GetMousePosition();
        var halfX = Back.Texture.Width / 2;
        var halfY = Back.Texture.Height / 2;
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
        DrawTexture(Back, (Int32)Position.X - Back.Texture.Width / 2, (Int32)Position.Y - Back.Texture.Height / 2, _hover ? Color.White : Color.Black);
        DrawTexture(Icon, (Int32)Position.X - Icon.Texture.Width / 2, (Int32)Position.Y - Icon.Texture.Height / 2, _hover ? Color.Black : Color.White);
    }
}
