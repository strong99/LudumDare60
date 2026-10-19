# BoilerKeep

Defend the Tower Core from Goblin invaders. Boilerkeep is a small Ludum Dare 60 game built in 48 hours. In this game you defend the tower core from enemy goblins that try and infiltrate. To defend you have an inventory of traps available, sadly these require pipes, and pipes can break.

BoilerKeep was made in 48 hours for [Ludum Dare 60](https://ldjam.com/events/ludum-dare/60/boilerkeep) (see the [competition page](https://ldjam.com/events/ludum-dare/)).

## The game

Goblins invade the tower and explore ever nook and cranny. When loot is found they carry it home. If Goblins get home, they tell their friends the exact route through the tower. Hold out until the core finishes activating an android into the world.

- **Build traps** (crushers, spikes, saws, steam vents) on open tiles.
- **Power machinery with steam** from boilers and relay boilers, connected by pipes along tile edges.
- **Walls block paths** Goblins avoid walls, but will tear them down if there is no other way to the core.
- **Repair and rebuild machinery** Boilers, pipes and traps wear down over time, and everything costs scrap. You earn scrap every wave and from kills.
- **Watch the enemy types:** scrapers take damage from everything, engineers resist saws and steam, and fighters have more hit points.

See [Docs/index.md](Docs/index.md) for the full design: steam and pressure mechanics, enemy behaviour and memory.

### Controls

| Action | Input |
| --- | --- |
| Open build menu | Left click a tile or edge |
| Close build menu / pause | `Esc` |
| Scroll camera | `WASD`, arrow keys, or mouse at the screen edge |
| Rotate camera | `Q` / `E` |
| Zoom | Mouse wheel, `Space` (in) / `Left Ctrl` (out) |

## Play

Pre-built desktop (Windows, Linux, macOS) and web (WebAssembly) versions are attached to each [GitHub release](../../releases).

## Build and run from source

Requirements:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2026 (optional)

Desktop:

```
dotnet run --project LudumDare60
```

Web (WebAssembly):

```
dotnet workload install wasm-tools
dotnet run --project LudumDare60.Web
```

## Project layout

| Path | Contents |
| --- | --- |
| `LudumDare60/` | Game code: enemies, tiles and traps, game states, UI, generators |
| `LudumDare60.Web/` | Blazor/WASM host for the browser build |
| `Docs/` | Game design notes |
| `Assets/` | Source art (Affinity, Krita); runtime assets live in `LudumDare60/Assets` |

Releases are built by [`.github/workflows/release.yml`](.github/workflows/release.yml) when a `v*` tag is pushed. It publishes the desktop builds, the web build and GitHub Pages.

## License

The source is readable, but you may not copy, edit or reuse it without consent. You are welcome to clone the repository and build it to play on your own machine.

## Credits

Tools: Affinity, Krita, Blender, Audacity, Visual Studio, Git.

Built with .NET 10, [Raylib-cs](https://github.com/ChrisDill/Raylib-cs) and Blazor WebAssembly for the web build.
