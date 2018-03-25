using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SharpCraft
{
    internal class WorldLoader
    {
        private static string dir = "SharpCraft_Data/saves/world";

        private static bool saving;

        public static void saveWorld(World w)
        {
            if (w == null || saving)
                return;

            saving = true;

            var bf = new BinaryFormatter();

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            try
            {
                var wcn = new WorldChunksNode(w);

                using (var fs = File.OpenWrite(dir + "/chunks.dat"))
                {
                    bf.Serialize(fs, wcn);
                }

                var wpn = new WorldPlayerNode(Game.INSTANCE.player);

                using (var fs = File.OpenWrite(dir + "/player.dat"))
                {
                    bf.Serialize(fs, wpn);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            saving = false;
        }

        public static World loadWorld()
        {
            var bf = new BinaryFormatter();

            if (!Directory.Exists(dir))
                return null;

            World world = null;

            try
            {
                WorldChunksNode wcn;
                WorldPlayerNode wpn;

                using (var fs = File.OpenRead(dir + "/chunks.dat"))
                {
                    wcn = (WorldChunksNode)bf.Deserialize(fs);
                }

                using (var fs = File.OpenRead(dir + "/player.dat"))
                {
                    wpn = (WorldPlayerNode)bf.Deserialize(fs);
                }

                world = World.Create(wcn.seed, wcn.caches);

                var player = new EntityPlayerSP(wpn.pos);
                Camera.INSTANCE.pitch = wpn.pitch;
                Camera.INSTANCE.yaw = wpn.yaw;

                for (int i = 0; i < wpn.hotbar.Length; i++)
                {
                    player.hotbar[i] = wpn.hotbar[i];
                }

                world.addEntity(player);

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