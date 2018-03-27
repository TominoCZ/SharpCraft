using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SharpCraft
{
    internal class WorldLoader
    {
        private static string savesFolder = "SharpCraft_Data/saves";

        public static void saveWorld(World w)
        {
            if (w == null)
                return;

            var bf = new BinaryFormatter();

            w.saveAllChunks();

            try
            {
                var wpn = new WorldPlayerNode(Game.INSTANCE.player);
                var wdn = new WorldDataNode(w);

                using (var fs = File.OpenWrite(w.saveRoot + "/player.dat"))
                {
                    bf.Serialize(fs, wpn);
                }

                using (var fs = File.OpenWrite(w.saveRoot + "/level.dat"))
                {
                    bf.Serialize(fs, wdn);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public static World loadWorld(string saveName)
        {
            var bf = new BinaryFormatter();

            var dir = $"{savesFolder}/{saveName}";

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
                Game.INSTANCE.Camera.pitch = wpn.pitch;
                Game.INSTANCE.Camera.yaw = wpn.yaw;

                for (int i = 0; i < wpn.hotbar.Length; i++)
                {
                    player.hotbar[i] = wpn.hotbar[i];
                }

                world.addEntity(player);
                world.loadChunk(new BlockPos(player.pos));
                Game.INSTANCE.player = player;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            return world;
        }
    }
}