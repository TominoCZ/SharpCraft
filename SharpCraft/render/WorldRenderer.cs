using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.block;
using SharpCraft.item;
using SharpCraft.model;
using SharpCraft.render.shader.shaders;
using SharpCraft.texture;
using SharpCraft.util;
using SharpCraft.world;
using SharpCraft.world.chunk;
using System;
using GL = OpenTK.Graphics.OpenGL.GL;
using TextureTarget = OpenTK.Graphics.OpenGL.TextureTarget;
using TextureUnit = OpenTK.Graphics.OpenGL.TextureUnit;

namespace SharpCraft.render
{
    internal class WorldRenderer
    {
        private ModelCubeOutline _selectionOutline;

        private ShaderTexturedCube _shaderTexturedCube;
        private ModelRaw _destroyProgressModel;

        private Vector4 _selectionOutlineColor = MathUtil.Hue(0);

        private int _hue;

        private int _renderDistance;

        private Vector3 lookVec;
        private Vector3 lastLookVec;

        private Vector3 motion;
        private Vector3 lastMotion;

        private float fov;
        private float lastFov;

        public int RenderDistance
        {
            get => _renderDistance;
            set => _renderDistance = MathHelper.Clamp(value, 3, int.MaxValue);
        }

        public WorldRenderer()
        {
            _selectionOutline = new ModelCubeOutline();
            _shaderTexturedCube = new ShaderTexturedCube();

            var cube = ModelHelper.createCubeModel();

            _destroyProgressModel = ModelManager.loadModelToVAO(cube, 3);

            RenderDistance = 8;
        }

        public void Update()
        {
            lastLookVec = lookVec;
            lastMotion = motion;
            lastFov = fov;

            if (SharpCraft.Instance.Player != null)
            {
                motion = SharpCraft.Instance.Player.motion;

                fov = SharpCraft.Instance.Player.motion.Xz.LengthFast > 0.15f && SharpCraft.Instance.Player.isRunning
                    ? Math.Clamp(fov * 1.065f, 0, SharpCraft.Instance.Camera.TargetFov + 6)
                    : Math.Clamp(fov * 0.965f, SharpCraft.Instance.Camera.TargetFov,
                        SharpCraft.Instance.Camera.TargetFov + 6);
            }

            _selectionOutlineColor = MathUtil.Hue(_hue = (_hue + 5) % 360);

            lookVec = SharpCraft.Instance.Camera.GetLookVec();
        }

        public void Render(World world, float partialTicks)
        {
            if (world == null) return;
            world.LoadManager.LoadImportantChunks();
            world.LoadManager.BuildImportantChunks();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TextureManager.TEXTURE_BLOCKS.ID);

            var hit = SharpCraft.Instance.MouseOverObject;

            if (hit.hit != null)
            {
                if (hit.hit is EnumBlock block && block != EnumBlock.AIR)
                    RenderBlockSelectionOutline(world, block, hit.blockPos);
            }

            RenderChunks(world);
            RenderSelectedItemBlock(partialTicks);
            RenderDestroyProgress(world);

            var partialFov = lastFov + (fov - lastFov) * partialTicks;
            var partialMotion = lastMotion + (motion - lastMotion) * partialTicks;

            SharpCraft.Instance.Camera.pitchOffset = partialMotion.Y * 0.025f;
            SharpCraft.Instance.Camera.SetFOV(partialFov);
        }

        private void RenderChunks(World world)
        {
            foreach (var chunk in world.Chunks.Values)
            {
                if (!chunk.ShouldRender(RenderDistance))
                    continue;

                if (chunk.QueuedForModelBuild)
                    RenderChunkOutline(chunk);

                chunk.Render();
            }
        }

        private void RenderDestroyProgress(World world)
        {
            var progresses = SharpCraft.Instance.DestroyProgresses;
            if (progresses.Count == 0)
                return;

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TextureManager.TEXTURE_DESTROY_PROGRESS.ID);

            GL.BlendFunc(BlendingFactorSrc.DstColor, BlendingFactorDest.Zero);

            _shaderTexturedCube.Bind();
            _shaderTexturedCube.UpdateGlobalUniforms();
            _shaderTexturedCube.UpdateModelUniforms(_destroyProgressModel);

            GL.BindVertexArray(_destroyProgressModel.vaoID);
            GL.EnableVertexAttribArray(0);

