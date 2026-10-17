using System.Numerics;

public sealed class Memory {
    private Dictionary<Vector2, MemoryTileType> _memory = [];
    public MemoryTileType IsKnown(Vector2 position) {
        if (_memory.TryGetValue(position, out var type)) {
            return type;
        }
        return MemoryTileType.None;
    }
    public void Store(Vector2 position, MemoryTileType type) {
        if (type == MemoryTileType.None) {
            _memory.Remove(position);
        }
        else {
            _memory[position] = type;
        }
    }
    public void SyncWith(Memory memory) {
        if (memory == this) {
            throw new Exception("Should not compare itself, should never happen");
        }

        foreach (var (position, type) in memory._memory) {
            _memory[position] = type;
        }

        memory._memory = new Dictionary<Vector2, MemoryTileType>(_memory);
    }
}
