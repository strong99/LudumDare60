using System.Runtime.InteropServices.JavaScript;
using static Raylib_cs.Raylib;

namespace LudumDare60.Web;

public static partial class WebApp {
    private static Double _previousTime;
    private static App _app = new();

    public static async Task Main(string[] args) {
        _app.Init();

        _previousTime = GetTime();
    }

    [JSExport]
    public static void UpdateFrame() {
        var currentTime = GetTime();
        var deltaTime = currentTime - _previousTime;
        _previousTime = currentTime;

        _app.Tick(deltaTime);
    }
}