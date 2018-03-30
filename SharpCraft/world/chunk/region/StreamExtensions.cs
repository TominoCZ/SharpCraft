using System;
using System.IO;

namespace SharpCraft.world.chunk.region
{
	public static class StreamExtensions
	{
		public static int ReadInt32(this Stream str)
		{
			byte[] pc = new byte[4];
			str.Read(pc, 0, pc.Length);
			return BitConverter.ToInt32(pc, 0);
		}

		public static short ReadInt16(this Stream str)
		{
			byte[] pc = new byte[2];
			str.Read(pc, 0, pc.Length);
			return BitConverter.ToInt16(pc, 0);
		}

		public static void WriteInt32(this Stream str, int val)
		{
			str.WriteByte((byte) ((val >> 24) & 0xFF));
			str.WriteByte((byte) ((val >> 16) & 0xFF));
			str.WriteByte((byte) ((val >> 08) & 0xFF));
			str.WriteByte((byte) ((val >> 00) & 0xFF));
		}
	}
}