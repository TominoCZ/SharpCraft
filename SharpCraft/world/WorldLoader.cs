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
                using (FileStream fs = new FileStream(w.SaveRoot + "/player.dat", FileMode.OpenOrCreate))
                {
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
                using (FileStream fs = new FileStream(w.SaveRoot + "/level.dat", FileMode.OpenOrCreate))
                {
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
                using (FileStream fs = new FileStream(dir + "/player.dat", FileMode.Open))
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
                using (FileStream fs = new FileStream(dir + "/level.dat", FileMode.Open))
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