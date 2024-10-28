import { dotnet } from './_framework/dotnet.js'

const { setModuleImports, getAssemblyExports, getConfig } = await dotnet
    .create();

setModuleImports('main.js', {
    renderer: {
        render: (width, height, buffer) => {
            const surface = document.getElementById("surface");
            const ctx = surface.getContext('2d');
            const clamped = new Uint8ClampedArray(buffer.slice());
            const image = new ImageData(clamped, width, height);
            ctx.putImageData(image, 0, 0);
            surface.style = "";
        }
    }
});

const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);

await dotnet.run();
