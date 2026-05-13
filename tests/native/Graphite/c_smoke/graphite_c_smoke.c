/*
 * Graphite C smoke test (Layer 2 of specs/002-graphite-backend-support/quickstart.md).
 *
 * Brings up a Vulkan instance + device on Mesa Lavapipe, threads it through the
 * new sk_graphite_* C API, draws a 256x256 white surface with a centered red
 * rounded rectangle, snaps + inserts + submits the recording (sync to CPU),
 * reads pixels back, and asserts that pixel (128, 128) is red.
 *
 * Exit codes are distinct per failure point so CI / a developer can tell which
 * step broke without re-running. Pass: 0.
 */

#include <stdbool.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include <vulkan/vulkan.h>

#include "include/c/sk_canvas.h"
#include "include/c/sk_graphite.h"
#include "include/c/sk_graphite_vulkan.h"
#include "include/c/sk_paint.h"
#include "include/c/sk_rrect.h"
#include "include/c/sk_surface.h"
#include "include/c/sk_types.h"

#define LOG(fmt, ...) fprintf(stderr, "[c_smoke] " fmt "\n", ##__VA_ARGS__)
#define DIE(code, fmt, ...) do { LOG(fmt, ##__VA_ARGS__); return (code); } while (0)

/* -------- Vulkan bring-up over Lavapipe -------- */

static VkInstance       g_instance = VK_NULL_HANDLE;
static VkPhysicalDevice g_physical = VK_NULL_HANDLE;
static VkDevice         g_device   = VK_NULL_HANDLE;
static VkQueue          g_queue    = VK_NULL_HANDLE;
static uint32_t         g_queue_family = 0;

static int bring_up_vulkan(void) {
    VkApplicationInfo app = {0};
    app.sType = VK_STRUCTURE_TYPE_APPLICATION_INFO;
    app.pApplicationName = "graphite_c_smoke";
    app.applicationVersion = 1;
    app.pEngineName = "SkiaSharp";
    app.engineVersion = 1;
    app.apiVersion = VK_API_VERSION_1_3;

    VkInstanceCreateInfo ici = {0};
    ici.sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO;
    ici.pApplicationInfo = &app;

    if (vkCreateInstance(&ici, NULL, &g_instance) != VK_SUCCESS) {
        DIE(20, "vkCreateInstance failed");
    }

    uint32_t pdcount = 0;
    if (vkEnumeratePhysicalDevices(g_instance, &pdcount, NULL) != VK_SUCCESS || pdcount == 0) {
        DIE(21, "no Vulkan physical devices found");
    }
    VkPhysicalDevice* pds = (VkPhysicalDevice*)malloc(sizeof(VkPhysicalDevice) * pdcount);
    if (!pds) DIE(22, "OOM enumerating physical devices");
    if (vkEnumeratePhysicalDevices(g_instance, &pdcount, pds) != VK_SUCCESS) {
        free(pds);
        DIE(23, "vkEnumeratePhysicalDevices failed");
    }
    /* Prefer Lavapipe (CPU) if available, otherwise pick the first device. */
    g_physical = pds[0];
    for (uint32_t i = 0; i < pdcount; i++) {
        VkPhysicalDeviceProperties props;
        vkGetPhysicalDeviceProperties(pds[i], &props);
        LOG("physical device %u: '%s' type=%d", i, props.deviceName, props.deviceType);
        if (props.deviceType == VK_PHYSICAL_DEVICE_TYPE_CPU) {
            g_physical = pds[i];
        }
    }
    free(pds);

    /* Find a graphics queue family. */
    uint32_t qfcount = 0;
    vkGetPhysicalDeviceQueueFamilyProperties(g_physical, &qfcount, NULL);
    if (qfcount == 0) DIE(24, "no Vulkan queue families");
    VkQueueFamilyProperties* qfs = (VkQueueFamilyProperties*)malloc(sizeof(VkQueueFamilyProperties) * qfcount);
    if (!qfs) DIE(25, "OOM enumerating queue families");
    vkGetPhysicalDeviceQueueFamilyProperties(g_physical, &qfcount, qfs);
    bool found_qf = false;
    for (uint32_t i = 0; i < qfcount; i++) {
        if (qfs[i].queueFlags & VK_QUEUE_GRAPHICS_BIT) {
            g_queue_family = i;
            found_qf = true;
            break;
        }
    }
    free(qfs);
    if (!found_qf) DIE(26, "no graphics-capable queue family");

    /* Create logical device with one graphics queue. */
    float qprio = 1.0f;
    VkDeviceQueueCreateInfo qci = {0};
    qci.sType = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO;
    qci.queueFamilyIndex = g_queue_family;
    qci.queueCount = 1;
    qci.pQueuePriorities = &qprio;

    VkDeviceCreateInfo dci = {0};
    dci.sType = VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO;
    dci.queueCreateInfoCount = 1;
    dci.pQueueCreateInfos = &qci;

    if (vkCreateDevice(g_physical, &dci, NULL, &g_device) != VK_SUCCESS) {
        DIE(27, "vkCreateDevice failed");
    }
    vkGetDeviceQueue(g_device, g_queue_family, 0, &g_queue);

    return 0;
}

