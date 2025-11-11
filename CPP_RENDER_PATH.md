# C++ MotionMark Render Path Analysis

This document traces the complete rendering path in the fast C++ MotionMark implementation.

## 1. Entry Point: main_mac.mm

**Location:** `/extern/skia/tools/sk_app/mac/main_mac.mm`

```cpp
int main(int argc, char * argv[]) {
    // Setup NSApp, menu, etc.
    Application* app = Application::Create(argc, argv, nullptr);
    
    // Run NSApp briefly to finish launching
    [NSApp run];  // Returns immediately due to AppDelegate stop
    
    // THE MAIN RENDER LOOP - Direct event loop
    while (![appDelegate done]) {
        // 1. Process all pending NSEvents (mouse, keyboard, etc.)
        NSEvent* event;
        do {
            event = [NSApp nextEventMatchingMask:NSEventMaskAny
                                       untilDate:[NSDate distantPast]  // NON-BLOCKING
                                          inMode:NSDefaultRunLoopMode
                                         dequeue:YES];
            [NSApp sendEvent:event];
        } while (event != nil);
        
        // 2. Paint ALL windows that are invalidated
        Window_mac::PaintWindows();  // DIRECT CALL - NO DISPLAY QUEUE
        
        // 3. App idle processing
        app->onIdle();  // Invalidates window for next frame
    }
}
```

**Key Points:**
- **NOT using `[NSApp run]` main loop** - it's stopped immediately
- **Custom tight loop** that processes events then paints directly
- **No display system** - calls `PaintWindows()` directly every iteration
- **Non-blocking event processing** - `distantPast` means "don't wait"

---

## 2. Window Painting: Window_mac::PaintWindows()

**Location:** `/extern/skia/tools/sk_app/mac/Window_mac.mm:191`

```cpp
void Window_mac::PaintWindows() {
    gWindowMap.foreach([&](Window_mac* window) {
        if (window->fIsContentInvalidated) {
            window->onPaint();  // DIRECT CALL
        }
    });
}
```

**Key Points:**
- **Static method** that paints all windows
- **Checks invalidation flag** - set by `app->onIdle()` each frame
- **Direct call to `onPaint()`** - no Cocoa drawing system involved

---

## 3. Window Painting: Window::onPaint()

**Location:** `/extern/skia/tools/sk_app/Window.cpp:89`

```cpp
void Window::onPaint() {
    if (!fWindowContext) return;
    if (!fIsActive) return;
    
    // 1. Get the backbuffer surface from WindowContext
    sk_sp<SkSurface> backbuffer = fWindowContext->getBackbufferSurface();
    if (backbuffer == nullptr) {
        return;
    }
    
    markInvalProcessed();  // Clear invalidation flag
    
    // 2. Call layer onPaint methods (MotionMarkLayer::onPaint)
    this->visitLayers([](Layer* layer) { layer->onPrePaint(); });
    this->visitLayers([=](Layer* layer) { layer->onPaint(backbuffer.get()); });
    
    // 3. Flush GPU commands
    #if defined(SK_GANESH)
        if (auto dContext = this->directContext()) {
            dContext->flushAndSubmit(backbuffer.get(), GrSyncCpu::kNo);
        }
    #endif
    
    // 4. Swap buffers
    fWindowContext->swapBuffers();
}
```

**Key Points:**
- Gets surface directly from WindowContext (no `drawRect:` callback)
- Calls layer paint methods directly
- Flushes GPU with `GrSyncCpu::kNo` (async)
- Swaps buffers immediately

---

## 4. MotionMark Scene Rendering

**Location:** `/samples/MotionMark.SkiaNative/motionmark_app.cpp:92`

