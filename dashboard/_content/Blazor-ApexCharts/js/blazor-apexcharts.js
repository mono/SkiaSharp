import ApexCharts from './apexcharts.esm.js?ver=4.0.0'

// export function for Blazor to point to the window.blazor_apexchart. To be compatible with the most JS Interop calls the window will be return.
export function get_apexcharts() {
    window.ApexCharts = ApexCharts
    return window;
}

window.blazor_apexchart = {

    getDotNetObjectReference(index, w) {
        var chartId = null;

        if (w !== undefined && w.config !== undefined) {
            chartId = w.config.chart.id;
        }

        if (w !== undefined && w.w !== undefined && w.w.config !== undefined) {
            chartId = w.w.config.chart.id;
        }

        if (index !== undefined && index.w !== undefined && index.w.config !== undefined) {
            chartId = index.w.config.chart.id;
        }

        if (index !== undefined && index.config !== undefined) {
            chartId = index.config.chart.id;
        }

        if (chartId != null) {
            return this.dotNetRefs.get(chartId);
        }
        return null;
    },

    getXAxisLabel(value, index, w) {

        if (window.wasmBinaryFile === undefined && window.WebAssembly === undefined) {
            console.warn("XAxis labels is only supported in Blazor WASM");
            return value;
        }

        var dotNetRef = this.getDotNetObjectReference(index, w);
        if (dotNetRef != null) {
            return dotNetRef.invokeMethod('JSGetFormattedXAxisValue', value);
        }

        return value;
    },

    getYAxisLabel(value, index, w) {

        if (window.wasmBinaryFile === undefined && window.WebAssembly === undefined) {
            console.warn("YAxis labels is only supported in Blazor WASM");
            return value;
        }

        var dotNetRef = this.getDotNetObjectReference(index, w);
        if (dotNetRef != null) {
            return dotNetRef.invokeMethod('JSGetFormattedYAxisValue', value);
        }

        return value;

    },

    findChart(id) {
        if (Apex._chartInstances === undefined) {
            return undefined;
        }
        return ApexCharts.getChartByID(id)

    },

    destroyChart(id) {
        var chart = this.findChart(id);
        if (chart !== undefined) {
            chart.destroy();
        }

        this.dotNetRefs.delete(id);

    },

    LogMethodCall(chart, method, data) {
        if (chart !== undefined) {
            if (chart.opts.debug === true) {
                console.log('------');
                console.log('Method:' + method);
                console.log("Chart Id: " + chart.opts.chart.id)
                if (data !== undefined) {
                    console.log(data);
                }
                console.log('------');
            }
        }
    },

    setGlobalOptions(options) {
        var opt = this.parseOptions(options);
      
        if (opt.debug === true) {
            console.log('------');
            console.log('Method: setGlobalOptions');
            console.log(opt);
            console.log('------');
        }

        opt._chartInstances = Apex._chartInstances;
        Apex = opt;
    },

    updateOptions(id, options, redrawPaths, animate, updateSyncedCharts, zoom) {
        var options = this.parseOptions(options);
        var chart = this.findChart(id);
        if (chart !== undefined) {
            this.LogMethodCall(chart, "updateOptions", options);
            chart.updateOptions(options, redrawPaths, animate, updateSyncedCharts);

            if (zoom !== null) {
                chart.zoomX(zoom.start, zoom.end);
            }

        }
    },

    appendData(id, data) {
        var newData = JSON.parse(data);
        var chart = this.findChart(id);
        if (chart !== undefined) {
            this.LogMethodCall(chart, "appendDate", newData);
            return chart.appendData(newData);
        }
    },

    toggleDataPointSelection(id, seriesIndex, dataPointIndex) {
        var chart = this.findChart(id);
        if (chart !== undefined) {
            this.LogMethodCall(chart, "toggleDataPointSelection [" + seriesIndex + '] [' + dataPointIndex + ']');
            var pointIndex;
            if (dataPointIndex !== null) {
                pointIndex = dataPointIndex;
            }

            return chart.toggleDataPointSelection(seriesIndex, pointIndex);
        }
    },

    zoomX(id, start, end) {
        var chart = this.findChart(id);
        if (chart !== undefined) {
            this.LogMethodCall(chart, 'zoomX ' + start + ", " + end);
            return chart.zoomX(start, end);
        }
    },

    resetSeries(id, shouldUpdateChart, shouldResetZoom) {
        var chart = this.findChart(id);
        if (chart !== undefined) {
            this.LogMethodCall(chart, 'resetSeries ' + shouldUpdateChart + ", " + shouldResetZoom);
            return chart.resetSeries(shouldUpdateChart, shouldResetZoom);
        }
    },

    setLocale(id, name) {
        var chart = this.findChart(id);
        if (chart !== undefined) {
            this.LogMethodCall(chart, 'setLocale ' + name);
            chart.setLocale(name);
            chart.update();
        }
    },

    dataUri(id, options) {
        var opt = JSON.parse(options);
        var chart = this.findChart(id);
        if (chart !== undefined) {
            this.LogMethodCall(chart, 'dataUri', options);
            return chart.dataURI(opt);
        }

        return '';
    },

    appendSeries(id, series, animate, overwriteInitialSeries) {
        var data = JSON.parse(series);
        var chart = this.findChart(id);
        if (chart !== undefined) {
            this.LogMethodCall(chart, 'appendSeries', series);
            chart.appendSeries(data, animate, overwriteInitialSeries);
        }
    },

    updateSeries(id, series, animate) {
        var data = JSON.parse(series);
        var chart = this.findChart(id);
        if (chart !== undefined) {
            this.LogMethodCall(chart, 'updateSeries', series);
            chart.updateSeries(data, animate);
        }
    },

    addPointAnnotation(id, annotation, pushToMemory) {
        var data = JSON.parse(annotation);
        var chart = this.findChart(id);
        if (chart !== undefined) {
            this.LogMethodCall(chart, 'addPointAnnotation', annotation);
            chart.addPointAnnotation(data, pushToMemory);
        }
    },

    addXaxisAnnotation(id, annotation, pushToMemory) {
        var data = JSON.parse(annotation);
        var chart = this.findChart(id);
        if (chart !== undefined) {
            this.LogMethodCall(chart, 'addXaxisAnnotation', annotation);
            chart.addXaxisAnnotation(data, pushToMemory);
        }
    },

    addYaxisAnnotation(id, annotation, pushToMemory) {
        var data = JSON.parse(annotation);
        var chart = this.findChart(id);
        if (chart !== undefined) {
            this.LogMethodCall(chart, 'addYaxisAnnotation', annotation);
            chart.addYaxisAnnotation(data, pushToMemory);
        }
    },

    clearAnnotations(id) {
        var chart = this.findChart(id);
        if (chart !== undefined) {
            this.LogMethodCall(chart, 'clearAnnotations');
            chart.clearAnnotations();
        }
    },

    removeAnnotation(chartid, id) {
        var chart = this.findChart(chartid);
        if (chart !== undefined) {
            this.LogMethodCall(chart, 'removeAnnotation', id);
            chart.removeAnnotation(id);
        }
    },

    toggleSeries(id, seriesName) {
        var chart = this.findChart(id);
        if (chart !== undefined) {
            this.LogMethodCall(chart, 'toggleSeries', seriesName);
            chart.toggleSeries(seriesName)
        }
    },

    showSeries(id, seriesName) {
        var chart = this.findChart(id);
        if (chart !== undefined) {
            this.LogMethodCall(chart, 'showSeries', seriesName);
            chart.showSeries(seriesName)
        }
    },

    hideSeries(id, seriesName) {
        var chart = this.findChart(id);
        if (chart !== undefined) {
            this.LogMethodCall(chart, 'hideSeries', seriesName);
            chart.hideSeries(seriesName)
        }
    },

    highlightSeries(id, seriesName) {
        var chart = this.findChart(id);
        if (chart !== undefined) {
            this.LogMethodCall(chart, 'highlightSeries', seriesName);
            chart.highlightSeries(seriesName)
        }
    },

    dotNetRefs: new Map(),

    renderChart(dotNetObject, container, options, events) {
        var options = this.parseOptions(options);

        if (options.debug == true) {
            console.log(options);
        }

        options.chart.events = {};

        if (options.tooltip != undefined && options.tooltip.customTooltip == true) {
            options.tooltip.custom = function ({ series, seriesIndex, dataPointIndex, w }) {
                var sourceId = 'apex-tooltip-' + w.globals.chartID;
                var source = document.getElementById(sourceId);
                if (source) {
                    return source.innerHTML;
                }
                return '...'
            };
        }

        if (events.hasDataPointLeave === true) {
            options.chart.events.dataPointMouseLeave = function (event, chartContext, config) {
                var selection = {
                    dataPointIndex: config.dataPointIndex,
                    seriesIndex: config.seriesIndex
                };

                dotNetObject.invokeMethodAsync('JSDataPointLeave', selection);
            }
        };

        if (events.hasDataPointEnter === true) {
            options.chart.events.dataPointMouseEnter = function (event, chartContext, config) {
                var selection = {
                    dataPointIndex: config.dataPointIndex,
                    seriesIndex: config.seriesIndex
                };

                dotNetObject.invokeMethodAsync('JSDataPointEnter', selection);
            }
        };

        if (events.hasDataPointSelection === true) {
            options.chart.events.dataPointSelection = function (event, chartContext, config) {
                var selection = {
                    dataPointIndex: config.dataPointIndex,
                    seriesIndex: config.seriesIndex,
                    selectedDataPoints: config.selectedDataPoints
                };

                dotNetObject.invokeMethodAsync('JSDataPointSelected', selection);
            }
        };

        if (events.hasMarkerClick === true) {
            options.chart.events.markerClick = function (event, chartContext, config) {
                var selection = {
                    dataPointIndex: config.dataPointIndex,
                    seriesIndex: config.seriesIndex,
                    selectedDataPoints: config.selectedDataPoints
                };

                dotNetObject.invokeMethodAsync('JSMarkerClick', selection);
            }
        };

        if (events.hasXAxisLabelClick === true) {
            options.chart.events.xAxisLabelClick = function (event, chartContext, config) {
                var data = {
                    labelIndex: config.labelIndex,
                    caption: event.target.innerHTML
                };

                dotNetObject.invokeMethodAsync('JSXAxisLabelClick', data);
            }
        };

        if (events.hasLegendClick === true) {
            options.chart.events.legendClick = function (chartContext, seriesIndex, config) {
                var legendClick = {
                    seriesIndex: seriesIndex,
                    collapsed: config.globals.collapsedSeriesIndices.indexOf(seriesIndex) !== -1
                };

                dotNetObject.invokeMethodAsync('JSLegendClicked', legendClick);
            }
        };

        if (events.hasSelection === true) {
            options.chart.events.selection = function (chartContext, config) {
                dotNetObject.invokeMethodAsync('JSSelected', config);
            };
        };

        if (events.hasBrushScrolled === true) {
            options.chart.events.brushScrolled = function (chartContext, config) {
                dotNetObject.invokeMethodAsync('JSBrushScrolled', config);
            };
        };

        if (events.hasZoomed === true) {
            options.chart.events.zoomed = function (chartContext, config) {
                dotNetObject.invokeMethodAsync('JSZoomed', config);
            };
        };

        if (events.hasAnimationEnd === true) {
            options.chart.events.animationEnd = function (chartContext, options) {
                dotNetObject.invokeMethodAsync('JSAnimationEnd');
            };
        };

        if (events.hasBeforeMount === true) {
            options.chart.events.beforeMount = function (chartContext, config) {
                dotNetObject.invokeMethodAsync('JSBeforeMount');
            };
        };

        if (events.hasMounted === true) {
            options.chart.events.mounted = function (chartContext, config) {
                dotNetObject.invokeMethodAsync('JSMounted');
            };
        };

        if (events.hasUpdated === true) {
            options.chart.events.updated = function (chartContext, config) {
                dotNetObject.invokeMethodAsync('JSUpdated');
            };
        };

        if (events.hasMouseMove === true) {
            options.chart.events.mouseMove = function (event, chartContext, config) {
                var selection = {
                    dataPointIndex: -1, // Documentation notes that these details are available in cartesian charts, this will prevent null reference in .NET callback
                    seriesIndex: -1
                };

                if (config.dataPointIndex >= 0)
                    selection.dataPointIndex = config.dataPointIndex;

                if (config.seriesIndex >= 0)
                    selection.seriesIndex = config.seriesIndex;

                dotNetObject.invokeMethodAsync('JSMouseMove', selection);
            };
        };

        if (events.hasMouseLeave === true) {
            options.chart.events.mouseLeave = function (event, chartContext, config) {
                dotNetObject.invokeMethodAsync('JSMouseLeave');
            };
        };

        if (events.hasClick === true) {
            options.chart.events.click = function (event, chartContext, config) {
                var selection = {
                    dataPointIndex: -1,
                    seriesIndex: -1
                };

                if (config.dataPointIndex >= 0)
                    selection.dataPointIndex = config.dataPointIndex;

                if (config.seriesIndex >= 0)
                    selection.seriesIndex = config.seriesIndex;

                dotNetObject.invokeMethodAsync('JSClick', selection);
            };
        };

        if (events.hasBeforeZoom === true) {
            options.chart.events.beforeZoom = function (chartContext, config) {
                if (config.yaxis !== undefined || Array.isArray(config.yaxis))
                    config.yaxis = undefined;

                var data = dotNetObject.invokeMethod('JSBeforeZoom', config);

                return {
                    xaxis: {
                        min: data.min,
                        max: data.max
                    }
                };
            };
        };

        if (events.hasBeforeResetZoom === true) {
            options.chart.events.beforeResetZoom = function (chartContext, opts) {
                var data = dotNetObject.invokeMethod('JSBeforeResetZoom');

                return {
                    xaxis: {
                        min: data.min,
                        max: data.max
                    }
                };
            };
        };

        if (events.hasScrolled === true) {
            options.chart.events.scrolled = function (chartContext, config) {
                dotNetObject.invokeMethodAsync('JSScrolled', config);
            };
        };

        //Always destroy chart if it exists
        this.destroyChart(options.chart.id);
        this.dotNetRefs.set(options.chart.id, dotNetObject)

        var chart = new ApexCharts(container, options);
        chart.render();

        if (options.debug == true) {
            console.log('Chart ' + options.chart.id + ' rendered');
        }
    },

    parseOptions(options) {
        return JSON.parse(options, (key, value) => {
            if (value && typeof value === 'object' && '@eval' in value) {
                value = value['@eval'];
                if (Array.isArray(value))
                    return value.map(item => eval?.("'use strict'; (" + item + ")"));
                else
                    return eval?.("'use strict'; (" + value + ")");
            }
            else {
                return value;
            }
        });
    }
}
