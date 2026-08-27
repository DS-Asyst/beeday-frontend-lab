const scopes = new Map();
const stack = [];

const focusableSelector = [
    'a[href]',
    'button:not([disabled])',
    'input:not([disabled]):not([type="hidden"])',
    'select:not([disabled])',
    'textarea:not([disabled])',
    '[tabindex]:not([tabindex="-1"])'
].join(',');

function isVisible(element) {
    const style = window.getComputedStyle(element);
    return style.visibility !== 'hidden' && style.display !== 'none' && !element.closest('[inert]');
}

function focusableElements(container) {
    return [...container.querySelectorAll(focusableSelector)].filter(isVisible);
}

function focusTarget(state) {
    const { container, initialFocusSelector } = state;
    let target = initialFocusSelector === 'self'
        ? container
        : initialFocusSelector
            ? container.querySelector(initialFocusSelector)
            : null;

    if (!target || !isVisible(target) || target.disabled) {
        target = container.querySelector('[autofocus]') ?? focusableElements(container)[0] ?? container;
    }

    target.focus({ preventScroll: true });
}

function isTopScope(state) {
    return stack.at(-1) === state.id;
}

function handleKeyDown(state, event) {
    if (event.key !== 'Tab' || !isTopScope(state)) {
        return;
    }

    const focusables = focusableElements(state.container);
    if (focusables.length === 0) {
        event.preventDefault();
        state.container.focus({ preventScroll: true });
        return;
    }

    const first = focusables[0];
    const last = focusables.at(-1);
    const active = document.activeElement;

    if (event.shiftKey && (active === first || !state.container.contains(active))) {
        event.preventDefault();
        last.focus({ preventScroll: true });
    } else if (!event.shiftKey && (active === last || !state.container.contains(active))) {
        event.preventDefault();
        first.focus({ preventScroll: true });
    }
}

function handleFocusIn(state, event) {
    if (isTopScope(state) && !state.container.contains(event.target)) {
        focusTarget(state);
    }
}

export function activate(id, initialFocusSelector, fallbackFocusSelector) {
    const container = document.getElementById(id);
    if (!container) {
        return false;
    }

    if (scopes.has(id)) {
        return true;
    }

    const previouslyFocused = document.activeElement instanceof HTMLElement
        ? document.activeElement
        : null;
    const state = { id, container, initialFocusSelector, fallbackFocusSelector, previouslyFocused };
    state.keyDownHandler = event => handleKeyDown(state, event);
    state.focusInHandler = event => handleFocusIn(state, event);

    container.addEventListener('keydown', state.keyDownHandler);
    document.addEventListener('focusin', state.focusInHandler, true);
    scopes.set(id, state);
    stack.push(id);

    queueMicrotask(() => {
        if (scopes.has(id) && isTopScope(state)) {
            focusTarget(state);
        }
    });

    return true;
}

export function focusFirstInvalid(id) {
    queueMicrotask(() => {
        const container = document.getElementById(id);
        const target = container?.querySelector('.invalid, [aria-invalid="true"]');
        target?.focus({ preventScroll: true });
    });
}

export function deactivate(id) {
    const state = scopes.get(id);
    if (!state) {
        return;
    }

    state.container.removeEventListener('keydown', state.keyDownHandler);
    document.removeEventListener('focusin', state.focusInHandler, true);
    scopes.delete(id);

    const stackIndex = stack.lastIndexOf(id);
    if (stackIndex >= 0) {
        stack.splice(stackIndex, 1);
    }

    if (state.previouslyFocused?.isConnected) {
        state.previouslyFocused.focus({ preventScroll: true });
        return;
    }

    const parentScope = scopes.get(stack.at(-1));
    if (parentScope) {
        focusTarget(parentScope);
        return;
    }

    // EXP32-F002 (Sprint 32.13): the real trigger for a dialog opened from a menu item (e.g. "+
    // Activity" -> "Habit") is already unmounted by the time this runs, because selecting the item
    // both closes the menu and opens the dialog - previouslyFocused is never connected again, and
    // there is no parent scope either, so focus would otherwise silently fall back to <body>. A
    // caller-supplied fallback (typically the menu's own persistent trigger button, which survives
    // the menu closing) recovers a sane keyboard position instead of losing it entirely.
    const fallback = state.fallbackFocusSelector ? document.querySelector(state.fallbackFocusSelector) : null;
    if (fallback && isVisible(fallback)) {
        fallback.focus({ preventScroll: true });
    }
}
