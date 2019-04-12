var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Demo;
        (function (Demo) {
            var Analytics = /** @class */ (function () {
                function Analytics() {
                }
                Analytics.reportPageView = function (screenName, appName) {
                    if (appName === void 0) { appName = "Uno.SkiaSharp"; }
                    if (Analytics.init(screenName, appName)) {
                        return "ok";
                    }
                    var gtag = window.gtag;
                    if (gtag) {
                        gtag("event", "screen_view", {
                            screen_name: screenName,
                            app_name: appName
                        });
                    }
                    else {
                        console.error("Google Analytics not present, can't report page view for " + screenName + ".");
                    }
                    return "ok";
                };
                Analytics.init = function (screenName, appName) {
                    if (Analytics.isLoaded) {
                        return false;
                    }
                    var script = "\n\t            window.dataLayer = window.dataLayer || [];\n\t            function gtag() { dataLayer.push(arguments); }\n\t            gtag('js', new Date());\n\t            gtag('config', 'UA-26688675-15');\n\n\t            gtag(\"event\", \"screen_view\", {screen_name: \"" + screenName + "\", app_name: \"" + appName + "\"});";
                    var script1 = document.createElement("script");
                    script1.type = "text/javascript";
                    script1.src = "https://www.googletagmanager.com/gtag/js?id=UA-26688675-15";
                    document.body.appendChild(script1);
                    var script2 = document.createElement("script");
                    script2.type = "text/javascript";
                    script2.innerText = script;
                    document.body.appendChild(script2);
                    Analytics.isLoaded = true;
                    return true;
                };
                Analytics.isLoaded = false;
                return Analytics;
            }());
            Demo.Analytics = Analytics;
        })(Demo = UI.Demo || (UI.Demo = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
//# sourceMappingURL=GoogleAnalytics.js.map