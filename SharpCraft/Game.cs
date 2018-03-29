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
using System.Threading.Tasks;
using OpenTK.Graphics;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.gui;
using SharpCraft.model;
using SharpCraft.render;
using SharpCraft.shader;
using SharpCraft.texture;
using SharpCraft.util;
using SharpCraft.world;
using SharpCraft.world.chunk;

namespace SharpCraft
{
    internal class Game : GameWindow
    {
        public EntityRenderer EntityRenderer;
        public float NearPlane = 0.1f;
        public float FarPlane = 1000f;
        public float Fov = 65;
        public GuiRenderer GuiRenderer;
        public HashSet<Key> KeysDown = new HashSet<Key>();
        public MouseOverObject MouseOverObject = new MouseOverObject();
        public EntityPlayerSP Player;
        public SkyboxRenderer SkyboxRenderer;
        public World World;
        public WorldRenderer WorldRenderer;
        private ConcurrentQueue<Method> _glContextQueue = new ConcurrentQueue<Method>();
        private Stopwatch _frameTimer = Stopwatch.StartNew();
        private WindowState _lastWindowState;

        public Camera Camera = new Camera();

        private Point _mouseLast;
        private float _mouseWheelLast;
        private Stopwatch _timer = Stopwatch.StartNew();

        private bool _wasSpaceDown;
        private int _fpsCounter;
        private float _sensitivity = 1;

        private string _glVersion;

        private static string _title;

        public Game() : base(640, 480, GraphicsMode.Default, _title, GameWindowFlags.Default, DisplayDevice.Default, 3, 3,
            GraphicsContextFlags.ForwardCompatible)
        {
            Instance = this;

            VSync = VSyncMode.Off;
            MakeCurrent();

            _glVersion = GL.GetString(StringName.ShadingLanguageVersion);
            Title = _title = $"SharpCraft Alpha 0.0.2 [GLSL {_glVersion}]";

            //TargetRenderFrequency = 30000;

            WorldRenderer = new WorldRenderer();
            EntityRenderer = new EntityRenderer();
            GuiRenderer = new GuiRenderer();
            SkyboxRenderer = new SkyboxRenderer();

            Console.WriteLine("DEBUG: stitching textures");
            TextureManager.stitchTextures();

            Init();
        }

        public delegate void Method();

        public static Game Instance { get; private set; }

        public GuiScreen GuiScreen { get; private set; }

        public void CloseGuiScreen()
        {
            GuiScreen?.onClose();
            GuiScreen = null;

            CursorVisible = false;
        }

        public Matrix4 CreateProjectionMatrix()
        {
            var matrix = Matrix4.Identity;

            var aspectRatio = (float)Width / Height;
            var yScale = (float)(1f / Math.Tan(MathHelper.DegreesToRadians(Fov / 2f)));
            var xScale = yScale / aspectRatio;
            var frustumLength = FarPlane - NearPlane;

            matrix.M11 = xScale;
            matrix.M22 = yScale;
            matrix.M33 = -((FarPlane + NearPlane) / frustumLength);
            matrix.M34 = -1;
            matrix.M43 = -((2 * NearPlane * FarPlane) / frustumLength);
            matrix.M44 = 0;

            return matrix;
        }

        public float GetRenderPartialTicks()
        {
            return (float)_timer.Elapsed.TotalMilliseconds / 50f;
        }

        public void OpenGuiScreen(GuiScreen guiScreen)
        {
            if (guiScreen == null)
            {
                CloseGuiScreen();
                return;
            }

            GuiScreen = guiScreen;

            var middle = new Point(ClientRectangle.Width / 2, ClientRectangle.Height / 2);
            middle = PointToScreen(middle);

            OpenTK.Input.Mouse.SetPosition(middle.X, middle.Y);
        }

        public void RunGlContext(Method m)
        {
            _glContextQueue.Enqueue(m);
        }

