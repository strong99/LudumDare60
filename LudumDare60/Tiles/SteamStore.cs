namespace LudumDare60.Tiles;

/// <summary>
/// Steam buffer of a single node (boiler, relay boiler, pipe, trap). Every node steps itself: it produces,
/// leaks, and equalises pressure with its neighbours. Pressure is steam relative to volume, so a larger
/// volume loses less pressure for the same amount of steam taken out.
/// </summary>
public sealed class SteamStore(Single volume, Single production = 0f, Int32 maxDamagePoints = 0) {
    private const Single _flowPerSecond = 25f;
    private const Single _leakPerPoint = 0.1f;
    private const Double _damageChancePerSecond = 0.002;

    private static Int32 _nextId;
    private static Int64 _frame;

    private readonly Int32 _id = Interlocked.Increment(ref _nextId);
    private Int64 _lastStepFrame = -1;

    public Single Volume { get; } = volume;
    public Single Production { get; } = production;
    public Int32 MaxDamagePoints { get; } = maxDamagePoints;
    public Single Steam { get; set; }
    public Int32 DamagePoints { get; set; }
    public Single Pressure => Volume > 0 ? Steam / Volume : 0f;
    public List<SteamStore> Neighbours { get; } = [];

    /// <summary>Call once per frame so nodes reachable through more than one tile only step once.</summary>
    public static void NextFrame() => _frame++;

    public Boolean TryConsume(Single amount) {
        if (Steam < amount) {
            return false;
        }

        Steam -= amount;
        return true;
    }

    public void Repair() => DamagePoints = 0;

    public void Step(Double deltaTime) {
        if (_lastStepFrame == _frame) {
            return;
        }

        _lastStepFrame = _frame;
        var dt = (Single)deltaTime;

        if (MaxDamagePoints > 0 && DamagePoints < MaxDamagePoints && Random.Shared.NextDouble() < _damageChancePerSecond * deltaTime) {
            DamagePoints++;
        }

        Steam = Math.Min(Volume, Steam + Production * dt);
        Steam *= 1f - Math.Min(1f, _leakPerPoint * DamagePoints * dt);

        // Each pair is exchanged once, by the node with the lowest id. Exchanges are applied immediately and
        // conserve steam. The rate is capped by the number of connections so pressures never cross.
        foreach (var other in Neighbours) {
            if (_id > other._id) {
                continue;
            }

            var degree = Math.Max(Neighbours.Count, other.Neighbours.Count);
            var rate = Math.Min(_flowPerSecond * dt, 0.5f / degree) * Math.Min(Volume, other.Volume);
            var flow = rate * (Pressure - other.Pressure);
            Steam = Math.Clamp(Steam - flow, 0f, Volume);
            other.Steam = Math.Clamp(other.Steam + flow, 0f, other.Volume);
        }
    }
}
