const activeElements = new Map();
export function initializeTouchEvents(element, dotNetRef) {
    if (!element || !dotNetRef)
        return;
    activeElements.set(element, dotNetRef);
    element.style.touchAction = "none";
    element.style.userSelect = "none";
    element.addEventListener("pointerdown", onPointerDown);
    element.addEventListener("pointermove", onPointerMove);
    element.addEventListener("pointerup", onPointerUp);
    element.addEventListener("pointercancel", onPointerCancel);
    element.addEventListener("pointerenter", onPointerEnter);
    element.addEventListener("pointerleave", onPointerLeave);
    element.addEventListener("wheel", onWheel, { passive: false });
}
export function disposeTouchEvents(element) {
    if (!element)
        return;
    activeElements.delete(element);
    element.style.touchAction = "";
    element.style.userSelect = "";
    element.removeEventListener("pointerdown", onPointerDown);
    element.removeEventListener("pointermove", onPointerMove);
    element.removeEventListener("pointerup", onPointerUp);
    element.removeEventListener("pointercancel", onPointerCancel);
    element.removeEventListener("pointerenter", onPointerEnter);
    element.removeEventListener("pointerleave", onPointerLeave);
    element.removeEventListener("wheel", onWheel);
}
function onPointerDown(e) {
    sendPointerEvent(e, 1 /* SKTouchAction.Pressed */);
    try {
        e.currentTarget.setPointerCapture(e.pointerId);
    }
    catch ( /* ignore */_a) { /* ignore */ }
}
function onPointerMove(e) {
    sendPointerEvent(e, 2 /* SKTouchAction.Moved */);
}
function onPointerUp(e) {
    sendPointerEvent(e, 3 /* SKTouchAction.Released */);
}
function onPointerCancel(e) {
    sendPointerEvent(e, 4 /* SKTouchAction.Cancelled */);
}
function onPointerEnter(e) {
    sendPointerEvent(e, 0 /* SKTouchAction.Entered */);
}
function onPointerLeave(e) {
    sendPointerEvent(e, 5 /* SKTouchAction.Exited */);
}
function onWheel(e) {
    const ref = activeElements.get(e.currentTarget);
    if (!ref)
        return;
    const rect = e.currentTarget.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;
    const delta = e.deltaMode === 0
        ? Math.round(-e.deltaY / 10)
        : (e.deltaY < 0 ? 1 : -1);
    ref.invokeMethodAsync("OnPointerEvent", {
        id: -1,
        action: 6 /* SKTouchAction.WheelChanged */,
        deviceType: 1 /* SKTouchDeviceType.Mouse */,
        mouseButton: 0 /* SKMouseButton.Unknown */,
        x,
        y,
        pressure: 0,
        inContact: false,
        wheelDelta: delta,
    });
    e.preventDefault();
}
function getDeviceType(pointerType) {
    switch (pointerType) {
        case "mouse": return 1 /* SKTouchDeviceType.Mouse */;
        case "pen": return 2 /* SKTouchDeviceType.Pen */;
        default: return 0 /* SKTouchDeviceType.Touch */;
    }
}
function getMouseButton(button) {
    switch (button) {
        case 0: return 1 /* SKMouseButton.Left */;
        case 1: return 2 /* SKMouseButton.Middle */;
        case 2: return 3 /* SKMouseButton.Right */;
        default: return 0 /* SKMouseButton.Unknown */;
    }
}
function sendPointerEvent(e, action) {
    const ref = activeElements.get(e.currentTarget);
    if (!ref)
        return;
    const rect = e.currentTarget.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;
    const deviceType = getDeviceType(e.pointerType);
    const mouseButton = getMouseButton(e.button);
    const inContact = e.buttons !== 0 || action === 1 /* SKTouchAction.Pressed */;
    ref.invokeMethodAsync("OnPointerEvent", {
        id: e.pointerId,
        action,
        deviceType,
        mouseButton,
        x,
        y,
        pressure: e.pressure,
        inContact,
        wheelDelta: 0,
    });
}
//# sourceMappingURL=SKTouchInterop.js.map