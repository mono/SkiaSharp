import { config as unoConfig } from "/SkiaSharp/staging/4131/gallery-uno/package_5947537d7caf45d2b637e3d7377d5ca7521a9741/uno-config.js";

const MAX_CACHE_CONCURRENCY = 10;

/**
 * Adds an array of files to a Cache using a sliding concurrency pool.
 * A slow or failed download only occupies one slot and never blocks others.
 *
 * @param {Cache} cache - The Cache to add files to.
 * @param {string[]} files - URLs to cache.
 * @param {number} maxConcurrency - Maximum parallel downloads.
 */
async function cacheFilesWithConcurrency(cache, files, maxConcurrency) {
    const pendingPuts = [];
    let inFlight = 0;
    let nextIndex = 0;

    // Download files with bounded concurrency. Cache writes are
    // started immediately but not awaited until the end, so they
    // never block the next download from starting.
    await new Promise(resolve => {
        if (files.length === 0) {
            return resolve();
        }
        function startNext() {
            while (inFlight < maxConcurrency && nextIndex < files.length) {
                const currentFile = files[nextIndex++];
                inFlight++;
                if (unoConfig.uno_enable_tracing) {
                    console.debug(`[ServiceWorker] caching ${currentFile}`);
                }
                fetch(currentFile)
                    .then(response => {
                        if (!response.ok) {
                            console.debug(`[ServiceWorker] Failed to fetch ${currentFile}: ${response.status} ${response.statusText}`);
                            return;
                        }
                        // Queue the cache write but don't wait for it
                        pendingPuts.push(
                            cache.put(currentFile, response).catch(e => {
                                console.debug(`[ServiceWorker] Failed to cache ${currentFile}: ${e.message}`);
                            })
                        );
                    })
                    .catch(e => {
                        console.debug(`[ServiceWorker] Failed to fetch ${currentFile}: ${e.message}`);
                    })
                    .then(() => {
                        inFlight--;
                        if (nextIndex < files.length) {
                            startNext();
                        } else if (inFlight === 0) {
                            resolve();
                        }
                    });
            }
        }
        startNext();
    });

    // Wait for all cache writes to finish
    await Promise.allSettled(pendingPuts);
}

