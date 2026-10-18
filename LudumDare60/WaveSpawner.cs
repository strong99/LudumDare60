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
    private static readonly EnemyKind _goblinEngineerr = new("Engineer", 5, 1.25f, 1f, 0.8f);
    private static readonly EnemyKind _goblinFigher = new("Fighter", 20, 1f, 0.2f, 0.2f);
    private readonly Random _random = new();

    private Double _waveTimer = _firstWaveDelay;
    private Double _enemyTimer;
    private Int32 _remainingInWave;
    private Tile? _entrance;

    public Int32 WaveNumber { get; private set; }

    public void Progress(Double deltaTime) {
        if (_remainingInWave == 0) {
            _waveTimer -= deltaTime;
            if (_waveTimer > 0) {
                return;
            }

            WaveNumber++;
            _remainingInWave = _baseWaveSize + (WaveNumber - 1) * _waveGrowth;
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
            if (!TrySpawnAt(_entrance)) {
                _remainingInWave = 0;
                return;
            }

            _remainingInWave--;
            _enemyTimer += _enemySpawnInterval;
        }
    }

    private Boolean TrySpawnAt(Tile entrance) {
        var enemy = new Enemy {
            Kind = _goblinExplorer,
            HitPoints = _goblinExplorer.MaxHitPoints,
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
