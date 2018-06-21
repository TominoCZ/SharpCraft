using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.gui;
using SharpCraft.item;
using SharpCraft.model;
using SharpCraft.render;
using SharpCraft.render.shader;
using SharpCraft.texture;
using SharpCraft.util;
using SharpCraft.world;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Bitmap = System.Drawing.Bitmap;
using Point = OpenTK.Point;
using Rectangle = System.Drawing.Rectangle;
using Size = OpenTK.Size;

namespace SharpCraft
{
    internal class SharpCraft : GameWindow
    {
        //string _dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/.sharpcraft";
        private string _dir = "./";

        public string GameFolderDir
        {
            get
            {
                if (!Directory.Exists(_dir))
                    Directory.CreateDirectory(_dir);

                return _dir;
            }
            set
            {
                _dir = value;

                if (!Directory.Exists(_dir))
                    Directory.CreateDirectory(_dir);
            }
        }

        private readonly List<ModMain> installedMods = new List<ModMain>();

        private ItemRegistry itemRegistry;
        private BlockRegistry blockRegistry;

        public WorldRenderer WorldRenderer;
        public EntityRenderer EntityRenderer;
        public ParticleRenderer ParticleRenderer;
        public SkyboxRenderer SkyboxRenderer;
        public GuiRenderer GuiRenderer;
        public FontRenderer FontRenderer;
        public KeyboardState KeyboardState;

        //public HashSet<Key> KeysDown = new HashSet<Key>();
        public MouseOverObject MouseOverObject = new MouseOverObject();

        private MouseOverObject _lastMouseOverObject = new MouseOverObject();

        public EntityPlayerSP Player;

        public World World;

        public ConcurrentDictionary<BlockPos, DestroyProgress> DestroyProgresses =
            new ConcurrentDictionary<BlockPos, DestroyProgress>();

        public Random Random = new Random();

        private readonly List<MouseButton> _mouseButtonsDown = new List<MouseButton>();
        private readonly ConcurrentQueue<Action> _glContextQueue = new ConcurrentQueue<Action>();
        //private DateTime _updateTimer = DateTime.Now;
        private readonly Stopwatch _updateTimer = Stopwatch.StartNew();
        private DateTime _lastFpsDate = DateTime.Now;
        private WindowState _lastWindowState;
        private readonly Thread _renderThread = Thread.CurrentThread;

        public static SharpCraft Instance { get; private set; }

        public Camera Camera;

        public GuiScreen GuiScreen { get; private set; }

        private Point _mouseLast;
        private float _mouseWheelLast;

        public bool IsPaused { get; private set; }
        private bool _takeScreenshot;
        private bool _wasSpaceDown;
        private int _fpsCounter;
        private int _fpsCounterLast;
        private long _interactionTickCounter;
        private float _sensitivity = 1;
        private float _partialTicks;
        private double _timer;
        private readonly string _glVersion;

        private static string _title;

        public SharpCraft() : base(680, 480, new GraphicsMode(32, 32, 0, 0), _title, GameWindowFlags.Default,
            DisplayDevice.Default, 3, 3, GraphicsContextFlags.ForwardCompatible)
        {
            Instance = this;
            Camera = new Camera();

            VSync = VSyncMode.Off;
            MakeCurrent();

            _glVersion = GL.GetString(StringName.ShadingLanguageVersion);
            Title = _title = $"SharpCraft Alpha 0.0.4 [GLSL {_glVersion}]";

            //TargetRenderFrequency = 60;

            Console.WriteLine("DEBUG: stitching Textures");
            TextureManager.LoadTextures();

            Init();
        }

        private void Init()
        {
            GlSetup();

            itemRegistry = new ItemRegistry();
            blockRegistry = new BlockRegistry();

            SettingsManager.Load();

            WorldRenderer = new WorldRenderer();
            EntityRenderer = new EntityRenderer();
            ParticleRenderer = new ParticleRenderer();
            SkyboxRenderer = new SkyboxRenderer();
            GuiRenderer = new GuiRenderer();
            FontRenderer = new FontRenderer();

            LoadMods();

            RegisterItemsAndBlocks();

            //load settings
            _sensitivity = SettingsManager.GetFloat("sensitivity");
            WorldRenderer.RenderDistance = SettingsManager.GetInt("renderdistance");

            OpenGuiScreen(new GuiScreenMainMenu());
        }

