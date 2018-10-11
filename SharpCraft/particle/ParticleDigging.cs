using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using SharpCraft_Client.block;
using SharpCraft_Client.json;
using SharpCraft_Client.model;
using SharpCraft_Client.render;
using SharpCraft_Client.util;
using SharpCraft_Client.world;
using Vector2 = OpenTK.Vector2;
using Vector3 = OpenTK.Vector3;

namespace SharpCraft_Client.particle
{
    internal class ParticleDigging : Particle
    {
        public BlockState State { get; }

        private Vector3 _rot;
        private Vector3 _lastRot;

        private readonly Vector3 _rotStep;

        public ParticleDigging(World world, Vector3 pos, Vector3 motion, float particleScale, BlockState state) : this(world, pos, motion, particleScale, state, FaceSides.AllSides[(int)MathUtil.NextFloat(0, 6)])
        {
        }

        public ParticleDigging(World world, Vector3 pos, Vector3 motion, float particleScale, BlockState state, FaceSides side) : base(world, pos, motion, particleScale, JsonModelLoader.TextureBlocks)
        {
            State = state;

            if (state.Model.RawModel is ModelBlockRaw)
            {
                var tex = state.Model.ParticleTexture;

                Vector2 start = tex.UVMin;
                Vector2 end = tex.UVMax;

                Vector2 size = end - start;

                Vector2 pixel = size / 16;

                UVmin = start + pixel * new Vector2(MathUtil.NextFloat(0, 12), MathUtil.NextFloat(0, 12));
                UVmax = UVmin + pixel * 4;
            }

            if (side == FaceSides.Up)
                Motion.Xz = SharpCraft.Instance.Camera.GetLookVec().Xz * 0.15f;

            Vector3 vec = new Vector3(MathUtil.NextFloat(-1), MathUtil.NextFloat(-1), MathUtil.NextFloat(-1));

            _rotStep = vec.Normalized() * MathUtil.NextFloat(40, 75);
        }

        public override void Update()
        {
            LastPos = Pos;
            _lastRot = _rot;

            LastParticleScale = ParticleScale;
            LastParticleAlpha = ParticleAlpha;

            if (!IsAlive) return;

            if (ParticleAge++ >= ParticleMaxAge)
            {
                if (ParticleAlpha >= 0.0015f)
                {
                    ParticleAlpha *= 0.525f;
                    ParticleScale *= 0.525f;
                }
                else
                    SetDead();
            }

            Motion.Y -= 0.04f * Gravity;

            Move();

            Motion.Xz *= 0.8864021f;

            if (OnGround)
            {
                Motion.Xz *= 0.6676801f;

                _rot.X = (float)Math.Round(_rot.X / 90) * 90;
                _rot.Z = (float)Math.Round(_rot.Z / 90) * 90;
            }
            else
            {
                _rot += _rotStep * MathHelper.Clamp((Motion.Xz * 5).LengthFast, OnGround ? 0 : 0.2f, 0.3f);
            }
        }

        public override void Render(float partialTicks)
        {
            Vector3 partialPos = Vector3.Lerp(LastPos, Pos, partialTicks);
            Vector3 partialRot = Vector3.Lerp(_lastRot, _rot, partialTicks);

            float partialScale = LastParticleScale + (ParticleScale - LastParticleScale) * partialTicks;
            float partialAlpha = LastParticleAlpha + (ParticleAlpha - LastParticleAlpha) * partialTicks;

            partialPos.Y += partialScale / 2.0f;

            ModelBaked model = ParticleRenderer.ParticleModel;
            model.Shader.SetMatrix4("transformationMatrix", MatrixHelper.CreateTransformationMatrix(partialPos, partialRot, partialScale));
            model.Shader.SetVector2("UVmin", UVmin);
            model.Shader.SetVector2("UVmax", UVmax);
            model.Shader.SetFloat("alpha", partialAlpha);

            GL.BindTexture(TextureTarget.Texture2D, TextureId);
            model.RawModel.Render();
        }
    }
}