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

        private int _hue;

        private int _renderDistance;

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
                RenderSelectedItemBlock();
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
            var shader = _selectionOutline.shader;

            var size = new Vector3(Chunk.ChunkSize, Chunk.ChunkHeight, Chunk.ChunkSize);

            _selectionOutline.bind();

	        shader.UpdateGlobalUniforms();
	        shader.UpdateModelUniforms(_selectionOutline.rawModel);
			shader.UpdateInstanceUniforms(MatrixHelper.createTransformationMatrix(ch.Pos, size),_selectionOutline);
            GL.Disable(EnableCap.CullFace);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            _selectionOutline.rawModel.Render(PrimitiveType.Quads);

            GL.Enable(EnableCap.CullFace);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);

            _selectionOutline.unbind();
        }

        private void RenderBlockSelectionOutline(World world, EnumBlock block, BlockPos pos)
        {

            var shader = _selectionOutline.shader;
            var bb = ModelRegistry.getModelForBlock(block, world.GetMetadata(pos))?.boundingBox;

            if (bb == null)
                return;

            var size = bb.size + Vector3.One * 0.0025f;

            _selectionOutline.bind();


	        shader.UpdateGlobalUniforms();
	        shader.UpdateModelUniforms(_selectionOutline.rawModel);
	        shader.UpdateInstanceUniforms(MatrixHelper.createTransformationMatrix(pos.ToVec() - Vector3.One * 0.00175f, size),_selectionOutline);

            GL.Disable(EnableCap.CullFace);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            _selectionOutline.rawModel.Render(PrimitiveType.Quads);

            GL.Enable(EnableCap.CullFace);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);

            _selectionOutline.unbind();
        }

        private void RenderSelectedItemBlock()
        {
            var stack = SharpCraft.Instance.Player.getEquippedItemStack();

            if (!stack?.IsEmpty == true)
            {
                if (stack.Item is ItemBlock itemBlock)
                {
                    var model = ModelRegistry.getModelForBlock(itemBlock.getBlock(), stack.Meta);

                    model.bind();
	                model.shader.UpdateGlobalUniforms();
	                model.shader.UpdateModelUniforms(_selectionOutline.rawModel);
	                model.shader.UpdateInstanceUniforms(MatrixHelper.createTransformationMatrix(
		                new Vector3(0.04125f, -0.08f, -0.1f) + SharpCraft.Instance.Camera.getLookVec() / 250,
		                new Vector3(0, 45, 0),
		                0.045f),model);

                    model.rawModel.Render(PrimitiveType.Quads);

                    model.unbind();
                }
            }
        }
    }
}