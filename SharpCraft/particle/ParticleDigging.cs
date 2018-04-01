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

        public ParticleDigging(World world, Vector3 pos, Vector3 motion, float particleScale, EnumBlock block, FaceSides side, int meta) : base(world, pos, motion, particleScale, TextureManager.blockTextureAtlasID)
        {
            this.block = block;

            var model = ModelRegistry.getModelForBlock(block, meta);

            if (model.rawModel is ModelBlockRaw mbr)
            {
                var uvs = mbr.GetUVs(side);

                var size = uvs.end - uvs.start;

                var pixel = size / 16;

                UVmin = uvs.start + MathUtil.NextFloat(0, 12) * pixel;
                UVmax = UVmin + pixel * 4;
            }

            lastParticleScale = this.particleScale *= MathUtil.NextFloat(1, 1.5f);

            rotStep = new Vector3(MathUtil.NextFloat(), MathUtil.NextFloat(), MathUtil.NextFloat()).Normalized() * MathHelper.Pi * 20;
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

            rot += rotStep * (motion.Xz * 4).LengthFast;

            motion.Y -= 0.04f * gravity;

            Move();

            motion.Xz *= 0.8664021f;

            if (onGround)
            {
                motion.Xz *= 0.6676801f;
            }
        }

        public override void Render(Matrix4 viewMatrix, float particalTicks)
        {
            var partialPos = lastPos + (pos - lastPos) * particalTicks;
            var partialRot = lastRot + (rot - lastRot) * particalTicks;

            var partialScale = lastParticleScale + (particleScale - lastParticleScale) * particalTicks;
            var partialAlpha = lastParticleAlpha + (particleAlpha - lastParticleAlpha) * particalTicks;

            var model = ParticleRenderer.ParticleModel;

            model.shader.loadTransformationMatrix(MatrixHelper.createTransformationMatrix(partialPos - Vector3.One * partialScale / 2, partialRot, partialScale));
            model.shader.loadVec3(Vector3.One, "lightColor");//TODO - later for when there are multi-color lgihts
            model.shader.loadVec2(UVmin, "UVmin");
            model.shader.loadVec2(UVmax, "UVmax");
            model.shader.loadFloat(partialAlpha, "alpha");

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            model.rawModel.Render(model.shader.renderType);
        }
    }
}
