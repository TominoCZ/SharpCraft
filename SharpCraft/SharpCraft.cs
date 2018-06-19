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
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Bitmap = System.Drawing.Bitmap;
using Point = OpenTK.Point;
using Rectangle = System.Drawing.Rectangle;
using Size = OpenTK.Size;

namespace SharpCraft
{
    internal class ModInfo
    {
        public readonly string ID;

        public readonly string Name;
        public readonly string Version;
        public readonly string Author;

        public bool IsValid =>
            String.IsNullOrEmpty(ID) ||
            String.IsNullOrEmpty(Name) ||
            String.IsNullOrEmpty(Version) ||
            String.IsNullOrEmpty(Author);

        public ModInfo(string id, string name, string version, string author)
        {
            ID = id;
            Name = name;
            Version = version;
            Author = author;
        }
    }

    internal abstract class ModMain
    {
        public ModInfo ModInfo { get; protected set; }

        protected ModMain(ModInfo modInfo)
        {
            ModInfo = modInfo;
        }

        public abstract void OnItemsAndBlocksRegistry(ItemsAndBlockRegistryEventArgs args);
    }

    internal class ItemsAndBlockRegistryEventArgs : EventArgs
    {
        private readonly Action<Item> _funcRegisterItem;
        private readonly Action<Block> _funcRegisterBlock;

        public ItemsAndBlockRegistryEventArgs(BlockRegistry blockRegistry, ItemRegistry itemRegistry)
        {
            _funcRegisterBlock = blockRegistry.Put;
            _funcRegisterItem = itemRegistry.Put;
        }

        public void Register(Block block)
        {
            _funcRegisterBlock(block);
        }

        public void Register(Item item)
        {
            _funcRegisterItem(item);
        }
    }

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

        public EventHandler<ItemsAndBlockRegistryEventArgs> OnBlockRegistryEvent;

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

        private readonly List<MouseButton> _mouseButtonsDown = new List<MouseButton>();
        private readonly ConcurrentQueue<Action> _glContextQueue = new ConcurrentQueue<Action>();
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

            #region model loading

            //Console.WriteLine("DEBUG: loading models");

            /*
            ModelBlock missingModel = new ModelBlock(EnumBlock.MISSING, shader);
            ModelBlock stoneModel = new ModelBlock(EnumBlock.STONE, shader);
            ModelBlock grassModel = new ModelBlock(EnumBlock.GRASS, shader);
            ModelBlock dirtModel = new ModelBlock(EnumBlock.DIRT, shader);
            ModelBlock cobblestoneModel = new ModelBlock(EnumBlock.COBBLESTONE, shader);
            ModelBlock planksModel = new ModelBlock(EnumBlock.PLANKS, shader);
            ModelBlock craftingTableModel = new ModelBlock(EnumBlock.CRAFTING_TABLE, shader, true);
            ModelBlock furnaceModel = new ModelBlock(EnumBlock.FURNACE, shader, true);
            ModelBlock bedrockModel = new ModelBlock(EnumBlock.BEDROCK, shader);
            ModelBlock rareModel = new ModelBlock(EnumBlock.RARE, shader);
            ModelBlock glassModel = new ModelBlock(EnumBlock.GLASS, shader, false, true);
            ModelBlock logModel = new ModelBlock(EnumBlock.LOG, shader);
            ModelBlock leavesModel = new ModelBlock(EnumBlock.LEAVES, shader, false, true);

            ModelBlock xrayModel = new ModelBlock(EnumBlock.XRAY, shader);
            

            ModelRegistry.RegisterBlockModel(missingModel, 0);

            ModelRegistry.RegisterBlockModel(stoneModel, 0);
            ModelRegistry.RegisterBlockModel(grassModel, 0);
            ModelRegistry.RegisterBlockModel(dirtModel, 0);
            ModelRegistry.RegisterBlockModel(cobblestoneModel, 0);
            ModelRegistry.RegisterBlockModel(planksModel, 0);
            ModelRegistry.RegisterBlockModel(craftingTableModel, 0);
            ModelRegistry.RegisterBlockModel(furnaceModel, 0);

            ModelRegistry.RegisterBlockModel(bedrockModel, 0);
            ModelRegistry.RegisterBlockModel(rareModel, 0);
            ModelRegistry.RegisterBlockModel(glassModel, 0);
            ModelRegistry.RegisterBlockModel(logModel, 0);
            ModelRegistry.RegisterBlockModel(leavesModel, 0);

            ModelRegistry.RegisterBlockModel(xrayModel, 0);
            */
            #endregion model loading

