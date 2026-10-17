namespace LudumDare60.Enemies;

public sealed class DeadBehaviour(World world, Enemy enemy) : EnemyBehaviour {
    private Double _despawnTimer = 5;
    public void Progress(Double deltaTime) {
        _despawnTimer -= deltaTime;
        if (_despawnTimer < 0) {
            world.Enemies.Remove(enemy);
        }
    }
}