        private void LoadMods() //WHY THE FUCK IS THIS IMPLEMENTED RIGHT NOW IN THIS GAME PHASE???!
        {
            if (Directory.Exists(_dir + "mods"))
            {
                string[] modFiles = Directory.GetFiles(_dir + "mods");

                foreach (string modFile in modFiles)
                {
                    IEnumerable<Type> modClassType = Assembly.LoadFile(modFile).GetModules()
                        .SelectMany(t => t.GetTypes())
                        .Where(t => t.IsSubclassOf(typeof(ModMain)));

                    if (modClassType.FirstOrDefault() is Type type && Activator.CreateInstance(type) is ModMain mm)
                    {
                        if (mm.ModInfo.IsValid)
                        {
                            Console.WriteLine("registered mod '" + mm.ModInfo.Name + "'!");

                            installedMods.Add(mm);
                        }
                        else
                        {
                            Console.WriteLine("registering mod '" + mm.ModInfo.Name + "' failed!");
                        }
                    }
                }
            }
        }

        private void RegisterItemsAndBlocks()
        {
            blockRegistry.Put(new BlockAir());
            blockRegistry.Put(new BlockStone());
            blockRegistry.Put(new BlockGrass());
            blockRegistry.Put(new BlockDirt());
            blockRegistry.Put(new BlockCobbleStone());
            blockRegistry.Put(new BlockPlanks());
            blockRegistry.Put(new BlockBedrock());
            blockRegistry.Put(new BlockLog());
            blockRegistry.Put(new BlockLeaves());
            blockRegistry.Put(new BlockGlass());
            blockRegistry.Put(new BlockCraftingTable());
            blockRegistry.Put(new BlockFurnace());
            blockRegistry.Put(new BlockSlab());
            blockRegistry.Put(new BlockRare());
            blockRegistry.Put(new BlockLadder());
            blockRegistry.Put(new BlockTallGrass());

            //POST - MOD Blocks and Items
            foreach (ModMain mod in installedMods)
            {
                mod.OnItemsAndBlocksRegistry(new RegistryEventArgs(blockRegistry, itemRegistry));
            }

            foreach (var block in BlockRegistry.AllBlocks())
            {
                itemRegistry.Put(new ItemBlock(block));
            }

            JsonModelLoader loader = new JsonModelLoader(Block.DefaultShader);

            blockRegistry.RegisterBlocksPost(loader);
            itemRegistry.RegisterItemsPost(loader);
        }

        public void StartGame()
        {
            World loadedWorld = WorldLoader.LoadWorld("MyWorld");

            if (loadedWorld == null)
            {
                Console.WriteLine("DEBUG: generating World");

                BlockPos playerPos = new BlockPos(MathUtil.NextFloat(-100, 100), 10, MathUtil.NextFloat(-100, 100));

                World = new World("MyWorld", "Tomlow's Fuckaround",
                    SettingsManager.GetValue("worldseed").GetHashCode());

                Player = new EntityPlayerSP(World,
                    new Vector3(playerPos.X, World.GetHeightAtPos(playerPos.X, playerPos.Z), playerPos.Z));

                World.AddEntity(Player);

                Player.SetItemStackInInventory(0, new ItemStack(new ItemBlock(BlockRegistry.GetBlock<BlockCraftingTable>())));
                Player.SetItemStackInInventory(1, new ItemStack(new ItemBlock(BlockRegistry.GetBlock<BlockFurnace>())));
                Player.SetItemStackInInventory(2, new ItemStack(new ItemBlock(BlockRegistry.GetBlock<BlockCobbleStone>())));
                Player.SetItemStackInInventory(3, new ItemStack(new ItemBlock(BlockRegistry.GetBlock<BlockPlanks>())));
                Player.SetItemStackInInventory(4, new ItemStack(new ItemBlock(BlockRegistry.GetBlock<BlockGlass>())));
                Player.SetItemStackInInventory(5, new ItemStack(new ItemBlock(BlockRegistry.GetBlock<BlockCraftingTable>())));
                Player.SetItemStackInInventory(6, new ItemStack(new ItemBlock(BlockRegistry.GetBlock<BlockSlab>())));
                Player.SetItemStackInInventory(7, new ItemStack(new ItemBlock(BlockRegistry.GetBlock<BlockLadder>())));
                Player.SetItemStackInInventory(8, new ItemStack(new ItemBlock(BlockRegistry.GetBlock<BlockTallGrass>())));
            }
            else
            {
                World = loadedWorld;
            }

            ResetMouse();

            MouseState state = OpenTK.Input.Mouse.GetState();
            _mouseLast = new Point(state.X, state.Y);
            //World.setBlock(new BlockPos(player.Pos), EnumBlock.RARE, 1, true); //test of block metadata, works perfectly
        }

