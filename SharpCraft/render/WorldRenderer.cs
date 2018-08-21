using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.item;
using SharpCraft.model;
using SharpCraft.render.shader;
using SharpCraft.texture;
using SharpCraft.util;
using SharpCraft.world;
using SharpCraft.world.chunk;
using System;
using System.Collections.Generic;
using SharpCraft.json;
using GL = OpenTK.Graphics.OpenGL.GL;
using TextureTarget = OpenTK.Graphics.OpenGL.TextureTarget;

namespace SharpCraft.render
{
    internal class WorldRenderer
    {
        private readonly ModelCubeOutline _selectionOutline;

        private readonly Shader _shaderTexturedCube;
        private readonly ModelRaw _destroyProgressModel;
        private ModelCustom _armModel;

        private Vector4 _selectionOutlineColor = MathUtil.Hue(0);

        private int _hue;

        private int _renderDistance;

        private Vector3 _lookVec;
        private Vector3 _lastLookVec;

        private Vector3 _motion;
        private Vector3 _lastMotion;

        private float _fov;
        private float _lastFov;
        private int _ticks;
        private int _ticksLast;
        private float _animationTimer;
        private float _animationTimerLast;
        private bool _buttonDown;
        private bool _animating;

        public int RenderDistance
        {
            get => _renderDistance;
            set => _renderDistance = MathHelper.Clamp(value, 3, int.MaxValue);
        }

        public WorldRenderer()
        {
            SharpCraft.Instance.MouseDown += (o, e) =>
            {
                if (e.Button == MouseButton.Left && !SharpCraft.Instance.IsPaused && SharpCraft.Instance.Focused && SharpCraft.Instance.GuiScreen == null)
                {
                    _buttonDown = true;
                }
            };
            SharpCraft.Instance.MouseUp += (o, e) => { if (e.Button == MouseButton.Left) _buttonDown = false; };

            _selectionOutline = new ModelCubeOutline();
            _shaderTexturedCube = new Shader("textured_cube", "UVmin", "UVmax");

            var cube = CubeModelBuilder.CreateCubeVertexes();

            _destroyProgressModel = ModelManager.LoadModel3ToVao(cube);
            JsonModelLoader.LoadCustomModel("entity/player/arm", Block.DefaultShader);

            RenderDistance = 8;
            _lastFov = _fov = SharpCraft.Instance.Camera.PartialFov;
        }

        public void Update()
        {
            _lastLookVec = _lookVec;

            _lastMotion = _motion;
            _lastFov = _fov;

            _ticksLast = _ticks;
            _animationTimerLast = _animationTimer;

            _ticks = (_ticks + 1) % 90;

            if (_ticksLast > _ticks)
                _ticksLast = _ticks - 1;

            var down = _buttonDown;

            if (down)
                _animating = true;

            if (_animationTimer < 10 && _animating)
            {
                _animationTimer += 1.625f;
            }
            else if (_animating)
            {
                _animating = false;

                _animationTimer = 0;
                _animationTimerLast = 0;
            }

            if (SharpCraft.Instance.Player != null)
            {
                _motion = SharpCraft.Instance.Player.Motion;

                _fov = SharpCraft.Instance.Player.Motion.Xz.LengthFast > 0.15f && SharpCraft.Instance.Player.IsRunning
                    ? MathHelper.Clamp(_fov * 1.065f, 0, SharpCraft.Instance.Camera.TargetFov + 6)
                    : MathHelper.Clamp(_fov * 0.965f, SharpCraft.Instance.Camera.TargetFov,
                        SharpCraft.Instance.Camera.TargetFov + 6);
            }

            _selectionOutlineColor = MathUtil.Hue(_hue = (_hue + 5) % 360);

            _lookVec = SharpCraft.Instance.Camera.GetLookVec();

            _armModel = JsonModelLoader.GetCustomModel("entity/player/arm");
        }

        public void Render(World world, float partialTicks)
        {
            if (world == null) return;
            world.LoadManager.LoadImportantChunks();
            world.LoadManager.BuildImportantChunks();

            GL.BindTexture(TextureTarget.Texture2D, JsonModelLoader.TextureBlocks);

            MouseOverObject hit = SharpCraft.Instance.MouseOverObject;

            if (hit.hit == HitType.Block)
            {
                var state = world.GetBlockState(hit.blockPos);
                if (!Equals(state, BlockRegistry.GetBlock<BlockAir>().GetState()))
                    RenderBlockSelectionOutline(state, hit.blockPos);
            }

            RenderChunks(world, partialTicks);
            RenderHand(partialTicks);
            RenderWorldWaypoints(world);
            RenderDestroyProgress(world);

            float partialFov = _lastFov + (_fov - _lastFov) * partialTicks;
            Vector3 partialMotion = _lastMotion + (_motion - _lastMotion) * partialTicks;

            SharpCraft.Instance.Camera.PitchOffset = partialMotion.Y * 0.025f;
            SharpCraft.Instance.Camera.SetFov(partialFov);
        }

