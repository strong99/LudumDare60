using LudumDare60.Enemies;
using System.Drawing;
using System.Numerics;

namespace LudumDare60;

public sealed class WaveSpawner(World world) {
    private const Double _firstWaveDelay = 10;
    private const Double _waveInterval = 30;
    private const Double _enemySpawnInterval = 1;
    private const Int32 _baseWaveSize = 3;
    private const Int32 _waveGrowth = 2;

    private static readonly EnemyKind _goblinExplorer = new("Explorer", 10, 1.5f, 0f, 0f);
    private static readonly EnemyKind _goblinEngineer = new("Engineer", 5, 1.25f, 1f, 0.8f);
    private static readonly EnemyKind _goblinFigher = new("Fighter", 20, 1f, 0.2f, 0.2f);
    private readonly Random _random = new();

    private Double _waveTimer = _firstWaveDelay;
    private Double _enemyTimer;
    private Int32 _remainingInWave;
    private List<EnemyKind> _queue = [];
    private Tile? _entrance;

    public Int32 WaveNumber { get; private set; }

    public void Progress(Double deltaTime) {
        if (_remainingInWave == 0) {
            _waveTimer -= deltaTime;
            if (_waveTimer > 0) {
                return;
            }

            WaveNumber++;
            _queue = BuildWave(WaveNumber);
            _remainingInWave = _queue.Count;
            _waveTimer += _waveInterval;
            _enemyTimer = 0;

            var entrances = world.Tiles.Cast<Tile>()
                .Where(t => t.Content is OpenTile { Content: TowerEntrance })
                .ToList();

            if (entrances.Count == 0) {
                return;
            }

            _entrance = entrances[_random.Next(entrances.Count)];
        }

        _enemyTimer -= deltaTime;

        while (_remainingInWave > 0 && _enemyTimer <= 0) {
            var kind = _queue[^_remainingInWave];
            if (!TrySpawnAt(_entrance, kind)) {
                _remainingInWave = 0;
                return;
            }

            _remainingInWave--;
            _enemyTimer += _enemySpawnInterval;
        }
    }

    private List<EnemyKind> BuildWave(Int32 wave) {
        var size = _baseWaveSize + (wave - 1) * _waveGrowth;

        var engineers = wave <= 2 ? 0 : wave <= 5 ? 1 : 2;

        var fighters = wave <= 3 ? 0 : Math.Min(5, wave - 2);

        if (wave > 5) {
            var explorerShare = Math.Max(0.25, 1.0 - (wave - 5) * 0.1);
            var explorers = (Int32)Math.Round(size * explorerShare);
            fighters = Math.Max(fighters, size - engineers - explorers);
        }

        engineers = Math.Min(engineers, size);
        fighters = Math.Min(fighters, size - engineers);

        var kinds = new List<EnemyKind>(size);
        kinds.AddRange(Enumerable.Repeat(_goblinEngineer, engineers));
        kinds.AddRange(Enumerable.Repeat(_goblinFigher, fighters));
        kinds.AddRange(Enumerable.Repeat(_goblinExplorer, size - engineers - fighters));

        for (var i = kinds.Count - 1; i > 0; i--) {
            var j = _random.Next(i + 1);
            (kinds[i], kinds[j]) = (kinds[j], kinds[i]);
        }

        return kinds;
    }

    private Boolean TrySpawnAt(Tile entrance, EnemyKind kind) {
        var enemy = new Enemy {
            Kind = kind,
            HitPoints = kind.MaxHitPoints,
            Memory = world.EnemyKnowledge.Clone(),
            Weapon = Random.Shared.Next(2) == 0 ? Weapon.Sword : Weapon.Mace,
            Position = new Vector2(entrance.X + 0.5f, entrance.Y + 0.5f),
        };
        enemy.Behaviour = new ExploreBehaviour(world, enemy);
        world.Enemies.Add(enemy);
        return true;
    }

    public void Draw() { }
}