        private void GameLoop()
        {
            TargetRenderFrequency = Focused ? 0 : 30;

            if (GuiScreen == null && !Focused)
                OpenGuiScreen(new GuiScreenIngameMenu());

            float wheelValue = Mouse.WheelPrecise;

            if (Player != null) // && GuiScreen == null)
            {
                if (AllowInput())
                {
                    if (wheelValue < _mouseWheelLast)
                        Player.SelectNextItem();
                    else if (wheelValue > _mouseWheelLast)
                        Player.SelectPreviousItem();

                    if (World?.GetChunk(new BlockPos(Player.Pos).ChunkPos()) == null)
                        Player.Motion = Vector3.Zero;

                    bool lmb = _mouseButtonsDown.Contains(MouseButton.Left);
                    bool rmb = _mouseButtonsDown.Contains(MouseButton.Right);

                    if (lmb || rmb)
                    {
                        _interactionTickCounter++;

                        BlockPos lastPos = _lastMouseOverObject.blockPos;

                        if (lmb && _lastMouseOverObject.hit == HitType.Block)
                        {
                            ParticleRenderer.SpawnDiggingParticle(_lastMouseOverObject);

                            if (_lastMouseOverObject.hit == MouseOverObject.hit && lastPos == MouseOverObject.blockPos)
                            {
                                if (!DestroyProgresses.TryGetValue(lastPos, out DestroyProgress progress))
                                {
                                    if (World?.GetBlockState(lastPos).Block.Hardness != -1)
                                    {
                                        DestroyProgresses.TryAdd(lastPos,
                                            progress = new DestroyProgress(lastPos, Player));
                                        progress.Progress = 0;
                                    }
                                }
                                else
                                {
                                    progress.Progress++;
                                }

                                if (progress.Destroyed)
                                    DestroyProgresses.TryRemove(progress.Pos, out DestroyProgress removed);
                            }
                            else ResetDestroyProgress(Player);
                        }

                        _lastMouseOverObject = MouseOverObject;

                        if (_interactionTickCounter % 4 == 0)
                        {
                            GetMouseOverObject();

                            if (rmb)
                                Player.PlaceBlock();
                        }
                    }
                    else
                    {
                        _interactionTickCounter = 0;
                        ResetDestroyProgress(Player);
                    }
                }
                else
                    ResetDestroyProgress(Player);
            }

            _mouseWheelLast = wheelValue;

            World?.Update(Player, WorldRenderer.RenderDistance);

            WorldRenderer?.Update();
            SkyboxRenderer?.Update();
            ParticleRenderer?.TickParticles();
        }

