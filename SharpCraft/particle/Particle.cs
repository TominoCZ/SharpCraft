using OpenTK;
using SharpCraft.entity;
using SharpCraft.util;
using SharpCraft.world;

namespace SharpCraft.particle
{
    public class Particle : Entity
    {
        public float particleScale;

        public int particleAge;
        public int particleMaxAge;

        protected float particleAlpha = 1;

        protected float lastParticleScale;
        protected float lastParticleAlpha = 1;

        protected int textureID;

        public Vector2 UVmin;
        public Vector2 UVmax;

        protected Particle(World world, Vector3 pos, Vector3 motion, float particleScale, int textureID) : this(world, pos, motion, particleScale, textureID, Vector2.Zero, Vector2.Zero)
        {
        }

        protected Particle(World world, Vector3 pos, Vector3 motion, float scale, int textureID, Vector2 UVmin, Vector2 UVmax) : base(world, pos, motion)
        {
            lastParticleScale = particleScale = scale / 10;

            this.textureID = textureID;
            this.UVmin = UVmin;
            this.UVmax = UVmax;

            collisionBoundingBox = new AxisAlignedBB(particleScale);
            boundingBox = collisionBoundingBox.offset(pos - (Vector3.UnitX * collisionBoundingBox.size.X / 2 + Vector3.UnitZ * collisionBoundingBox.size.Z / 2));

            particleMaxAge = (int)MathUtil.NextFloat(10, 50);

            gravity = 0.9f;
        }

        public override void Update()
        {
            lastParticleScale = particleScale;
            lastParticleAlpha = particleAlpha;

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

            base.Update();
        }

        public override void Render(float partialTicks)
        {
            Vector3 partialPos = Vector3.Lerp(LastPos, Pos, partialTicks);
            float partialScale = lastParticleScale + (particleScale - lastParticleScale) * partialTicks;

            /*
            ModelBaked<Particle> model = ParticleRenderer.ParticleModel;

            model.Shader.UpdateGlobalUniforms();
            model.Shader.UpdateModelUniforms();
            model.Shader.UpdateInstanceUniforms(MatrixHelper.CreateTransformationMatrix(partialPos - Vector3.One * partialScale / 20, Vector3.Zero, partialScale), this);

            GL.BindTexture(TextureTarget.Texture2D, textureID);
            model.RawModel.Render(PrimitiveType.Quads);*/
        }

        public float GetAlpha()
        {
            return lastParticleAlpha + (particleAlpha - lastParticleAlpha) * SharpCraft.Instance.GetPartialTicksForRender();
        }
    }
}