using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using OpenTK;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.world.chunk;

namespace SharpCraft.world
{
    internal class WorldLoader
    {
        private static string _savesFolder = "saves";

        public static void SaveWorld(World w)
        {
            if (w == null)
                return;

            var bf = new BinaryFormatter();

            w.SaveAllChunks();

            try
            {
                var wpn = new WorldPlayerNode(Game.Instance.Player);
                var wdn = new WorldDataNode(w);

                using (var fs = File.OpenWrite(w.SaveRoot + "/player.dat"))
                {
                    bf.Serialize(fs, wpn);
                }

                using (var fs = File.OpenWrite(w.SaveRoot + "/level.dat"))
                {
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
            var bf = new BinaryFormatter();

            var dir = $"{_savesFolder}/{saveName}";

            if (!Directory.Exists(dir))
                return null;

            World world = null;

            try
            {
                WorldDataNode wdn;
                WorldPlayerNode wpn;

                using (var fs = File.OpenRead(dir + "/player.dat"))
                {
                    wpn = (WorldPlayerNode)bf.Deserialize(fs);
                }

                using (var fs = File.OpenRead(dir + "/level.dat"))
                {
                    wdn = (WorldDataNode)bf.Deserialize(fs);
                }

                world = new World(saveName, wdn.levelName, wdn.seed);

                var player = new EntityPlayerSP(wpn.pos);
                Game.Instance.Camera.pitch = wpn.pitch;
                Game.Instance.Camera.yaw = wpn.yaw;

                for (int i = 0; i < wpn.hotbar.Length; i++)
                {
                    player.hotbar[i] = wpn.hotbar[i];
                }

                world.AddEntity(player);
                world.LoadChunk(new BlockPos(player.pos).ChunkPos());
                Game.Instance.Player = player;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            return world;
        }
    }
}