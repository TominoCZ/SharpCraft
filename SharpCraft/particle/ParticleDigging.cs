using SharpCraft.block;
using SharpCraft.model;
using SharpCraft.util;
using SharpCraft.world;
using System;
using System.Collections.Generic;
using System.Numerics;
using OpenTK.Graphics.OpenGL;
using SharpCraft.render;
using SharpCraft.texture;
using Vector2 = OpenTK.Vector2;
using Vector3 = OpenTK.Vector3;

namespace SharpCraft.particle
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

        public ParticleDigging(World world, Vector3 pos, Vector3 motion, float particleScale, BlockState state, FaceSides side) : base(world, pos, motion, particleScale, JsonModelLoader.TEXTURE_BLOCKS)
        {
            State = state;

            ModelBlock model = JsonModelLoader.GetModelForBlock(state.Block.UnlocalizedName);

            if (model.RawModel is ModelBlockRaw mbr)
            {
                Vector2 start;
                Vector2 end;

                if (state.Block.IsFullCube)
                {
                    List<float> uvs = new List<float>(8);
                    mbr.AppendUvsForSide(side, ref uvs);

                    start = new Vector2(uvs[0], uvs[1]);
                    end = new Vector2(uvs[4], uvs[5]); //4,5 because that's the 3. vertex and the local UV there is 1,1
                }
                else
                {
                    var tex = model.GetParticleTexture();

                    start = tex.UVMin;
                    end = tex.UVMax;
                }

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

            lastParticleScale = particleScale;
            lastParticleAlpha = particleAlpha;

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

                _rot.X = (float)Math.Round(_rot.X / 90) * 90;
                _rot.Z = (float)Math.Round(_rot.Z / 90) * 90;
            }
            else
            {
                _rot += _rotStep * Math.Clamp((Motion.Xz * 5).LengthFast, onGround ? 0 : 0.2f, 0.3f);
            }
        }

        public override void Render(float partialTicks)
        {
            Vector3 partialPos = Vector3.Lerp(LastPos, Pos, partialTicks);
            Vector3 partialRot = Vector3.Lerp(_lastRot, _rot, partialTicks);

            float partialScale = lastParticleScale + (particleScale - lastParticleScale) * partialTicks;

            partialPos.Y += partialScale / 2;

            ModelBaked<Particle> model = ParticleRenderer.ParticleModel;
            model.Shader.UpdateGlobalUniforms();
            model.Shader.UpdateModelUniforms();
            model.Shader.UpdateInstanceUniforms(MatrixHelper.CreateTransformationMatrix(partialPos, partialRot, partialScale), this);

            GL.BindTexture(TextureTarget.Texture2D, textureID);
            model.RawModel.Render(PrimitiveType.Quads);
        }
    }
}