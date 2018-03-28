using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.model;
using SharpCraft.shader;
using SharpCraft.texture;
using SharpCraft.util;
using SharpCraft.world.chunk;
using GL = OpenTK.Graphics.OpenGL.GL;
using TextureTarget = OpenTK.Graphics.OpenGL.TextureTarget;
using TextureUnit = OpenTK.Graphics.OpenGL.TextureUnit;

namespace SharpCraft.render
{
    internal class WorldRenderer
    {
        private ModelCubeOutline _selectionOutline;

        private Vector4 selectionOutlineColor;

        private Stopwatch updateTimer;

        private int hue;

        private int _renderDistance;

        public int RenderDistance
        {
            get => _renderDistance;
            set => _renderDistance = MathHelper.Clamp(value, 3, int.MaxValue);
        }

        public bool AltRenderMethod;

        public WorldRenderer()
        {
            _selectionOutline = new ModelCubeOutline(new ShaderBlockOutline());

            RenderDistance = 8;

            updateTimer = Stopwatch.StartNew();
        }

        public void render(Matrix4 viewMatrix)
        {
            if (Game.Instance.World == null)
                return;

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TextureManager.blockTextureAtlasID);

            var hit = Game.Instance.MouseOverObject;

            if (hit.hit != null && hit.hit is EnumBlock block && block != EnumBlock.AIR)
                renderBlockSelectionOutline(viewMatrix, block, hit.blockPos);

            if (AltRenderMethod)
                renderWorld2(viewMatrix);
            else
                renderWorld(viewMatrix);

            if (Game.Instance.Player != null)
            {
                renderSelectedItemBlock();
            }
        }

        private void renderWorld(Matrix4 viewMatrix)
        {
            foreach (var data in Game.Instance.World.Chunks.Values)
            {
                if (!data.Chunk.IsWithinRenderDistance())
                    continue;

                foreach (var shader in data.Model.fragmentPerShader.Keys)
                {
                    var chunkFragmentModel = data.Model.getFragmentModelWithShader(shader);
                    if (chunkFragmentModel == null)
                        continue;

                    chunkFragmentModel.bind();

                    shader.loadVec3(Vector3.One, "lightColor");
                    shader.loadViewMatrix(viewMatrix);

                    shader.loadTransformationMatrix(
                        MatrixHelper.createTransformationMatrix(data.Chunk.ChunkPos.toVec()));

                    GL.DrawArrays(shader.renderType, 0, chunkFragmentModel.rawModel.vertexCount);

                    chunkFragmentModel.unbind();
                }
            }
        }

        private void renderWorld2(Matrix4 viewMatrix)
        {
            List<ChunkData> visibleChunks = new List<ChunkData>(100);

            foreach (var data in Game.Instance.World.Chunks.Values)
            {
                if (data.Chunk.IsWithinRenderDistance())
                    visibleChunks.Add(data);
            }

            for (int i = 0; i < visibleChunks.Count; i++)
            {
                var data = visibleChunks[i];

                var mat = MatrixHelper.createTransformationMatrix(data.Chunk.ChunkPos.toVec());

                foreach (var shader in data.Model.fragmentPerShader.Keys)
                {
                    var chunkFragmentModel = data.Model.getFragmentModelWithShader(shader);
                    if (chunkFragmentModel == null)
                        continue;

                    chunkFragmentModel.bind();

                    shader.loadVec3(Vector3.One, "lightColor");
                    shader.loadViewMatrix(viewMatrix);
                    shader.loadTransformationMatrix(mat);

                    GL.DrawArrays(shader.renderType, 0, chunkFragmentModel.rawModel.vertexCount);

                    chunkFragmentModel.unbind();
                }
            }

            visibleChunks.Clear();
        }

        private void renderBlockSelectionOutline(Matrix4 viewMatrix, EnumBlock block, BlockPos pos)
        {
            if (updateTimer.ElapsedMilliseconds >= 50)
            {
                hue = (hue + 5) % 365;

                selectionOutlineColor = getHue(hue);

                updateTimer.Restart();
            }

            var shader = (ShaderBlockOutline)_selectionOutline.shader;
            var bb = ModelRegistry.getModelForBlock(block, Game.Instance.World.GetMetadata(pos))?.boundingBox;

            if (bb == null)
                return;

            var size = bb.size + Vector3.One * 0.0025f;

            _selectionOutline.bind();

            shader.loadVec4(selectionOutlineColor, "colorIn");
            shader.loadViewMatrix(viewMatrix);
            shader.loadTransformationMatrix(MatrixHelper.createTransformationMatrix(pos.toVec() - Vector3.One * 0.00175f, size));

            GL.Disable(EnableCap.CullFace);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            GL.DrawArrays(shader.renderType, 0, _selectionOutline.rawModel.vertexCount);

            GL.Enable(EnableCap.CullFace);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);

            _selectionOutline.unbind();
        }

        private void renderSelectedItemBlock()
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

                GL.DrawArrays(model.shader.renderType, 0, model.rawModel.vertexCount);

                model.unbind();
            }
        }

        private Vector4 getHue(int hue)
        {
            var rads = MathHelper.DegreesToRadians(hue);

            var r = (float)(Math.Sin(rads) * 0.5 + 0.5);
            var g = (float)(Math.Sin(rads + MathHelper.PiOver3 * 2) * 0.5 + 0.5);
            var b = (float)(Math.Sin(rads + MathHelper.PiOver3 * 4) * 0.5 + 0.5);

            return Vector4.UnitX * r + Vector4.UnitY * g + Vector4.UnitZ * b + Vector4.UnitW;
        }
    }
}