// wwwroot/js/serialInterop.js
// Envuelve la Web Serial API para que Blazor WASM hable con el Arduino por USB,
// usando un protocolo de líneas JSON: cada mensaje es un string JSON terminado en '\n'.
// Requiere Chrome/Edge (desktop o Android). No funciona en Firefox/Safari.

let port = null;
let reader = null;
let writer = null;
let keepReading = false;
const decoder = new TextDecoder();
const encoder = new TextEncoder();
let lineBuffer = "";

export function isSupported() {
    return "serial" in navigator;
}

// Abre el selector nativo del navegador para elegir el puerto.
// OJO: el navegador exige que esto se dispare desde un gesto de usuario
// (ej. el OnClick de un botón), si no, requestPort() falla silenciosamente.
export async function requestAndOpen(baudRate, dotNetRef) {
    if (!("serial" in navigator)) {
        throw new Error("Este navegador no soporta Web Serial API (usá Chrome/Edge).");
    }

    port = await navigator.serial.requestPort();
    await port.open({ baudRate: baudRate });

    writer = port.writable.getWriter();
    keepReading = true;
    lineBuffer = "";
    readLoop(dotNetRef); // sin await: corre en paralelo, en segundo plano
    return true;
}

async function readLoop(dotNetRef) {
    reader = port.readable.getReader();
    try {
        while (keepReading) {
            const { value, done } = await reader.read();
            if (done) break;
            if (value && value.length > 0) {
                // Acumulamos texto porque un mensaje puede llegar partido en
                // varias lecturas, o varios mensajes pueden llegar juntos.
                lineBuffer += decoder.decode(value, { stream: true });

                let newlineIndex;
                while ((newlineIndex = lineBuffer.indexOf("\n")) >= 0) {
                    const line = lineBuffer.slice(0, newlineIndex).trim();
                    lineBuffer = lineBuffer.slice(newlineIndex + 1);
                    if (line.length > 0) {
                        await dotNetRef.invokeMethodAsync("OnSerialLineReceived", line);
                    }
                }
            }
        }
    } catch (err) {
        await dotNetRef.invokeMethodAsync("OnSerialError", err.message);
    } finally {
        reader.releaseLock();
    }
}

// jsonString: un JSON ya serializado, ej '{"hexIdx":7,"resource":"trigo"}'
export async function sendJson(jsonString) {
    if (!writer) throw new Error("El puerto no está abierto todavía.");
    await writer.write(encoder.encode(jsonString + "\n"));
}

export async function close() {
    keepReading = false;
    try {
        if (reader) await reader.cancel();
        if (writer) writer.releaseLock();
        if (port) await port.close();
    } finally {
        port = null;
        reader = null;
        writer = null;
        lineBuffer = "";
    }
}

// // ---- Modo simulación: para probar el pipeline completo sin el Arduino a mano ----

// let simTimer = null;

// export function startSimulation(dotNetRef, intervalMs) {
//     stopSimulation();
//     simTimer = setInterval(() => {
//         const json = buildFakeJson();
//         dotNetRef.invokeMethodAsync("OnSerialLineReceived", json);
//     }, intervalMs);
// }

// export function stopSimulation() {
//     if (simTimer) {
//         clearInterval(simTimer);
//         simTimer = null;
//     }
// }

// // Manda UN mensaje simulado puntual (el JSON que vos le pases), sin esperar el timer.
// export function sendSimulatedMessage(dotNetRef, jsonString) {
//     dotNetRef.invokeMethodAsync("OnSerialLineReceived", jsonString);
// }

// function buildFakeJson() {
//     const hexIdx = Math.floor(Math.random() * 19);
//     const recursos = ["madera", "ladrillo", "oveja", "trigo", "piedra"];
//     const resource = recursos[Math.floor(Math.random() * recursos.length)];
//     return JSON.stringify({ hexIdx, resource });
// }
