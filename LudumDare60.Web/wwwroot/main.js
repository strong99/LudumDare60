import { dotnet } from './_framework/dotnet.js'

globalThis.getBaseAddress = function () { return location.origin + '/'; };

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
