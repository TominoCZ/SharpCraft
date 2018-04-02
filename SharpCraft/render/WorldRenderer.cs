using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.model;
using SharpCraft.texture;
using SharpCraft.util;
using SharpCraft.world;
using SharpCraft.world.chunk;
using System;
using System.Diagnostics;
using SharpCraft.item;
using SharpCraft.render.shader;
using SharpCraft.render.shader.shaders;
using GL = OpenTK.Graphics.OpenGL.GL;
using TextureTarget = OpenTK.Graphics.OpenGL.TextureTarget;
using TextureUnit = OpenTK.Graphics.OpenGL.TextureUnit;

namespace SharpCraft.render
{
    internal class WorldRenderer
    {
        private ModelCubeOutline _selectionOutline;
        private Vector4 _selectionOutlineColor = MathUtil.Hue(0);

        private int _hue;

        private int _renderDistance;

        private Vector3 lookVec;
        private Vector3 lastLookVec;

        public int RenderDistance
        {
            get => _renderDistance;
            set => _renderDistance = MathHelper.Clamp(value, 3, int.MaxValue);
        }

        public WorldRenderer()
        {
            _selectionOutline = new ModelCubeOutline();

            RenderDistance = 8;
        }

        public void Update()
        {
            lastLookVec = lookVec;
            lookVec = SharpCraft.Instance.Camera.GetLookVec();

            _hue = (_hue + 5) % 360;

            _selectionOutlineColor = MathUtil.Hue(_hue);
        }

        public void Render(World world, float partialTicks)
        {
            if (world == null) return;
            world.LoadManager.LoadImportantChunks();
            world.LoadManager.BuildImportantChunks();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TextureManager.blockTextureAtlasID);

            var hit = SharpCraft.Instance.MouseOverObject;

            if (hit.hit != null && hit.hit is EnumBlock block && block != EnumBlock.AIR)
                RenderBlockSelectionOutline(world, block, hit.blockPos);

            RenderChunks(world);

            if (SharpCraft.Instance.Player != null)
            {
                GL.Enable(EnableCap.DepthClamp);
                RenderSelectedItemBlock(partialTicks);
                GL.Disable(EnableCap.DepthClamp);
            }
        }

        private void RenderChunks(World world)
        {
            foreach (var chunk in world.Chunks.Values)
            {
                if (chunk.ShouldRender(RenderDistance))
                {
                    if (chunk.ModelGenerating) RenderChunkOutline(chunk);
                    chunk.Render();
                }
            }
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
            var bb = ModelRegistry.getModelForBlock(block, world.GetMetadata(pos))?.boundingBox;

            if (bb == null)
                return;

            var size = bb.size + Vector3.One * 0.0025f;

            _selectionOutline.Bind();
            _selectionOutline.SetColor(_selectionOutlineColor);

            shader.UpdateGlobalUniforms();
            shader.UpdateModelUniforms(_selectionOutline.RawModel);
            shader.UpdateInstanceUniforms(MatrixHelper.CreateTransformationMatrix(pos.ToVec() - Vector3.One * 0.00175f, size), _selectionOutline);

            GL.Disable(EnableCap.CullFace);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            _selectionOutline.RawModel.Render(PrimitiveType.Quads);

            GL.Enable(EnableCap.CullFace);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);

            _selectionOutline.Unbind();
        }

        private void RenderSelectedItemBlock(float partialTicks)
        {
            var stack = SharpCraft.Instance.Player.GetEquippedItemStack();

            if (!stack?.IsEmpty == true)
            {
                if (stack.Item is ItemBlock itemBlock)
                {
                    var model = ModelRegistry.getModelForBlock(itemBlock.getBlock(), stack.Meta);

                    var partialLookVec = lastLookVec + (lookVec - lastLookVec) * partialTicks;
                    var rotVec = new Vector2(-SharpCraft.Instance.Camera.pitch, -SharpCraft.Instance.Camera.yaw);

                    var pos_o = new Vector3(1.25f, 1.25f, 1.25f);

                    var r = Matrix4.CreateRotationX(rotVec.X) * Matrix4.CreateRotationY(rotVec.Y);

                    var s = Matrix4.CreateScale(0.5f);
                    var t0 = Matrix4.CreateTranslation(Vector3.One * -0.5f);
                    var t1 = Matrix4.CreateTranslation(partialLookVec * pos_o);
                    var t2 = Matrix4.CreateTranslation(SharpCraft.Instance.Camera.pos);

                    var mat = t0 * r * s * t1 * t2;

                    model.Bind();

                    model.Shader.UpdateGlobalUniforms();
                    model.Shader.UpdateModelUniforms(model.RawModel);
                    model.Shader.UpdateInstanceUniforms(mat, model);

                    model.RawModel.Render(PrimitiveType.Quads);

                    model.Unbind();
                }
            }
        }
    }
}