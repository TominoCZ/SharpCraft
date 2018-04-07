using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.block;
using SharpCraft.model;
using SharpCraft.render;
using SharpCraft.texture;
using SharpCraft.util;
using SharpCraft.world;

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

            var model = ModelRegistry.GetModelForBlock(block, meta);

            if (model.RawModel is ModelBlockRaw mbr)
            {
                var uvs = mbr.GetUVs(side);

                var size = uvs.end - uvs.start;

                var pixel = size / 16;

                UVmin = uvs.start + pixel * new Vector2(MathUtil.NextFloat(0, 12), MathUtil.NextFloat(0, 12));
                UVmax = UVmin + pixel * 4;
            }

            rotStep = new Vector3(MathUtil.NextFloat(), MathUtil.NextFloat(), MathUtil.NextFloat()).Normalized() * MathHelper.Pi * 22;
        }

        public override void Update()
        {
            lastParticleScale = particleScale;
            lastParticleAlpha = particleAlpha;

            lastPos = pos;
            lastRot = rot;

            if (!isAlive) return;

            if (particleAge++ >= particleMaxAge)
            {
                if (particleAlpha >= 0.0015f)
                {
                    particleAlpha *= 0.525f;
                    particleScale *= 0.725f;
                }
                else
                    SetDead();
            }

            rot += rotStep * (motion.Xz * 5).LengthFast;

            motion.Y -= 0.04f * gravity;

            Move();

            motion.Xz *= 0.8664021f;

            if (onGround)
            {
                motion.Xz *= 0.6676801f;
            }
        }

        public override void Render(float particalTicks)
        {
            var partialPos = lastPos + (pos - lastPos) * particalTicks;
            var partialRot = lastRot + (rot - lastRot) * particalTicks;

            var partialScale = lastParticleScale + (particleScale - lastParticleScale) * particalTicks;

            var model = ParticleRenderer.ParticleModel;
            model.Shader.UpdateGlobalUniforms();
            model.Shader.UpdateModelUniforms();
            model.Shader.UpdateInstanceUniforms(MatrixHelper.CreateTransformationMatrix(partialPos - Vector3.One * partialScale / 2, partialRot, partialScale), this);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            model.RawModel.Render(PrimitiveType.Quads);
        }
    }
}