```cpp
void MotionMarkLayer::onPaint(SkSurface* surface) {
    SkCanvas* canvas = surface->getCanvas();
    canvas->clear(fBackgroundPaint.getColor());
    
    if (fElements.empty()) {
        return;
    }
    
    // Calculate scaling
    const float scaleX = static_cast<float>(fWidth) / static_cast<float>(kGridWidth + 1);
    const float scaleY = static_cast<float>(fHeight) / static_cast<float>(kGridHeight + 1);
    const float uniformScale = std::max(0.0f, std::min(scaleX, scaleY));
    
    const float offsetX = (static_cast<float>(fWidth) - uniformScale * (kGridWidth + 1)) * 0.5f;
    const float offsetY = (static_cast<float>(fHeight) - uniformScale * (kGridHeight + 1)) * 0.5f;
    
    // Build and draw paths
    SkPathBuilder pathBuilder;
    bool pathStarted = false;
    
    for (size_t i = 0; i < fElements.size(); ++i) {
        Element& element = fElements[i];
        
        if (!pathStarted) {
            const SkPoint start = this->toPoint(element.start, uniformScale, offsetX, offsetY);
            pathBuilder.moveTo(start);
            pathStarted = true;
        }
        
        // Add segment based on kind (Line/Quad/Cubic)
        switch (element.kind) {
            case SegmentKind::kLine:
                pathBuilder.lineTo(this->toPoint(element.end, uniformScale, offsetX, offsetY));
                break;
            case SegmentKind::kQuad:
                pathBuilder.quadTo(this->toPoint(element.control1, uniformScale, offsetX, offsetY),
                                 this->toPoint(element.end, uniformScale, offsetX, offsetY));
                break;
            case SegmentKind::kCubic:
                pathBuilder.cubicTo(this->toPoint(element.control1, uniformScale, offsetX, offsetY),
                                  this->toPoint(element.control2, uniformScale, offsetX, offsetY),
                                  this->toPoint(element.end, uniformScale, offsetX, offsetY));
                break;
        }
        
        // Finalize path when split or at end
        const bool finalize = element.split || i + 1 == fElements.size();
        if (finalize && !pathBuilder.isEmpty()) {
            fStrokePaint.setColor(element.color);
            fStrokePaint.setStrokeWidth(element.width);
            canvas->drawPath(pathBuilder.detach(), fStrokePaint);
            pathStarted = false;
        }
    }
}
```

**Key Points:**
- Receives `SkSurface*` directly (not creating one)
- Uses `SkPathBuilder` to batch path segments
- Draws paths with stroke paint
- No disposal needed - just uses the provided surface

---

## 5. OpenGL Context & Buffer Swap

**Location:** `/extern/skia/tools/window/mac/GaneshGLWindowContext_mac.mm`

### Context Creation (onInitializeContext):

```cpp
sk_sp<const GrGLInterface> GLWindowContext_mac::onInitializeContext() {
    if (!fGLContext) {
        // Create pixel format with MSAA, stencil, etc.
        fPixelFormat = skwindow::GetGLPixelFormat(fDisplayParams->msaaSampleCount());
        
        // Create OpenGL context
        fGLContext = [[NSOpenGLContext alloc] initWithFormat:fPixelFormat shareContext:nil];
        
        // Enable high-DPI
        [fMainView setWantsBestResolutionOpenGLSurface:YES];
        [fGLContext setView:fMainView];
    }
    
    // Set vsync
    GLint swapInterval = fDisplayParams->disableVsync() ? 0 : 1;
    [fGLContext setValues:&swapInterval forParameter:NSOpenGLCPSwapInterval];
    
    // Make current
    [fGLContext makeCurrentContext];
    
    // Clear buffers
    GR_GL_CALL(gl, ClearStencil(0));
    GR_GL_CALL(gl, ClearColor(0, 0, 0, 255));
    GR_GL_CALL(gl, StencilMask(0xffffffff));
    GR_GL_CALL(gl, Clear(GL_STENCIL_BUFFER_BIT | GL_COLOR_BUFFER_BIT));
    
    // Setup viewport
    CGFloat backingScaleFactor = skwindow::GetBackingScaleFactor(fMainView);
    fWidth = fMainView.bounds.size.width * backingScaleFactor;
    fHeight = fMainView.bounds.size.height * backingScaleFactor;
    GR_GL_CALL(gl, Viewport(0, 0, fWidth, fHeight));
    
    return GrGLInterfaces::MakeMac();
}
```

### Buffer Swap (onSwapBuffers):

```cpp
void GLWindowContext_mac::onSwapBuffers() {
    GrDirectContext* dContext = fSurface->recordingContext()->asDirectContext();
    
    // Flush with PRESENT access (no sync)
    dContext->flush(fSurface.get(), SkSurfaces::BackendSurfaceAccess::kPresent, {});
    
    // Swap OpenGL buffers
    [fGLContext flushBuffer];
}
```