        private void RenderChunks(World world, float partialTicks)
        {
            foreach (Chunk chunk in world.Chunks.Values)
            {
                if (!chunk.ShouldRender(RenderDistance))
                    continue;

                if (chunk.QueuedForModelBuild)
                    RenderChunkOutline(chunk);

                chunk.Render(partialTicks);
            }
        }

        private void RenderWorldWaypoints(World world)
        {
            var wps = world.GetWaypoints();

            if (wps.Count == 0)
                return;

            GL.BindTexture(TextureTarget.Texture2D, JsonModelLoader.TextureBlocks);

            ModelBlock model = BlockRegistry.GetBlock<BlockRare>().GetState().Model;

            float size = 0.25f;

            GL.DepthRange(0, Camera.NearPlane);
            model.Bind();

            foreach (Waypoint wp in wps)
            {
                var mat = MatrixHelper.CreateTransformationMatrix(wp.Pos.ToVec() + Vector3.One / 2 * (1 - size),
                    new Vector3(45, 30, 35.264f), size);
                model.Shader.SetMatrix4("transformationMatrix", mat);

                model.RawModel.Render();
            }

            model.Unbind();

            GL.DepthRange(0, 1);
        }

        private void RenderDestroyProgress(World world)
        {
            var progresses = SharpCraft.Instance.DestroyProgresses;
            if (progresses.Count == 0)
                return;

            GL.BindTexture(TextureTarget.Texture2D, TextureManager.TEXTURE_DESTROY_PROGRESS.ID);
            GL.BlendFunc(BlendingFactor.DstColor, BlendingFactor.Zero);

            GL.BindVertexArray(_destroyProgressModel.VaoID);
            GL.EnableVertexAttribArray(0);

            _shaderTexturedCube.Bind();

            Vector3 sizeO = Vector3.One * 0.0045f;

            foreach (KeyValuePair<BlockPos, DestroyProgress> pair in progresses)
            {
                BlockState state = world.GetBlockState(pair.Key);

                if (state.Block == BlockRegistry.GetBlock<BlockAir>())
                    continue;

                Matrix4 mat = MatrixHelper.CreateTransformationMatrix(pair.Key.ToVec() - sizeO / 2 + state.Block.BoundingBox.Min, state.Block.BoundingBox.Size + sizeO);

                var tex = TextureManager.TEXTURE_DESTROY_PROGRESS;

                Vector2 pixel = new Vector2(1f / tex.TextureSize.Width, 1f / tex.TextureSize.Height);

                Vector2 min = new Vector2(0, 32 * (int)(pair.Value.PartialProgress * 8)) * pixel;
                Vector2 max = min + 32 * pixel;

                _shaderTexturedCube.SetMatrix4("transformationMatrix", mat);
                _shaderTexturedCube.SetVector2("UVmin", min);
                _shaderTexturedCube.SetVector2("UVmax", max);
                //TODO "colorIn"
                _destroyProgressModel.Render();
            }

            GL.DisableVertexAttribArray(0);
            GL.BindVertexArray(0);

            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _shaderTexturedCube.Unbind();
        }

        private void RenderChunkOutline(Chunk ch)
        {
            Shader shader = _selectionOutline.Shader;

            Vector3 size = new Vector3(Chunk.ChunkSize, Chunk.ChunkHeight, Chunk.ChunkSize);

            _selectionOutline.Bind();
            _selectionOutline.SetColor(Vector4.One);

            shader.SetMatrix4("transformationMatrix", MatrixHelper.CreateTransformationMatrix(ch.Pos, size));

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.DrawArrays(PrimitiveType.Lines, 0, _selectionOutline.RawModel.VertexCount);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            _selectionOutline.Unbind();
        }

