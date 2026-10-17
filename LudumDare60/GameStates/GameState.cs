public interface GameState {
    void Progress(Double deltaTime);
    void Draw();
    void Activate();
    void Deactivate();
}
