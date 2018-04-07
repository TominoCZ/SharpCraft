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
                     + MathUtil.NextFloat(-0.21f, 0.21f) * Math.Abs(moo.boundingBox.max.Y - moo.boundingBox.min.Y);
                var f2 = moo.hitVec.Z
                     + MathUtil.NextFloat(-0.21f, 0.21f) * Math.Abs(moo.boundingBox.max.Z - moo.boundingBox.min.Z);

                var vec = new Vector3(MathUtil.NextFloat(-0.025f, 0.025f), MathUtil.NextFloat(-0.025f, 0.025f), MathUtil.NextFloat(-0.025f, 0.025f));

                var pos = vec;
                pos.X += f0;
                pos.Y += f1;
                pos.Z += f2;

                var motion = vec + moo.normal * 0.075f;

                var ok = SharpCraft.Instance.DestroyProgresses.TryGetValue(moo.blockPos, out var progress);

                AddParticle(new ParticleDigging(
                    SharpCraft.Instance.World,
                    pos,
                    motion,
                    0.4f + (ok ? progress.Percentage : 0),
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
                        var motion = new Vector3(MathUtil.NextFloat(-0.2f, 0.2f), MathUtil.NextFloat(-0.1f + y / perAxis * 0.2f, 0.2f), MathUtil.NextFloat(-0.2f, 0.2f));

                        var particle = new ParticleDigging(SharpCraft.Instance.World, posVec + newPos, motion, MathUtil.NextFloat(1, 1.75f), block, meta);

                        AddParticle(particle);
                    }
                }
            }
        }
    }
}
