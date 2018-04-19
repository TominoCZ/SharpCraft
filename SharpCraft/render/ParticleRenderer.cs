using System;
using System.Collections.Generic;
using OpenTK;
using SharpCraft.block;
using SharpCraft.entity;
using SharpCraft.model;
using SharpCraft.particle;
using SharpCraft.render.shader;
using SharpCraft.render.shader.shaders;
using SharpCraft.util;
using SharpCraft.world;

namespace SharpCraft.render
{
    class ParticleRenderer
    {
        private List<Particle> _particles;

        public static ModelBaked<Particle> ParticleModel;

        static ParticleRenderer()
        {
            ParticleModel = new ModelParticle(new ShaderParticle());
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
                var particle = _particles[i];

                particle.Update();

                if (!particle.isAlive)
                    _particles.Remove(particle);
            }
        }

        public void SpawnDiggingParticle(MouseOverObject moo)
        {
            if (moo.hit is EnumBlock block)
            {
                var f0 = moo.hitVec.X
                     + MathUtil.NextFloat(-0.21f, 0.21f) * Math.Abs(moo.boundingBox.max.X - moo.boundingBox.min.X);
                var f1 = moo.hitVec.Y
                     + MathUtil.NextFloat(0, 0.1f) * Math.Abs(moo.boundingBox.max.Y - moo.boundingBox.min.Y);
                var f2 = moo.hitVec.Z
                     + MathUtil.NextFloat(-0.21f, 0.21f) * Math.Abs(moo.boundingBox.max.Z - moo.boundingBox.min.Z);

                if (moo.sideHit == FaceSides.Down)
                    f1 = moo.boundingBox.min.Y - 0.05f;
                else if (moo.sideHit == FaceSides.East)
                    f0 = moo.boundingBox.max.X + 0.05f;
                else if (moo.sideHit == FaceSides.North)
                    f2 = moo.boundingBox.min.Z - 0.05f;
                else if (moo.sideHit == FaceSides.South)
                    f2 = moo.boundingBox.max.Z + 0.05f;
                else if (moo.sideHit == FaceSides.Up)
                    f1 = moo.boundingBox.max.Y + 0.1f;
                else if (moo.sideHit == FaceSides.West)
                    f0 = moo.boundingBox.min.X - 0.05f;

                var pos = new Vector3(f0, f1, f2) + moo.normal * 0.1f;

                var motion = moo.normal * MathUtil.NextFloat(0.0075f, 0.03f);
                var mult = 0.75f / (MathUtil.Distance(pos, moo.hitVec) + 0.01f);
                motion.Xz *= mult;

                motion.Y += 0.02f;

                var ok = SharpCraft.Instance.DestroyProgresses.TryGetValue(moo.blockPos, out var progress);

                AddParticle(new ParticleDigging(
                    SharpCraft.Instance.World,
                    pos,
                    motion,
                    0.35f + (ok ? progress.PartialProgress * 0.5f : 0),
                    block,
                    moo.sideHit,
                    SharpCraft.Instance.World.GetMetadata(moo.blockPos)));
            }
        }

        public void SpawnDestroyParticles(BlockPos pos, EnumBlock block, int meta)
        {
            var posVec = pos.ToVec();

            var perAxis = 3;
            var step = 1f / perAxis;

            for (var x = 0f; x < perAxis; x++)
            {
                for (var y = 0f; y < perAxis; y++)
                {
                    for (var z = 0f; z < perAxis; z++)
                    {
                        var newPos = new Vector3(x, y, z) * step + Vector3.One * step / 2f;
                        var motion = new Vector3(MathUtil.NextFloat(-0.15f, 0.15f), MathUtil.NextFloat(-0.1f + y / perAxis * 0.2f, 0.2f), MathUtil.NextFloat(-0.15f, 0.15f));

                        var particle = new ParticleDigging(SharpCraft.Instance.World, posVec + newPos, motion, MathUtil.NextFloat(1, 1.5f), block, meta);

                        AddParticle(particle);
                    }
                }
            }
        }
    }
}
