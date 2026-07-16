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

    return { observe, unobserve };
})();