            SettingsManager.Load();

            WorldRenderer = new WorldRenderer();
            EntityRenderer = new EntityRenderer();
            ParticleRenderer = new ParticleRenderer();
            SkyboxRenderer = new SkyboxRenderer();
            GuiRenderer = new GuiRenderer();
            FontRenderer = new FontRenderer();

            //timer.InfiniteFps = true;

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
            blockRegistry.Put(new BlockGrass());
            blockRegistry.Put(new BlockAir());

            //POST - MOD Blocks and Items
            foreach (ModMain mod in installedMods)
            {
                mod.OnItemsAndBlocksRegistry(new ItemsAndBlockRegistryEventArgs(blockRegistry, itemRegistry));
            }

            blockRegistry.RegisterBlocksPost();
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

                //Player.SetItemStackInInventory(0, new ItemStack(new ItemBlock(BlockRegistry.GetBlock("crafting_table"))));
                //Player.SetItemStackInInventory(1, new ItemStack(new ItemBlock(BlockRegistry.GetBlock("furnace"))));
                //Player.SetItemStackInInventory(2, new ItemStack(new ItemBlock(BlockRegistry.GetBlock("cobblestone"))));
                //Player.SetItemStackInInventory(3, new ItemStack(new ItemBlock(BlockRegistry.GetBlock("planks"))));
                //Player.SetItemStackInInventory(4, new ItemStack(new ItemBlock(BlockRegistry.GetBlock("glass"))));
                //Player.SetItemStackInInventory(5, new ItemStack(new ItemBlock(BlockRegistry.GetBlock("crafting_table"))));
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
                                    DestroyProgresses.TryAdd(lastPos, progress = new DestroyProgress(lastPos, Player));
                                else
                                    progress.Progress++;

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
                                ModelBlock model = JsonModelLoader.GetModelForBlock(state.Block.UnlocalizedName);

                                AxisAlignedBB bb = model.boundingBox.offset(pos.ToVec());

