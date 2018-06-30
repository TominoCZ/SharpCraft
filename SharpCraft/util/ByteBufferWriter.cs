using System;
using System.IO;
using OpenTK;

namespace SharpCraft.util
{
    public class ByteBufferWriter : IDisposable
    {
        private readonly MemoryStream _data;
        private readonly BinaryWriter _writer;

        public int Code { get; }

        public ByteBufferWriter(int code)
        {
            Code = code;

            _data = new MemoryStream(0);
            _writer = new BinaryWriter(_data);

            _writer.Write(code);
        }

        public void WriteInt32(Int32 i)
        {
            _writer.Write(i);
        }

        public void WriteFloat(float f)
        {
            _writer.Write(f);
        }

        public void WriteVec2(Vector2 vec)
        {
            WriteFloat(vec.X);
            WriteFloat(vec.Y);
        }

        public void WriteString(string s)
        {
            _writer.Write(s);
        }

        public void WriteBoolean(bool b)
        {
            _writer.Write(b);
        }

        public void WriteGuid(Guid g)
        {
            _writer.Write(g.ToString());
        }

        public byte[] ToArray()
        {
            var data = _data.ToArray();

            Dispose();

            return data;
        }

        public void Dispose()
        {
            _writer.Dispose();
            _data.Dispose();
        }
    }
}