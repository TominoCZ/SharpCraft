using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.block;
using SharpCraft.model;
using SharpCraft.render;
using SharpCraft.texture;
using SharpCraft.util;
using SharpCraft.world;
using System;
using System.Collections.Generic;

namespace SharpCraft.particle
{
    internal class ParticleDigging : Particle
    {
        public BlockState state { get; }

        protected Vector3 rot;
        protected Vector3 lastRot;

        protected Vector3 rotStep;

        public ParticleDigging(World world, Vector3 pos, Vector3 motion, float particleScale, BlockState state) : this(world, pos, motion, particleScale, state, FaceSides.AllSides[(int)MathUtil.NextFloat(0, 6)])
        {
        }

        public ParticleDigging(World world, Vector3 pos, Vector3 motion, float particleScale, BlockState state, FaceSides side) : base(world, pos, motion, particleScale, JsonModelLoader.TEXTURE_BLOCKS)
        {
            this.state = state;

            ModelBlock model = JsonModelLoader.GetModelForBlock(state.Block.UnlocalizedName);

            if (model.RawModel is ModelBlockRaw mbr)
            {
                List<float> uvs = new List<float>(8);
                mbr.AppendUvsForSide(side, ref uvs);

                Vector2 start = new Vector2(uvs[0], uvs[1]);
                Vector2 end = new Vector2(uvs[4], uvs[5]); //4,5 because that's the 3. vertex and the local UV there is 1,1

                Vector2 size = end -start;

                Vector2 pixel = size / 16;

                UVmin = start + pixel * new Vector2(MathUtil.NextFloat(0, 12), MathUtil.NextFloat(0, 12));
                UVmax = UVmin + pixel * 4;
            }

            if (side == FaceSides.Up)
                Motion.Xz = SharpCraft.Instance.Camera.GetLookVec().Xz * 0.15f;

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
            /*
            ModelBaked<Particle> model = ParticleRenderer.ParticleModel;
            model.Shader.UpdateGlobalUniforms();
            model.Shader.UpdateModelUniforms();
            model.Shader.UpdateInstanceUniforms(MatrixHelper.CreateTransformationMatrix(partialPos - (Vector3.UnitX + Vector3.UnitZ) * partialScale / 2, partialRot, partialScale), this);

            GL.BindTexture(TextureTarget.Texture2D, textureID);
            model.RawModel.Render(PrimitiveType.Quads);*/
        }
    }
}