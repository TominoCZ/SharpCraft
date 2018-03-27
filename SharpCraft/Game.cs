using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Schema;
using OpenTK.Graphics;
using OpenTK.Platform.Windows;

namespace SharpCraft
{
    internal class Game : GameWindow
    {
        public EntityRenderer entityRenderer;
        public float NEAR_PLANE = 0.1f;
        public float FAR_PLANE = 1000f;
        public float FOV = 65;
        public GuiRenderer guiRenderer;
        public HashSet<Key> keysDown = new HashSet<Key>();
        public MouseOverObject mouseOverObject = new MouseOverObject();
        public EntityPlayerSP player;
        public SkyboxRenderer skyboxRenderer;
        public World world;
        public WorldRenderer worldRenderer;
        private ConcurrentQueue<Method> _glContextQueue = new ConcurrentQueue<Method>();
        private Stopwatch frameTimer = Stopwatch.StartNew();
        private WindowState lastWindowState;

        private Point mouseLast;
        private float mouseWheelLast;
        private Stopwatch timer = Stopwatch.StartNew();

        private bool wasSpaceDown;
        private float sensitivity = 1;
        private double lastRenderTime;

        private string glVersion;

        private static string title;

        public Game() : base(640, 480, GraphicsMode.Default, title, GameWindowFlags.Default, DisplayDevice.Default, 3, 3, GraphicsContextFlags.ForwardCompatible)
        {
            INSTANCE = this;

            VSync = VSyncMode.Off;
            MakeCurrent();

            glVersion = GL.GetString(StringName.ShadingLanguageVersion);
            Title = title = $"SharpCraft Alpha 0.0.2 [GLSL {glVersion}]";

            TargetRenderFrequency = 300;

            worldRenderer = new WorldRenderer();
            entityRenderer = new EntityRenderer();
            guiRenderer = new GuiRenderer();
            skyboxRenderer = new SkyboxRenderer();

            Console.WriteLine("DEBUG: stitching textures");
            TextureManager.stitchTextures();

            init();
        }

        public delegate void Method();

        public static Game INSTANCE { get; private set; }

        public GuiScreen guiScreen { get; private set; }

        public void closeGuiScreen()
        {
            guiScreen?.onClose();
            guiScreen = null;

            CursorVisible = false;
        }

        public Matrix4 createProjectionMatrix()
        {
            var matrix = Matrix4.Identity;

            float aspectRatio = (float)Width / Height;
            float y_scale = (float)(1f / Math.Tan(MathHelper.DegreesToRadians(FOV / 2f)));
            float x_scale = y_scale / aspectRatio;
            float frustum_length = FAR_PLANE - NEAR_PLANE;

            matrix.M11 = x_scale;
            matrix.M22 = y_scale;
            matrix.M33 = -((FAR_PLANE + NEAR_PLANE) / frustum_length);
            matrix.M34 = -1;
            matrix.M43 = -((2 * NEAR_PLANE * FAR_PLANE) / frustum_length);
            matrix.M44 = 0;

            return matrix;
        }

        public float getRenderPartialTicks()
        {
            return (float)timer.Elapsed.TotalMilliseconds / 50f;
        }

        public void openGuiScreen(GuiScreen guiScreen)
        {
            if (guiScreen == null)
            {
                closeGuiScreen();
                return;
            }

            this.guiScreen = guiScreen;

            var middle = new Point(ClientRectangle.Width / 2, ClientRectangle.Height / 2);
            middle = PointToScreen(middle);

            OpenTK.Input.Mouse.SetPosition(middle.X, middle.Y);
        }

        public void runGlContext(Method m)
        {
            _glContextQueue.Enqueue(m);
        }

