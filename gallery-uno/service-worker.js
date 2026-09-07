import { config as unoConfig } from "./uno-config.js";

const CACHE_NAME = 'bcce95e0-c7b8-4a80-9514-76689a2fa589';
const WEBAPP_PATH = './';

// Absolute paths, as WEBAPP_PATH may be relative (./); the worker sits at the root of the app
const APP_ROOT = new URL(WEBAPP_PATH, self.location).pathname;
const PACKAGE_ROOT = new URL(`${unoConfig.uno_app_base}/`, self.location).pathname;
const pathOf = file => new URL(file, self.location).pathname;

// Network-first requests fall back to the cache after this long
const NETWORK_TIMEOUT_MS = 4000;
const PRECACHE_CONCURRENCY = 3;

const tracing = unoConfig.uno_enable_tracing;
const fetchRetries = parseInt(unoConfig.environmentVariables["UNO_BOOTSTRAP_FETCH_RETRIES"] || "1");

function trace(message) {
    if (tracing) {
        console.debug(`[ServiceWorker] ${message}`);
    }
}

/**
 * URLs whose content never changes: the package folder is named after a hash of its content,
 * and the .NET SDK fingerprints _framework files.
 */
function isImmutable(url) {
    const path = url.pathname;

    return path.startsWith(PACKAGE_ROOT)
        || (path.startsWith(`${APP_ROOT}_framework/`) && /\.[a-z0-9]{10}\.[a-z]+$/.test(path));
}

const appFiles = new Set([WEBAPP_PATH, `${WEBAPP_PATH}index.html`, `${WEBAPP_PATH}uno-config.js`, ...unoConfig.offline_files].map(pathOf));

/**
 * The app's own files. Other requests (API calls, streams, user data) are left to the browser: they must not be
 * cut off by the network timeout, nor cached and replayed for a different query.
 */
function isAppFile(url) {
    const path = url.pathname;

    return appFiles.has(path) || path.startsWith(PACKAGE_ROOT) || path.startsWith(`${APP_ROOT}_framework/`);
}

function globToRegExp(glob) {
    const escaped = glob.trim().replace(/[.+^${}()|[\]\\]/g, "\\$&").replace(/\*\*/g, "\u0000").replace(/\*/g, "[^/]*").replace(/\u0000/g, ".*");
    return new RegExp(`(^|/)${escaped}$`, "i");
}

const precacheExclusions = (unoConfig.uno_pwa_precache_exclude || []).filter(g => g).map(globToRegExp);

