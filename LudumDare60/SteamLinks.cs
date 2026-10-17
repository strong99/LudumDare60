using LudumDare60.Tiles;

namespace LudumDare60;

/// <summary>
/// Works out which steam nodes are neighbours. Pipes connect to pipes sharing a corner of the tile grid,
/// machinery connects to the pipes on its tile edges and to machinery on neighbouring tiles. Run when the
/// layout changes; the nodes distribute the steam themselves.
/// </summary>
public static class SteamLinks {
    public static void Connect(World world) {
        var all = new HashSet<SteamStore>();
        var corners = new Dictionary<(Int32 X, Int32 Y), List<SteamStore>>();
        var groups = new List<List<SteamStore>>();

        foreach (var tile in world.Tiles) {
            if (tile.Content is not OpenTile open) {
                continue;
            }

            var group = new List<SteamStore>(5);
            for (var i = 0; i < open.Edges.Length; i++) {
                if (open.Edges[i] is not SteamConnected pipe) {
                    continue;
                }

                group.Add(pipe.Store);
                var (a, b) = (EdgeSide)i switch {
                    EdgeSide.North => ((tile.X, tile.Y), (tile.X + 1, tile.Y)),
                    EdgeSide.South => ((tile.X, tile.Y + 1), (tile.X + 1, tile.Y + 1)),
                    EdgeSide.East => ((tile.X + 1, tile.Y), (tile.X + 1, tile.Y + 1)),
                    _ => ((tile.X, tile.Y), (tile.X, tile.Y + 1)),
                };
                AddToCorner(corners, a, pipe.Store);
                AddToCorner(corners, b, pipe.Store);
            }

            if (MachineAt(open) is { } machine) {
                group.Add(machine.Store);
                groups.Add(group);

                // Machines on neighbouring tiles share steam directly, as if connected.
                foreach (var (nx, ny) in new[] { (tile.X + 1, tile.Y), (tile.X, tile.Y + 1) }) {
                    if (nx < world.Size && ny < world.Size
                        && world.Tiles[nx, ny].Content is OpenTile next
                        && MachineAt(next) is { } neighbour
                        && !ReferenceEquals(neighbour, machine)) {
                        groups.Add([machine.Store, neighbour.Store]);
                    }
                }
            }

            all.UnionWith(group);
        }

        foreach (var store in all) {
            store.Neighbours.Clear();
        }

        foreach (var group in groups.Concat(corners.Values)) {
            foreach (var store in group) {
                foreach (var other in group) {
                    if (!ReferenceEquals(store, other) && !store.Neighbours.Contains(other)) {
                        store.Neighbours.Add(other);
                    }
                }
            }
        }
    }

    private static SteamConnected? MachineAt(OpenTile tile) =>
        (tile.Content is ReferenceContent reference ? reference.Owner : tile.Content) as SteamConnected;

    private static void AddToCorner(Dictionary<(Int32, Int32), List<SteamStore>> corners, (Int32, Int32) key, SteamStore store) {
        if (!corners.TryGetValue(key, out var list)) {
            corners[key] = list = [];
        }

        if (!list.Contains(store)) {
            list.Add(store);
        }
    }
}