        public void startGame()
        {
            var loadedWorld = WorldLoader.loadWorld("MyWorld");

            if (loadedWorld == null)
            {
                Console.WriteLine("DEBUG: generating world");

                var r = new Random();

                var playerPos = new BlockPos(-100 + (float)r.NextDouble() * 200, 0,
                                    -100 + (float)r.NextDouble() * 200);

                world = new World("MyWorld", "Tomlow's Fuckaround", SettingsManager.getValue("worldseed").GetHashCode());

                player = new EntityPlayerSP(new Vector3(playerPos.x, world.getHeightAtPos(playerPos.x, playerPos.z),
                    playerPos.z));

                world.addEntity(player);

                player.setItemStackInHotbar(0, new ItemStack(new ItemBlock(EnumBlock.CRAFTING_TABLE)));
                player.setItemStackInHotbar(1, new ItemStack(new ItemBlock(EnumBlock.FURNACE)));
                player.setItemStackInHotbar(2, new ItemStack(new ItemBlock(EnumBlock.COBBLESTONE)));
                player.setItemStackInHotbar(3, new ItemStack(new ItemBlock(EnumBlock.PLANKS)));
                player.setItemStackInHotbar(4, new ItemStack(new ItemBlock(EnumBlock.GLASS)));
                player.setItemStackInHotbar(5, new ItemStack(new ItemBlock(EnumBlock.XRAY)));
            }
            else
            {
                world = loadedWorld;
            }

            resetMouse();

            var state = OpenTK.Input.Mouse.GetState();
            mouseLast = new Point(state.X, state.Y);

            runUpdateThreads();

            ShaderManager.updateProjectionMatrix();

            //world.setBlock(new BlockPos(player.pos), EnumBlock.RARE, 1, true); //test of block metadata, works perfectly
        }

        private bool allowInput()
        {
            return guiScreen == null && !(CursorVisible = !Focused);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ShaderManager.cleanup();

            ModelManager.cleanup();
            TextureManager.cleanUp();

            if (world != null)
                WorldLoader.saveWorld(world);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (!keysDown.Contains(e.Key))
                keysDown.Add(e.Key);

            switch (e.Key)
            {
                case Key.R:
                    if (e.Control)
                    {
                        ShaderManager.reload();
                        SettingsManager.load();

                        worldRenderer.RenderDistance = SettingsManager.getInt("renderdistance");
                        worldRenderer.AltRenderMethod = SettingsManager.getBool("altrendermethod");
                        sensitivity = SettingsManager.getFloat("sensitivity");

                        if (e.Shift)
                        {
                            TextureManager.reload();
                            world?.destroyChunkModels();
                        }
                    }
                    break;
                case Key.Escape:
                    if (guiScreen is GuiScreenMainMenu)
                        return;

                    if (guiScreen != null)
                        closeGuiScreen();
                    else
                    {
                        openGuiScreen(new GuiScreenIngameMenu());
                    }
                    break;
                case Key.F11:
                    if (WindowState != WindowState.Fullscreen)
                    {
                        lastWindowState = WindowState;
                        WindowState = WindowState.Fullscreen;
                    }
                    else
                        WindowState = lastWindowState;
                    break;
            }

            if (guiScreen == null)
            {
                for (int i = 0; i < 9; i++)
                {
                    if (e.Key == Key.Number1 + i)
                    {
                        player?.setSelectedSlot(i);

                        break;
                    }
                }
            }

            if (e.Key == (Key.LAlt | Key.F4))
                Exit();
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            keysDown.Remove(e.Key);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.IsPressed)
            {
                if (guiScreen == null)
                {
                    getMouseOverObject();

                    if (mouseOverObject.hit is EnumBlock)
                    {
                        var pos = mouseOverObject.blockPos;

                        //pickBlock
                        if (e.Button == MouseButton.Middle)
                        {
                            var clickedBlock = world.getBlock(pos);

                            if (clickedBlock != EnumBlock.AIR)
                            {
                                player.setItemStackInSelectedSlot(new ItemStack(new ItemBlock(clickedBlock), 1,
                                    world.getMetadata(pos)));
                            }
                        }

                        //place/interact
                        if (e.Button == MouseButton.Right)
                        {
                            var block = world.getBlock(pos);
                            var model = ModelRegistry.getModelForBlock(block, world.getMetadata(pos));

                            if (model != null && model.canBeInteractedWith)
                            {
                                switch (block)
                                {
                                    case EnumBlock.FURNACE:
                                    case EnumBlock.CRAFTING_TABLE:
                                        openGuiScreen(new GuiScreenCrafting());
                                        break;
                                }
                            }
                            else if (player.getEquippedItemStack()?.Item is ItemBlock itemBlock)
                            {
                                pos = pos.offset(mouseOverObject.sideHit);

                                var blockAtPos = world.getBlock(pos);

                                var heldBlock = itemBlock.getBlock();
                                var blockBB = ModelRegistry.getModelForBlock(heldBlock, world.getMetadata(pos))
                                    .boundingBox.offset(pos.vector);

                                if (blockAtPos == EnumBlock.AIR && world.getIntersectingEntitiesBBs(blockBB).Count == 0)
                                {
                                    var posUnder = pos.offset(EnumFacing.DOWN);

                                    var blockUnder = world.getBlock(posUnder);
                                    var blockAbove = world.getBlock(pos.offset(EnumFacing.UP));

                                    if (blockUnder == EnumBlock.GRASS && heldBlock != EnumBlock.GLASS)
                                        world.setBlock(posUnder, EnumBlock.DIRT, 0, false);
                                    if (blockAbove != EnumBlock.AIR && blockAbove != EnumBlock.GLASS && heldBlock == EnumBlock.GRASS)
                                        world.setBlock(pos, EnumBlock.DIRT, 0, true);
                                    else
                                        world.setBlock(pos, heldBlock, player.getEquippedItemStack().Meta, true);
                                }
                            }
                        }

                        //break
                        if (e.Button == MouseButton.Left)
                            world.setBlock(pos, EnumBlock.AIR, 0, true);
                    }
                }
                else
                {
                    var state = OpenTK.Input.Mouse.GetCursorState();
                    var point = PointToClient(new Point(state.X, state.Y));

                    guiScreen.onMouseClick(point.X, point.Y);
                }
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            lastRenderTime = e.Time;

            while (_glContextQueue.Count > 0)
            {
                if (_glContextQueue.TryDequeue(out var func))
                    func?.Invoke();
            }

            if (timer.ElapsedMilliseconds > 50 - e.Time * 1000)
            {
                GameLoop();
                timer.Restart();
            }

            renderScreen(getRenderPartialTicks());

            SwapBuffers();
            ProcessEvents(false);
        }