**Key Points:**
- Uses `NSOpenGLContext` directly (not NSOpenGLView)
- Sets pixel format with MSAA, stencil, double-buffering
- Makes context current manually
- Flushes with `kPresent` access (optimized for display)
- Calls `flushBuffer` to swap OpenGL buffers

---

## 6. Pixel Format Configuration

**Location:** `/extern/skia/tools/window/mac/MacWindowGLUtils.h` (used by GaneshGLWindowContext)

```cpp
static inline NSOpenGLPixelFormat* GetGLPixelFormat(int sampleCount) {
    constexpr int kMaxAttributes = 19;
    NSOpenGLPixelFormatAttribute attributes[kMaxAttributes];
    int numAttributes = 0;
    
    attributes[numAttributes++] = NSOpenGLPFAAccelerated;
    attributes[numAttributes++] = NSOpenGLPFAClosestPolicy;
    attributes[numAttributes++] = NSOpenGLPFADoubleBuffer;
    attributes[numAttributes++] = NSOpenGLPFAOpenGLProfile;
    attributes[numAttributes++] = NSOpenGLProfileVersion3_2Core;
    attributes[numAttributes++] = NSOpenGLPFAColorSize;
    attributes[numAttributes++] = 24;
    attributes[numAttributes++] = NSOpenGLPFAAlphaSize;
    attributes[numAttributes++] = 8;
    attributes[numAttributes++] = NSOpenGLPFADepthSize;
    attributes[numAttributes++] = 0;
    attributes[numAttributes++] = NSOpenGLPFAStencilSize;
    attributes[numAttributes++] = 8;
    
    if (sampleCount > 1) {
        attributes[numAttributes++] = NSOpenGLPFAMultisample;
        attributes[numAttributes++] = NSOpenGLPFASampleBuffers;
        attributes[numAttributes++] = 1;
        attributes[numAttributes++] = NSOpenGLPFASamples;
        attributes[numAttributes++] = sampleCount;
    } else {
        attributes[numAttributes++] = NSOpenGLPFASampleBuffers;
        attributes[numAttributes++] = 0;
    }
    attributes[numAttributes++] = 0;
    
    return [[NSOpenGLPixelFormat alloc] initWithAttributes:attributes];
}
```

---

## Summary: Why It's Fast

### 1. **No Cocoa Drawing System**
- Doesn't use `setNeedsDisplay:` / `drawRect:`
- No display link / run loop / event queue
- Direct `PaintWindows()` call every frame

### 2. **Tight Render Loop**
```
while (!done) {
    processEvents();    // Non-blocking
    PaintWindows();     // Direct call
    onIdle();           // Invalidate for next frame
}
```

### 3. **Direct OpenGL Context Management**
- Creates `NSOpenGLContext` directly
- Calls `makeCurrentContext` manually
- Calls `flushBuffer` directly for swap

### 4. **No UI Thread Marshalling**
- Everything runs on main thread
- No `DispatchQueue.main.async`
- No Cocoa view hierarchy overhead

### 5. **Optimized GPU Flushing**
- `flushAndSubmit(backbuffer, GrSyncCpu::kNo)` - async
- `flush(surface, BackendSurfaceAccess::kPresent)` - optimized for display
- Immediate buffer swap

### 6. **Simple View Hierarchy**
- Plain `NSView` (MainView) - no custom drawing
- Context renders directly to view's backing layer
- No intermediate buffers or compositing

---

## What SkiaSharp Must Do Differently

The current SkiaSharp macOS sample uses:
- ✗ `NSApplicationMain` + storyboard (Cocoa event loop)
- ✗ `SKGLView` with `setNeedsDisplay:` (display queue)
- ✗ Timer/DisplayLink triggering invalidation (indirect)

To match C++ performance, it needs:
- ✓ **Tight render loop** (like current implementation)
- ✓ **Direct OpenGL context calls** (bypass view hierarchy)
- ✓ **Manual buffer management** (like C++ WindowContext)
- ✓ **Async GPU flushing** (`GrSyncCpu::kNo`)

The current implementation using `RenderDirect()` in a tight loop is **the correct approach** and should match C++ performance!