if (unoConfig.environmentVariables["UNO_BOOTSTRAP_DEBUGGER_ENABLED"] !== "True") {
    console.debug("[ServiceWorker] Initializing");
    let uno_enable_tracing = unoConfig.uno_enable_tracing;

    // Get the number of fetch retries from environment variables or default to 1
    const fetchRetries = parseInt(unoConfig.environmentVariables["UNO_BOOTSTRAP_FETCH_RETRIES"] || "1");

    self.addEventListener('install', function (e) {
        console.debug('[ServiceWorker] Installing offline worker');
        e.waitUntil(
            caches.open('d3d88cb2-1865-403a-9c75-7ae5be6aae15').then(async function (cache) {
                console.debug('[ServiceWorker] Caching app binaries and content');

                await cacheFilesWithConcurrency(cache, unoConfig.offline_files, MAX_CACHE_CONCURRENCY);

                // Add the runtime's own files to the cache. We cannot use the
                // existing cached content from the runtime as the keys contain a
                // hash we cannot reliably compute.
                try {
                    // Replace dynamic import with fetch and eval for web worker compatibility
                    // In .NET 10+, dotnet.boot.js was merged with dotnet.js for performance
                    // Use the fingerprinted filename from config for proper caching
                    const response = await fetch(`/_framework/${unoConfig.dotnet_js_filename}`);
                    if (!response.ok) {
                        throw new Error(`Failed to fetch ${unoConfig.dotnet_js_filename}: ${response.status} ${response.statusText}`);
                    }

                    let scriptContent = await response.text();

                    // The parsing assumes that this block is present:
                    // https://github.com/dotnet/runtime/blob/41c9fa2d39a02d98cdead08e72f961e77b7888b0/src/tasks/Microsoft.NET.Sdk.WebAssembly.Pack.Tasks/BootJsonBuilderHelper.cs#L74
                    const match = scriptContent.match(/.*?\/\*json-start\*\/([\s\S]*?)\/\*json-end\*\//);

                    // If found, wrap it with parentheses so eval can treat it as an object literal
                    const c = match ? `(${match[1]})` : null;

                    if (!c) {
                        throw `Invalid config`;
                    }

                    const bootJson = eval(c);
                    const monoConfigResources = bootJson.resources || {};

                    function extractFilenames(resource) {
                        if (!resource) return [];
                        if (Array.isArray(resource)) {
                            // Format 3: array of {virtualPath, name, integrity}
                            return resource.filter(e => e && e.name).map(e => e.name);
                        }
                        // Format 1 or 2: object
                        return Object.keys(resource).map(key => {
                            const val = resource[key];
                            // Format 2: value is an object with a name field (fingerprinted filename)
                            // Format 1: value is a plain string (hash), use the key as the filename
                            return (val && typeof val === 'object' && val.name) ? val.name : key;
                        });
                    }

                    const frameworkUris = [
                        monoConfigResources.coreAssembly,
                        monoConfigResources.assembly,
                        monoConfigResources.lazyAssembly,
                        monoConfigResources.jsModuleWorker,
                        monoConfigResources.jsModuleGlobalization,
                        monoConfigResources.jsModuleNative,
                        monoConfigResources.jsModuleRuntime,
                        monoConfigResources.wasmNative,
                        monoConfigResources.icu
                    ].flatMap(extractFilenames).map(
                        filename => `/_framework/${filename}`
                    );
                    await cacheFilesWithConcurrency(cache, frameworkUris, MAX_CACHE_CONCURRENCY);
                } catch (e) {
                    // Centralized error handling for the entire boot.json processing
                    console.error('[ServiceWorker] Error processing boot configuration:', e.message);
                }
            })
        );
    });

    // Cache cleanup logic to prevent storage bloat
    // This removes any old caches that might have been created by previous
    // versions of the service worker, helping prevent storage quota issues
    self.addEventListener('activate', event => {
        event.waitUntil(
            caches.keys().then(function (cacheNames) {
                return Promise.all(
                    cacheNames.filter(function (cacheName) {
                        return cacheName !== 'd3d88cb2-1865-403a-9c75-7ae5be6aae15';
                    }).map(function (cacheName) {
                        console.debug('[ServiceWorker] Deleting old cache:', cacheName);
                        return caches.delete(cacheName);
                    })
                );
            }).then(function () {
                return self.clients.claim();
            })
        );
    });

    self.addEventListener('fetch', event => {
        event.respondWith(
            (async function () {
                // FIXED: Critical fix for "already used" Request objects #956
                // Request objects can only be used once in a fetch operation
                // Cloning the request allows for reuse in fallback scenarios
                const requestClone = event.request.clone();

                try {
                    // Network first mode to get fresh content every time, then fallback to
                    // cache content if needed.
                    return await fetch(requestClone);
                } catch (err) {
                    // Logging to track network failures
                    console.debug(`[ServiceWorker] Network fetch failed, falling back to cache for: ${requestClone.url}`);

                    const cachedResponse = await caches.match(event.request);
                    if (cachedResponse) {
                        return cachedResponse;
                    }

                    // Add retry mechanism - attempt to fetch again if retries are configured
                    if (fetchRetries > 0) {
                        console.debug(`[ServiceWorker] Resource not in cache, attempting ${fetchRetries} network retries for: ${requestClone.url}`);

                        // Try multiple fetch attempts with exponential backoff
                        for (let retryCount = 0; retryCount < fetchRetries; retryCount++) {
                            try {
                                // Exponential backoff between retries (500ms, 1s, 2s, etc.)
                                const retryDelay = Math.pow(2, retryCount) * 500;
                                await new Promise(resolve => setTimeout(resolve, retryDelay));

                                if (uno_enable_tracing) {
                                    console.debug(`[ServiceWorker] Retry attempt ${retryCount + 1}/${fetchRetries} for: ${requestClone.url}`);
                                }

                                // Need a fresh request clone for each retry
                                return await fetch(event.request.clone());
                            } catch (retryErr) {
                                if (uno_enable_tracing) {
                                    console.debug(`[ServiceWorker] Retry ${retryCount + 1} failed: ${retryErr.message}`);
                                }
                                // Continue to next retry attempt
                            }
                        }
                    }

                    // Graceful error handling with a proper HTTP response
                    // Rather than letting the fetch fail with a generic error,
                    // we return a controlled 503 Service Unavailable response
                    console.error(`[ServiceWorker] Resource not available in cache or network after ${fetchRetries} retries: ${requestClone.url}`);
                    return new Response('Network error occurred, and resource was not found in cache.', {
                        status: 503,
                        statusText: 'Service Unavailable',
                        headers: new Headers({
                            'Content-Type': 'text/plain'
                        })
                    });
                }
            })()
        );
    });
}
else {
    // In development, always fetch from the network and do not enable offline support.
    // This is because caching would make development more difficult (changes would not
    // be reflected on the first load after each change).
    // It also breaks the hot reload feature because VS's browserlink is not always able to
    // inject its own framework in the served scripts and pages.
    self.addEventListener('fetch', () => { });
}