            foreach (var pair in progresses)
            {
                var block = world.GetBlock(pair.Key);

                if (block == EnumBlock.AIR)
                    continue;

                var meta = world.GetMetadata(pair.Key);

                var model = ModelRegistry.GetModelForBlock(block, meta);

                var v = 32 * (int)(pair.Value.PartialProgress * 8);

                var size_o = Vector3.One * 0.0045f;

                var mat = MatrixHelper.CreateTransformationMatrix(pair.Key.ToVec() - size_o / 2, model.boundingBox.size + size_o);

                _shaderTexturedCube.UpdateInstanceUniforms(mat);
                _shaderTexturedCube.UpdateUVs(TextureManager.TEXTURE_DESTROY_PROGRESS, 0, v, 32);
            }
            _destroyProgressModel.Render(PrimitiveType.Quads);

            GL.DisableVertexAttribArray(0);
            GL.BindVertexArray(0);

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            _shaderTexturedCube.Unbind();
        }

        private void RenderChunkOutline(Chunk ch)
        {
            var shader = _selectionOutline.Shader;

            var size = new Vector3(Chunk.ChunkSize, Chunk.ChunkHeight, Chunk.ChunkSize);

            _selectionOutline.Bind();
            _selectionOutline.SetColor(Vector4.One);

            shader.UpdateGlobalUniforms();
            shader.UpdateModelUniforms(_selectionOutline.RawModel);
            shader.UpdateInstanceUniforms(MatrixHelper.CreateTransformationMatrix(ch.Pos, size), _selectionOutline);

            GL.Disable(EnableCap.CullFace);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            _selectionOutline.RawModel.Render(PrimitiveType.Quads);

            GL.Enable(EnableCap.CullFace);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);

            _selectionOutline.Unbind();
        }

        private void RenderBlockSelectionOutline(World world, EnumBlock block, BlockPos pos)
        {
            var shader = _selectionOutline.Shader;
            var bb = ModelRegistry.GetModelForBlock(block, world.GetMetadata(pos))?.boundingBox;

            if (bb == null)
                return;

            var size = Vector3.One * 0.005f;

            _selectionOutline.Bind();
            _selectionOutline.SetColor(_selectionOutlineColor);

            shader.UpdateGlobalUniforms();
            shader.UpdateModelUniforms(_selectionOutline.RawModel);
            shader.UpdateInstanceUniforms(MatrixHelper.CreateTransformationMatrix(pos.ToVec() - size / 2f, bb.size + size), _selectionOutline);

            GL.LineWidth(2);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            _selectionOutline.RawModel.Render(PrimitiveType.Quads);

            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);

            _selectionOutline.Unbind();
        }

        private void RenderSelectedItemBlock(float partialTicks)
        {
            if (SharpCraft.Instance.Player == null)
                return;

            var stack = SharpCraft.Instance.Player.GetEquippedItemStack();

            if (stack == null || stack.IsEmpty)
                return;

            if (stack.Item is ItemBlock itemBlock)
            {
                var model = ModelRegistry.GetModelForBlock(itemBlock.GetBlock(), stack.Meta);

                var partialLookVec = lastLookVec + (lookVec - lastLookVec) * partialTicks;
                var partialMotion = lastMotion + (motion - lastMotion) * partialTicks;

                var rotVec = new Vector2(-SharpCraft.Instance.Camera.pitch, -SharpCraft.Instance.Camera.yaw);

                var offset = new Vector3(1.3f, -1.25f, 0.3f) - partialMotion * Vector3.UnitY * 0.1f;

                var r1 = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(45));
                var r2 = Matrix4.CreateRotationX(rotVec.X - SharpCraft.Instance.Camera.pitchOffset) * Matrix4.CreateRotationY(rotVec.Y);

                var s = Matrix4.CreateScale(0.5525f);
                var t0 = Matrix4.CreateTranslation(Vector3.One * -0.5f);
                var t1 = Matrix4.CreateTranslation(SharpCraft.Instance.Camera.pos + SharpCraft.Instance.Camera.GetLookVec() + partialLookVec * 0.1f);
                var t_final = Matrix4.CreateTranslation(offset);

                var mat = t0 * r1 * Matrix4.CreateScale(model.boundingBox.size) * t_final * r2 * s * t1;

                GL.DepthRange(0, 0.1f);

                model.Bind();

                model.Shader.UpdateGlobalUniforms();
                model.Shader.UpdateModelUniforms(model.RawModel);
                model.Shader.UpdateInstanceUniforms(mat, model);

                model.RawModel.Render(PrimitiveType.Quads);

                model.Unbind();

                GL.DepthRange(0, 1);
            }
        }
    }
}