        protected override void OnResize(EventArgs e)
        {
            if (ClientSize.Width < 640)
                ClientSize = new Size(640, ClientSize.Height);
            if (ClientSize.Height < 480)
                ClientSize = new Size(ClientSize.Width, 480);

            base.OnResize(e);

            GL.Viewport(ClientRectangle);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, ClientRectangle.Width, ClientRectangle.Height, 0, NEAR_PLANE, FAR_PLANE);

            ShaderManager.updateProjectionMatrix();
        }

        private void checkChunks()
        {
            if (world == null || player == null)
                return;

            for (int z = -worldRenderer.RenderDistance; z <= worldRenderer.RenderDistance; z++)
            {
                for (int x = -worldRenderer.RenderDistance; x <= worldRenderer.RenderDistance; x++)
                {
                    var pos = new BlockPos(x * 16 + player.pos.X, 0, z * 16 + player.pos.Z).chunkPos().offset(8, 0, 8);
                    var dist = MathUtil.distance(pos.vector.Xz, Camera.INSTANCE.pos.Xz);

                    if (dist <= worldRenderer.RenderDistance * 16)
                    {
                        checkChunk(pos);
                    }
                }
            }
        }

        private void checkChunk(BlockPos pos)
        {
            var chunk = world.getChunkFromPos(pos);

            if (chunk == null)// || !world.isChunkGenerated(pos)))
            {
                if (!world.loadChunk(pos.chunkPos()))
                {
                    world.beginGenerateChunk(pos, true);
                    Console.WriteLine("DEBUG: chunk generated");
                }
            }
            else if (chunk.isDirty || !world.doesChunkHaveModel(pos) && world.areNeighbourChunksGenerated(pos))
            {
                world.beginUpdateModelForChunk(pos);
            }
        }

        private void GameLoop()
        {
            if (guiScreen == null && !Focused)
                openGuiScreen(new GuiScreenIngameMenu());

            var wheelValue = Mouse.WheelPrecise;

            if (player != null && guiScreen == null)
            {
                if (wheelValue < mouseWheelLast)
                    player.selectNextItem();
                else if (wheelValue > mouseWheelLast)
                    player.selectPreviousItem();

                if (world?.getChunkFromPos(new BlockPos(player.pos)) == null)
                    player.motion = Vector3.Zero;
            }

            mouseWheelLast = wheelValue;

            world?.updateEntities();
        }

