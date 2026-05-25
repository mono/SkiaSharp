// Bootstrap the .NET WASM runtime, then install the test-host facade
// on globalThis. Playwright tests call:
//   await window.skiaTestHost.ready;
//   await window.skiaTestHost.renderScene(rendererName, sceneName, w, h);
// → base64 RGBA8888/Premul pixels.

import { dotnet } from './_framework/dotnet.js';

const status = document.getElementById('status');

const { getAssemblyExports } = await dotnet
  .withDiagnosticTracing(false)
  .create();

// Don't call dotnet.run(): it runs Main and immediately tears the runtime
// down, which makes subsequent [JSExport] calls throw "runtime already
// exited". getAssemblyExports lazily boots the runtime AND keeps it alive
// servicing exports — exactly the lifecycle we want.
const exports = await getAssemblyExports('RenderHost.Wasm');

globalThis.skiaTestHost = {
  // Lets Playwright wait deterministically before the first render call
  // rather than racing the runtime boot.
  ready: true,

  renderScene: async (rendererName, sceneName, width, height) => {
    return await exports.SkiaSharp.Tests.RenderHost.Wasm.Program.RenderSceneAsync(
      rendererName, sceneName, width, height);
  },
};

status.dataset.ready = 'true';
status.textContent = 'Render host ready.';
