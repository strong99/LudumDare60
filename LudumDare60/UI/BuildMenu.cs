using LudumDare60.Tiles;
using LudumDare60;
﻿using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.UI;

public sealed class BuildMenu : UIElement {
    private const Single _innerRadius = 50f;
    private const Single _outerRadius = 130f;
    private const Int32 _fontSize = 16;
    private const Single _gapDegrees = 2f;
     
    private readonly (String Label, Action Action)[] _options;
    private readonly Action _close;
    private readonly Vector2 _centre = GetMousePosition();
    private Int32 _hovered = -1;

    public BuildMenu(World world, Tile worldTile, OpenTile tile, EdgeSide edge, ResourceManager resources, Action close) {
        _close = close;
        _options = [
            ("clear edge", () => world.SetEdge(worldTile, edge, null)),
            ("clear tile", () => tile.Content = null),
            ("pipe", () => world.SetEdge(worldTile, edge, new Pipe(resources))),
            ("wall", () => world.SetEdge(worldTile, edge, new Wall(resources))),
            ("boiler", () => tile.Content = new Boiler()),
            ("steam trap", () => tile.Content = new Steam()),
            ("crusher", () => tile.Content = new Crusher()),
            ("saw", () => tile.Content = new Saw()),
            ("nails", () => tile.Content = new Nails()),
        ];
        if (tile.Edges[(Int32)edge] is null) {
            _options = _options.Where(x=>x.Label != "clear edge").ToArray();
        }
        if (tile.Edges[(Int32)edge] is null) {
            _options = _options.Where(x=>x.Label != "clear tile").ToArray();
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
                _options[_hovered].Action();
                _close();
            }
            else if (distance > _outerRadius) {
                _close();
            }
        }
    }

    public void Draw() {
        for (var i = 0; i < _options.Length; i++) {
            var start = SectorStart(i) + _gapDegrees / 2;
            var end = start + SectorSize - _gapDegrees;
            var colour = i == _hovered ? Color.Orange : Color.DarkBrown;
            DrawRing(_centre, _innerRadius, _outerRadius, start, end, 24, Fade(colour, 0.9f));

            var mid = (start + end) / 2f * MathF.PI / 180f;
            var labelRadius = (_innerRadius + _outerRadius) / 2f;
            var labelPosition = _centre + new Vector2(MathF.Cos(mid), MathF.Sin(mid)) * labelRadius;
            var label = _options[i].Label;
            DrawText(label, (Int32)labelPosition.X - MeasureText(label, _fontSize) / 2, (Int32)labelPosition.Y - _fontSize / 2, _fontSize, Color.White);
        }
    }
}