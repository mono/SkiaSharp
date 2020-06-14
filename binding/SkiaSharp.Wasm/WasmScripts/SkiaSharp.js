"use strict";
/// <reference path="../../../externals/typescript/types/emscripten/index.d.ts" />
var SkiaSharp;
(function (SkiaSharp) {
    class SkiaApi {
        static createJsMethod(monoMethod) {
            return (...args) => {
                return monoMethod.apply(undefined, args);
            };
        }
        static bindMembers(type, members) {
            let ptrs = [];
            for (let member in members) {
                let monoMethod = Module.mono_bind_static_method(`${type}:${member}`);
                let jsMethod = SkiaApi.createJsMethod(monoMethod);
                let wasmMethod = Module.addFunction(jsMethod, members[member]);
                ptrs.push(wasmMethod);
            }
            return ptrs;
        }
    }
    SkiaSharp.SkiaApi = SkiaApi;
})(SkiaSharp || (SkiaSharp = {}));