        public void StartGame()
        {
            var loadedWorld = WorldLoader.LoadWorld("MyWorld");

            if (loadedWorld == null)
            {
                Console.WriteLine("DEBUG: generating world");

                var r = new Random();

                var playerPos = new BlockPos(0, 10, 0); //TODO DEBUG //-100 + (float)r.NextDouble() * 200, 0, -100 + (float)r.NextDouble() * 200);

                World = new World("MyWorld", "Tomlow's Fuckaround", SettingsManager.GetValue("worldseed").GetHashCode());

                Player = new EntityPlayerSP(new Vector3(playerPos.X, World.GetHeightAtPos(playerPos.X, playerPos.Z),
                    playerPos.Z));

                World.AddEntity(Player);

                Player.setItemStackInHotbar(0, new ItemStack(new ItemBlock(EnumBlock.CRAFTING_TABLE)));
                Player.setItemStackInHotbar(1, new ItemStack(new ItemBlock(EnumBlock.FURNACE)));
                Player.setItemStackInHotbar(2, new ItemStack(new ItemBlock(EnumBlock.COBBLESTONE)));
                Player.setItemStackInHotbar(3, new ItemStack(new ItemBlock(EnumBlock.PLANKS)));
                Player.setItemStackInHotbar(4, new ItemStack(new ItemBlock(EnumBlock.GLASS)));
                Player.setItemStackInHotbar(5, new ItemStack(new ItemBlock(EnumBlock.XRAY)));
            }
            else
            {
                World = loadedWorld;
            }

            ResetMouse();

            var state = OpenTK.Input.Mouse.GetState();
            _mouseLast = new Point(state.X, state.Y);

            RunUpdateThreads();

            ShaderManager.updateProjectionMatrix();

            //world.setBlock(new BlockPos(player.pos), EnumBlock.RARE, 1, true); //test of block metadata, works perfectly
        }

        private bool AllowInput()
        {
            return GuiScreen == null && !(CursorVisible = !Focused);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ShaderManager.cleanup();

            ModelManager.cleanup();
            TextureManager.cleanUp();

            if (World != null)
                WorldLoader.SaveWorld(World);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (!KeysDown.Contains(e.Key))
                KeysDown.Add(e.Key);

            switch (e.Key)
            {
                case Key.R:
                    if (e.Control)
                    {
                        ShaderManager.reload();
                        SettingsManager.Load();

                        WorldRenderer.RenderDistance = SettingsManager.GetInt("renderdistance");
                        _sensitivity = SettingsManager.GetFloat("sensitivity");

                        if (e.Shift)
                        {
                            TextureManager.reload();
                            World?.DestroyChunkModels();
                        }
                    }

                    break;
                case Key.Escape:
                    if (GuiScreen is GuiScreenMainMenu)
                        return;

                    if (GuiScreen != null)
                        CloseGuiScreen();
                    else
                    {
                        OpenGuiScreen(new GuiScreenIngameMenu());
                    }

                    break;
                case Key.F11:
                    if (WindowState != WindowState.Fullscreen)
                    {
                        _lastWindowState = WindowState;
                        WindowState = WindowState.Fullscreen;
                    }
                    else
                        WindowState = _lastWindowState;

                    break;
            }

            if (GuiScreen == null)
            {
                for (var i = 0; i < 9; i++)
                {
                    if (e.Key == Key.Number1 + i)
                    {
                        Player?.setSelectedSlot(i);

                        break;
                    }
                }
            }

            if (e.Key == (Key.LAlt | Key.F4))
                Exit();
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            KeysDown.Remove(e.Key);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.IsPressed)
            {
                if (GuiScreen == null)
                {
                    GetMouseOverObject();

                    if (MouseOverObject.hit is EnumBlock)
                    {
                        var pos = MouseOverObject.blockPos;

                        //pickBlock
                        if (e.Button == MouseButton.Middle)
                        {
                            var clickedBlock = World.GetBlock(pos);

                            if (clickedBlock != EnumBlock.AIR)
                            {
                                Player.setItemStackInSelectedSlot(new ItemStack(new ItemBlock(clickedBlock), 1,
                                    World.GetMetadata(pos)));
                            }
                        }

                        //place/interact
                        if (e.Button == MouseButton.Right) //TODO move this code to the player class
                        {
                            var block = World.GetBlock(pos);
                            var model = ModelRegistry.getModelForBlock(block, World.GetMetadata(pos));

                            if (model != null && model.canBeInteractedWith)
                            {
                                switch (block)
                                {
                                    case EnumBlock.FURNACE:
                                    case EnumBlock.CRAFTING_TABLE:
                                        OpenGuiScreen(new GuiScreenCrafting());
                                        break;
                                }
                            }
                            else if (Player.getEquippedItemStack()?.Item is ItemBlock itemBlock)
                            {
                                pos = pos.Offset(MouseOverObject.sideHit);

                                var blockAtPos = World.GetBlock(pos);

                                var heldBlock = itemBlock.getBlock();
                                var blockBb = ModelRegistry.getModelForBlock(heldBlock, World.GetMetadata(pos))
                                    .boundingBox.offset(pos.ToVec());

                                if (blockAtPos == EnumBlock.AIR && World.GetIntersectingEntitiesBBs(blockBb).Count == 0)
                                {
                                    var posUnder = pos.Offset(FaceSides.Down);

                                    var blockUnder = World.GetBlock(posUnder);
                                    var blockAbove = World.GetBlock(pos.Offset(FaceSides.Up));

                                    if (blockUnder == EnumBlock.GRASS && heldBlock != EnumBlock.GLASS)
                                        World.SetBlock(posUnder, EnumBlock.DIRT, 0);
                                    if (blockAbove != EnumBlock.AIR && blockAbove != EnumBlock.GLASS && heldBlock == EnumBlock.GRASS)
                                        World.SetBlock(pos, EnumBlock.DIRT, 0);
                                    else
                                        World.SetBlock(pos, heldBlock, Player.getEquippedItemStack().Meta);
                                }
                            }
                        }

                        //break
                        if (e.Button == MouseButton.Left)
                            World.SetBlock(pos, EnumBlock.AIR, 0);
                    }
                }
                else
                {
                    var state = OpenTK.Input.Mouse.GetCursorState();
                    var point = PointToClient(new Point(state.X, state.Y));

                    GuiScreen.onMouseClick(point.X, point.Y);
                }
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            while (_glContextQueue.Count > 0)
            {
                if (_glContextQueue.TryDequeue(out var func))
                    func?.Invoke();
            }

            if (_timer.ElapsedMilliseconds > 50 - e.Time * 1000)
            {
                GameLoop();
                _timer.Restart();
            }

            RenderScreen(GetRenderPartialTicks());

            _fpsCounter++;

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
            GL.Ortho(0, ClientRectangle.Width, ClientRectangle.Height, 0, NearPlane, FarPlane);

            ShaderManager.updateProjectionMatrix();
        }

