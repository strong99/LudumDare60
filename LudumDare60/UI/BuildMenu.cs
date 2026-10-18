using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.UI;

public sealed class BuildMenu : UIElement {
    private const Single _innerRadius = 50f;
    private const Single _outerRadius = 130f;
    private const Int32 _fontSize = 16;
    private const Single _gapDegrees = 2f;

    private sealed record Option(String Label, Int32 ScrapCost, Func<Boolean> CanPlace, Action Place, Action<Vector3, Color>? DrawPreview = null, Boolean PreviewHasFloor = false, String? Steam = null);

    private readonly Option[] _options;
    private readonly World _world;
    private readonly OpenTile _openTile;
    private readonly Action _close;
    private readonly Vector3 _previewCentre;
    private readonly Vector2 _centre = GetMousePosition();
    private Int32 _hovered = -1;
    private readonly TextureHandle _scrapIcon;

    public BuildMenu(World world, Tile worldTile, OpenTile tile, EdgeSide edge, ResourceManager resources, TextureHandle scrapIcon, Action close) {
        _scrapIcon = scrapIcon;
        _close = close;
        _previewCentre = new Vector3(worldTile.X + 0.5f, 0f, worldTile.Y + 0.5f);

        var crusherModel = resources.GetOrLoadModel(Crusher.FileName);
        var pipe = new Pipe(resources);
        var wall = new Wall(resources);
        var boiler = new Boiler(resources);
        var relay = new RelayBoiler(resources);
        var saw = new Saw(resources, world, worldTile);
        var nails = new Spikes(resources, world, worldTile);
        var steam = new Steam(resources, world, worldTile);

        Tile? CrusherBody() => world.GetNeighbour(worldTile, edge) is { } body && world.IsFree(body) ? body : null;
        _world = world;
        _openTile = tile;
        _options = [
            new("clear edge", 2, () => true, () => world.SetEdge(worldTile, edge, null)),
            new("clear tile", 2, () => true, () => world.ClearContent(worldTile)),
            new("repair", 3, () => true, () => { foreach (var store in DamagedStores(tile, edge)) { store.Repair(); } }),
            new("pipe", 2, () => true, () => world.SetEdge(worldTile, edge, pipe),
                (centre, tint) => pipe.Draw(centre, edge, tint)),
            new("wall", 20, () => true, () => world.SetEdge(worldTile, edge, wall),
                (centre, tint) => wall.Draw(centre, edge, tint)),
            new("boiler", 50, () => true, () => world.SetContent(worldTile, boiler),
                boiler.Draw, PreviewHasFloor: boiler.RendersOwnFloor, Steam: "+10 /s"),
            new("relay", 30, () => true, () => world.SetContent(worldTile, relay),
                relay.Draw, PreviewHasFloor: relay.RendersOwnFloor, Steam: "buffer"),
            new("steam", 40, () => true, () => world.SetContent(worldTile, steam),
                steam.Draw, PreviewHasFloor: steam.RendersOwnFloor, Steam: $"{Steam.SteamCost} / hit"),
            new("crusher", 80,
                () => CrusherBody() is not null,
                () => world.TryPlace(new Crusher(resources, world, worldTile, CrusherBody()!, edge)),
                (centre, tint) => Crusher.Draw(crusherModel, centre, edge, 0, 0, tint), Steam: $"{Crusher.SteamCost} / stroke"),
            new("saw", 60, () => true, () => world.SetContent(worldTile, saw),
                saw.Draw, PreviewHasFloor: saw.RendersOwnFloor, Steam: $"{Saw.SteamCost} / hit"),
            new("spikes", 25, () => true, () => world.SetContent(worldTile, nails),
                nails.Draw, PreviewHasFloor: nails.RendersOwnFloor, Steam: $"{Spikes.SteamCost} / hit"),
        ];
        if (!DamagedStores(tile, edge).Any()) {
            _options = _options.Where(x => x.Label != "repair").ToArray();
        }
        if (tile.Edges[(Int32)edge] is null) {
            _options = _options.Where(x => x.Label != "clear edge").ToArray();
        }
        if (tile.Content is null) {
            _options = _options.Where(x => x.Label != "clear tile").ToArray();
        }
    }


    private static IEnumerable<SteamStore> DamagedStores(OpenTile tile, EdgeSide edge) {
        var content = tile.Content is ReferenceContent reference ? reference.Owner : tile.Content;
        return new[] { (tile.Edges[(Int32)edge] as SteamConnected)?.Store, (content as SteamConnected)?.Store }
            .OfType<SteamStore>()
            .Where(s => s.DamagePoints > 0);
    }

    private Single SectorSize => 360f / _options.Length;
    private Single SectorStart(Int32 index) => -90f - SectorSize / 2 + index * SectorSize;

    public void Progress(Double deltaTime) {
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

    private void DrawCostPanel(Option option) {
        const Int32 size = 14;
        var y = (Int32)_centre.Y - 22;
        var title = option.Label;
        DrawText(title, (Int32)_centre.X - MeasureText(title, size) / 2, y, size, Color.White);

        var cost = option.ScrapCost.ToString();
        var scrapWidth = _scrapIcon.Texture.Width + 4 + MeasureText(cost, size);
        var x = (Int32)_centre.X - scrapWidth / 2;
        y += size + 4;
        DrawTexture(_scrapIcon, x, y + (size - _scrapIcon.Texture.Height) / 2, Color.White);
        DrawText(cost, x + _scrapIcon.Texture.Width + 4, y, size, _world.CanAfford(option.ScrapCost) ? Color.White : Color.Red);

        if (option.Steam is { } steam) {
            y += size + 4;
            var width = size + 4 + MeasureText(steam, 12);
            x = (Int32)_centre.X - width / 2;
            // Steam icon: a small cloud.
            var cy = y + size / 2;
            DrawCircle(x + 4, cy + 2, 4, Color.SkyBlue);
            DrawCircle(x + 9, cy - 1, 5, Color.SkyBlue);
            DrawCircle(x + 13, cy + 2, 4, Color.SkyBlue);
            DrawText(steam, x + size + 4, y + 1, 12, Color.SkyBlue);
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
            var label = option.Label;
            var textColour = CanBuild(option) ? Color.White : Color.Gray;
            DrawText(label, (Int32)labelPosition.X - MeasureText(label, _fontSize) / 2, (Int32)labelPosition.Y - _fontSize / 2, _fontSize, textColour);
        }

        if (_hovered >= 0) {
            DrawCostPanel(_options[_hovered]);
        }
    }
}
