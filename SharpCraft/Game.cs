using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace SharpCraft
{
    class Start
    {
        static void Main(string[] args)
        {
            var wnd = new Game();

            wnd.Run(60);
        }
    }

    class Game : GameWindow
    {
        public float NEAR_PLANE = 0.1f;
        public float FAR_PLANE = 1000f;

        public float FOV = 65;

        private WindowState lastWindowState;

        public static Game INSTANCE { get; private set; }

        public static List<ThreadLock> MAIN_THREAD_QUEUE = new List<ThreadLock>();

        public MouseOverObject mouseOverObject = new MouseOverObject();

        public List<Key> keysDown = new List<Key>();

        private Stopwatch timer = Stopwatch.StartNew();
        private Stopwatch frameTimer = Stopwatch.StartNew();

        private Point mouseLast;
        private float mouseWheelLast;

        public WorldRenderer worldRenderer;
        public EntityRenderer entityRenderer;
        public GuiRenderer guiRenderer;
        public SkyboxRenderer skyboxRenderer;

        public GuiScreen guiScreen { get; private set; }
        public EntityPlayerSP player;
        public World world;

        public Game()
        {
            INSTANCE = this;

            Title = "OpenGL Game";
            CursorVisible = false;

            VSync = VSyncMode.Off;
            MakeCurrent();

            worldRenderer = new WorldRenderer();
            entityRenderer = new EntityRenderer();
            guiRenderer = new GuiRenderer();
            skyboxRenderer = new SkyboxRenderer();

            Console.WriteLine("DEBUG: stitching textures");
            TextureManager.stitchTextures();

            init();
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
            var rareModelUnlit = new ModelBlock(EnumBlock.RARE, shader_unlit, false);
            var glassModel = new ModelBlock(EnumBlock.GLASS, shader, false);

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
            ModelRegistry.registerBlockModel(rareModelUnlit, 1);
            ModelRegistry.registerBlockModel(glassModel, 0);

            ModelRegistry.registerBlockModel(xrayModel, 0);

            SettingsManager.load();

            openGuiScreen(new GuiScreenMainMenu());
        }

        public void startGame()
        {
            var loadedWorld = WorldLoader.loadWorld();

            if (loadedWorld == null)
            {
                Console.WriteLine("DEBUG: generating world");

                var r = new Random();

                var playerPos = new BlockPos(-100 + (float)r.NextDouble() * 200, 0,
                                    -100 + (float)r.NextDouble() * 200);

                world = new World(0);
                world.beginGenerateChunk(playerPos);

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

        private void runUpdateThreads()
        {
            new Thread(() =>
            {
                while (true)
                {
                    if (Visible)
                    {
                        checkChunks();
                    }

                    Thread.Sleep(50);
                }
            })
            { IsBackground = true }.Start();

            new Thread(() =>
            {
                bool wasSpaceDown = false;

                while (true)
                {
                    if (Visible)
                    {
                        var state = Mouse.GetState();

                        var point = new Point(state.X, state.Y);

                        if (guiScreen == null)
                        {
                            if (!(CursorVisible = !Focused))
                            {
                                var delta = new Point(mouseLast.X - point.X, mouseLast.Y - point.Y);

                                Camera.INSTANCE.yaw -= delta.X / 1000f;
                                Camera.INSTANCE.pitch -= delta.Y / 1000f;

                                if (keysDown.Contains(Key.Space) && !wasSpaceDown && player.onGround)
                                {
                                    wasSpaceDown = true;
                                    player.motion.Y = 0.475F;
                                }
                                else if ((!keysDown.Contains(Key.Space) || player.onGround) && wasSpaceDown)
                                    wasSpaceDown = false;

                                getMouseOverObject();

                                resetMouse();
                            }
                        }

                        mouseLast = point;
                    }

                    Thread.Sleep(5);
                }
            })
            { IsBackground = true }.Start();
        }

        private void resetMouse()
        {
            var middle = PointToScreen(new Point(ClientSize.Width / 2, ClientSize.Height / 2));
            OpenTK.Input.Mouse.SetPosition(middle.X, middle.Y);
        }

        private void prepare()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.DepthClamp);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (frameTimer.ElapsedMilliseconds >= 1000)
            {
                frameTimer.Restart();

                Console.WriteLine($"{1 / e.Time:F} FPS");
            }

            if (timer.ElapsedMilliseconds >= 50)
            {
                GameLoop();
                timer.Restart();
            }

            float partialTicks = getRenderPartialTicks();

            renderScreen(partialTicks);

            SwapBuffers();
            ProcessEvents(false);

            if (MAIN_THREAD_QUEUE.Count > 0)
            {
                for (int i = 0; i < MAIN_THREAD_QUEUE.Count; i++)
                {
                    var task = MAIN_THREAD_QUEUE[0];
                    {
                        if (task != null)
                            task.ExecuteCode();

                        MAIN_THREAD_QUEUE.Remove(task);
                    }
                }
            }
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
            }

            mouseWheelLast = wheelValue;

            world?.updateEntities();
        }

        private void checkChunks()
        {
            if (world == null || player == null)
                return;

            for (int z = -worldRenderer.RenderDistance; z <= worldRenderer.RenderDistance; z++)
            {
                for (int x = -worldRenderer.RenderDistance; x <= worldRenderer.RenderDistance; x++)
                {
                    var pos = new BlockPos(x * 16 + player.pos.X, 0, z * 16 + player.pos.Z).ChunkPos() + new BlockPos(8, 0, 8);
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

            if (chunk == null || !world.isChunkGenerated(pos))
            {
                world.beginGenerateChunk(pos);
            }
            else if (!world.doesChunkHaveModel(pos))
            {
                world.beginUpdateModelForChunk(pos, true);
            }
            else if (chunk.isDirty)
            {
                world.beginUpdateModelForChunk(pos);
            }
        }

        public void closeGuiScreen()
        {
            guiScreen?.onClose();
            guiScreen = null;

            CursorVisible = false;
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

        public float getRenderPartialTicks()
        {
            return (float)timer.Elapsed.TotalMilliseconds / 50f;
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
                        var vec = new Vector3(x, y, z) + camPos;
                        float f = (vec - Camera.INSTANCE.pos).LengthFast;

                        if (f <= radius)
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

                                        final = new MouseOverObject()
                                        {
                                            hit = block,
                                            hitVec = hitPos,
                                            blockPos = p,
                                            normal = normal,
                                            sideHit = sideHit
                                        };
                                    }
                                }
                            }
                        }
                    }
                }
            }

            mouseOverObject = final;
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

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (!keysDown.Contains(e.Key))
                keysDown.Add(e.Key);

            if (e.Key == Key.Escape)
            {
                if (guiScreen is GuiScreenMainMenu)
                    return;

                if (guiScreen != null)
                    closeGuiScreen();
                else
                {
                    openGuiScreen(new GuiScreenIngameMenu());
                }
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

            if (e.Key == Key.F11)
            {
                if (WindowState != WindowState.Fullscreen)
                {
                    lastWindowState = WindowState;
                    WindowState = WindowState.Fullscreen;
                }
                else
                    WindowState = lastWindowState;
            }
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

        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);

            ProcessEvents(false);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ShaderManager.cleanup();

            ModelManager.cleanup();
            TextureManager.cleanUp();

            if (world != null)
                WorldLoader.saveWorld(world);
        }
    }

    class SettingsManager
    {
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

                    if (parsed.Contains("renderdistance="))
                    {
                        int.TryParse(split[1], out var num);
                        Game.INSTANCE.worldRenderer.RenderDistance = num;
                    }
                }
            }
        }

        public static void save()
        {
            var dir = "SharpCraft_Data";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var file = dir + "/settings.txt";

            StringBuilder sb = new StringBuilder();
            sb.Append($"renderDistance={Game.INSTANCE.worldRenderer.RenderDistance}");

            File.WriteAllText(file, sb.ToString());
        }
    }
}