        private void GameLoop()
        {
            if (GuiScreen == null && !Focused)
                OpenGuiScreen(new GuiScreenIngameMenu());

            var wheelValue = Mouse.WheelPrecise;

            if (Player != null && GuiScreen == null)
            {
                if (wheelValue < _mouseWheelLast)
                    Player.selectNextItem();
                else if (wheelValue > _mouseWheelLast)
                    Player.selectPreviousItem();

                if (World?.GetChunk(new BlockPos(Player.pos).ChunkPos()) == null)
                    Player.motion = Vector3.Zero;
            }

            _mouseWheelLast = wheelValue;

            World?.UpdateEntities();
        }

        private void GetMouseOverObject()
        {
            var radius = 5;

            var final = new MouseOverObject();

            var dist = float.MaxValue;

            var camPos = Vector3.One * 0.5f + Camera.pos;

            for (var z = -radius; z <= radius; z++)
            {
                for (var y = -radius; y <= radius; y++)
                {
                    for (var x = -radius; x <= radius; x++)
                    {
                        var vec = camPos;
                        vec.X += x;
                        vec.Y += y;
                        vec.Z += z;

                        var f = (vec - Camera.pos).LengthFast;

                        if (f <= radius + 0.5f)
                        {
                            var pos = new BlockPos(vec);
                            var block = World.GetBlock(pos);

                            if (block != EnumBlock.AIR)
                            {
                                var model = ModelRegistry.getModelForBlock(block, World.GetMetadata(pos));
                                var bb = model.boundingBox.offset(pos.ToVec());

                                var hitSomething = RayHelper.rayIntersectsBB(Camera.pos,
                                    Camera.getLookVec(), bb, out var hitPos, out var normal);

                                if (hitSomething)
                                {
                                    var sideHit = FaceSides.Up;

                                    if (normal.X < 0)
                                        sideHit = FaceSides.West;
                                    else if (normal.X > 0)
                                        sideHit = FaceSides.East;
                                    if (normal.Y < 0)
                                        sideHit = FaceSides.Down;
                                    else if (normal.Y > 0)
                                        sideHit = FaceSides.Up;
                                    if (normal.Z < 0)
                                        sideHit = FaceSides.North;
                                    else if (normal.Z > 0)
                                        sideHit = FaceSides.South;

                                    var p = new BlockPos(hitPos - normal * 0.5f);

                                    var l = Math.Abs((Camera.pos - (p.ToVec() + Vector3.One * 0.5f)).Length);

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

            MouseOverObject = final;
        }

        private void Init()
        {
            Console.WriteLine("DEBUG: loading models");

            var shader = new ShaderBlock("block", PrimitiveType.Quads);
            var shaderUnlit = new ShaderBlockUnlit("block_unlit", PrimitiveType.Quads);

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

            SettingsManager.Load();

            _sensitivity = SettingsManager.GetFloat("sensitivity");
            WorldRenderer.RenderDistance = SettingsManager.GetInt("renderdistance");

            OpenGuiScreen(new GuiScreenMainMenu());
        }

        private void Prepare()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.DepthClamp);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
        }

        private void RenderScreen(float partialTicks)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Prepare();

            if (World != null)
            {
                var viewMatrix = Camera.View;

                WorldRenderer.Render(World, viewMatrix);
                EntityRenderer.render(partialTicks);

                SkyboxRenderer.render(viewMatrix);
            }

            //render other gui
            if (Player != null)
            {
                GuiRenderer.renderCrosshair();
                GuiRenderer.renderHUD();
            }

            //render gui screen
            if (GuiScreen != null)
            {
                CursorVisible = true;
                GuiRenderer.render(GuiScreen);
            }
        }

        private void ResetMouse()
        {
            var middle = PointToScreen(new Point(ClientSize.Width / 2, ClientSize.Height / 2));
            OpenTK.Input.Mouse.SetPosition(middle.X, middle.Y);
        }

        private void RunUpdateThreads()
        {
            new Thread(() =>
                {
                    while (true)
                    {
                        if (!IsDisposed && Visible)
                        {
	                        GetMouseOverObject();
	                        World?.update(Player,WorldRenderer.RenderDistance);
                        }

                        Thread.Sleep(50);
                    }
                })
            { IsBackground = true }.Start();

            new Thread(() =>
                {
                    while (true)
                    {
                        if (!IsDisposed && Visible)
                        {
                            var state = Mouse.GetState();

                            var point = new Point(state.X, state.Y);

                            if (AllowInput())
                            {
                                var delta = new Point(_mouseLast.X - point.X, _mouseLast.Y - point.Y);

                                Camera.yaw -= delta.X / 1000f * _sensitivity;
                                Camera.pitch -= delta.Y / 1000f * _sensitivity;

                                ResetMouse();
                            }

                            _mouseLast = point;

                            if (KeysDown.Contains(Key.Space) && !_wasSpaceDown && Player.onGround)
                            {
                                _wasSpaceDown = true;
                                Player.motion.Y = 0.475F;
                            }
                            else if ((!KeysDown.Contains(Key.Space) || Player.onGround) && _wasSpaceDown)
                                _wasSpaceDown = false;

                            if (_frameTimer.ElapsedMilliseconds >= 1000)
                            {
                                _frameTimer.Restart();

                                Title = $"{_title} - FPS: {_fpsCounter}";
                                _fpsCounter = 0;
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

        public static void Load()
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

            Save();
        }

        public static void Save()
        {
            var dir = "SharpCraft_Data";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var file = dir + "/settings.txt";

            var sb = new StringBuilder();

            var keys = _settings.Keys.ToArray();

            for (var index = 0; index < keys.Length - 1; index++)
            {
                var key = keys[index];

                sb.AppendLine($"{key}={GetValue(key)}");
            }

            var last = _settings.Last();

            sb.Append($"{last.Key}={GetValue(last.Key)}");

            File.WriteAllText(file, sb.ToString());
        }

        public static string GetValue(string variable)
        {
            return _settings[variable];
        }

        public static int GetInt(string variable)
        {
            return int.Parse(GetValue(variable));
        }

        public static float GetFloat(string variable)
        {
            return float.Parse(GetValue(variable));
        }

        public static bool GetBool(string variable)
        {
            return bool.Parse(GetValue(variable));
        }
    }

    internal class Start
    {
        [STAThread]
        private static void Main(string[] args)
        {
	        ThreadPool.SetMinThreads(0, 0);
	        ThreadPool.SetMinThreads(Math.Max(1,Environment.ProcessorCount), 0);

            using (var game = new Game())
            {
                game.Run(30.0);
            }
        }
    }
}