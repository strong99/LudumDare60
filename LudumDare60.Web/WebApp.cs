using System.Runtime.InteropServices.JavaScript;
using static Raylib_cs.Raylib;

namespace LudumDare60.Web;

public static partial class WebApp {
    private static Double _previousTime;
    private static App _app = new();

    public static async Task Main(string[] args) {
        ExtractAssets();

        App.FullscreenOverride = ToggleBrowserFullscreen;
        _app.Init();

        _previousTime = GetTime();
    }

    /// <summary>
    /// Workaround:Load the files into the virtual file system for Raylib as 
    /// the WasmFilesToIncludeInFileSystem won't work with this dotnet SDK 
    /// it seems
    /// </summary>
    private static void ExtractAssets() {
        var assembly = typeof(WebApp).Assembly;
        foreach (var resourceName in assembly.GetManifestResourceNames()) {
            var path = resourceName.Replace('\\', '/');
            if (!path.StartsWith("Assets/", StringComparison.Ordinal)) {
                continue;
            }

            var directory = Path.GetDirectoryName(path);
            if (!String.IsNullOrEmpty(directory)) {
                Directory.CreateDirectory(directory);
            }

            using var source = assembly.GetManifestResourceStream(resourceName)!;
            using var destination = File.Create(path);
            source.CopyTo(destination);
        }
    }

    [JSImport("globalThis.toggleBrowserFullscreen")]
    private static partial void ToggleBrowserFullscreen();

    [JSImport("globalThis.getViewportWidth")]
    private static partial Int32 GetViewportWidth();

    [JSImport("globalThis.getViewportHeight")]
    private static partial Int32 GetViewportHeight();

    private static void SyncViewportSize() {
        var width = GetViewportWidth();
        var height = GetViewportHeight();
        if (width != GetScreenWidth() || height != GetScreenHeight()) {
            SetWindowSize(width, height);
        }
    }

    [JSExport]
    public static void UpdateFrame() {
        SyncViewportSize();

        var currentTime = GetTime();
        var deltaTime = currentTime - _previousTime;
        _previousTime = currentTime;

        _app.Tick(deltaTime);
    }
}
