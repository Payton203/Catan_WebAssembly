window.hexGridInterop = (function () {
    // Mapeamos cada elemento observado a su propio ResizeObserver,
    // así Dispose puede desconectar el correcto sin afectar otras instancias del componente.
    const observers = new WeakMap();

    function observe(element, dotnetRef) {
        if (!element) return;

        const observer = new ResizeObserver((entries) => {
            for (const entry of entries) {
                const width = entry.contentRect.width;
                dotnetRef.invokeMethodAsync('OnContainerResized', width);
            }
        });

        observer.observe(element);
        observers.set(element, observer);

        // disparo inicial inmediato, no hay que esperar a un resize real
        const initialWidth = element.getBoundingClientRect().width;
        dotnetRef.invokeMethodAsync('OnContainerResized', initialWidth);
    }

    function unobserve(element) {
        if (!element) return;
        const observer = observers.get(element);
        if (observer) {
            observer.disconnect();
            observers.delete(element);
        }
    }

    function observeParent(element, dotnetRef) {
        if (!element) return;
        const parent = element.parentElement.parentElement.parentElement; // catan-inner → catan-board → panel-tablero

        const observer = new ResizeObserver((entries) => {
            for (const entry of entries) {
                dotnetRef.invokeMethodAsync('OnContainerResized', entry.contentRect.width);
            }
        });

        observer.observe(parent);
        observers.set(element, observer);

        const initialWidth = parent.getBoundingClientRect().width;
        dotnetRef.invokeMethodAsync('OnContainerResized', initialWidth);
    }

    return { observe, unobserve, observeParent};
})();