        private void RenderBlockSelectionOutline(BlockState state, BlockPos pos)
        {
            var shader = _selectionOutline.Shader;
            AxisAlignedBb bb = state.Block.BoundingBox;

            if (bb == null)
                return;

            GL.LineWidth(2);
            GL.PointSize(5);
            //GL.DepthRange(0, 0.1f);

            _selectionOutline.Bind();
            _selectionOutline.SetColor(_selectionOutlineColor);

            var mat = MatrixHelper.CreateTransformationMatrix(pos.ToVec() + bb.Min, bb.Size + Vector3.One * 0.001f);

            shader.SetMatrix4("transformationMatrix", mat);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.DrawArrays(PrimitiveType.Lines, 0, _selectionOutline.RawModel.VertexCount);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);
            GL.DrawArrays(PrimitiveType.Points, 0, _selectionOutline.RawModel.VertexCount);

            _selectionOutline.RawModel.Render();

            _selectionOutline.Unbind();

            //GL.DepthRange(0, 1f);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.LineWidth(1);
            GL.PointSize(1);
        }

        private void RenderHand(float partialTicks)
        {
            if (SharpCraft.Instance.Player == null)
                return;

            ItemStack stack = SharpCraft.Instance.Player.GetEquippedItemStack();

            if (stack == null || stack.IsEmpty)
            {
                RenderArmModel(partialTicks);
                return;
            }

            if (stack.Item is ItemBlock itemBlock)
            {
                RenderBlockInHand(itemBlock.Block, stack.Meta, partialTicks);
            }
            else if (stack.Item != null)
            {
                RenderItemInHand(stack.Item, partialTicks);
            }
        }

