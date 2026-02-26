
type DotNetRef = {
	invokeMethodAsync(name: string, data: object): void;
}

const enum SKTouchAction {
	Entered = 0,
	Pressed = 1,
	Moved = 2,
	Released = 3,
	Cancelled = 4,
	Exited = 5,
	WheelChanged = 6,
}

const enum SKTouchDeviceType {
	Touch = 0,
	Mouse = 1,
	Pen = 2,
}

const enum SKMouseButton {
	Unknown = 0,
	Left = 1,
	Middle = 2,
	Right = 3,
}

const activeElements = new Map<HTMLElement, DotNetRef>();

export function initializeTouchEvents(element: HTMLElement, dotNetRef: DotNetRef): void {
	if (!element || !dotNetRef) return;

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

export function disposeTouchEvents(element: HTMLElement): void {
	if (!element) return;

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

function onPointerDown(e: PointerEvent): void {
	sendPointerEvent(e, SKTouchAction.Pressed);
	try { (e.currentTarget as HTMLElement).setPointerCapture(e.pointerId); } catch { /* ignore */ }
}

function onPointerMove(e: PointerEvent): void {
	sendPointerEvent(e, SKTouchAction.Moved);
}

function onPointerUp(e: PointerEvent): void {
	sendPointerEvent(e, SKTouchAction.Released);
}

function onPointerCancel(e: PointerEvent): void {
	sendPointerEvent(e, SKTouchAction.Cancelled);
}

function onPointerEnter(e: PointerEvent): void {
	sendPointerEvent(e, SKTouchAction.Entered);
}

function onPointerLeave(e: PointerEvent): void {
	sendPointerEvent(e, SKTouchAction.Exited);
}

function onWheel(e: WheelEvent): void {
	const ref = activeElements.get(e.currentTarget as HTMLElement);
	if (!ref) return;

	const rect = (e.currentTarget as HTMLElement).getBoundingClientRect();
	const x = e.clientX - rect.left;
	const y = e.clientY - rect.top;

	const delta = e.deltaMode === 0
		? Math.round(-e.deltaY / 10)
		: (e.deltaY < 0 ? 1 : -1);

	ref.invokeMethodAsync("OnPointerEvent", {
		id: -1,
		action: SKTouchAction.WheelChanged,
		deviceType: SKTouchDeviceType.Mouse,
		mouseButton: SKMouseButton.Unknown,
		x,
		y,
		pressure: 0,
		inContact: false,
		wheelDelta: delta,
	});

	e.preventDefault();
}

function getDeviceType(pointerType: string): SKTouchDeviceType {
	switch (pointerType) {
		case "mouse": return SKTouchDeviceType.Mouse;
		case "pen": return SKTouchDeviceType.Pen;
		default: return SKTouchDeviceType.Touch;
	}
}

function getMouseButton(button: number): SKMouseButton {
	switch (button) {
		case 0: return SKMouseButton.Left;
		case 1: return SKMouseButton.Middle;
		case 2: return SKMouseButton.Right;
		default: return SKMouseButton.Unknown;
	}
}

function sendPointerEvent(e: PointerEvent, action: SKTouchAction): void {
	const ref = activeElements.get(e.currentTarget as HTMLElement);
	if (!ref) return;

	const rect = (e.currentTarget as HTMLElement).getBoundingClientRect();
	const x = e.clientX - rect.left;
	const y = e.clientY - rect.top;
	const deviceType = getDeviceType(e.pointerType);
	const mouseButton = getMouseButton(e.button);
	const inContact = e.buttons !== 0 || action === SKTouchAction.Pressed;

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
