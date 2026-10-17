using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.GameStates;

public sealed class PlayState(App app) : GameState {
    private Camera3D _camera;
    private CountDownCounter _countDownCounter;
    private ScrapCounter _scrapCounter;
    private BuildMenu? _buildMenu;
    private Tile? _hoveredTile = null;
    private Edge? _hoveredEdge = null;
    private EdgeSide _hoveredEdgeIndex = EdgeSide.North;
    public void Progress(Double deltaTime) {
        var world = app.World!;

        HandleScroll();
        if (_buildMenu is null) {
            HandleTileHover();
        }
        HandleBuildMenu();

        foreach (var tile in world.Tiles) {
            tile.Progress(deltaTime);
        }
        foreach (var enemy in world.Enemies) {
            enemy.Progress(deltaTime);
        }
        _countDownCounter.Progress(deltaTime);
        _scrapCounter.Progress(deltaTime);
        _buildMenu?.Progress(deltaTime);
    }
    private void HandleScroll() {
        const Single speed = 10f;
        const Single edgeSize = 20f;

        var mouseCoords = GetMousePosition();
        var direction = Vector2.Zero;

        if (IsKeyDown(KeyboardKey.Left) || mouseCoords.X < edgeSize) {
            direction.X -= 1;
        }
        if (IsKeyDown(KeyboardKey.Right) || mouseCoords.X > GetScreenWidth() - edgeSize) {
            direction.X += 1;
        }
        if (IsKeyDown(KeyboardKey.Up) || mouseCoords.Y < edgeSize) {
            direction.Y -= 1;
        }
        if (IsKeyDown(KeyboardKey.Down) || mouseCoords.Y > GetScreenHeight() - edgeSize) {
            direction.Y += 1;
        }

        if (direction == Vector2.Zero) {
            return;
        }

        var offset = Vector2.Normalize(direction) * speed * GetFrameTime();
        var size = app.World!.Size;
        var target = _camera.Target;
        var newX = Math.Clamp(target.X + offset.X, 0, size);
        var newZ = Math.Clamp(target.Z + offset.Y, 0, size);
        var move = new Vector3(newX - target.X, 0, newZ - target.Z);

        _camera.Position += move;
        _camera.Target += move;
    }
    private void HandleTileHover() {
        var world = app.World!;
        var ray = GetScreenToWorldRay(GetMousePosition(), _camera);
        if (MathF.Abs(ray.Direction.Y) < 1e-6f) {
            return;
        }

        _hoveredTile = null;

        foreach (var planeY in (ReadOnlySpan<Single>)[1f, 0f]) {
            var distance = (planeY - ray.Position.Y) / ray.Direction.Y;
            if (distance < 0) {
                continue;
            }

            var hit = ray.Position + ray.Direction * distance;
            var tileX = (Int32)MathF.Floor(hit.X);
            var tileY = (Int32)MathF.Floor(hit.Z);
            if (tileX < 0 || tileY < 0 || tileX >= world.Size || tileY >= world.Size) {
                continue;
            }

            var tile = world.Tiles[tileX, tileY];
            if (planeY > 0 && tile.Content is not SolidTile) {
                continue;
            }

            // Nearest edge
            var offsetX = hit.X - (tileX + 0.5f);
            var offsetZ = hit.Z - (tileY + 0.5f);
            _hoveredEdgeIndex = MathF.Abs(offsetX) > MathF.Abs(offsetZ)
                ? (offsetX > 0 ? EdgeSide.East : EdgeSide.West)
                : (offsetZ > 0 ? EdgeSide.South : EdgeSide.North);
            _hoveredEdge = tile.Content is OpenTile open ? open.Edges[(Int32)_hoveredEdgeIndex] : null;

            _hoveredTile = tile;
            break;
        }
    }
    private void HandleBuildMenu() {
        if (_buildMenu is null && _hoveredTile is { Content: OpenTile open } hovered) {
            if (IsMouseButtonPressed(MouseButton.Left)) {
                _buildMenu = new BuildMenu(app.World!, hovered, open, _hoveredEdgeIndex, app.Resources, () => _buildMenu = null);
            }
        }
        else if (_buildMenu is not null && IsKeyPressed(KeyboardKey.Escape)) {
            _buildMenu = null;
        }
    }
    public void Draw() {
        var world = app.World!;

        BeginMode3D(_camera);

        foreach (var tile in world.Tiles) {
            tile.Draw();
        }
        foreach (var enemy in world.Enemies) {
            enemy.Draw();
        }

        if (_hoveredTile is { } hovered) {
            var solid = hovered.Content is SolidTile;
            var height = solid ? 1f : 0.1f;
            var centre = new Vector3(hovered.X + 0.5f, solid ? 0.5f : -0.05f, hovered.Y + 0.5f);
            DrawCubeWires(centre, 1.02f, height + 0.02f, 1.02f, Color.Yellow);

            if (!solid) {
                const Single thickness = 0.1f;
                var (offset, size) = _hoveredEdgeIndex switch {
                    EdgeSide.North => (new Vector3(0, 0, -0.5f), new Vector3(1, 0.1f, thickness)),
                    EdgeSide.South => (new Vector3(0, 0, 0.5f), new Vector3(1, 0.1f, thickness)),
                    EdgeSide.East => (new Vector3(0.5f, 0, 0), new Vector3(thickness, 0.1f, 1)),
                    _ => (new Vector3(-0.5f, 0, 0), new Vector3(thickness, 0.1f, 1)),
                };

                var edgeCentre = centre + offset + new Vector3(0, 0.06f, 0);
                DrawCube(edgeCentre, size.X, size.Y, size.Z, Color.Orange);
                DrawCubeWires(edgeCentre, size.X, size.Y, size.Z, Color.Red);
            }
        }

        EndMode3D();

        //BeginMode2D();
        //DrawBuildMenu
        //DrawScrapCounter
        _countDownCounter.Draw();
        _scrapCounter.Draw();
        _buildMenu?.Draw();
        //EndMode2D();
    }
    public void Activate() {
        var world = app.World!;
        var centre = world.Size / 2f;
        _camera = new Camera3D {
            Position = new Vector3(centre, 12, world.Size + 2),
            Target = new Vector3(centre, 0, centre),
            Up = Vector3.UnitY,
            FovY = 45,
            Projection = CameraProjection.Perspective,
        };
        _countDownCounter = new(3600);
        _scrapCounter = new(() => app.World.Scraps);
    }
    public void Deactivate() {

    }
}
