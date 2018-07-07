using SharpCraft.block;
using SharpCraft.entity;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace SharpCraft.world
{
    internal class WorldLoader
    {
        public static void SaveWorld(World w)
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
            
            try
            {
                WorldWaypointNode wwn = new WorldWaypointNode(w);
                var json = JsonConvert.SerializeObject(wwn, Formatting.Indented);

                File.WriteAllText(w.SaveRoot + "/waypoints.dat", json);
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

            if (world != null)
            {
                try
                {
                    string json = File.ReadAllText(dir + "/waypoints.dat");
                    var wwn = JsonConvert.DeserializeObject<WorldWaypointNode>(json);
                    wwn.Load(world);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }

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

    class WorldWaypointNode
    {
        [JsonProperty] private List<WaypointNode> waypoints = new List<WaypointNode>();

        public WorldWaypointNode()
        {

        }

        public WorldWaypointNode(World w)
        {
            foreach (var waypoint in w.GetWaypoints())
            {
                var wn = new WaypointNode
                {
                    name = waypoint.Name,
                    x = waypoint.Pos.X,
                    y = waypoint.Pos.Y,
                    z = waypoint.Pos.Z,
                    r = waypoint.Color.R,
                    g = waypoint.Color.G,
                    b = waypoint.Color.B
                };

                waypoints.Add(wn);
            }
        }

        public void Load(World w)
        {
            foreach (var node in waypoints)
            {
                string name = node.name;
                BlockPos pos = new BlockPos(node.x, node.y, node.z);
                Color color = Color.FromArgb(node.r, node.g, node.b);

                w.AddWaypoint(pos, color, name);
            }
        }

        class WaypointNode
        {
            [JsonProperty] public int x;
            [JsonProperty] public int y;
            [JsonProperty] public int z;

            [JsonProperty] public int r;
            [JsonProperty] public int g;
            [JsonProperty] public int b;

            [JsonProperty] public string name;
        }
    }
}