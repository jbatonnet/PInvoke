using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

using Utf8Json;

namespace PInvoke.Server
{
    public enum JsonTokenType
    {
        StartObject,
        EndObject,
        StartArray,
        EndArray,
        PropertyName,
        String,
        Boolean,
        Number,
        Null
    }

    public class FastJsonStreamReader : IDisposable
    {
        public JsonTokenType TokenType => this.tokenType;
        public long Position
        {
            get
            {
                long offset = this.jsonReader.GetCurrentOffsetUnsafe();
                return this.bufferOffset + offset;
            }
        }
        public int Depth => this.tokenStack.Count;

        private readonly Stream stream;
        private JsonReader jsonReader;

        // Buffer
        private readonly int tokenSize;
        private readonly int bufferSize;

        private long bufferOffset = -1;
        private byte[] buffer;

        // State
        private Stack<JsonTokenType> tokenStack = new Stack<JsonTokenType>();
        private JsonToken previousToken;
        private JsonTokenType tokenType;

        public FastJsonStreamReader(Stream stream, int bufferSize = 128 * 1024)
        {
            this.stream = stream;
            this.bufferSize = bufferSize;

            this.buffer = new byte[this.bufferSize * 2];

            this.Reset();
        }

        public void Reset() => this.Seek(0);
        public void Seek(long offset)
        {
            if (this.bufferOffset != -1 && offset == this.Position)
            {
                return;
            }

            long bufferOffset = offset - (offset % this.bufferSize);

            if (this.stream.Position != bufferOffset)
            {
                this.stream.Seek(bufferOffset, SeekOrigin.Begin);
            }

            this.stream.Read(this.buffer, 0, this.bufferSize * 2);

            this.bufferOffset = bufferOffset;

            this.jsonReader = new JsonReader(this.buffer, (int)(offset - bufferOffset));

            this.tokenStack.Clear();

            JsonToken token = this.jsonReader.GetCurrentJsonToken();
            if (token != JsonToken.BeginObject)
            {
                throw new Exception("Must seek to a StartObject token");
            }

            this.ProcessToken();
        }

        public void Read()
        {
            this.jsonReader.ReadNext();

            this.ProcessToken();
            this.CheckBuffer();
        }
        public void Skip()
        {
            this.Skip(this.Depth);
        }
        public void Skip(int depth)
        {
            if (this.Depth < depth)
            {
                return;
            }

            do
            {
                JsonToken token;

                while (true)
                {
                    this.jsonReader.ReadNext();
                    token = this.jsonReader.GetCurrentJsonToken();

                    switch (token)
                    {
                        case JsonToken.BeginObject: this.tokenStack.Push(JsonTokenType.StartObject); break;
                        case JsonToken.EndObject: this.tokenStack.Pop(); break;
                        case JsonToken.BeginArray: this.tokenStack.Push(JsonTokenType.StartArray); break;
                        case JsonToken.EndArray: this.tokenStack.Pop(); break;

                        default: continue;
                    }

                    break;
                }

                this.CheckBuffer();

                this.previousToken = token;
            }
            while (this.Depth >= depth);

            this.Read();
        }

        public bool ReadBoolean()
        {
            bool result = this.jsonReader.ReadBoolean();

            this.ProcessToken();
            this.CheckBuffer();

            return result;
        }
        public double ReadDouble()
        {
            // The base ReadDouble method fails with other cultures
            ArraySegment<byte> numberSegment = this.jsonReader.ReadNumberSegment();
            string numberText = Encoding.ASCII.GetString(numberSegment.Array, numberSegment.Offset, numberSegment.Count);

            double result = double.Parse(numberText, CultureInfo.InvariantCulture);

            this.ProcessToken();
            this.CheckBuffer();

            return result;
        }
        public string ReadString()
        {
            string result = this.jsonReader.ReadString();

            this.ProcessToken();
            this.CheckBuffer();

            return result;
        }

        private void ProcessToken()
        {
            JsonToken token = this.jsonReader.GetCurrentJsonToken();

            if (token == JsonToken.NameSeparator || token == JsonToken.ValueSeparator)
            {
                this.previousToken = token;
                this.jsonReader.ReadNext();
                token = this.jsonReader.GetCurrentJsonToken();
            }

            switch (token)
            {
                case JsonToken.BeginObject:
                    this.tokenStack.Push(JsonTokenType.StartObject);
                    this.tokenType = JsonTokenType.StartObject;
                    break;

                case JsonToken.EndObject:
                    this.tokenStack.Pop();
                    this.tokenType = JsonTokenType.EndObject;
                    break;

                case JsonToken.BeginArray:
                    this.tokenStack.Push(JsonTokenType.StartArray);
                    this.tokenType = JsonTokenType.StartArray;
                    break;

                case JsonToken.EndArray:
                    this.tokenStack.Pop();
                    this.tokenType = JsonTokenType.EndArray;
                    break;

                case JsonToken.String:
                    if (this.tokenStack.Peek() == JsonTokenType.StartObject && (this.previousToken == JsonToken.BeginObject || this.previousToken == JsonToken.ValueSeparator))
                    {
                        this.tokenType = JsonTokenType.PropertyName;
                    }
                    else
                    {
                        this.tokenType = JsonTokenType.String;
                    }
                    break;

                case JsonToken.True:
                case JsonToken.False:
                    this.tokenType = JsonTokenType.Boolean;
                    break;

                case JsonToken.Null:
                    this.tokenType = JsonTokenType.Null;
                    break;

                case JsonToken.Number:
                    this.tokenType = JsonTokenType.Number;
                    break;
            }

            this.previousToken = token;
        }
        private void CheckBuffer()
        {
            long offset = this.jsonReader.GetCurrentOffsetUnsafe();

            if (offset < this.bufferSize)
            {
                return;
            }

            Array.Copy(this.buffer, this.bufferSize, this.buffer, 0, this.bufferSize);

            this.stream.Read(this.buffer, this.bufferSize, this.bufferSize);

            this.bufferOffset += this.bufferSize;
            this.jsonReader = new JsonReader(this.buffer, (int)(offset - this.bufferSize));
        }

        public void Dispose()
        {
        }
    }
}