        private void GlSetup()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.DepthClamp);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.CullFace(CullFaceMode.Back);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.ActiveTexture(TextureUnit.Texture0);
        }

        public void GetMouseOverObject()
        {
            if (World == null)
                return;

            float radius = 5.5f;

            MouseOverObject final = new MouseOverObject();

            float dist = float.MaxValue;

            Vector3 camPos = Vector3.One * 0.5f + Camera.pos;

            var air = BlockRegistry.GetBlock<BlockAir>();

            for (float z = -radius; z <= radius; z++)
            {
                for (float y = -radius; y <= radius; y++)
                {
                    for (float x = -radius; x <= radius; x++)
                    {
                        Vector3 vec = camPos;
                        vec.X += x;
                        vec.Y += y;
                        vec.Z += z;

                        float f = (vec - Camera.pos).LengthFast;

                        if (f <= radius + 0.5f)
                        {
                            BlockPos pos = new BlockPos(vec);
                            BlockState state = World.GetBlockState(pos);

                            if (state.Block != air)
                            {
                                AxisAlignedBB bb = state.Block.BoundingBox.offset(pos.ToVec());

                                bool hitSomething = RayHelper.RayIntersectsBB(Camera.pos,
                                    Camera.GetLookVec(), bb, out Vector3 hitPos, out Vector3 normal);

                                if (hitSomething)
                                {
                                    FaceSides sideHit = FaceSides.Null;

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

                                    BlockPos p = new BlockPos(hitPos - normal * bb.size / 2);

                                    if (sideHit == FaceSides.Null)
                                        continue;

                                    float l = Math.Abs((Camera.pos - (p.ToVec() + bb.size / 2)).Length);

                                    if (l < dist)
                                    {
                                        dist = l;

                                        final.hit = HitType.Block;
                                        final.hitVec = hitPos;
                                        final.blockPos = p;
                                        final.normal = normal;
                                        final.sideHit = sideHit;

                                        final.boundingBox = bb;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            MouseOverObject = final;
        }

        private void ResetDestroyProgress(EntityPlayerSP player)
        {
            foreach (DestroyProgress progress in DestroyProgresses.Values)
            {
                if (progress.Player == player)
                    DestroyProgresses.TryRemove(progress.Pos, out DestroyProgress removed);
            }
        }

        private void ResetMouse()
        {
            Point middle = PointToScreen(new Point(ClientSize.Width / 2, ClientSize.Height / 2));
            OpenTK.Input.Mouse.SetPosition(middle.X, middle.Y);
        }

        public void RunGlTasks()
        {
            if (_glContextQueue.Count == 0)
                return;

            while (_glContextQueue.Count > 0)
            {
                if (_glContextQueue.TryDequeue(out Action func))
                    func?.Invoke();
            }

            GL.Flush();
        }

        private void HandleMouseMovement()
        {
            MouseState state = Mouse.GetState();

            Point point = new Point(state.X, state.Y);

            if (AllowInput())
            {
                Point delta = new Point(_mouseLast.X - point.X, _mouseLast.Y - point.Y);

                Camera.yaw -= delta.X / 1000f * _sensitivity;
                Camera.pitch -= delta.Y / 1000f * _sensitivity;

                //ResetMouse();

                if (KeyboardState.IsKeyDown(Key.Space) && !_wasSpaceDown && Player.onGround)
                {
                    _wasSpaceDown = true;
                    Player.Motion.Y = 0.475F;
                }
                else if ((!KeyboardState.IsKeyDown(Key.Space) || Player.onGround) && _wasSpaceDown)
                    _wasSpaceDown = false;
            }

            _mouseLast = point;
        }

        public void RunGlContext(Action a)
        {
            if (_renderThread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId) a();
            else _glContextQueue.Enqueue(a);
        }

        private bool AllowInput()
        {
            return GuiScreen == null && !(CursorVisible = !Focused);
        }

        public void OpenGuiScreen(GuiScreen guiScreen)
        {
            if (guiScreen == null)
            {
                CloseGuiScreen();
                return;
            }

            GuiScreen = guiScreen;

            if (guiScreen.DoesGuiPauseGame)
                IsPaused = true;

            Point middle = new Point(ClientRectangle.Width / 2, ClientRectangle.Height / 2);
            middle = PointToScreen(middle);

            OpenTK.Input.Mouse.SetPosition(middle.X, middle.Y);
        }

        public void CloseGuiScreen()
        {
            if (GuiScreen == null)
                return;

            if (GuiScreen.DoesGuiPauseGame)
                IsPaused = false;

            GuiScreen.OnClose();
            GuiScreen = null;

            CursorVisible = false;
        }

        private void CaptureScreen()
        {
            Bitmap bmp = new Bitmap(ClientSize.Width, ClientSize.Height,
                System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            using (bmp)
            {
                BitmapData bData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadWrite, bmp.PixelFormat);

                GL.ReadBuffer(ReadBufferMode.Back);
                // read the data directly into the bitmap's buffer (bitmap is stored in BGRA)
                GL.ReadPixels(0, 0, ClientSize.Width, ClientSize.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                    PixelType.UnsignedByte, bData.Scan0);

                bmp.UnlockBits(bData);
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

                DateTime time = DateTime.UtcNow;

                string dir = $"{GameFolderDir}/screenshots";
                string file =
                    $"{dir}/{time.Year}-{time.Month}-{time.Day}_{time.TimeOfDay.Hours}.{time.TimeOfDay.Minutes}.{time.TimeOfDay.Seconds}.png";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (FileStream fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite,
                    FileShare.ReadWrite))
                {
                    bmp.Save(fs, ImageFormat.Png);
                }
            }
        }

        public int GetFPS()
        {
            return _fpsCounterLast;
        }

        public float GetPartialTicksForRender()
        {
            return _partialTicks;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            DateTime now = DateTime.Now;

            _partialTicks = (float)(_updateTimer.Elapsed.TotalSeconds / TargetUpdatePeriod);

            if ((now - _lastFpsDate).TotalMilliseconds >= 1000)
            {
                _fpsCounterLast = _fpsCounter;
                _fpsCounter = 0;
                _lastFpsDate = now;
            }

            RunGlTasks();

            HandleMouseMovement();
            Camera.UpdateViewMatrix();

            //RENDER SCREEN
            if (World != null)
            {
                WorldRenderer.Render(World, _partialTicks);
                ParticleRenderer.Render(_partialTicks);
                EntityRenderer.Render(_partialTicks);
                SkyboxRenderer.Render(_partialTicks);
            }

            //render other gui
            if (Player != null)
            {
                GuiRenderer.RenderCrosshair();
                GuiRenderer.RenderHUD();
            }

            //render gui screen
            if (GuiScreen != null)
            {
                CursorVisible = true;
                GuiRenderer.Render(GuiScreen);
            }

            if (_takeScreenshot)
            {
                _takeScreenshot = false;

                CaptureScreen();
            }

            SwapBuffers();

            _fpsCounter++;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            _updateTimer.Restart();

            if (!IsDisposed && Visible)
                GetMouseOverObject();

            GameLoop();

            //_updateTimer = DateTime.Now;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.IsPressed)
            {
                if (GuiScreen == null)
                {
                    GetMouseOverObject();

                    //pickBlock
                    if (e.Button == MouseButton.Middle)
                        Player?.PickBlock();

                    //place/interact
                    if (e.Button == MouseButton.Right || e.Button == MouseButton.Left)
                    {
                        _interactionTickCounter = 0;
                        Player?.OnClick(e.Button);
                    }
                }
                else
                {
                    MouseState state = OpenTK.Input.Mouse.GetCursorState();
                    Point point = PointToClient(new Point(state.X, state.Y));

                    GuiScreen.OnMouseClick(point.X, point.Y, e.Button);
                }

                if (!_mouseButtonsDown.Contains(e.Button))
                    _mouseButtonsDown.Add(e.Button);
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            _mouseButtonsDown.Remove(e.Button);

            if (e.Button == MouseButton.Left && AllowInput())
                ResetDestroyProgress(Player);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Key.P:
                    Player?.World?.AddWaypoint(new BlockPos(Player.Pos).Offset(FaceSides.Up),
                        new OpenTK.Color(255, 0, 0, 127), "TEST");
                    break;

                case Key.Escape:
                    if (GuiScreen is GuiScreenMainMenu || KeyboardState.IsKeyDown(Key.Escape))
                        return;

                    if (GuiScreen != null)
                        CloseGuiScreen();
                    else
                        OpenGuiScreen(new GuiScreenIngameMenu());
                    break;

                case Key.E:
                    if (KeyboardState.IsKeyDown(Key.E))
                        return;

                    if (GuiScreen is GuiScreenInventory)
                    {
                        CloseGuiScreen();
                        return;
                    }

                    if (GuiScreen == null)
                        OpenGuiScreen(new GuiScreenInventory());
                    break;

                case Key.F2:
                    _takeScreenshot = true;
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

            if (AllowInput())
            {
                switch (e.Key)
                {
                    case Key.Q:
                        if (KeyboardState.IsKeyDown(Key.Q))
                            return;

                        if (e.Control)
                            Player.DropHeldStack();
                        else
                            Player?.DropHeldItem();
                        break;

                    case Key.R:
                        if (!e.Control || KeyboardState.IsKeyDown(Key.R))
                            return;

                        Shader.ReloadAll();
                        SettingsManager.Load();
                        TextureManager.Reload();

                        WorldRenderer.RenderDistance = SettingsManager.GetInt("renderdistance");
                        _sensitivity = SettingsManager.GetFloat("sensitivity");

                        if (e.Shift)
                            World?.DestroyChunkModels();

                        break;
                }

                if (GuiScreen == null)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (e.Key == Key.Number1 + i)
                        {
                            Player?.SetSelectedSlot(i);

                            break;
                        }
                    }
                }
            }

            if (e.Alt && e.Key == Key.F4)
                Exit();

            KeyboardState = e.Keyboard;
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);

            KeyboardState = e.Keyboard;
        }

        protected override void OnResize(EventArgs e)
        {
            if (ClientSize.Width < 640)
                ClientSize = new Size(640, ClientSize.Height);
            if (ClientSize.Height < 480)
                ClientSize = new Size(ClientSize.Width, 480);

            GL.Viewport(ClientRectangle);
            //GL.MatrixMode(MatrixMode.Projection);
            //GL.LoadIdentity();
            //GL.Ortho(0, ClientRectangle.Width, ClientRectangle.Height, 0, Camera.NearPlane, Camera.FarPlane);

            Camera.UpdateProjectionMatrix();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Shader.DestroyAll();
            ModelManager.Cleanup();
            TextureManager.Destroy();

            if (World != null)
                WorldLoader.SaveWorld(World);

            base.OnClosing(e);
        }
    }
}