        private void getMouseOverObject()
        {
            int radius = 5;

            MouseOverObject final = new MouseOverObject();

            float dist = float.MaxValue;

            var camPos = Vector3.One * 0.5f + Camera.INSTANCE.pos;

            for (int z = -radius; z <= radius; z++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    for (int x = -radius; x <= radius; x++)
                    {
                        var vec = camPos;
                        vec.X += x;
                        vec.Y += y;
                        vec.Z += z;

                        float f = (vec - Camera.INSTANCE.pos).LengthFast;

                        if (f <= radius + 0.5f)
                        {
                            var pos = new BlockPos(vec);
                            var block = world.getBlock(pos);

                            if (block != EnumBlock.AIR)
                            {
                                var model = ModelRegistry.getModelForBlock(block, world.getMetadata(pos));
                                var bb = model.boundingBox.offset(pos.vector);

                                var hitSomething = RayHelper.rayIntersectsBB(Camera.INSTANCE.pos,
                                    Camera.INSTANCE.getLookVec(), bb, out var hitPos, out var normal);

                                if (hitSomething)
                                {
                                    var sideHit = EnumFacing.UP;

                                    if (normal.X < 0)
                                        sideHit = EnumFacing.WEST;
                                    else if (normal.X > 0)
                                        sideHit = EnumFacing.EAST;
                                    if (normal.Y < 0)
                                        sideHit = EnumFacing.DOWN;
                                    else if (normal.Y > 0)
                                        sideHit = EnumFacing.UP;
                                    if (normal.Z < 0)
                                        sideHit = EnumFacing.NORTH;
                                    else if (normal.Z > 0)
                                        sideHit = EnumFacing.SOUTH;

                                    var p = new BlockPos(hitPos - normal * 0.5f);

                                    var l = Math.Abs((Camera.INSTANCE.pos - (p.vector + Vector3.One * 0.5f)).Length);

                                    if (l < dist)
                                    {
                                        dist = l;

                                        final.hit = block;
                                        final.hitVec = hitPos;
                                        final.blockPos = p;
                                        final.normal = normal;
                                        final.sideHit = sideHit;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            mouseOverObject = final;
        }

        private void init()
        {
            Console.WriteLine("DEBUG: loading models");

            var shader = new ShaderBlock("block", PrimitiveType.Quads);
            var shader_unlit = new ShaderBlockUnlit("block_unlit", PrimitiveType.Quads);

            var stoneModel = new ModelBlock(EnumBlock.STONE, shader, false);
            var grassModel = new ModelBlock(EnumBlock.GRASS, shader, false);
            var dirtModel = new ModelBlock(EnumBlock.DIRT, shader, false);
            var cobblestoneModel = new ModelBlock(EnumBlock.COBBLESTONE, shader, false);
            var planksModel = new ModelBlock(EnumBlock.PLANKS, shader, false);
            var craftingTableModel = new ModelBlock(EnumBlock.CRAFTING_TABLE, shader, true);
            var furnaceModel = new ModelBlock(EnumBlock.FURNACE, shader, true);
            var bedrockModel = new ModelBlock(EnumBlock.BEDROCK, shader, false);
            var rareModel = new ModelBlock(EnumBlock.RARE, shader, false);
            var glassModel = new ModelBlock(EnumBlock.GLASS, shader, false);
            var logModel = new ModelBlock(EnumBlock.LOG, shader, false);

            var xrayModel = new ModelBlock(EnumBlock.XRAY, shader, false);

            ModelRegistry.registerBlockModel(stoneModel, 0);
            ModelRegistry.registerBlockModel(grassModel, 0);
            ModelRegistry.registerBlockModel(dirtModel, 0);
            ModelRegistry.registerBlockModel(cobblestoneModel, 0);
            ModelRegistry.registerBlockModel(planksModel, 0);
            ModelRegistry.registerBlockModel(craftingTableModel, 0);
            ModelRegistry.registerBlockModel(furnaceModel, 0);

            ModelRegistry.registerBlockModel(bedrockModel, 0);
            ModelRegistry.registerBlockModel(rareModel, 0);
            ModelRegistry.registerBlockModel(logModel, 0);
            ModelRegistry.registerBlockModel(glassModel, 0);

            ModelRegistry.registerBlockModel(xrayModel, 0);

            SettingsManager.load();

            sensitivity = SettingsManager.getFloat("sensitivity");
            worldRenderer.RenderDistance = SettingsManager.getInt("renderdistance");
            worldRenderer.AltRenderMethod = SettingsManager.getBool("altrendermethod");

            openGuiScreen(new GuiScreenMainMenu());
        }

        private void prepare()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.DepthClamp);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
        }

        private void renderScreen(float partialTicks)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            prepare();

            var viewMatrix = MatrixHelper.createViewMatrix(Camera.INSTANCE);

            if (world != null)
            {
                worldRenderer.render(viewMatrix);
                entityRenderer.render(partialTicks);

                skyboxRenderer.render(viewMatrix);
            }

            //render other gui
            if (player != null)
            {
                guiRenderer.renderCrosshair();
                guiRenderer.renderHUD();
            }

            //render gui screen
            if (guiScreen != null)
            {
                CursorVisible = true;
                guiRenderer.render(guiScreen);
            }
        }

        private void resetMouse()
        {
            var middle = PointToScreen(new Point(ClientSize.Width / 2, ClientSize.Height / 2));
            OpenTK.Input.Mouse.SetPosition(middle.X, middle.Y);
        }

        private void runUpdateThreads()
        {
            new Thread(() =>
                {
                    while (true)
                    {
                        if (Visible)
                        {
                            checkChunks();

                            getMouseOverObject();

                            foreach (var data in world.Chunks.Values)
                            {
                                data.chunk.tick();

                                if (!data.chunk.isWithinRenderDistance())
                                    _glContextQueue.Enqueue(() => world.unloadChunk(data.chunk.chunkPos));
                            }
                        }

                        Thread.Sleep(50);
                    }
                })
            { IsBackground = true }.Start();

            new Thread(() =>
                {
                    while (true)
                    {
                        if (Visible)
                        {
                            var state = Mouse.GetState();

                            var point = new Point(state.X, state.Y);

                            if (allowInput())
                            {
                                var delta = new Point(mouseLast.X - point.X, mouseLast.Y - point.Y);

                                Camera.INSTANCE.yaw -= delta.X / 1000f * sensitivity;
                                Camera.INSTANCE.pitch -= delta.Y / 1000f * sensitivity;

                                resetMouse();
                            }

                            mouseLast = point;

                            if (keysDown.Contains(Key.Space) && !wasSpaceDown && player.onGround)
                            {
                                wasSpaceDown = true;
                                player.motion.Y = 0.475F;
                            }
                            else if ((!keysDown.Contains(Key.Space) || player.onGround) && wasSpaceDown)
                                wasSpaceDown = false;

                            if (frameTimer.ElapsedMilliseconds >= 80)
                            {
                                frameTimer.Restart();

                                Title = $"{title} - FPS: {1f / lastRenderTime:0}";
                            }
                        }
                        Thread.Sleep(3);
                    }
                })
            { IsBackground = true }.Start();
        }
    }

    internal class SettingsManager
    {
        private static Dictionary<string, string> _settings = new Dictionary<string, string>();

        static SettingsManager()
        {
            _settings.Add("sensitivity", "1");
            _settings.Add("renderdistance", "8");
            _settings.Add("worldseed", "yeet");
            _settings.Add("altrendermethod", "false");
        }

        public static void load()
        {
            var dir = "SharpCraft_Data";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var file = dir + "/settings.txt";

            if (File.Exists(file))
            {
                var data = File.ReadLines(file);

                foreach (var line in data)
                {
                    var parsed = line.Trim().Replace(" ", "").ToLower();
                    var split = parsed.Split('=');

                    if (split.Length < 2)
                        continue;

                    var variable = split[0];
                    var value = split[1];

                    if (_settings.ContainsKey(variable))
                    {
                        _settings.Remove(variable);
                        _settings.Add(variable, value);
                    }
                }
            }

            save();
        }

        public static void save()
        {
            var dir = "SharpCraft_Data";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var file = dir + "/settings.txt";

            StringBuilder sb = new StringBuilder();

            var keys = _settings.Keys.ToArray();

            for (var index = 0; index < keys.Length - 1; index++)
            {
                var key = keys[index];

                sb.AppendLine($"{key}={getValue(key)}");
            }

            var last = _settings.Last();

            sb.Append($"{last.Key}={getValue(last.Key)}");

            File.WriteAllText(file, sb.ToString());
        }

        public static string getValue(string variable)
        {
            return _settings[variable];
        }

        public static int getInt(string variable)
        {
            return int.Parse(getValue(variable));
        }

        public static float getFloat(string variable)
        {
            return float.Parse(getValue(variable));
        }

        public static bool getBool(string variable)
        {
            return bool.Parse(getValue(variable));
        }
    }

    internal class Start
    {
        [STAThread]
        private static void Main(string[] args)
        {
            int threads = 1;

            while (ThreadPool.SetMinThreads(threads, 5) && threads++ < 4)
            {

            }

            while (ThreadPool.SetMaxThreads(threads, 5) && threads++ < 8)
            {

            }

            using (Game game = new Game())
            {
                game.Run(30.0);
            }
        }
    }
}