                                bool hitSomething = RayHelper.rayIntersectsBB(Camera.pos,
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

                                    BlockPos p = new BlockPos(hitPos - normal * 0.5f);

                                    if (sideHit == FaceSides.Null)
                                        continue;

                                    float l = Math.Abs((Camera.pos - (p.ToVec() + Vector3.One * 0.5f)).Length);

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

            _partialTicks = (float)(_updateTimer.Elapsed.TotalMilliseconds / 50.0);

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

    internal class SettingsManager
    {
        private static readonly Dictionary<string, string> _settings = new Dictionary<string, string>();

        static SettingsManager()
        {
            _settings.Add("sensitivity", "1");
            _settings.Add("renderdistance", "1");
            _settings.Add("worldseed", "yeet");
        }

        public static void Load()
        {
            string file = SharpCraft.Instance.GameFolderDir + "/settings.txt";

            if (File.Exists(file))
            {
                IEnumerable<string> data = File.ReadLines(file);

                foreach (string line in data)
                {
                    string parsed = line.Trim().Replace(" ", "").ToLower();
                    string[] split = parsed.Split('=');

                    if (split.Length < 2)
                        continue;

                    string variable = split[0];
                    string value = split[1];

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
            string file = SharpCraft.Instance.GameFolderDir + "/settings.txt";

            StringBuilder sb = new StringBuilder();

            string[] keys = _settings.Keys.ToArray();

            for (int index = 0; index < keys.Length - 1; index++)
            {
                string key = keys[index];

                sb.AppendLine($"{key}={GetValue(key)}");
            }

            KeyValuePair<string, string> last = _settings.Last();

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
            ThreadPool.SetMaxThreads(1000, 1000);

            using (SharpCraft game = new SharpCraft())
            {
                game.Run(20);
            }
        }
    }

    static class CubeModelBuilder
    {
        private static readonly Dictionary<FaceSides, float[]> _cube = new Dictionary<FaceSides, float[]>();

        static CubeModelBuilder()
        {
            _cube.Add(FaceSides.North, new float[]
            {
                1, 1, 0,
                1, 0, 0,
                0, 0, 0,
                0, 1, 0
            });
            _cube.Add(FaceSides.South, new float[]
            {
                0, 1, 1,
                0, 0, 1,
                1, 0, 1,
                1, 1, 1
            });
            _cube.Add(FaceSides.East, new float[]
            {
                1, 1, 1,
                1, 0, 1,
                1, 0, 0,
                1, 1, 0
            });
            _cube.Add(FaceSides.West, new float[]
            {
                0, 1, 0,
                0, 0, 0,
                0, 0, 1,
                0, 1, 1
            });
            _cube.Add(FaceSides.Up, new float[]
            {
                0, 1, 0,
                0, 1, 1,
                1, 1, 1,
                1, 1, 0
            });
            _cube.Add(FaceSides.Down, new float[]
            {
                0, 0, 1,
                0, 0, 0,
                1, 0, 0,
                1, 0, 1
            });
        }

        public static void AppendCubeModel(JsonCube cube, Dictionary<TextureType, string> modelTextures, Dictionary<string, TextureMapElement> textureMap, ref float[] vertexes, ref float[] normals, ref float[] uvs, int n)
        {
            int startIndex2 = n * 48;
            int startIndex3 = n * 72;

            int faceIndex = 0;

            foreach (var pair in cube.Faces)
            {
                int uvIndex = 8 * faceIndex;

                TextureType side = pair.Key;
                JsonCubeFaceUv textureNode = pair.Value;

                //textureNode.Texture isn't the name of the texture file! it is '#side', '#block', '#bottom', ... TODO - if '#' is not present, use the texture from the texture map
                if (textureNode.Texture[0] == '#')
                {
                    string sideName = textureNode.Texture.Substring(1).ToLower();

                    if (Enum.TryParse(sideName, out TextureType sideParsed))
                    {
                        string textureNameForFace = modelTextures[sideParsed];

                        if (textureMap.TryGetValue(textureNameForFace, out var tme))
                        {
                            var minU = tme.UVMin.X + textureNode.UV[0] / 16f;
                            var minV = tme.UVMin.Y + textureNode.UV[1] / 16f;
                            var maxU = tme.UVMax.X + textureNode.UV[2] / 16f;
                            var maxV = tme.UVMax.Y + textureNode.UV[3] / 16f;

                            uvs[startIndex2 + uvIndex] = minU;
                            uvs[startIndex2 + uvIndex + 1] = minV;

                            uvs[startIndex2 + uvIndex + 2] = minU;
                            uvs[startIndex2 + uvIndex + 3] = maxV;

                            uvs[startIndex2 + uvIndex + 4] = maxU;
                            uvs[startIndex2 + uvIndex + 5] = maxV;

                            uvs[startIndex2 + uvIndex + 6] = maxU;
                            uvs[startIndex2 + uvIndex + 7] = minV;
                        }
                    }
                }

                AppendFace(side, cube.From, cube.To, ref vertexes, ref normals, startIndex3 + 12 * faceIndex);

                faceIndex++;
            }
        }

        public static void AppendFace(TextureType side, int[] from, int[] to, ref float[] vertexes, ref float[] normals, int startIndex)
        {
            FaceSides normal = FaceSides.Parse(side); //TextureType parsed to FaceSides, also a normal of this face
            float[] unitFace = _cube[normal]; //one side of the cube in unit size

            float x = from[0] / 16f;
            float y = from[1] / 16f;
            float z = from[2] / 16f;

            float sx = to[0] / 16f - x; //the size of the cube part
            float sy = to[1] / 16f - y;
            float sz = to[2] / 16f - z;

            for (var i = 0; i < unitFace.Length; i += 3)
            {
                float vx = unitFace[i] * sx;
                float vy = unitFace[i + 1] * sy;
                float vz = unitFace[i + 2] * sz;

                vertexes[startIndex + i] = x + vx;
                vertexes[startIndex + i + 1] = y + vy;
                vertexes[startIndex + i + 2] = z + vz;

                normals[startIndex + i] = normal.x;
                normals[startIndex + i + 1] = normal.y;
                normals[startIndex + i + 2] = normal.z;
            }
        }
    }

    public class JsonModelLoader
    {
        public static int TEXTURE_BLOCKS;

        private static Dictionary<string, BlockModel> _blockModels = new Dictionary<string, BlockModel>();

        private static readonly Dictionary<string, ModelBlock> _modelsForRender = new Dictionary<string, ModelBlock>();

        private static JsonModelLoader _instance;

        public JsonModelLoader(Shader<ModelBlock> blockShader)
        {
            if (_instance != null)
                throw new Exception("There can only be one instance of the JsonModelLoader class!");

            _instance = this;
            TEXTURE_BLOCKS = StitchBlocks(blockShader);
            //here we have the block models already loaded

            //foreach (var model in _blockModels)
            //{
            //var texturedModel = CreateTexturedCubeModel(model.Value.Sprites);

            //BlockModelForRender mfr = new BlockModelForRender(texturedModel, blockShader);

            //_modelsForRender.Add(model.Key, mfr);
            //}
        }

        private int StitchBlocks(Shader<ModelBlock> blockShader)
        {
            string dir = $"{SharpCraft.Instance.GameFolderDir}\\SharpCraft_Data\\assets\\models\\block";

            if (!Directory.Exists(dir))
                return 0;

            // string[] files = Directory.GetFiles(dir); //TODO - ONLY LOAD JSONS FOR REGISTERED BLOCKS!

            var listOfBlocks = BlockRegistry.AllBlocks();

            List<string> nonDuplicateTextures = new List<string>();

            var blockModels = new ConcurrentDictionary<string, BlockJSONModel>();

            foreach (var block in listOfBlocks)
            {
                string file = $"{dir}\\{block.UnlocalizedName}.json";

                if (!File.Exists(file))
                    continue;

                BlockJSONModel bjm = JsonConvert.DeserializeObject<BlockJSONModel>(File.ReadAllText(file));

                string blockName = Path.GetFileNameWithoutExtension(file);

                blockModels.TryAdd(blockName, bjm); //save what block is using what model

                foreach (var pair in bjm.Textures) //iterating over the textureMap in the Json model
                {
                    if (!nonDuplicateTextures.Contains(pair.Value))
                    {
                        nonDuplicateTextures.Add(pair.Value); //add the current texture name to a list of all textureMap if isn't already there
                    }
                }
            }

            var textureMapElements = new Dictionary<string, TextureMapElement>(); //each texture name has it's UV values TODO - maybe make a TextureMap class where this could be used

            var id = Stitch(nonDuplicateTextures.ToArray(), 16, textureMapElements); // stitch all textureMap, return the texture ID of the registered texture in VRAM

            //TODO - if json doesn't contain cube model, assume it's a full cube
            foreach (var pair in blockModels) //one model per registered block
            {
                string name = pair.Key;
                BlockJSONModel model = pair.Value;

                float[] vertexes = new float[72 * model.Cubes.Length];
                float[] normals = new float[72 * model.Cubes.Length];
                float[] uvs = new float[48 * model.Cubes.Length];

                for (var index = 0; index < model.Cubes.Length; index++)
                {
                    var cube = model.Cubes[index];

                    CubeModelBuilder.AppendCubeModel(cube, model.Textures, textureMapElements, ref vertexes, ref normals, ref uvs, index);
                }

                ModelBlock mb = new ModelBlock(blockShader, ModelManager.LoadBlockModelToVAO(vertexes, normals, uvs));

                _modelsForRender.Add(name, mb);
            }

            #region OLD
            /*
            foreach (var pair in textureMap) //iterate over every block name and it's textureMap
            {
                var blockName = pair.Key;
                var model = pair.Value;

                BlockModel blockModel = new BlockModel(); //create a model for the block

                if (model.Textures.TryGetValue(TextureType.Block, out string tex1)
                    && textureMap.TryGetValue(tex1, out TextureMapElement tp1)) // if json contains a 'block' texture, set that texture for all 6 sides first
                {
                    blockModel.SetSpriteForAllSides(tp1);
                }

                if (model.Textures.TryGetValue(TextureType.Side, out string tex2)
                    && textureMap.TryGetValue(tex2, out TextureMapElement tp2)) // if json contains a 'side' texture, set that texture for all 4 sides
                {
                    blockModel.SetSpriteForSides(tp2);
                }

                foreach (var texture in model.Textures) //lastly, load all textureMap like north, south,... and override if are set by 'side' or 'block'
                {
                    if (texture.Key != TextureType.Side
                        && textureMap.TryGetValue(texture.Value, out TextureMapElement sprite))
                        blockModel.AddSpriteForSide(texture.Key, sprite);
                }

                _blockModels.Add(blockName, blockModel); //register block model for name
            }*/

            #endregion

            return id;
        }

        private int Stitch(string[] textures, int textureSize, Dictionary<string, TextureMapElement> sprites)
        {
            Bitmap map = new Bitmap(256, 256);

            int id;

            using (map)
            {
                int countX = 0;
                int countY = 0;

                foreach (var texName in textures)
                {
                    Vector2 start = new Vector2((float)countX / map.Width, (float)countY / map.Height);
                    Vector2 end = start + new Vector2((float)textureSize / map.Width, (float)textureSize / map.Height);

                    TextureMapElement mapElement = new TextureMapElement(start, end);

                    WriteBitmap(map, texName, textureSize, ref countX, ref countY);

                    sprites.Add(texName, mapElement);
                }

                map.Save("debug.png");

                id = TextureManager.LoadTexture(map);
            }

            return id;
        }

        private void WriteBitmap(Bitmap textureMap, string texName, int textureSize, ref int countX, ref int countY)
        {
            Bitmap tex = new Bitmap(Bitmap.FromFile($"{SharpCraft.Instance.GameFolderDir}\\SharpCraft_Data\\assets\\Textures\\blocks\\{texName}.png"), textureSize, textureSize);

            using (Graphics g = Graphics.FromImage(textureMap))
            {
                g.DrawImage(tex, countX, countY);

                countX += textureSize;

                if (countX + textureSize > textureMap.Width)
                {
                    countX = 0;
                    countY += textureSize;
                }
            }
        }

        public static ModelBlock GetModelForBlock(string blockName)
        {
            _modelsForRender.TryGetValue(blockName, out var model);
            return model;
        }

        private Dictionary<FaceSides, RawQuad> CreateTexturedCubeModel(Dictionary<TextureType, TextureMapElement> sprites)
        {
            Dictionary<FaceSides, RawQuad> quads = new Dictionary<FaceSides, RawQuad>();

            foreach (var pair in sprites)
            {
                var data = ModelHelper.GetCubeSideVertexes(pair.Key);

                var uvNode = new TextureUVNode(pair.Value.UVMin, pair.Value.UVMax);

                RawQuad quad = new RawQuad(
                    data,
                    uvNode.ToArray(),
                    ModelHelper.CalculateNormals(data),
                    3);

                quads.Add(FaceSides.Parse(pair.Key), quad);
            }

            return quads;
        }
    }

    class BlockModelForRender : ModelBaked<ModelBlock> //TODO change
    {
        public BlockModelForRender(Dictionary<FaceSides, RawQuad> data, Shader<ModelBlock> shader) : base(null, shader)
        {
            RawModel = ModelManager.LoadBlockModelToVAO(data);
        }
    }

    class BlockModel
    {
        public Dictionary<TextureType, TextureMapElement> Sprites = new Dictionary<TextureType, TextureMapElement>();

        public void AddSpriteForSide(TextureType type, TextureMapElement mapElement) //TODO sides, not type -> cuz what about the particle texture??
        {
            if (type != TextureType.side && type != TextureType.block)
            {
                Sprites.Remove(type);
                Sprites.Add(type, mapElement);
            }
        }

        public void SetSpriteForAllSides(TextureMapElement mapElement)
        {
            Sprites.TryAdd(TextureType.top, mapElement);
            Sprites.TryAdd(TextureType.bottom, mapElement);

            SetSpriteForSides(mapElement);
        }

        public void SetSpriteForSides(TextureMapElement mapElement)
        {
            Sprites.TryAdd(TextureType.north, mapElement);
            Sprites.TryAdd(TextureType.south, mapElement);
            Sprites.TryAdd(TextureType.west, mapElement);
            Sprites.TryAdd(TextureType.east, mapElement);
        }
    }

    class TextureMapElement
    {
        public Vector2 UVMin { get; }
        public Vector2 UVMax { get; }

        public TextureMapElement(Vector2 uvMin, Vector2 uvMax)
        {
            UVMin = uvMin;
            UVMax = uvMax;
        }
    }

    public enum TextureType
    {
        top,
        bottom,
        north,
        south,
        west,
        east,
        block,
        side
    }

    public enum HitType
    {
        Nothing,
        Block,
        Entity
    }
}