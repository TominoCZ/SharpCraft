﻿using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.block;
using SharpCraft.model;
using SharpCraft.render;
using SharpCraft.texture;
using SharpCraft.util;
using SharpCraft.world;
using System;

namespace SharpCraft.particle
{
    internal class ParticleDigging : Particle
    {
        public EnumBlock block { get; }

        protected Vector3 rot;
        protected Vector3 lastRot;

        protected Vector3 rotStep;

        public ParticleDigging(World world, Vector3 pos, Vector3 motion, float particleScale, EnumBlock block, int meta) : this(world, pos, motion, particleScale, block, FaceSides.AllSides[(int)MathUtil.NextFloat(0, 6)], meta)
        {
        }

        public ParticleDigging(World world, Vector3 pos, Vector3 motion, float particleScale, EnumBlock block, FaceSides side, int meta) : base(world, pos, motion, particleScale, TextureManager.TEXTURE_BLOCKS.ID)
        {
            this.block = block;

            ModelBlock model = ModelRegistry.GetModelForBlock(block, meta);

            if (model.RawModel is ModelBlockRaw mbr)
            {
                TextureUVNode uvs = mbr.GetUVs(side);

                Vector2 size = uvs.end - uvs.start;

                Vector2 pixel = size / 16;

                UVmin = uvs.start + pixel * new Vector2(MathUtil.NextFloat(0, 12), MathUtil.NextFloat(0, 12));
                UVmax = UVmin + pixel * 4;
            }

            if (side == FaceSides.Up)
                this.Motion.Xz = SharpCraft.Instance.Camera.GetLookVec().Xz * 0.15f;

            Vector3 vec = new Vector3(MathUtil.NextFloat(-1), MathUtil.NextFloat(-1), MathUtil.NextFloat(-1));

            rotStep = vec.Normalized() * MathUtil.NextFloat(40, 75);
        }

        public override void Update()
        {
            lastParticleScale = particleScale;
            lastParticleAlpha = particleAlpha;

            LastPos = Pos;
            lastRot = rot;

            if (!isAlive) return;

            if (particleAge++ >= particleMaxAge)
            {
                if (particleAlpha >= 0.0015f)
                {
                    particleAlpha *= 0.575f;
                    particleScale *= 0.625f;
                }
                else
                    SetDead();
            }

            Motion.Y -= 0.04f * gravity;

            Move();

            Motion.Xz *= 0.8864021f;

            if (onGround)
            {
                Motion.Xz *= 0.6676801f;

                rot.X = (float)Math.Round(rot.X / 90) * 90;
                rot.Z = (float)Math.Round(rot.Z / 90) * 90;
            }
            else
            {
                rot += rotStep * Math.Clamp((Motion.Xz * 5).LengthFast, onGround ? 0 : 0.2f, 0.3f);
            }
        }

        public override void Render(float partialTicks)
        {
            Vector3 partialPos = Vector3.Lerp(LastPos, Pos, partialTicks);
            Vector3 partialRot = Vector3.Lerp(lastRot, rot, partialTicks);

            float partialScale = lastParticleScale + (particleScale - lastParticleScale) * partialTicks;

            ModelBaked<Particle> model = ParticleRenderer.ParticleModel;
            model.Shader.UpdateGlobalUniforms();
            model.Shader.UpdateModelUniforms();
            model.Shader.UpdateInstanceUniforms(MatrixHelper.CreateTransformationMatrix(partialPos - (Vector3.UnitX + Vector3.UnitZ) * partialScale / 2, partialRot, partialScale), this);
            
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            model.RawModel.Render(PrimitiveType.Quads);
        }
    }
}