export class SKTouchInterop {
    static start(element, elementId, callback) {
        if ((!element && !elementId) || !callback)
            return;
        SKTouchInterop.init();
        element = element || document.querySelector('[' + elementId + ']');
        const touchElement = element;
        touchElement.SKTouchInterop = {
            callback: callback,
        };
        element.style.touchAction = "none";
        element.style.userSelect = "none";
        element.addEventListener("pointerdown", SKTouchInterop.onPointerDown);
        element.addEventListener("pointermove", SKTouchInterop.onPointerMove);
        element.addEventListener("pointerup", SKTouchInterop.onPointerUp);
        element.addEventListener("pointercancel", SKTouchInterop.onPointerCancel);
        element.addEventListener("pointerenter", SKTouchInterop.onPointerEnter);
        element.addEventListener("pointerleave", SKTouchInterop.onPointerLeave);
        element.addEventListener("wheel", SKTouchInterop.onWheel, { passive: false });
        SKTouchInterop.elements.set(elementId, element);
    }
    static stop(elementId) {
        if (!elementId || !SKTouchInterop.elements)
            return;
        const element = SKTouchInterop.elements.get(elementId);
        if (!element)
            return;
        SKTouchInterop.elements.delete(elementId);
        element.style.touchAction = "";
        element.style.userSelect = "";
        element.removeEventListener("pointerdown", SKTouchInterop.onPointerDown);
        element.removeEventListener("pointermove", SKTouchInterop.onPointerMove);
        element.removeEventListener("pointerup", SKTouchInterop.onPointerUp);
        element.removeEventListener("pointercancel", SKTouchInterop.onPointerCancel);
        element.removeEventListener("pointerenter", SKTouchInterop.onPointerEnter);
        element.removeEventListener("pointerleave", SKTouchInterop.onPointerLeave);
        element.removeEventListener("wheel", SKTouchInterop.onWheel);
    }
    static init() {
        if (SKTouchInterop.elements)
            return;
        SKTouchInterop.elements = new Map();
    }
    static onPointerDown(e) {
        SKTouchInterop.sendPointerEvent(e, 1 /* SKTouchAction.Pressed */);
        try {
            e.currentTarget.setPointerCapture(e.pointerId);
        }
        catch (_a) {
            /* ignore */
        }
    }
    static onPointerMove(e) {
        SKTouchInterop.sendPointerEvent(e, 2 /* SKTouchAction.Moved */);
    }
    static onPointerUp(e) {
        SKTouchInterop.sendPointerEvent(e, 3 /* SKTouchAction.Released */);
    }
    static onPointerCancel(e) {
        SKTouchInterop.sendPointerEvent(e, 4 /* SKTouchAction.Cancelled */);
    }
    static onPointerEnter(e) {
        SKTouchInterop.sendPointerEvent(e, 0 /* SKTouchAction.Entered */);
    }
    static onPointerLeave(e) {
        SKTouchInterop.sendPointerEvent(e, 5 /* SKTouchAction.Exited */);
    }
    static onWheel(e) {
        const touchElement = e.currentTarget;
        if (!touchElement || !touchElement.SKTouchInterop)
            return;
        const instance = touchElement.SKTouchInterop;
        const rect = touchElement.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;
        const delta = e.deltaMode === 0
            ? Math.round(-e.deltaY / 10)
            : (e.deltaY < 0 ? 1 : -1);
        const data = {
            id: -1,
            action: 6 /* SKTouchAction.WheelChanged */,
            deviceType: 1 /* SKTouchDeviceType.Mouse */,
            mouseButton: 0 /* SKMouseButton.Unknown */,
            x,
            y,
            pressure: 0,
            inContact: false,
            wheelDelta: delta,
        };
        SKTouchInterop.invokeCallback(instance.callback, data);
        e.preventDefault();
    }
    static getDeviceType(pointerType) {
        switch (pointerType) {
            case "mouse": return 1 /* SKTouchDeviceType.Mouse */;
            case "pen": return 2 /* SKTouchDeviceType.Pen */;
            default: return 0 /* SKTouchDeviceType.Touch */;
        }
    }
    static getMouseButton(button) {
        switch (button) {
            case 0: return 1 /* SKMouseButton.Left */;
            case 1: return 2 /* SKMouseButton.Middle */;
            case 2: return 3 /* SKMouseButton.Right */;
            default: return 0 /* SKMouseButton.Unknown */;
        }
    }
    static sendPointerEvent(e, action) {
        const touchElement = e.currentTarget;
        if (!touchElement || !touchElement.SKTouchInterop)
            return;
        const instance = touchElement.SKTouchInterop;
        const rect = touchElement.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;
        const deviceType = SKTouchInterop.getDeviceType(e.pointerType);
        const mouseButton = SKTouchInterop.getMouseButton(e.button);
        const inContact = e.buttons !== 0 || action === 1 /* SKTouchAction.Pressed */;
        const data = {
            id: e.pointerId,
            action,
            deviceType,
            mouseButton,
            x,
            y,
            pressure: e.pressure,
            inContact,
            wheelDelta: 0,
        };
        SKTouchInterop.invokeCallback(instance.callback, data);
    }
    static invokeCallback(callback, data) {
        if (typeof callback === 'function') {
            callback(data);
        }
        else {
            callback.invokeMethod('OnPointerEvent', data);
        }
    }
}
//# sourceMappingURL=SKTouchInterop.js.map