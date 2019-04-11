declare const MonoRuntime: Uno.UI.Interop.IMonoRuntime;
declare const MonoSupport: any;

module Uno.UI.Interop {
	export interface IMonoRuntime {
		mono_string(str: string): Interop.IMonoStringHandle;
		conv_string(strHandle: Interop.IMonoStringHandle): string;
	}
    export interface IMonoStringHandle {
    }
}