importScripts("./bytebuf.js");
const ws = [];
const COUNT = %{SOCKET_COUNT}%;
const PATH = (location.protocol.startsWith("https") ? "wss://" : "ws://") + location.host;
for(let i = 0; i < COUNT; i++) {
    const socket = ws[i] = new WebSocket(`${PATH}/${i}`);
    socket.onclose = () => postMessage({type: 1});
    socket.onopen = checkOpenFinished;
    socket.onerror = e => console.log(e);
    socket.onmessage = receiveMessage;
}

function checkOpenFinished() {
    for(let i = 0; i < COUNT; i++) {
        if(!ws[i] || ws[i].readyState != 1) return;
    }
    postMessage({type: 0});
}

let canvas, context2d, chunkType;

self.onmessage = e => {
    const data = e.data;
    switch(data.type) {
        case "canvas":
            canvas = data.value;
            context2d = canvas.getContext("2d");
            break;
        case "packet":
            ws[0].send(data.value);
            break;
        case "br":
            for(let i = 0; i < COUNT; i++)
                ws[i].send(data.value);
            break;
    }
};

async function receiveMessage(e) {
    const buf = new ByteBuf(new Uint8Array(await e.data.arrayBuffer()));
    // while(buf.readableLength > 0) {
    try {
        await receivePacket(buf);
    }catch(e) {
        console.error(e);
    }
    // }
};

async function receivePacket(packet) {
    switch(packet.readByte()) {
        case 0:
            chunkType = packet.readByte();
            console.log(chunkType);
            break;
        case 1:
            await receiveFullScreen(packet);
            break;
        case 2:
            await receiveScreenChunk(packet);
            break;
        case 3:
            receiveCursorType(packet);
            break;
        case 4:
            receiveAudio(packet);
            break;
    }
}

function receiveAudio(packet) {
    const channels = packet.readByte();
    const sampleRate = packet.readVarInt();
    const bitsPerSample = packet.readVarInt();
    const bitMax = 1 << bitsPerSample;
    const divMax = bitMax / 2;
    const bytePerSample = bitsPerSample >> 3;
    const amount = Math.floor(packet.readableLength / channels / bytePerSample);
    const data = [];
    for (let i = 0; i < channels; i++) data[i] = new Float32Array(amount);
    for (let i = 0; i < amount; i++) {
        for (let channel = 0; channel < channels; channel++) {
            const v = packet.readInt(bytePerSample);
            data[channel][i] = (((v + divMax) % bitMax) - divMax) / divMax;
        }
    }
    postMessage({ type: 2, value: { channels, sampleRate, chunk: data } });
}

function receiveCursorType(packet) {
    switch(packet.readVarInt()) {
        case 0: postMessage({type: 3, value: "none"}); break;
        case 65539: postMessage({type: 3, value: "default"}); break;
        case 65541: postMessage({type: 3, value: "text"}); break;
        case 65543: postMessage({type: 3, value: "wait"}); break;
        case 65545: postMessage({type: 3, value: "crosshair"}); break;
        case 65549: postMessage({type: 3, value: "nwse-resize"}); break;
        case 65551: postMessage({type: 3, value: "nesw-resize"}); break;
        case 65553: postMessage({type: 3, value: "ew-resize"}); break;
        case 65555: postMessage({type: 3, value: "ns-resize"}); break;
        case 65557: postMessage({type: 3, value: "move"}); break;
        case 65559: postMessage({type: 3, value: "not-allowed"}); break;
        case 65561: postMessage({type: 3, value: "progress"}); break;
        case 65563: postMessage({type: 3, value: "pointer"}); break;
        case 13896596: postMessage({type: 3, value: "grabbing"}); break;
        case 31327887: postMessage({type: 3, value: "alias"}); break;
        case 32770565: postMessage({type: 3, value: "col-resize"}); break;
        case 38668561: postMessage({type: 3, value: "vertical-text"}); break;
        case 62917193: postMessage({type: 3, value: "zoom-in"}); break;
        case 64882867: postMessage({type: 3, value: "cell"}); break;
        case 69339379: postMessage({type: 3, value: "grab"}); break;
        case 85000401: postMessage({type: 3, value: "row-resize"}); break;
        case 132646983: postMessage({type: 3, value: "copy"}); break;
        case 186320971: postMessage({type: 3, value: "zoom-out"}); break;
    }
}

async function receiveScreenChunk(packet) {
    const compress = packet.readByte();
    const pixels = packet.readUint8Array();
    switch(compress) {
        case 1:
            await drawCompressChunk(pixels, packet);
            break;
        default:
            await drawRawChunk(pixels, packet);
            break;
    }
}

function setImageData(imageData, pixels) {
    let idx = 0;
    const w = imageData.width << 2;
    for(let offset = 0; offset < w; offset += 4) {
        let r, g, b;
        switch(chunkType) {
            case 0:
                b = pixels[idx++];
                g = pixels[idx++];
                r = pixels[idx++];
                break;
            case 1:
                r = pixels[idx + 1] >> 3;
                g = ((pixels[idx + 1] & 0b111) << 3) | (pixels[idx] >> 5);
                b = pixels[idx] & 0b11111;
                // r = pixels[idx] >> 3;
                // g = (pixels[idx] & 0b111) | (pixels[idx + 1] >> 5);
                // b = pixels[idx + 1] & 0b11111;
                b = (b * 527 + 23) >> 6;
                g = (g * 259 + 33) >> 6;
                r = (r * 527 + 23) >> 6;
                // b = pixels[idx] >> 3 << 3;
                // g = ((pixels[idx] & 7) << 5) | (pixels[idx + 1] >> 5 << 2);
                // r = (pixels[idx + 1] & 31) << 3;
                idx += 2;
                break;
            case 2:
                b = Math.round((pixels[idx] >> 5) * 36.42857142857);
                g = Math.round(((pixels[idx] >> 2) & 7) * 36.42857142857);
                r = Math.round(((pixels[idx] << 1) & 7) * 36.42857142857);
                idx++;
                break;
        }
        imageData.data[offset] = r;
        imageData.data[offset + 1] = g;
        imageData.data[offset + 2] = b;
        imageData.data[offset + 3] = 255;
    }
    return idx;
}

async function drawRawChunk(pixels, packet) {
    const pixelPer = 3 - chunkType;
    const imageData = new ImageData(pixels.length / pixelPer, 1);
    setImageData(imageData, pixels);
    const image = await createImageBitmap(imageData);
    drawImageDataChunk(image, packet);
    image.close();
}

async function drawCompressChunk(pixels, packet) {
    const image = await createImageBitmap(new Blob([pixels]));
    drawImageDataChunk(image, packet);
    image.close();
}

function drawImageDataChunk(image, packet) {
    let idx = 0;
    while (packet.readableLength > 0) {
        const pos = packet.readVarInt();
        let length = packet.readVarInt();
        const dx = pos % canvas.width;
        let dy = Math.floor(pos / canvas.width);
        if (dx != 0) {
            const w = Math.min(canvas.width - dx, length);

            context2d.drawImage(image, idx, 0, w, 1, dx, dy, w, 1);
            idx += w;
            length -= w;
            dy++;
        }
        while (length) {
            const w = Math.min(length, canvas.width);
            context2d.drawImage(image, idx, 0, w, 1, 0, dy, w, 1);
            idx += w;
            length -= w;
            dy++;
        }
    }
}

async function receiveFullScreen(packet) {
    const image = await createImageBitmap(new Blob([packet.read(packet.readableLength)]));
    canvas.width = image.width;
    canvas.height = image.height;
    context2d.drawImage(image, 0, 0);
    image.close();
}