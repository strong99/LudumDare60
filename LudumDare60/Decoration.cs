using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace LudumDare60;

public interface Decoration {
    void Draw();
    void Progress(Double deltaTime);
}
public sealed class StaticDecoration(ModelHandle model, Vector3 position, Single rotation) : Decoration {
    private readonly ModelHandle _model = model;
    private readonly Vector3 _position = position;
    private readonly Single _rotation = rotation;
    public void Draw() {
        DrawModelEx(_model, _position, Vector3.UnitY, _rotation,Vector3.One, Color.White);
    }

    public void Progress(Double deltaTime) {
        
    }
}

public sealed class ElevatorDecoration(ModelHandle model, Vector3 position, Single rotation) : Decoration {
    private readonly ModelHandle _model = model;
    private readonly Vector3 _position = position;
    private readonly Single _rotation = rotation;
    private Double _time = 0;
    public void Draw() {
        var offset = new Vector3(0, (MathF.Sin((Single)(_time / 10f)) - 1f) * 10f, 0);
        DrawModelEx(_model, _position + offset, Vector3.UnitY, _rotation,Vector3.One, Color.White);
    }

    public void Progress(Double deltaTime) {
        _time += deltaTime;
    }
}
