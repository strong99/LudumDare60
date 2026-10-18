import { dotnet } from './_framework/dotnet.js'

globalThis.getBaseAddress = function () { return document.baseURI; };

globalThis.getViewportWidth = function () { return Math.max(1, Math.floor(window.innerWidth)); };
globalThis.getViewportHeight = function () { return Math.max(1, Math.floor(window.innerHeight)); };

globalThis.toggleBrowserFullscreen = function () {
    if (document.fullscreenElement) {
        document.exitFullscreen();
    } else {
        document.documentElement.requestFullscreen().catch(e => console.warn('Fullscreen refused:', e));
    }
};

const { getAssemblyExports, getConfig, runMain } = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationCulture(navigator.language)
    .create();

const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);

dotnet.instance.Module['canvas'] = document.getElementById('canvas');

let running = true;
function mainLoop() {
    if (!running) return;
    try {
        exports.LudumDare60.Web.WebApp.UpdateFrame();
    } catch (e) {
        console.error('UpdateFrame threw:', e, e && e.stack);
    }
    window.requestAnimationFrame(mainLoop);
}

window.addEventListener('beforeunload', () => { running = false; });

await runMain();

window.requestAnimationFrame(mainLoop);
