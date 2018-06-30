using System;
using System.IO;
using OpenTK;

namespace SharpCraft.util
{
    public class ByteBufferReader : IDisposable
    {
        private readonly MemoryStream _data;
        private readonly BinaryReader _reader;

        public int Code { get; }

        public ByteBufferReader(byte[] data)
        {
            _data = new MemoryStream(data);
            _reader = new BinaryReader(_data);

            Code = _reader.ReadInt32();
        }

        public int ReadInt32()
        {
            return _reader.ReadInt32();
        }

        public float ReadFloat()
        {
            return _reader.ReadSingle();
        }

        public Vector2 ReadVec2()
        {
            return new Vector2(ReadFloat(), ReadFloat());
        }

        public string ReadString()
        {
            return _reader.ReadString();
        }

        public bool ReadBoolean()
        {
            return _reader.ReadBoolean();
        }

        public Guid ReadGuid()
        {
            return Guid.Parse(ReadString());
        }

        public void Dispose()
        {
            _reader.Dispose();
            _data.Dispose();
        }
    }
}