static void tear_down_vulkan(void) {
    if (g_device   != VK_NULL_HANDLE) { vkDestroyDevice(g_device, NULL); g_device = VK_NULL_HANDLE; }
    if (g_instance != VK_NULL_HANDLE) { vkDestroyInstance(g_instance, NULL); g_instance = VK_NULL_HANDLE; }
}

/* -------- async readback callback -------- */

typedef struct {
    int  done;
    int  ok;
    void* dst;
    int  dst_row_bytes;
    int  width;
    int  height;
} read_state_t;

static void async_read_cb(void* userData, const sk_graphite_async_read_result_t* result) {
    read_state_t* s = (read_state_t*)userData;
    if (!result || sk_graphite_async_read_result_get_count(result) == 0) { s->done = 1; return; }
    const void* src = sk_graphite_async_read_result_get_data(result, 0);
    size_t      src_rb = sk_graphite_async_read_result_get_row_bytes(result, 0);
    if (!src) { s->done = 1; return; }
    size_t copy = (src_rb < (size_t)s->dst_row_bytes) ? src_rb : (size_t)s->dst_row_bytes;
    for (int y = 0; y < s->height; y++) {
        memcpy((char*)s->dst + (size_t)y * s->dst_row_bytes,
               (const char*)src + (size_t)y * src_rb,
               copy);
    }
    s->ok = 1;
    s->done = 1;
}

/* GetProc adapter: routes Skia's name->fn lookups through the Vulkan loader. */
static VKAPI_ATTR void (VKAPI_PTR* sk_get_proc(void* userData, const char* name, vk_instance_t* instance, vk_device_t* device))(void) {
    (void)userData;
    if (device != NULL) {
        return (VKAPI_ATTR void (VKAPI_PTR*)(void))vkGetDeviceProcAddr((VkDevice)device, name);
    }
    if (instance != NULL) {
        return (VKAPI_ATTR void (VKAPI_PTR*)(void))vkGetInstanceProcAddr((VkInstance)instance, name);
    }
    /* Pre-instance lookup: vkCreateInstance, vkEnumerateInstance*. */
    return (VKAPI_ATTR void (VKAPI_PTR*)(void))vkGetInstanceProcAddr(NULL, name);
}

/* -------- main: drive the smoke -------- */

