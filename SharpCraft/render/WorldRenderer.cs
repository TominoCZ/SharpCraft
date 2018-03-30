using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.model;
using SharpCraft.shader;
using SharpCraft.texture;
using SharpCraft.util;
using SharpCraft.world;
using SharpCraft.world.chunk;
using GL = OpenTK.Graphics.OpenGL.GL;
using TextureTarget = OpenTK.Graphics.OpenGL.TextureTarget;
using TextureUnit = OpenTK.Graphics.OpenGL.TextureUnit;

namespace SharpCraft.render
{
    internal class WorldRenderer
    {
        private ModelCubeOutline _selectionOutline;

        private Vector4 _selectionOutlineColor;

        private Stopwatch _updateTimer;

        private int _hue;

        private int _renderDistance;

        public int RenderDistance
        {
            get => _renderDistance;
            set => _renderDistance = MathHelper.Clamp(value, 3, int.MaxValue);
        }

        public WorldRenderer()
        {
            _selectionOutline = new ModelCubeOutline(new ShaderBlockOutline());

            RenderDistance = 8;

            _updateTimer = Stopwatch.StartNew();
        }

        public void Render(World world, Matrix4 viewMatrix)
        {
            if (world == null) return;
	        world.LoadManager.LoadImportantChunks();
	        world.LoadManager.BuildImportantChunks();


            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TextureManager.blockTextureAtlasID);

            var hit = Game.Instance.MouseOverObject;

            if (hit.hit != null && hit.hit is EnumBlock block && block != EnumBlock.AIR)
                RenderBlockSelectionOutline(world, viewMatrix, block, hit.blockPos);

            RenderChunks(world, viewMatrix);

            if (Game.Instance.Player != null)
            {
                RenderSelectedItemBlock();
            }
        }

        private void RenderChunks(World world, Matrix4 viewMatrix)
        {
            foreach (var chunk in world.Chunks.Values)
            {
                if (chunk.ShouldRender(RenderDistance))
                {
                    if(chunk.ModelGenerating)RenderChunkOutline(chunk, viewMatrix);
                    chunk.Render(viewMatrix);
                }
            }
        }

        private void RenderChunkOutline(Chunk ch, Matrix4 viewMatrix)
        {
            var shader = (ShaderBlockOutline)_selectionOutline.shader;

            var size = new Vector3(Chunk.ChunkSize, Chunk.ChunkHeight, Chunk.ChunkSize);

            _selectionOutline.bind();

            shader.loadVec4(ch.HasData ? Vector4.One : Vector4.UnitX, "colorIn");
            shader.loadViewMatrix(viewMatrix);
            shader.loadTransformationMatrix(MatrixHelper.createTransformationMatrix(ch.Pos.ToVec(), size));

            GL.Disable(EnableCap.CullFace);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            _selectionOutline.rawModel.Render(shader.renderType);

            GL.Enable(EnableCap.CullFace);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);

            _selectionOutline.unbind();
        }

        private void RenderBlockSelectionOutline(World world, Matrix4 viewMatrix, EnumBlock block, BlockPos pos)
        {
            if (_updateTimer.ElapsedMilliseconds >= 50)
            {
                _hue = (_hue + 5) % 365;

                _selectionOutlineColor = GetHue(_hue);

                _updateTimer.Restart();
            }

            var shader = (ShaderBlockOutline)_selectionOutline.shader;
            var bb = ModelRegistry.getModelForBlock(block, world.GetMetadata(pos))?.boundingBox;

            if (bb == null)
                return;

            var size = bb.size + Vector3.One * 0.0025f;

            _selectionOutline.bind();

            shader.loadVec4(_selectionOutlineColor, "colorIn");
            shader.loadViewMatrix(viewMatrix);
            shader.loadTransformationMatrix(MatrixHelper.createTransformationMatrix(pos.ToVec() - Vector3.One * 0.00175f, size));

            GL.Disable(EnableCap.CullFace);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            _selectionOutline.rawModel.Render(shader.renderType);

            GL.Enable(EnableCap.CullFace);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);

            _selectionOutline.unbind();
        }

        private void RenderSelectedItemBlock()
        {
            var stack = Game.Instance.Player.getEquippedItemStack();

            if (stack?.Item is ItemBlock itemBlock)
            {
                var model = ModelRegistry.getModelForBlock(itemBlock.getBlock(), stack.Meta);

                model.bind();
                model.shader.loadVec3(Vector3.One, "lightColor");
                model.shader.loadViewMatrix(Matrix4.Identity);

                model.shader.loadTransformationMatrix(MatrixHelper.createTransformationMatrix(
                    new Vector3(0.04125f, -0.065f, -0.1f) + Game.Instance.Camera.getLookVec() / 200,
                    new Vector3(-2, -11, 0), 0.045f));

                model.rawModel.Render(model.shader.renderType);

                model.unbind();
            }
        }

        private Vector4 GetHue(int hue)
        {
            var rads = MathHelper.DegreesToRadians(hue);

            var r = (float)(Math.Sin(rads) * 0.5 + 0.5);
            var g = (float)(Math.Sin(rads + MathHelper.PiOver3 * 2) * 0.5 + 0.5);
            var b = (float)(Math.Sin(rads + MathHelper.PiOver3 * 4) * 0.5 + 0.5);

            return Vector4.UnitX * r + Vector4.UnitY * g + Vector4.UnitZ * b + Vector4.UnitW;
        }
    }
}