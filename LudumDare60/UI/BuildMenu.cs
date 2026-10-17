using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.UI;

public sealed class BuildMenu : UIElement {
    private const Single _innerRadius = 50f;
    private const Single _outerRadius = 130f;
    private const Int32 _fontSize = 16;
    private const Single _gapDegrees = 2f;

    private sealed record Option(String Label, Int32 ScrapCost, Func<Boolean> CanPlace, Action Place, Action<Vector3, Color>? DrawPreview = null, Boolean PreviewHasFloor = false);

    private readonly Option[] _options;
    private readonly World _world;
    private readonly OpenTile _openTile;
    private readonly Action _close;
    private readonly Vector3 _previewCentre;
    private readonly Vector2 _centre = GetMousePosition();
    private Int32 _hovered = -1;

    public BuildMenu(World world, Tile worldTile, OpenTile tile, EdgeSide edge, ResourceManager resources, Action close) {
        _close = close;
        _previewCentre = new Vector3(worldTile.X + 0.5f, -0.05f, worldTile.Y + 0.5f);

        var crusherModel = resources.GetOrLoadModel(Crusher.FileName);
        var pipe = new Pipe(resources);
        var wall = new Wall(resources);
        var boiler = new Boiler(resources);
        var saw = new Saw(resources, world, worldTile);
        var nails = new Nails(resources);

        Tile? CrusherBody() => world.GetNeighbour(worldTile, edge) is { } body && world.IsFree(body) ? body : null;
        _world = world;
        _openTile = tile;
        _options = [
            new("clear edge", 2, () => true, () => world.SetEdge(worldTile, edge, null)),
            new("clear tile", 2, () => true, () => world.ClearContent(worldTile)),
            new("pipe", 5, () => true, () => world.SetEdge(worldTile, edge, pipe),
                (centre, tint) => pipe.Draw(centre, edge, tint)),
            new("wall", 20, () => true, () => world.SetEdge(worldTile, edge, wall),
                (centre, tint) => wall.Draw(centre, edge, tint)),
            new("boiler", 50, () => true, () => world.SetContent(worldTile, boiler),
                boiler.Draw, PreviewHasFloor: boiler.RendersOwnFloor),
            new("steam trap", 40, () => true, () => world.SetContent(worldTile, new Steam()), PreviewHasFloor: true),
            new("crusher", 80,
                () => CrusherBody() is not null,
                () => world.TryPlace(new Crusher(resources, world, worldTile, CrusherBody()!, edge)),
                (centre, tint) => Crusher.Draw(crusherModel, centre, edge, 0, 0, tint)),
            new("saw", 60, () => true, () => world.SetContent(worldTile, saw),
                saw.Draw, PreviewHasFloor: saw.RendersOwnFloor),
            new("nails", 25, () => true, () => world.SetContent(worldTile, nails),
                nails.Draw, PreviewHasFloor: nails.RendersOwnFloor),
        ];
        if (tile.Edges[(Int32)edge] is null) {
            _options = _options.Where(x => x.Label != "clear edge").ToArray();
        }
        if (tile.Edges[(Int32)edge] is null) {
            _options = _options.Where(x => x.Label != "clear tile").ToArray();
        }
    }

    private Single SectorSize => 360f / _options.Length;
    private Single SectorStart(Int32 index) => -90f - SectorSize / 2 + index * SectorSize;

    public void Progress(double deltaTime) {
        var offset = GetMousePosition() - _centre;
        var distance = offset.Length();
        _hovered = -1;
        if (distance >= _innerRadius && distance <= _outerRadius) {
            var degrees = MathF.Atan2(offset.Y, offset.X) * 180f / MathF.PI + 90f + SectorSize / 2;
            degrees = (degrees % 360f + 360f) % 360f;
            _hovered = (Int32)(degrees / SectorSize) % _options.Length;
        }

        if (IsMouseButtonPressed(MouseButton.Left)) {
            if (_hovered >= 0) {
                var option = _options[_hovered];
                if (CanBuild(option)) {
                    option.Place();
                    _world.Spend(option.ScrapCost);
                    _close();
                }
            }
            else if (distance > _outerRadius) {
                _close();
            }
        }
    }

    private Boolean CanBuild(Option option) => _world.CanAfford(option.ScrapCost) && option.CanPlace();

    public void ApplyPreviewState() {
        if (_hovered >= 0 && _options[_hovered].PreviewHasFloor) {
            _openTile.HideFloorThisFrame = true;
        }
    }

    public void DrawPreview() {
        if (_hovered >= 0 && _options[_hovered] is { DrawPreview: { } draw } option) {
            draw(_previewCentre, CanBuild(option) && _world.CanAfford(option.ScrapCost) ? Color.Blue : Color.Red);
        }
    }

    public void Draw() {
        for (var i = 0; i < _options.Length; i++) {
            var option = _options[i];
            var start = SectorStart(i) + _gapDegrees / 2;
            var end = start + SectorSize - _gapDegrees;
            var colour = i == _hovered ? Color.Orange : _world.CanAfford(option.ScrapCost) ? Color.DarkBrown : Color.Red;
            DrawRing(_centre, _innerRadius, _outerRadius, start, end, 24, Fade(colour, 0.9f));

            var mid = (start + end) / 2f * MathF.PI / 180f;
            var labelRadius = (_innerRadius + _outerRadius) / 2f;
            var labelPosition = _centre + new Vector2(MathF.Cos(mid), MathF.Sin(mid)) * labelRadius;
            var label = option.ScrapCost > 0 ? $"{option.Label} ({option.ScrapCost})" : option.Label;
            var textColour = CanBuild(option) ? Color.White : Color.Gray;
            DrawText(label, (Int32)labelPosition.X - MeasureText(label, _fontSize) / 2, (Int32)labelPosition.Y - _fontSize / 2, _fontSize, textColour);
        }
    }
}