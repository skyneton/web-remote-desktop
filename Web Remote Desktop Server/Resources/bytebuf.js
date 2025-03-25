class ByteBuf {
    #readBuffer;
    #position = 0;
    #buffer = [];
    #encoder = new TextEncoder("utf-8");
    #decoder = new TextDecoder("utf-8");

    constructor(readBuffer = undefined) {
        if (readBuffer instanceof ArrayBuffer) {
            this.#readBuffer = new Uint8Array(readBuffer);
        } else if (readBuffer instanceof Uint8Array || typeof readBuffer === 'undefined' || typeof readBuffer === 'string') {
            this.#readBuffer = readBuffer;
        }
    }

    clear() {
        this.#buffer = [];
    }

    flush() {
        const arr = new Uint8Array(this.#buffer);
        this.#buffer = [];
        return arr;
    }

    toString() {
        return String.fromCharCode.apply(this, this.#buffer);
    }

    writeVarInt(int) {
        while((int & -128) != 0) {
            this.#buffer.push(int & 127 | 128);
            int >>>= 7;
        }

        this.#buffer.push(int);
    }

    writeString(str) {
        const data = this.#encoder.encode(str);
        this.writeUint8Array(data);
    }

    writeUint8Array(array) {
        this.writeVarInt(array.length);
        Array.prototype.push.apply(this.#buffer, array);
    }

    write(array) {
        Array.prototype.push.apply(this.#buffer, array);
    }

    writeBool(value) {
        this.#buffer.push(value ? 1 : 0);
    }

    writeByte(value) {
        this.#buffer.push(value & 255);
    }

    readByte() {
        if(typeof this.#readBuffer === 'string')
            return this.#readBuffer.charCodeAt(this.#position++);
        return this.#readBuffer[this.#position++];
    }

    read(count) {
        const array = new Uint8Array(count);
        for(let i = 0; i < count; i++)
            array[i] = this.readByte();
        return array
    }

    readUint8Array() {
        return this.read(this.readVarInt());
    }

    readVarInt() {
        let value = 0;
        let byteLength = 0;
        let byte;
        while(((byte = this.readByte()) & 0x80) == 0x80) {
            value |= (byte & 0x7F) << byteLength++ * 7;
            if(byteLength > 5) throw Error("VarInt too long.");
        }
        return value | (byte & 0x7F) << byteLength * 7;
    }

    readInt(amount) {
        let value = 0;
        for(let i = 0; i < amount; i++) {
            const byte = this.readByte();
            value |= byte << i * 8;
        }
        return value;
    }

    readString() {
        return this.#decoder.decode(this.read(this.readVarInt()))
    }

    readBool() {
        return !!this.readByte();
    }

    get length() {
        return this.#buffer.length;
    }

    get readableLength() {
        return this.#readBuffer.length - this.#position;
    }

    get buf() {
        return this.#buffer;
    }
}