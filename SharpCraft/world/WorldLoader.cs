﻿using SharpCraft.block;
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

            var bf = new BinaryFormatter();

            w.SaveAllChunks();

            try
            {
                var wpn = new WorldPlayerNode(SharpCraft.Instance.Player);
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

            var dir = $"{SharpCraft.Instance.GameFolderDir}saves/{saveName}";

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

                world = wdn.GetWorld(saveName);

                var player = wpn.GetPlayer(world);

                world.AddEntity(player);
                world.LoadChunk(new BlockPos(player.pos).ChunkPos());
                SharpCraft.Instance.Player = player;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            return world;
        }
    }
}