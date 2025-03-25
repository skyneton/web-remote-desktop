class ChunkAudioProcessor extends AudioWorkletProcessor {
    channels = 0;
    buffer;
    constructor() {
        super();
        this.port.onmessage = this.handleMessage.bind(this);
    }

    handleMessage(e) {
        const newChannels = e.data.channels;
        if (this.channels != newChannels) {
            this.channels = newChannels;
            this.buffer = [];
            for (let i = 0; i < newChannels; i++) {
                this.buffer[i] = new Float32Array(0);
            }
        }
        const chunk = e.data.chunk;
        for (let i = 0; i < newChannels; i++) {
            const newBuffer = new Float32Array(this.buffer[i].length + chunk[i].length);
            newBuffer.set(this.buffer[i]);
            newBuffer.set(chunk[i], this.buffer[i].length);
            this.buffer[i] = newBuffer;
        }
    }

    process(inputList, outputList) {
        const output = outputList[0];
        const channelLength = Math.min(output.length, this.channels);
        for (let i = 0; i < channelLength; i++) {
            const buffering = output[i];
            const length = buffering.length;
            if (length > this.buffer[i].length) {
                buffering.fill(0, length);
                continue;
            }
            buffering.set(this.buffer[i].subarray(0, length));
            this.buffer[i] = this.buffer[i].subarray(length);
        }
        return true;
    }
}

registerProcessor("audio-processor", ChunkAudioProcessor);