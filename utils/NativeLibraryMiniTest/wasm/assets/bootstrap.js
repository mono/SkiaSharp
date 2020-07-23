var App = {
    init() {
        try {
            BINDING.call_static_method ("[NativeLibraryMiniTest] NativeLibraryMiniTest.Program:Main", []);
        } catch (e) {
            console.error(e);
        }
    },
};
