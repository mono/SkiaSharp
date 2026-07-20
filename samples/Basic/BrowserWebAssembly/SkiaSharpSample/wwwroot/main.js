import { dotnet } from './_framework/dotnet.js'

const { getAssemblyExports, getConfig } = await dotnet.create();

const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);
const api = exports.SampleRenderer;

// Start the runtime (Main loops forever to keep it alive for JSExport)
dotnet.run().catch(() => {});

// Wait briefly for the runtime to stabilize
await new Promise(r => setTimeout(r, 300));

// Render the image and display it
try {
    const base64 = api.RenderPng(800, 600);
    const img = document.getElementById('output');
    img.src = 'data:image/png;base64,' + base64;
    img.classList.remove('d-none');
    document.getElementById('loading')?.remove();
} catch (e) {
    console.error('Render failed:', e);
    const loading = document.getElementById('loading');
    if (loading) loading.innerHTML = '<div class="text-danger">Failed to render: ' + e.message + '</div>';
}
