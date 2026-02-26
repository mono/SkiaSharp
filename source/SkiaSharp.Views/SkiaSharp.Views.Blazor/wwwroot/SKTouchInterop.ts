
type SKTouchCallback = DotNet.DotNetObjectReference | ((data: object) => void);

type SKTouchElement = {
	SKTouchInterop: SKTouchInstance;
} & HTMLElement

type SKTouchInstance = {
	callback: SKTouchCallback;
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

export class SKTouchInterop {
	static elements: Map<string, HTMLElement>;

	public static start(element: HTMLElement, elementId: string, callback: SKTouchCallback): void {
		if ((!element && !elementId) || !callback)
			return;

		SKTouchInterop.init();

		element = element || document.querySelector('[' + elementId + ']');

		const touchElement = element as SKTouchElement;
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

	public static stop(elementId: string): void {
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

		SKTouchInterop.elements = new Map<string, HTMLElement>();
	}

	static onPointerDown(e: PointerEvent): void {
		SKTouchInterop.sendPointerEvent(e, SKTouchAction.Pressed);
		try {
			(e.currentTarget as HTMLElement).setPointerCapture(e.pointerId);
		} catch {
			/* ignore */
		}
	}

	static onPointerMove(e: PointerEvent): void {
		SKTouchInterop.sendPointerEvent(e, SKTouchAction.Moved);
	}

	static onPointerUp(e: PointerEvent): void {
		SKTouchInterop.sendPointerEvent(e, SKTouchAction.Released);
	}

	static onPointerCancel(e: PointerEvent): void {
		SKTouchInterop.sendPointerEvent(e, SKTouchAction.Cancelled);
	}

	static onPointerEnter(e: PointerEvent): void {
		SKTouchInterop.sendPointerEvent(e, SKTouchAction.Entered);
	}

	static onPointerLeave(e: PointerEvent): void {
		SKTouchInterop.sendPointerEvent(e, SKTouchAction.Exited);
	}

	static onWheel(e: WheelEvent): void {
		const touchElement = (e.currentTarget as SKTouchElement);
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
			action: SKTouchAction.WheelChanged,
			deviceType: SKTouchDeviceType.Mouse,
			mouseButton: SKMouseButton.Unknown,
			x,
			y,
			pressure: 0,
			inContact: false,
			wheelDelta: delta,
		};

		SKTouchInterop.invokeCallback(instance.callback, data);
		e.preventDefault();
	}

	static getDeviceType(pointerType: string): SKTouchDeviceType {
		switch (pointerType) {
			case "mouse": return SKTouchDeviceType.Mouse;
			case "pen": return SKTouchDeviceType.Pen;
			default: return SKTouchDeviceType.Touch;
		}
	}

	static getMouseButton(button: number): SKMouseButton {
		switch (button) {
			case 0: return SKMouseButton.Left;
			case 1: return SKMouseButton.Middle;
			case 2: return SKMouseButton.Right;
			default: return SKMouseButton.Unknown;
		}
	}

	static sendPointerEvent(e: PointerEvent, action: SKTouchAction): void {
		const touchElement = (e.currentTarget as SKTouchElement);
		if (!touchElement || !touchElement.SKTouchInterop)
			return;

		const instance = touchElement.SKTouchInterop;
		const rect = touchElement.getBoundingClientRect();
		const x = e.clientX - rect.left;
		const y = e.clientY - rect.top;
		const deviceType = SKTouchInterop.getDeviceType(e.pointerType);
		const mouseButton = SKTouchInterop.getMouseButton(e.button);
		const inContact = e.buttons !== 0 || action === SKTouchAction.Pressed;

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

	static invokeCallback(callback: SKTouchCallback, data: object): void {
		if (typeof callback === 'function') {
			callback(data);
		} else {
			callback.invokeMethod('OnPointerEvent', data);
		}
	}
}
