window.onload = () => {
    const canvas = document.getElementsByClassName("screen")[0];
    const offscreen = canvas.transferControlToOffscreen();
    const workers = [];
    
    const COUNT = "%{SOCKET_COUNT}%";
    // const buffer = new Uint8Array(new SharedArrayBuffer(10));
    for(let i = 0; i < COUNT; i++) {
        workers[i] = new Worker("socket.js");
        workers[i].onmessage = receiveMessage(workers, workers[i]);
        workers[i].postMessage({ type: "socket", value: i });
        // workers[i].postMessage({ type: "buffer", value: buffer });
    }
    workers[0].postMessage({ type: "canvas", value: offscreen }, [offscreen]);

    document.getElementsByClassName("login-btn")[0].addEventListener("click", e => {
        for(const worker of workers) {
            if (!worker.connected) {
                alert("Server is not connected. Please wait.");
                return;
            }
        }
        const buf = new ByteBuf();
        buf.writeString(document.getElementsByClassName("input-password")[0].value);
        const packet = buf.flush();
        for(const worker of workers) {
            worker.postMessage({ type: "packet", value: packet });
        }
        buf.writeVarInt(screen.availWidth);
        buf.writeVarInt(screen.availHeight);
        workers[0].postMessage({ type: "packet", value: buf.flush() });
        document.body.setAttribute("connected", true);
    });

    document.addEventListener("keydown", e => {
        if (!document.body.hasAttribute("connected")) return;
        e.preventDefault();
        e.stopPropagation();
        const buf = new ByteBuf();
        buf.writeVarInt(0);
        buf.writeVarInt(e.which || e.keyCode);
        // buf.writeString(e.code);
        // buf.writeString(e.key);
        workers[0].postMessage({ type: "packet", value: buf.flush() });
    });

    document.addEventListener("keyup", e => {
        if (!document.body.hasAttribute("connected")) return;
        e.preventDefault();
        e.stopPropagation();
        const buf = new ByteBuf();
        buf.writeVarInt(1);
        buf.writeVarInt(e.which || e.keyCode);
        // buf.writeString(e.code);
        // buf.writeString(e.key);
        workers[0].postMessage({ type: "packet", value: buf.flush() });
    });

    canvas.addEventListener("contextmenu", e => e.preventDefault());

    canvas.addEventListener("mousemove", e => {
        if (!document.body.hasAttribute("connected")) return;
        const x = Math.max(0, Math.min(canvas.width, e.offsetX));
        const y = Math.max(0, Math.min(canvas.height, e.offsetY));
        const buf = new ByteBuf();
        buf.writeVarInt(2);
        buf.writeVarInt(x);
        buf.writeVarInt(y);
        workers[0].postMessage({ type: "packet", value: buf.flush() });
    });

    canvas.addEventListener("mousedown", e => {
        if (!document.body.hasAttribute("connected")) return;
        const x = Math.max(0, Math.min(canvas.width, e.offsetX));
        const y = Math.max(0, Math.min(canvas.height, e.offsetY));
        const buf = new ByteBuf();
        buf.writeVarInt(3);
        buf.writeVarInt(x);
        buf.writeVarInt(y);
        buf.writeVarInt(e.button);
        workers[0].postMessage({ type: "packet", value: buf.flush() });
        e.preventDefault();
    });

    canvas.addEventListener("mouseup", e => {
        if (!document.body.hasAttribute("connected")) return;
        const x = Math.max(0, Math.min(canvas.width, e.offsetX));
        const y = Math.max(0, Math.min(canvas.height, e.offsetY));
        const buf = new ByteBuf();
        buf.writeVarInt(4);
        buf.writeVarInt(x);
        buf.writeVarInt(y);
        buf.writeVarInt(e.button);
        workers[0].postMessage({ type: "packet", value: buf.flush() });
        e.preventDefault();
    });

    canvas.addEventListener("wheel", e => {
        if (!document.body.hasAttribute("connected")) return;
        const x = Math.max(0, Math.min(canvas.width, e.offsetX));
        const y = Math.max(0, Math.min(canvas.height, e.offsetY));
        const buf = new ByteBuf();
        buf.writeVarInt(5);
        buf.writeVarInt(x);
        buf.writeVarInt(y);
        buf.writeVarInt(e.wheelDelta);
        workers[0].postMessage({ type: "packet", value: buf.flush() });
        e.preventDefault();
    });
}

let audioContext, audioWorklet;
let soundPlaying = false;
const receiveMessage = (workers, worker) => async e => {
    switch (e.data.type) {
        case 0:
            worker.connected = true;
            break;
        case 1:
            alert("Disconnected");
            break;
        case 2:
            if(location.protocol !== "https:") break;
            if (!audioContext) {
                audioContext = new AudioContext({ sampleRate: e.data.value.sampleRate });
                await audioContext.audioWorklet.addModule("audio-processor.js");
                audioWorklet = new AudioWorkletNode(audioContext, "audio-processor", { outputChannelCount: [e.data.value.channels] });
                audioWorklet.connect(audioContext.destination);
                audioContext.resume();
            }
            audioWorklet?.port?.postMessage(e.data.value);
            break;
        case 3:
            const canvas = document.getElementsByClassName("screen")[0];
            canvas.setAttribute("cursor", e.data.value);
            break;
        case 4:
            for(const worker of workers) {
                worker.postMessage({type: "buffer", value: e.data.value});
            }
            break;
    }
};