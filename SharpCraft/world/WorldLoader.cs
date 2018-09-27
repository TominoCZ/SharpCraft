using Newtonsoft.Json;
using SharpCraft.block;
using SharpCraft.entity;
using System;
using System.IO;

// ReSharper disable InconsistentNaming

namespace SharpCraft.world
{
    internal class WorldLoader
    {
        public static void SaveWorld(WorldClient w)
        {
            if (w == null)
                return;

            w.SaveAllChunks();

            try
            {
                WorldPlayerNode wpn = new WorldPlayerNode(SharpCraft.Instance.Player);

                var json = JsonConvert.SerializeObject(wpn, Formatting.Indented);

                File.WriteAllText(w.SaveRoot + "/player.dat", json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            try
            {
                WorldDataNode wdn = new WorldDataNode(w);
                var json = JsonConvert.SerializeObject(wdn, Formatting.Indented);

                File.WriteAllText(w.SaveRoot + "/level.dat", json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public static World LoadWorld(string saveName)
        {
            string dir = $"{SharpCraft.Instance.GameFolderDir}/saves/{saveName}";

            if (!Directory.Exists(dir))
                return null;

            WorldDataNode wdn = null;
            WorldPlayerNode wpn = null;

            try
            {
                string json = File.ReadAllText(dir + "/player.dat");
                wpn = JsonConvert.DeserializeObject<WorldPlayerNode>(json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            try
            {
                string json = File.ReadAllText(dir + "/level.dat");
                wdn = JsonConvert.DeserializeObject<WorldDataNode>(json);
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