int main(void) {
    LOG("starting");

    if (!sk_graphite_backend_is_available(VULKAN_SK_GRAPHITE_BACKEND)) {
        DIE(10, "Vulkan Graphite backend not built into libSkiaSharp.so");
    }

    int rc = bring_up_vulkan();
    if (rc != 0) return rc;
    LOG("Vulkan device created (queueFamily=%u)", g_queue_family);

    sk_graphite_vk_backend_context_init_t init = {0};
    init.fInstance            = (vk_instance_t*)g_instance;
    init.fPhysicalDevice      = (vk_physical_device_t*)g_physical;
    init.fDevice              = (vk_device_t*)g_device;
    init.fQueue               = (vk_queue_t*)g_queue;
    init.fGraphicsQueueIndex  = g_queue_family;
    init.fMaxAPIVersion       = VK_API_VERSION_1_3;
    init.fGetProc             = sk_get_proc;
    init.fGetProcUserData     = NULL;
    init.fProtectedContext    = false;

    sk_graphite_vk_backend_context_t* bc = sk_graphite_vk_backend_context_new(&init);
    if (!bc) { tear_down_vulkan(); DIE(30, "sk_graphite_vk_backend_context_new returned NULL"); }

    sk_graphite_context_t* ctx = sk_graphite_context_make_vulkan(bc, NULL);
    if (!ctx) {
        sk_graphite_vk_backend_context_delete(bc);
        tear_down_vulkan();
        DIE(31, "sk_graphite_context_make_vulkan returned NULL");
    }
    LOG("Graphite context created (max_texture_size=%d)", sk_graphite_context_get_max_texture_size(ctx));

    sk_graphite_recorder_t* rec = sk_graphite_context_make_recorder(ctx, -1);
    if (!rec) DIE(32, "make_recorder returned NULL");

    sk_imageinfo_t info;
    info.colorspace = NULL;
    info.width      = 256;
    info.height     = 256;
    info.colorType  = RGBA_8888_SK_COLORTYPE;
    info.alphaType  = PREMUL_SK_ALPHATYPE;

    sk_surface_t* surf = sk_graphite_surface_make_render_target(rec, &info, /*mipmapped=*/false, /*props=*/NULL);
    if (!surf) DIE(33, "sk_graphite_surface_make_render_target returned NULL");

    sk_canvas_t* canvas = sk_surface_get_canvas(surf);
    if (!canvas) DIE(34, "sk_surface_get_canvas returned NULL");

    /* Clear to white. */
    sk_canvas_clear(canvas, sk_color_set_argb(0xFF, 0xFF, 0xFF, 0xFF));

    /* Red, anti-aliased paint. */
    sk_paint_t* paint = sk_paint_new();
    sk_paint_set_antialias(paint, true);
    sk_paint_set_color(paint, sk_color_set_argb(0xFF, 0xFF, 0x00, 0x00));

    /* Draw a centered rounded rect: bounds (32, 32, 224, 224), corner radius 24. */
    sk_rect_t bounds;
    bounds.left   = 32.0f;
    bounds.top    = 32.0f;
    bounds.right  = 224.0f;
    bounds.bottom = 224.0f;
    sk_canvas_draw_round_rect(canvas, &bounds, 24.0f, 24.0f, paint);

    sk_paint_delete(paint);

    /* Snap + insert + submit (sync). */
    sk_graphite_recording_t* rcd = sk_graphite_recorder_snap(rec);
    if (!rcd) DIE(40, "recorder snap returned NULL");

    sk_graphite_insert_recording_info_t iri = {0};
    iri.fRecording = rcd;

    sk_graphite_insert_status_t st = sk_graphite_context_insert_recording(ctx, &iri);
    if (st != SUCCESS_SK_GRAPHITE_INSERT_STATUS) {
        LOG("insert_recording status=%d", (int)st);
        sk_graphite_recording_delete(rcd);
        DIE(41, "insert_recording non-success");
    }

    sk_graphite_submit_info_t si = {0};
    si.fSync = true;
    si.fMarkBoundary = false;
    si.fFrameID = 0;
    if (!sk_graphite_context_submit(ctx, &si)) {
        sk_graphite_recording_delete(rcd);
        DIE(42, "submit returned false");
    }

    sk_graphite_recording_delete(rcd);

    /* Read full 256x256 buffer. */
    const int W = 256, H = 256;
    uint32_t* pixels = (uint32_t*)calloc((size_t)W * H, sizeof(uint32_t));
    if (!pixels) DIE(49, "OOM allocating readback buffer");
    sk_imageinfo_t dst_info;
    dst_info.colorspace = NULL;
    dst_info.width      = W;
    dst_info.height     = H;
    dst_info.colorType  = RGBA_8888_SK_COLORTYPE;
    dst_info.alphaType  = PREMUL_SK_ALPHATYPE;
    /* Async readback. sk_surface_read_pixels does NOT work on Graphite-backed
       surfaces in production builds (Skia gates it on GPU_TEST_UTILS), so we
       drive Context::asyncRescaleAndReadPixels through the C API directly,
       spinning on checkAsyncWorkCompletion until the callback fires. */
    read_state_t state = { 0, 0, pixels, W * 4, W, H };

    sk_irect_t src_rect = { 0, 0, W, H };
    sk_graphite_context_async_rescale_and_read_pixels_surface(
        ctx, surf, &dst_info, &src_rect,
        SRC_SK_GRAPHITE_RESCALE_GAMMA,
        REPEATED_LINEAR_SK_GRAPHITE_RESCALE_MODE,
        async_read_cb, &state);

    /* Drive completion. Submit non-syncing first so any pending recordings
       have actually been dispatched to the GPU. */
    sk_graphite_submit_info_t empty_submit = {0};
    sk_graphite_context_submit(ctx, &empty_submit);
    while (!state.done) {
        sk_graphite_context_check_async_work_completion(ctx);
    }

    if (!state.ok) {
        free(pixels);
        DIE(50, "async readback failed");
    }

    uint32_t px = pixels[(size_t)128 * W + 128];
    /* RGBA_8888 layout: low byte = R. */
    uint8_t r = (px >>  0) & 0xFF;
    uint8_t g = (px >>  8) & 0xFF;
    uint8_t b = (px >> 16) & 0xFF;
    uint8_t a = (px >> 24) & 0xFF;
    LOG("pixel @ (128, 128) = R=%u G=%u B=%u A=%u (raw=0x%08X)", r, g, b, a, px);
    /* Sample a corner pixel too — should be white background. */
    uint32_t corner = pixels[0];
    LOG("pixel @ (0, 0)     = R=%u G=%u B=%u A=%u (raw=0x%08X)",
        (corner >>  0) & 0xFF, (corner >>  8) & 0xFF, (corner >> 16) & 0xFF, (corner >> 24) & 0xFF, corner);
    free(pixels);

    int retval = 0;
    if (r < 200 || g > 50 || b > 50) {
        LOG("FAIL: pixel is not red");
        retval = 60;
    } else {
        LOG("PASS: pixel is red");
    }

    /* Tear down in reverse order. */
    sk_surface_unref(surf);
    sk_graphite_recorder_delete(rec);
    sk_graphite_context_delete(ctx);
    sk_graphite_vk_backend_context_delete(bc);
    tear_down_vulkan();

    return retval;
}