async function readBootFiles() {
    // In .NET 10+, the boot config is embedded in dotnet.js
    const response = await fetch(`${WEBAPP_PATH}_framework/${unoConfig.dotnet_js_filename}`);
    if (!response.ok) {
        throw new Error(`Failed to fetch ${unoConfig.dotnet_js_filename}: ${response.status} ${response.statusText}`);
    }

    // See https://github.com/dotnet/runtime/blob/41c9fa2d39a02d98cdead08e72f961e77b7888b0/src/tasks/Microsoft.NET.Sdk.WebAssembly.Pack.Tasks/BootJsonBuilderHelper.cs#L74
    const match = (await response.text()).match(/\/\*json-start\*\/([\s\S]*?)\/\*json-end\*\//);
    if (!match) {
        throw new Error("Invalid boot config");
    }

    const resources = JSON.parse(match[1]).resources || {};

    const names = resource => !resource ? []
        : Array.isArray(resource) ? resource.filter(e => e && e.name).map(e => e.name)
            : Object.keys(resource).map(key => resource[key] && typeof resource[key] === "object" && resource[key].name ? resource[key].name : key);

    return [
        resources.coreAssembly,
        resources.assembly,
        resources.lazyAssembly,
        resources.jsModuleWorker,
        resources.jsModuleGlobalization,
        resources.jsModuleNative,
        resources.jsModuleRuntime,
        resources.wasmNative,
        resources.icu
    ].flatMap(names).map(name => `${WEBAPP_PATH}_framework/${name}`);
}

/** Caches the files that are not cached yet, a few at a time so they don't compete with the app. */
async function precache(files) {
    const cache = await caches.open(CACHE_NAME);
    const pending = [];
    for (const file of files) {
        if (!(await cache.match(file, { ignoreSearch: true }))) {
            pending.push(file);
        }
    }

    let next = 0;
    const worker = async () => {
        while (next < pending.length) {
            const file = pending[next++];
            try {
                const response = await fetch(file);
                if (response.ok) {
                    await cache.put(file, response);
                    trace(`Cached ${file}`);
                } else {
                    trace(`Failed to fetch ${file}: ${response.status}`);
                }
            } catch (e) {
                trace(`Failed to fetch ${file}: ${e.message}`);
            }
        }
    };

    await Promise.all(Array.from({ length: PRECACHE_CONCURRENCY }, worker));
}

/**
 * Fetches and reads the whole body, within the timeout when there is one: a connection dropping mid-body must
 * fall back to the cache or be retried, instead of handing the page a truncated response.
 */
async function fetchFully(request, timeoutMs) {
    const controller = new AbortController();
    const timer = timeoutMs ? setTimeout(() => controller.abort(), timeoutMs) : undefined;
    try {
        // A navigation request can't be combined with a RequestInit
        const response = await fetch(request.mode === "navigate" ? request.url : request, { signal: controller.signal });

        // These statuses can't carry a body, not even an empty one
        const body = [101, 204, 205, 304].includes(response.status) ? null : await response.arrayBuffer();
        return new Response(body, { status: response.status, statusText: response.statusText, headers: response.headers });
    } finally {
        clearTimeout(timer);
    }
}

async function putInCache(request, response) {
    try {
        const cache = await caches.open(CACHE_NAME);
        await cache.put(request, response);
    } catch (e) {
        trace(`Failed to cache ${request.url}: ${e.message}`);
    }
}

/** Immutable files: the cached copy is always right, so the network is only used on a miss. */
async function cacheFirst(event) {
    const request = event.request;
    const cached = await caches.match(request, { ignoreSearch: true });
    if (cached) {
        return cached;
    }

    const response = await fetch(request);
    if (response.ok) {
        event.waitUntil(putInCache(request, response.clone()));
    }
    return response;
}

/** Everything else: fresh from the network when it answers in time, else from the cache. */
async function networkFirst(event) {
    const request = event.request;
    try {
        const response = await fetchFully(request.clone(), NETWORK_TIMEOUT_MS);
        if (response.ok && !/no-store/i.test(response.headers.get("cache-control") ?? "")) {
            event.waitUntil(putInCache(request, response.clone()));
        }
        return response;
    } catch (err) {
        trace(`Network failed or timed out, falling back to the cache for ${request.url}`);
    }

    // Retried requests carry a cache-busting query, precached ones don't
    const cached = await caches.match(request, { ignoreSearch: true });
    if (cached) {
        return cached;
    }

    // No timeout here: this is the last resort for a large file on a slow connection
    for (let retry = 0; retry < fetchRetries; retry++) {
        await new Promise(resolve => setTimeout(resolve, Math.pow(2, retry) * 500));
        try {
            return await fetchFully(request.clone());
        } catch (e) {
            trace(`Retry ${retry + 1} failed for ${request.url}: ${e.message}`);
        }
    }

    console.error(`[ServiceWorker] Resource not available in cache or network: ${request.url}`);
    return new Response('Network error occurred, and resource was not found in cache.', {
        status: 503,
        statusText: 'Service Unavailable',
        headers: new Headers({ 'Content-Type': 'text/plain' })
    });
}

if (unoConfig.environmentVariables["UNO_BOOTSTRAP_DEBUGGER_ENABLED"] !== "True") {
    console.debug("[ServiceWorker] Initializing");

    // Only what the app needs to start offline. Its files were just downloaded by the page,
    // so most come from the HTTP cache.
    self.addEventListener('install', event => {
        event.waitUntil((async () => {
            const bootFiles = [WEBAPP_PATH, `${WEBAPP_PATH}uno-config.js`, `${WEBAPP_PATH}_framework/${unoConfig.dotnet_js_filename}`]
                .concat(unoConfig.offline_files.filter(f => pathOf(f).startsWith(PACKAGE_ROOT) && /\.(js|css)$/.test(f)));

            try {
                bootFiles.push(...await readBootFiles());
            } catch (e) {
                console.error('[ServiceWorker] Error processing boot configuration:', e.message);
            }

            await precache(bootFiles);
        })());
    });

    self.addEventListener('activate', event => {
        event.waitUntil((async () => {
            // Drop caches of previous versions to avoid storage bloat
            for (const name of await caches.keys()) {
                if (name !== CACHE_NAME) {
                    console.debug('[ServiceWorker] Deleting old cache:', name);
                    await caches.delete(name);
                }
            }

            await self.clients.claim();
        })());
    });

    // Sent by the bootstrapper once the app is running: caches the remaining files, so the app also works
    // offline after this first visit. Not done in 'activate', since fetches wait for activation to complete.
    // Every open tab sends the message, so they share a single run
    let backgroundPrecache = null;

    self.addEventListener('message', event => {
        if (event.data === 'uno-precache') {
            backgroundPrecache ??= precache(unoConfig.offline_files.filter(f => !precacheExclusions.some(r => r.test(f))))
                .finally(() => backgroundPrecache = null);
            event.waitUntil(backgroundPrecache);
        }
    });

    self.addEventListener('fetch', event => {
        const url = new URL(event.request.url);
        if (event.request.method !== "GET" || url.origin !== self.location.origin || !isAppFile(url)) {
            return;
        }

        event.respondWith(isImmutable(url) ? cacheFirst(event) : networkFirst(event));
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
