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
        private string _dir = "./SharpCraft_Data";

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

        private readonly List<ModMain> _installedMods = new List<ModMain>();

        private ItemRegistry _itemRegistry;
        private BlockRegistry _blockRegistry;
        private RecipeRegistry _recipeRegistry;

        public WorldRenderer WorldRenderer;
        public EntityRenderer EntityRenderer;
        public ParticleRenderer ParticleRenderer;
        public SkyboxRenderer SkyboxRenderer;
        public GuiRenderer GuiRenderer;
        public FontRenderer FontRenderer;
        public KeyboardState KeyboardState;

        private FBO _frameBuffer;

        //public HashSet<Key> KeysDown = new HashSet<Key>();
        public MouseOverObject MouseOverObject = new MouseOverObject();

        private MouseOverObject _lastMouseOverObject = new MouseOverObject();

        public EntityPlayerSp Player;

        public World World;

        public ConcurrentDictionary<BlockPos, DestroyProgress> DestroyProgresses =
            new ConcurrentDictionary<BlockPos, DestroyProgress>();

        public Random Random = new Random();

        private readonly List<MouseButton> _mouseButtonsDown = new List<MouseButton>();
        private readonly ConcurrentQueue<Action> _glContextQueue = new ConcurrentQueue<Action>();
        private DateTime _updateTimer = DateTime.Now;
        private DateTime _lastFpsDate = DateTime.Now;
        private WindowState _lastWindowState;
        private readonly Thread _renderThread = Thread.CurrentThread;

        public static SharpCraft Instance { get; private set; }

        public Camera Camera;

        public GuiScreen GuiScreen { get; private set; }

        private Point _mouseLast;
        private float _mouseWheelLast;

        public bool IsPaused { get; private set; }
        public bool IsLocal { get; private set; } = true;

        private bool _takeScreenshot;
        private int _fpsCounter;
        private int _fpsCounterLast;
        private long _interactionTickCounter;
        private float _sensitivity = 1;

        private float _partialTicks;

        private static string _title;

        public SharpCraft() : base(680, 480, new GraphicsMode(32, 32, 0, 0), _title, GameWindowFlags.Default,
            DisplayDevice.Default, 3, 3, GraphicsContextFlags.ForwardCompatible)
        {
            Instance = this;
            Camera = new Camera();

            VSync = VSyncMode.Off;
            MakeCurrent();

            Title = _title = $"SharpCraft Alpha 0.0.4 [GLSL {GL.GetString(StringName.ShadingLanguageVersion)}]";

            Console.WriteLine("DEBUG: stitching Textures");
            TextureManager.LoadTextures();

            Init();
        }

        private void Init()
        {
            //TODO - just a test - WORKS!
            //_installedMods.Add(new TestMod());

            GlSetup();

            _itemRegistry = new ItemRegistry();
            _blockRegistry = new BlockRegistry();
            _recipeRegistry = new RecipeRegistry();

            WorldRenderer = new WorldRenderer();
            EntityRenderer = new EntityRenderer();
            GuiRenderer = new GuiRenderer();
            FontRenderer = new FontRenderer();

            SettingsManager.Load();

            LoadMods();

            RegisterItemsAndBlocks();

            OpenGuiScreen(new GuiScreenMainMenu());
        }

        private void GlSetup()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.DepthClamp);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.CullFace(CullFaceMode.Back);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.ActiveTexture(TextureUnit.Texture0);

            _frameBuffer = new FBO(Width, Height, false, 4);
        }

        private void LoadMods()
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

                            _installedMods.Add(mm);
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
            //register materails
            Material.RegisterMaterial(new Material("air", true));
            Material.RegisterMaterial(new Material("tallgrass", true));
            Material.RegisterMaterial(new Material("grass", false));
            Material.RegisterMaterial(new Material("dirt", false));
            Material.RegisterMaterial(new Material("stone", false));
            Material.RegisterMaterial(new Material("wood", false));

            _blockRegistry.Put(new BlockAir());
            _blockRegistry.Put(new BlockStone());
            _blockRegistry.Put(new BlockGrass());
            _blockRegistry.Put(new BlockDirt());
            _blockRegistry.Put(new BlockCobbleStone());
            _blockRegistry.Put(new BlockPlanks());
            _blockRegistry.Put(new BlockBedrock());
            _blockRegistry.Put(new BlockLog());
            _blockRegistry.Put(new BlockLeaves());
            _blockRegistry.Put(new BlockGlass());
            _blockRegistry.Put(new BlockCraftingTable());
            _blockRegistry.Put(new BlockFurnace());
            //_blockRegistry.Put(new BlockSlab());
            _blockRegistry.Put(new BlockRare());
            _blockRegistry.Put(new BlockLadder());
            _blockRegistry.Put(new BlockTallGrass());

            //POST - MOD Blocks and Items
            foreach (ModMain mod in _installedMods)
            {
                mod.OnItemsAndBlocksRegistry(new RegistryEventArgs(_blockRegistry, _itemRegistry, _recipeRegistry));
            }

            foreach (var block in BlockRegistry.AllBlocks())
            {
                _itemRegistry.Put(new ItemBlock(block));
            }

            _itemRegistry.Put(new ItemPickaxe("wood"));
            _itemRegistry.Put(new ItemPickaxe("stone"));
            _itemRegistry.Put(new ItemPickaxe("rare"));

            var log = ItemRegistry.GetItem(BlockRegistry.GetBlock<BlockLog>());
            var wood = ItemRegistry.GetItem(BlockRegistry.GetBlock<BlockPlanks>());
            var cobble = ItemRegistry.GetItem(BlockRegistry.GetBlock<BlockCobbleStone>());
            var rare = ItemRegistry.GetItem(BlockRegistry.GetBlock<BlockRare>());

            Item[] recipe =
            {
                cobble, cobble, cobble,
                null, wood, null,
                null, wood, null
            };
            _recipeRegistry.RegisterRecipe(recipe, ItemRegistry.GetItem("sharpcraft", "pick_stone"));

            recipe = new[]
            {
                rare, rare, rare,
                null, wood, null,
                null, wood, null
            };
            _recipeRegistry.RegisterRecipe(recipe, ItemRegistry.GetItem("sharpcraft", "pick_rare"));

            recipe = new[]
            {
                wood, wood, wood,
                null, wood, null,
                null, wood, null
            };
            _recipeRegistry.RegisterRecipe(recipe, ItemRegistry.GetItem("sharpcraft", "pick_wood"));

            recipe = new[]
            {
                cobble, cobble, cobble,
                cobble, null, cobble,
                cobble, cobble, cobble
            };
            _recipeRegistry.RegisterRecipe(recipe, ItemRegistry.GetItem(BlockRegistry.GetBlock<BlockFurnace>()));

            recipe = new[]
            {
                wood, wood, null,
                wood, wood, null,
                null, null, null
            };
            _recipeRegistry.RegisterRecipe(recipe, ItemRegistry.GetItem(BlockRegistry.GetBlock<BlockCraftingTable>()));

            recipe = new[]
            {
                log, null, null,
                null, null, null,
                null, null, null
            };
            _recipeRegistry.RegisterRecipe(recipe, new ItemStack(wood, 4), true);

            recipe = new[]
            {
                wood, null, null,
                wood, null, null,
                null, null, null
            };
            _recipeRegistry.RegisterRecipe(recipe, wood);

            foreach (ModMain mod in _installedMods)
            {
                mod.OnRecipeRegistry(new RecipeRegistryEventArgs(_recipeRegistry));
            }

            JsonModelLoader loader = new JsonModelLoader(Block.DefaultShader, new Shader<ModelItem>("block"));

            _blockRegistry.RegisterBlocksPost(loader);
            _itemRegistry.RegisterItemsPost(loader);

            LangUtil.LoadLang(SettingsManager.GetString("lang"));//TODO - find a proper placement for this line
        }

        public void StartGame()
        {
            //load settings
            _sensitivity = SettingsManager.GetFloat("sensitivity");
            WorldRenderer.RenderDistance = SettingsManager.GetInt("renderdistance");

            LoadWorld("MyWorld");

            Player.OnPickup(new ItemStack(ItemRegistry.GetItem("sharpcraft", "pick_stone")));
            Player.OnPickup(new ItemStack(ItemRegistry.GetItem("sharpcraft", "pick_wood")));
            Player.OnPickup(new ItemStack(ItemRegistry.GetItem("sharpcraft", "pick_rare")));
        }

        public void LoadWorld(string saveName)
        {
            if (World != null)
                return;

            ParticleRenderer = new ParticleRenderer();
            SkyboxRenderer = new SkyboxRenderer();

            World loadedWorld = WorldLoader.LoadWorld(saveName);

            if (loadedWorld == null)
            {
                Console.WriteLine("DEBUG: generating World");

                BlockPos playerPos = new BlockPos(0, 10, 0);//MathUtil.NextFloat(-100, 100));

                World = new World("MyWorld", "Tomlow's Fuckaround", SettingsManager.GetString("worldseed").GetHashCode());

                Player = new EntityPlayerSp(World, playerPos.ToVec());

                World.AddEntity(Player);

                Player.SetItemStackInInventory(0, new ItemStack(ItemRegistry.GetItem(BlockRegistry.GetBlock<BlockCraftingTable>())));
                Player.SetItemStackInInventory(1, new ItemStack(ItemRegistry.GetItem(BlockRegistry.GetBlock<BlockFurnace>())));
                Player.SetItemStackInInventory(2, new ItemStack(ItemRegistry.GetItem(BlockRegistry.GetBlock<BlockCobbleStone>())));
                Player.SetItemStackInInventory(3, new ItemStack(ItemRegistry.GetItem(BlockRegistry.GetBlock<BlockPlanks>())));
                Player.SetItemStackInInventory(4, new ItemStack(ItemRegistry.GetItem(BlockRegistry.GetBlock<BlockGlass>())));
                Player.SetItemStackInInventory(5, new ItemStack(ItemRegistry.GetItem(BlockRegistry.GetBlock<BlockCraftingTable>())));
                Player.SetItemStackInInventory(7, new ItemStack(ItemRegistry.GetItem(BlockRegistry.GetBlock<BlockLadder>())));
                Player.SetItemStackInInventory(8, new ItemStack(ItemRegistry.GetItem(BlockRegistry.GetBlock<BlockTallGrass>())));

                WorldLoader.SaveWorld(World); //TODO - this is really dirty
            }
            else
            {
                World = loadedWorld;
            }

            ResetMouse();

            MouseState state = OpenTK.Input.Mouse.GetState();
            _mouseLast = new Point(state.X, state.Y);
        }

        public void Disconnect()
        {
            ParticleRenderer = null;
            SkyboxRenderer = null;

            if (IsLocal)
            {
                WorldLoader.SaveWorld(World);
            }

            World?.DestroyChunkModels();

            Player = null;

            World?.ChunkData.Cleanup();
            World?.LoadManager.Cleanup();
            World = null;
        }

        private void GameLoop()
        {
            TargetRenderFrequency = Focused ? 0 : 60;

            if (GuiScreen == null && !Focused)
                OpenGuiScreen(new GuiScreenIngameMenu());

            float wheelValue = Mouse.WheelPrecise;

            if (Player != null)
            {
                if (AllowIngameInput())
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
                                    progress.Progress += Player.GetEquippedItemStack() is ItemStack st && !st.IsEmpty ? st.Item.GetMiningSpeed(World?.GetBlockState(lastPos).Block.Material) : 1;
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
                                Player.OnClick(MouseButton.Right);
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

        public void GetMouseOverObject()
        {
            if (World == null)
                return;

            float radius = 5.5f;

            MouseOverObject final = new MouseOverObject();

            float dist = float.MaxValue;

            Vector3 camPos = Vector3.One * 0.5f + Camera.Pos;

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

                        float f = (vec - Camera.Pos).LengthFast;

                        if (f <= radius + 0.5f)
                        {
                            BlockPos pos = new BlockPos(vec);
                            BlockState state = World.GetBlockState(pos);

                            if (state.Block != air)
                            {
                                AxisAlignedBb bb = state.Block.BoundingBox.Offset(pos.ToVec());

                                bool hitSomething = RayHelper.RayIntersectsBB(Camera.Pos,
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

                                    BlockPos p = new BlockPos(hitPos - normal * bb.Size / 2);

                                    if (sideHit == FaceSides.Null)
                                        continue;

                                    float l = Math.Abs((Camera.Pos - (p.ToVec() + bb.Size / 2)).Length);

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

        private void ResetDestroyProgress(EntityPlayerSp player)
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

            if (AllowIngameInput())
            {
                Point delta = new Point(_mouseLast.X - point.X, _mouseLast.Y - point.Y);

                Camera.Yaw -= delta.X / 1000f * _sensitivity;
                Camera.Pitch -= delta.Y / 1000f * _sensitivity;

                //ResetMouse();
            }

            _mouseLast = point;
        }

        public void RunGlContext(Action a)
        {
            if (_renderThread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId) a();
            else _glContextQueue.Enqueue(a);
        }

        public bool AllowIngameInput()
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

        public int GetFps()
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

            //_frameBuffer.Bind();//TODO
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            DateTime now = DateTime.Now;

            _partialTicks = (float)(((now - _updateTimer).TotalSeconds + e.Time) / TargetUpdatePeriod);

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
                WorldRenderer?.Render(World, _partialTicks);
                ParticleRenderer?.Render(_partialTicks);
                EntityRenderer?.Render(_partialTicks);
                SkyboxRenderer?.Render(_partialTicks);
            }

            //render other gui
            if (Player != null)
            {
                GuiRenderer?.RenderCrosshair();
                GuiRenderer?.RenderHUD();
            }

            //render gui screen
            if (GuiScreen != null)
            {
                CursorVisible = true;
                GuiRenderer?.Render(GuiScreen);
            }

            if (_takeScreenshot)
            {
                _takeScreenshot = false;

                CaptureScreen();
            }

            //_frameBuffer.BindDefault();
            //_frameBuffer.CopyColorToScreen();//TODO

            SwapBuffers();

            _fpsCounter++;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            _updateTimer = DateTime.Now;

            if (!IsDisposed && Visible)
                GetMouseOverObject();

            GameLoop();
        }

        protected override void OnFocusedChanged(EventArgs e)
        {
            Console.WriteLine("Focus changed to " + Focused);

            if (!Focused)
                _mouseButtonsDown.Clear();
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

            if (e.Button == MouseButton.Left && AllowIngameInput())
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

            if (AllowIngameInput())
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
                        {
                            JsonModelLoader.Reload();
                            World?.DestroyChunkModels();
                        }

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