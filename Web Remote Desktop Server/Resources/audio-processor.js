class ChunkAudioProcessor extends AudioWorkletProcessor {
    channels = 0;
    buffer;
    starting = false;
    constructor() {
        super();
        this.port.onmessage = this.handleMessage.bind(this);
    }

    handleMessage(e) {
        const newChannels = e.data.channels;
        if(this.channels != newChannels) {
            this.channels = newChannels;
            this.buffer = [];
            for(let i = 0; i < newChannels; i++) {
                this.buffer[i] = new Float32Array(0);
            }
        }
        const chunk = e.data.chunk;
        for(let i = 0; i < newChannels; i++) {
            const newBuffer = new Float32Array(this.buffer[i].length + chunk[i].length);
            newBuffer.set(this.buffer[i]);
            newBuffer.set(chunk[i], this.buffer[i].length);
            this.buffer[i] = newBuffer;
        }
        if(!this.starting && this.buffer[0].length > chunk[0].length * 3) {
            this.starting = true;
        }
    }

    process(inputList, outputList) {
        const output = outputList[0];
        const channelLength = Math.min(output.length, this.channels);
        for(let i = 0; i < channelLength; i++) {
            const buffering = output[i];
            const length = buffering.length;
            if(!this.starting) {
                buffering.fill(0, length);
                continue;
            }
            if(length > this.buffer[i].length) {
                buffering.fill(0, length);
                this.starting = false;
                continue;
            }
            buffering.set(this.buffer[i].subarray(0, length));
            this.buffer[i] = this.buffer[i].subarray(length);
        }
        return true;
    }
}

registerProcessor("audio-processor", ChunkAudioProcessor);