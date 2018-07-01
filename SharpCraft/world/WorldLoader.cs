using SharpCraft.block;
using SharpCraft.entity;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SharpCraft.world
{
    internal class WorldLoader
    {
        public static void SaveWorld(World w)
        {
            if (w == null)
                return;

            BinaryFormatter bf = new BinaryFormatter();

            w.SaveAllChunks();

            try
            {
                WorldPlayerNode wpn = new WorldPlayerNode(SharpCraft.Instance.Player);
                using (FileStream fs = File.OpenWrite(w.SaveRoot + "/player.dat"))
                {
                    fs.SetLength(0);
                    fs.Position = 0;
                    bf.Serialize(fs, wpn);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            try
            {
                WorldDataNode wdn = new WorldDataNode(w);
                using (FileStream fs = File.OpenWrite(w.SaveRoot + "/level.dat"))
                {
                    fs.SetLength(0);
                    fs.Position = 0;
                    bf.Serialize(fs, wdn);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public static World LoadWorld(string saveName)
        {
            BinaryFormatter bf = new BinaryFormatter();

            string dir = $"{SharpCraft.Instance.GameFolderDir}saves/{saveName}";

            if (!Directory.Exists(dir))
                return null;

            WorldDataNode wdn = null;
            WorldPlayerNode wpn = null;

            try
            {
                using (FileStream fs = File.OpenRead(dir + "/player.dat"))
                {
                    fs.Position = 0;
                    wpn = (WorldPlayerNode)bf.Deserialize(fs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            try
            {
                using (FileStream fs = File.OpenRead(dir + "/level.dat"))
                {
                    fs.Position = 0;
                    wdn = (WorldDataNode)bf.Deserialize(fs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            var world = wdn?.GetWorld(saveName);

            EntityPlayerSp player = wpn?.GetPlayer(world);

            if (player != null)
            {
                world?.AddEntity(player);

                world?.LoadChunk(new BlockPos(player.Pos).ChunkPos());
                SharpCraft.Instance.Player = player;
            }

            return world;
        }
    }
}