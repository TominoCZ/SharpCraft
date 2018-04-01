using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.model;
using SharpCraft.render;
using SharpCraft.texture;
using SharpCraft.util;
using SharpCraft.world;

namespace SharpCraft.particle
{
    class Particle : Entity
    {
        public float particleScale;

        public int particleAge;
        public int particleMaxAge;

        protected float particleAlpha = 1;

        protected float lastParticleScale;
        protected float lastParticleAlpha = 1;

        protected int textureID;

        protected Vector2 UVmin;
        protected Vector2 UVmax;

        protected Particle(World world, Vector3 pos, Vector3 motion, float particleScale, int textureID) : this(world, pos, motion, particleScale, textureID, Vector2.Zero, Vector2.Zero)
        {

        }

        protected Particle(World world, Vector3 pos, Vector3 motion, float particleScale, int textureID, Vector2 UVmin, Vector2 UVmax) : base(world, pos, motion)
        {
            this.particleScale = particleScale / 10;

            this.textureID = textureID;
            this.UVmin = UVmin;
            this.UVmax = UVmax;

            collisionBoundingBox = new AxisAlignedBB(0, 0, 0, this.particleScale, this.particleScale, this.particleScale);
            boundingBox = collisionBoundingBox.offset(pos + Vector3.UnitY * collisionBoundingBox.size.Y / 2);

            particleMaxAge = (int)MathUtil.NextFloat(10, 50);

            gravity = 0.9875f;
        }

        public override void Update()
        {
            lastParticleScale = particleScale;
            lastParticleAlpha = particleAlpha;

            base.Update();

            if (particleAge++ >= particleMaxAge)
            {
                if (particleAlpha >= 0.0015f)
                {
                    particleAlpha *= 0.575f;
                    particleScale *= 0.725f;
                }
                else
                    SetDead();
            }
        }

        public override void Render(Matrix4 viewMatrix, float particalTicks)
        {
            var partialPos = lastPos + (pos - lastPos) * particalTicks;

            var partialScale = lastParticleScale + (particleScale - lastParticleScale) * particalTicks;
            var partialAlpha = lastParticleAlpha + (particleAlpha - lastParticleAlpha) * particalTicks;

            var model = ParticleRenderer.ParticleModel;

            model.shader.loadTransformationMatrix(MatrixHelper.createTransformationMatrix(partialPos - Vector3.One * partialScale / 2, Vector3.Zero, partialScale));
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
