using LudumDare60.Generators;
using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60.GameStates;

public sealed class PlayState(App app) : GameState {
    private Camera3D _camera;
    private SsaoPass _ssao = null!;
    private CountDownCounter _countDownCounter;
    private ScrapCounter _scrapCounter;
    private TowerCoreCounter _towerCoreCounter;
    private WaveSpawner _waveSpawner = null!;
    private BuildMenu? _buildMenu;
    private Tile? _hoveredTile = null;
    private Edge? _hoveredEdge = null;
    private EdgeSide _hoveredEdgeIndex = EdgeSide.North;
    private TowerCore _towerCore;
    private Boolean _showSteamDebug = true;
    private MusicGenerator<DramaticPlayMusicGeneratorStrategy>? _music;
    public void Progress(Double deltaTime) {
        _music?.Progress();

        var world = app.World!;

        if (IsKeyPressed(KeyboardKey.F3)) {
            _showSteamDebug = !_showSteamDebug;
        }

        HandleScroll();
        if (_buildMenu is null) {
            HandleTileHover();
        }
        HandleBuildMenu();

        SteamStore.NextFrame();
        foreach (var decoration in world.Decorations) {
            decoration.Progress(deltaTime);
        }
        foreach (var tile in world.Tiles) {
            tile.Progress(deltaTime);
        }
        _waveSpawner.Progress(deltaTime);
        foreach (var enemy in world.Enemies.ToList()) {
            enemy.Progress(deltaTime);
        }
        _countDownCounter.Progress(deltaTime);
        _scrapCounter.Progress(deltaTime);
        _buildMenu?.Progress(deltaTime);
        _towerCoreCounter.Progress(deltaTime);

        if (_countDownCounter.TimeUp) {
            app.SetNextGameState(new GameOverState(app, true));
        }
        else if (_towerCore.HitPoints <= 0) {
            app.SetNextGameState(new GameOverState(app, false));
        }
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

        _ssao.Begin();
        BeginMode3D(_camera);

        _buildMenu?.ApplyPreviewState();

        foreach (var decoration in world.Decorations) {
            decoration.Draw();
        }
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

        _buildMenu?.DrawPreview();

        EndMode3D();
        _ssao.End();

        if (_showSteamDebug) {
            DrawSteamDebug(world);
        }

        //BeginMode2D();
        //DrawBuildMenu
        //DrawScrapCounter
        _countDownCounter.Draw();
        _scrapCounter.Draw();
        _buildMenu?.Draw();
        _towerCoreCounter.Draw();
        //EndMode2D();
    }

    private void DrawSteamDebug(World world) {
        var seen = new HashSet<SteamStore>();
        foreach (var tile in world.Tiles) {
            if (tile.Content is not OpenTile open) {
                continue;
            }

            var centre = new Vector3(tile.X + 0.5f, 0f, tile.Y + 0.5f);
            if (open.Content is SteamConnected machine && seen.Add(machine.Store)) {
                var s = machine.Store;
                DrawDebugLabel(centre + new Vector3(0, 0.6f, 0), $"{s.Pressure:0.00} ({s.Steam:0.0}/{s.Volume:0}) d{s.DamagePoints}", 14);
            }

            for (var i = 0; i < open.Edges.Length; i++) {
                if (open.Edges[i] is not SteamConnected pipe || !seen.Add(pipe.Store)) {
                    continue;
                }

                var offset = (EdgeSide)i switch {
                    EdgeSide.North => new Vector3(0, 0, -0.5f),
                    EdgeSide.South => new Vector3(0, 0, 0.5f),
                    EdgeSide.East => new Vector3(0.5f, 0, 0),
                    _ => new Vector3(-0.5f, 0, 0),
                };
                var label = pipe.Store.DamagePoints > 0 ? $"{pipe.Store.Pressure:0.00} d{pipe.Store.DamagePoints}" : $"{pipe.Store.Pressure:0.00}";
                DrawDebugLabel(centre + offset + new Vector3(0, 0.3f, 0), label, 10);
            }
        }
    }

    private void DrawDebugLabel(Vector3 position, String text, Int32 fontSize) {
        var screen = GetWorldToScreen(position, _camera);
        DrawText(text, (Int32)screen.X - MeasureText(text, fontSize) / 2, (Int32)screen.Y - fontSize / 2, fontSize, Color.Yellow);
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
        _ssao = new SsaoPass(_camera.FovY);
        _waveSpawner = new WaveSpawner(world);
        _countDownCounter = new(3600);
        _scrapCounter = new(() => app.World.Scraps);

        _towerCore = world.Tiles
            .Cast<Tile>()
            .Select(t => (t.Content as OpenTile)?.Content as TowerCore)
            .FirstOrDefault(c => c is not null);
        _towerCoreCounter = new(() => (Int32)MathF.Ceiling(_towerCore?.HitPoints ?? 0));

        _music = new(new DramaticPlayMusicGeneratorStrategy());
        _music.Play();
    }
    public void Deactivate() {
        _ssao.Dispose();
        _music?.Dispose();
        _music = null;
    }
}
