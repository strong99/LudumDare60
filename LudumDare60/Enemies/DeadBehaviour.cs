namespace LudumDare60.Enemies;

public sealed class DeadBehaviour : EnemyBehaviour {
    private readonly World _world;
    private readonly Enemy _enemy;
    private Double _despawnTimer = 5;

    public DeadBehaviour(World world, Enemy enemy) {
        _world = world;
        _enemy = enemy;
        _world.RaiseEnemyDied(enemy);
    }

    public void Progress(Double deltaTime) {
        _despawnTimer -= deltaTime;
        if (_despawnTimer < 0) {
            _world.Enemies.Remove(_enemy);
        }
    }
}
