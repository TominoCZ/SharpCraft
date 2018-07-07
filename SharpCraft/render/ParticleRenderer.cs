using OpenTK;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.model;
using SharpCraft.particle;
using SharpCraft.render.shader;
using SharpCraft.util;
using SharpCraft.world;
using System;
using System.Collections.Generic;

namespace SharpCraft.render
{
    internal class ParticleRenderer
    {
        private readonly List<Particle> _particles;

        public static ModelBaked ParticleModel;

        static ParticleRenderer()
        {
            ParticleModel = new ModelParticle(new Shader("particle", "UVmin", "UVmax", "alpha"));
        }

        public ParticleRenderer()
        {
            _particles = new List<Particle>();
        }

        public void AddParticle(Particle particle)
        {
            if (particle != null)
                _particles.Add(particle);
        }

        public void Render(float partialTicks)
        {
            ParticleModel.Bind();

            for (int i = 0; i < _particles.Count; i++)
                _particles[i].Render(partialTicks);

            ParticleModel.Unbind();
        }

        public void TickParticles()
        {
            for (int i = 0; i < _particles.Count; i++)
            {
                Particle particle = _particles[i];

                particle.Update();

                if (!particle.IsAlive)
                    _particles.Remove(particle);
            }
        }

        public void SpawnDiggingParticle(MouseOverObject moo)
        {
            if (moo.hit == HitType.Block)
            {
                var state = SharpCraft.Instance.World.GetBlockState(moo.blockPos);

                if (state.Model == null)
                    return;

                float f0 = moo.hitVec.X
                     + MathUtil.NextFloat(-0.21f, 0.21f) * Math.Abs(moo.boundingBox.Max.X - moo.boundingBox.Min.X);
                float f1 = moo.hitVec.Y
                     + MathUtil.NextFloat(0, 0.1f) * Math.Abs(moo.boundingBox.Max.Y - moo.boundingBox.Min.Y);
                float f2 = moo.hitVec.Z
                     + MathUtil.NextFloat(-0.21f, 0.21f) * Math.Abs(moo.boundingBox.Max.Z - moo.boundingBox.Min.Z);

                if (moo.sideHit == FaceSides.Down)
                    f1 = moo.boundingBox.Min.Y - 0.05f;
                else if (moo.sideHit == FaceSides.East)
                    f0 = moo.boundingBox.Max.X + 0.05f;
                else if (moo.sideHit == FaceSides.North)
                    f2 = moo.boundingBox.Min.Z - 0.05f;
                else if (moo.sideHit == FaceSides.South)
                    f2 = moo.boundingBox.Max.Z + 0.05f;
                else if (moo.sideHit == FaceSides.Up)
                    f1 = moo.boundingBox.Max.Y + 0.1f;
                else if (moo.sideHit == FaceSides.West)
                    f0 = moo.boundingBox.Min.X - 0.05f;

                Vector3 pos = new Vector3(f0, f1, f2) + moo.normal * 0.1f;

                Vector3 motion = moo.normal * MathUtil.NextFloat(0.0075f, 0.03f);
                float mult = 0.75f / (Vector3.Distance(pos, moo.hitVec) + 0.01f);
                motion.Xz *= mult;

                motion.Y += 0.02f;

                bool ok = SharpCraft.Instance.DestroyProgresses.TryGetValue(moo.blockPos, out DestroyProgress progress);

                AddParticle(new ParticleDigging(
                    SharpCraft.Instance.World,
                    pos,
                    motion,
                    0.35f + (ok ? progress.PartialProgress * 0.5f : 0),
                    state,
                    moo.sideHit));
            }
        }

        public void SpawnDestroyParticles(BlockPos pos, BlockState state)
        {
            Vector3 posVec = pos.ToVec();

            int perAxis = 4;
            float step = 1f / perAxis;

            Vector3 halfVec = Vector3.One * 0.5f;

            for (float x = 0f; x < perAxis; x++)
            {
                for (float y = 0f; y < perAxis; y++)
                {
                    for (float z = 0f; z < perAxis; z++)
                    {
                        Vector3 localPos = new Vector3(x, y, z) * step;
                        Vector3 worldPos = localPos + halfVec * step;

                        Vector3 vec = localPos - halfVec;
                        Vector3 motion = vec.Normalized() * 0.2f;

                        motion.X += MathUtil.NextFloat(-0.05f, 0.05f);
                        motion.Y += MathUtil.NextFloat(-0.05f, 0.05f);
                        motion.Z += MathUtil.NextFloat(-0.05f, 0.05f);

                        ParticleDigging particle = new ParticleDigging(SharpCraft.Instance.World, posVec + worldPos, motion, MathUtil.NextFloat(1, 1.5f), state);

                        AddParticle(particle);
                    }
                }
            }
        }
    }
}