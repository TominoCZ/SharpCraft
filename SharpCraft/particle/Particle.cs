using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.entity;
using SharpCraft.model;
using SharpCraft.render;
using SharpCraft.util;
using SharpCraft.world;

namespace SharpCraft.particle
{
    public class Particle : Entity
    {
        public float ParticleScale;

        public int ParticleAge;
        public int ParticleMaxAge;

        protected float ParticleAlpha = 1;

        protected float LastParticleScale;
        protected float LastParticleAlpha = 1;

        protected int TextureId;

        public Vector2 UVmin;
        public Vector2 UVmax;

        protected Particle(World world, Vector3 pos, Vector3 motion, float particleScale, int textureId) : this(world, pos, motion, particleScale, textureId, Vector2.Zero, Vector2.Zero)
        {
        }

        protected Particle(World world, Vector3 pos, Vector3 motion, float scale, int textureId, Vector2 uVmin, Vector2 uVmax) : base(world, pos, motion)
        {
            LastParticleScale = ParticleScale = scale / 10;

            TextureId = textureId;
            UVmin = uVmin;
            UVmax = uVmax;

            CollisionBoundingBox = new AxisAlignedBb(ParticleScale);
            BoundingBox = CollisionBoundingBox.Offset(pos - (Vector3.UnitX * CollisionBoundingBox.Size.X / 2 + Vector3.UnitZ * CollisionBoundingBox.Size.Z / 2));

            ParticleMaxAge = (int)MathUtil.NextFloat(10, 50);

            Gravity = 0.9f;
        }

        public override void Update()
        {
            LastParticleScale = ParticleScale;
            LastParticleAlpha = ParticleAlpha;

            if (ParticleAge++ >= ParticleMaxAge)
            {
                if (ParticleAlpha >= 0.0015f)
                {
                    ParticleAlpha *= 0.575f;
                    ParticleScale *= 0.725f;
                }
                else
                    SetDead();
            }

            base.Update();
        }

        public override void Render(float partialTicks)
        {
            Vector3 partialPos = Vector3.Lerp(LastPos, Pos, partialTicks);
            float partialScale = LastParticleScale + (ParticleScale - LastParticleScale) * partialTicks;

            partialPos.Y += partialScale / 2;

            ModelBaked model = ParticleRenderer.ParticleModel;
            
            model.Shader.SetMatrix4("transformationMatrix", MatrixHelper.CreateTransformationMatrix(partialPos, Vector3.Zero, partialScale));

            GL.BindTexture(TextureTarget.Texture2D, TextureId);
            model.RawModel.Render();
        }

        public float GetAlpha()
        {
            return LastParticleAlpha + (ParticleAlpha - LastParticleAlpha) * SharpCraft.Instance.GetPartialTicksForRender();
        }
    }
}