        private void RenderArmModel(float partialTicks)
        {
            if (_armModel == null)
                return;

            GL.DepthRange(0, Camera.NearPlane);

            if (!SharpCraft.Instance.Player.OnGround)
                _motion *= Vector3.UnitY;

            Vector3 partialLookVec = _lastLookVec + (_lookVec - _lastLookVec) * partialTicks;
            Vector3 partialMotion = _lastMotion + (_motion - _lastMotion) * partialTicks;

            float partialTick = (_ticksLast + (_ticks - _ticksLast) * partialTicks) / 90f;
            float partialAnimationTimer = _animationTimerLast + (_animationTimer - _animationTimerLast) * partialTicks;

            float angle = partialTick * MathHelper.TwoPi * (SharpCraft.Instance.Player.IsRunning ? 7 : 5);

            float height = 0.075f * MathHelper.Clamp(partialMotion.Xz.Length * 5, 0, 1);

            float offsetX = (float)Math.Cos(angle) * height;
            float offsetY = (float)Math.Sin(partialTick * 360 > 180 ? -angle * 2 + MathHelper.Pi : angle * 2) * height * 0.5f;

            Vector2 rotVec = new Vector2(-SharpCraft.Instance.Camera.Pitch, -SharpCraft.Instance.Camera.Yaw);

            Vector3 offset = new Vector3(0.85f + offsetX, -0.75f + offsetY, -0.3f);
            offset.Y -= MathHelper.Clamp(partialMotion.Y, -0.35f, 0.35f) / 15f;

            float angle1 = MathHelper.Clamp(partialAnimationTimer * 1.2f, 0, 10) / 10 * MathHelper.Pi;
            float angle2 = MathHelper.Clamp(partialAnimationTimer, 0, 10) / 10 * MathHelper.Pi;
            float angle3 = MathHelper.Clamp(partialAnimationTimer * 1.5f, 0, 10) / 10 * MathHelper.Pi;

            float animationSine1 = (float)Math.Sin(angle1);
            float animationSine2 = (float)Math.Sin(angle2);
            float animationSine3 = (float)Math.Sin(angle3);

            offset.Z -= animationSine2 * 0.75f;
            offset.X -= animationSine1 * 0.75f;
            offset.Y -= animationSine1 * 0.5f;

            Matrix4 animationMatrix = Matrix4.CreateTranslation(0, -0.5f, 0)
                                      * Matrix4.CreateRotationY(animationSine1 * MathHelper.PiOver3 / 1.5f)
                                      * Matrix4.CreateRotationX(-animationSine2 * MathHelper.PiOver4)
                                      * Matrix4.CreateRotationZ(animationSine1 * MathHelper.PiOver6 / 1.35f)
                                      * Matrix4.CreateTranslation(0, 0.5f, 0);

            Matrix4 r1 = Matrix4.CreateRotationZ(MathHelper.PiOver2) * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(45)) * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(-40)) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(5));
            Matrix4 r2 = Matrix4.CreateRotationX(rotVec.X - SharpCraft.Instance.Camera.PitchOffset) * Matrix4.CreateRotationY(rotVec.Y);

            Matrix4 s = Matrix4.CreateScale(1.5f);
            Matrix4 t0 = Matrix4.CreateTranslation(Vector3.One * -0.5f);
            Matrix4 t1 = Matrix4.CreateTranslation(SharpCraft.Instance.Camera.Pos + SharpCraft.Instance.Camera.GetLookVec() + partialLookVec * 0.1f);
            Matrix4 tFinal = Matrix4.CreateTranslation(offset);

            Matrix4 mat = t0 * r1 * animationMatrix * tFinal * r2 * s * t1;

            GL.BindTexture(TextureTarget.Texture2D, _armModel.TextureID);

            _armModel.Bind();
            _armModel.Shader.SetMatrix4("transformationMatrix", mat);

            _armModel.RawModel.Render();

            _armModel.Unbind();

            GL.DepthRange(0, 1);
        }

        private void RenderBlockInHand(Block block, short meta, float partialTicks)
        {
            GL.BindTexture(TextureTarget.Texture2D, JsonModelLoader.TextureBlocks);
            GL.DepthRange(0, Camera.NearPlane);

            ModelBlock model = block.GetState(meta).Model;

            if (model == null)
                return;

            if (!SharpCraft.Instance.Player.OnGround)
                _motion *= Vector3.UnitY;

            Vector3 partialLookVec = _lastLookVec + (_lookVec - _lastLookVec) * partialTicks;
            Vector3 partialMotion = _lastMotion + (_motion - _lastMotion) * partialTicks;

            float partialTick = (_ticksLast + (_ticks - _ticksLast) * partialTicks) / 90f;
            float partialAnimationTimer = _animationTimerLast + (_animationTimer - _animationTimerLast) * partialTicks;

            float angle = partialTick * MathHelper.TwoPi * (SharpCraft.Instance.Player.IsRunning ? 7 : 5);

            float height = 0.15f * MathHelper.Clamp(partialMotion.Xz.Length * 5, 0, 1);

            float offsetX = (float)Math.Cos(angle) * height;
            float offsetY = (float)Math.Sin(partialTick * 360 > 180 ? -angle * 2 + MathHelper.Pi : angle * 2) * height * 0.5f;

            Vector2 rotVec = new Vector2(-SharpCraft.Instance.Camera.Pitch, -SharpCraft.Instance.Camera.Yaw);

            float itemBlockOffsetY = block.BoundingBox.Size.Y / 2 - 0.5f;

            Vector3 offset = new Vector3(1.35f + offsetX, -1.25f - itemBlockOffsetY + offsetY, 0.3f);
            offset.Y -= MathHelper.Clamp(partialMotion.Y, -0.35f, 0.35f) / 10f;

            float angle1 = MathHelper.Clamp(partialAnimationTimer * 1.2f, 0, 10) / 10 * MathHelper.Pi;
            float angle2 = MathHelper.Clamp(partialAnimationTimer, 0, 10) / 10 * MathHelper.Pi;
            float angle3 = MathHelper.Clamp(partialAnimationTimer * 1.5f, 0, 10) / 10 * MathHelper.Pi;

            float animationSine1 = (float)Math.Sin(angle1);
            float animationSine2 = (float)Math.Sin(angle2);
            float animationSine3 = (float)Math.Sin(angle3);

            offset.Z -= animationSine3 * 0.75f;
            offset.X -= animationSine2 * 0.5f;
            offset.Y -= animationSine2 * 0.45f;

            Matrix4 animationMatrix = Matrix4.CreateTranslation(0, 1.25f, 0)
                                      * Matrix4.CreateRotationY(animationSine1 / 8 * MathHelper.PiOver3)
                                      * Matrix4.CreateRotationX(-animationSine2 * MathHelper.PiOver3)
                                      * Matrix4.CreateRotationZ(animationSine3 / 2 * MathHelper.PiOver2)
                                      * Matrix4.CreateTranslation(0, -1.25f, 0);

            Matrix4 r1 = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(45));
            Matrix4 r2 = Matrix4.CreateRotationX(rotVec.X - SharpCraft.Instance.Camera.PitchOffset) * Matrix4.CreateRotationY(rotVec.Y);

            Matrix4 s = Matrix4.CreateScale(0.5525f);
            Matrix4 t0 = Matrix4.CreateTranslation(Vector3.One * -0.5f);
            Matrix4 t1 = Matrix4.CreateTranslation(SharpCraft.Instance.Camera.Pos + SharpCraft.Instance.Camera.GetLookVec() + partialLookVec * 0.1f);
            Matrix4 tFinal = Matrix4.CreateTranslation(offset);

            Matrix4 mat = t0 * r1 * animationMatrix * tFinal * r2 * s * t1;

            model.Bind();

            model.Shader.SetMatrix4("transformationMatrix", mat);

            model.RawModel.Render();

            model.Unbind();

            GL.DepthRange(0, 1);
        }

        private void RenderItemInHand(Item item, float partialTicks)
        {
            GL.BindTexture(TextureTarget.Texture2D, JsonModelLoader.TextureItems);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Disable(EnableCap.CullFace);
            GL.DepthRange(0, Camera.NearPlane);

            ModelItem model = JsonModelLoader.GetModelForItem(item);

            if (model == null)
                return;

            if (!SharpCraft.Instance.Player.OnGround)
                _motion *= Vector3.UnitY;

            Vector3 partialLookVec = _lastLookVec + (_lookVec - _lastLookVec) * partialTicks;
            Vector3 partialMotion = _lastMotion + (_motion - _lastMotion) * partialTicks;

            float partialTick = (_ticksLast + (_ticks - _ticksLast) * partialTicks) / 90f;
            float partialAnimationTimer = _animationTimerLast + (_animationTimer - _animationTimerLast) * partialTicks;

            float angle = partialTick * MathHelper.TwoPi * (SharpCraft.Instance.Player.IsRunning ? 7 : 5);

            float height = 0.1f * MathHelper.Clamp(partialMotion.Xz.Length * 5, 0, 1);

            float offsetX = (float)Math.Cos(angle) * height;
            float offsetY = (float)Math.Sin(partialTick * 360 > 180 ? -angle * 2 + MathHelper.Pi : angle * 2) * height * 0.5f;

            Vector2 rotVec = new Vector2(-SharpCraft.Instance.Camera.Pitch, -SharpCraft.Instance.Camera.Yaw);

            Vector3 offset = new Vector3(0.85f + offsetX, -0.4f + offsetY, 1);
            offset.Y -= MathHelper.Clamp(partialMotion.Y, -0.35f, 0.35f) / 10f;

            float angle1 = MathHelper.Clamp(partialAnimationTimer * 1.2f, 0, 10) / 10 * MathHelper.Pi;
            float angle2 = MathHelper.Clamp(partialAnimationTimer, 0, 10) / 10 * MathHelper.Pi;
            float angle3 = MathHelper.Clamp(partialAnimationTimer * 1.5f, 0, 10) / 10 * MathHelper.Pi;

            float animationSine1 = (float)Math.Sin(angle1);
            float animationSine2 = (float)Math.Sin(angle2);
            float animationSine3 = (float)Math.Sin(angle3);

            offset.Z -= animationSine3 * 0.75f;
            offset.X -= animationSine2 * 0.5f;
            offset.Y -= animationSine2 * 0.25f;

            Matrix4 animationMatrix = Matrix4.CreateTranslation(0, 1.25f, 0)
                                      * Matrix4.CreateRotationY(animationSine1 / 8 * MathHelper.PiOver3)
                                      * Matrix4.CreateRotationX(-animationSine2 * MathHelper.PiOver3)
                                      * Matrix4.CreateRotationZ(animationSine3 / 2 * MathHelper.PiOver2)
                                      * Matrix4.CreateTranslation(0, -1.25f, 0);

            Matrix4 r1 = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(5));
            Matrix4 r2 = Matrix4.CreateRotationX(rotVec.X - SharpCraft.Instance.Camera.PitchOffset) * Matrix4.CreateRotationY(rotVec.Y);

            Matrix4 s = Matrix4.CreateScale(0.5525f);
            Matrix4 t0 = Matrix4.CreateTranslation(Vector3.One * -0.5f);
            Matrix4 t1 = Matrix4.CreateTranslation(SharpCraft.Instance.Camera.Pos + SharpCraft.Instance.Camera.GetLookVec() + partialLookVec * 0.1f);
            Matrix4 tFinal = Matrix4.CreateTranslation(offset);

            Matrix4 mat = t0 * r1 * animationMatrix * tFinal * r2 * s * t1;

            model.Bind();

            model.Shader.SetMatrix4("transformationMatrix", mat);

            model.RawModel.Render();

            model.Unbind();

            GL.DepthRange(0, 1);
            GL.Enable(EnableCap.CullFace);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }
    }
}