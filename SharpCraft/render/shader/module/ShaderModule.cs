using OpenTK;
using SharpCraft.model;

namespace SharpCraft.render.shader.module
{
    public abstract class ShaderModule<TRenderable>
    {
        protected readonly Shader<TRenderable> Parent;

        public ShaderModule(Shader<TRenderable> parent)
        {
            Parent = parent;
        }

        public abstract void InitUniforms();

        public virtual void UpdateGlobalUniforms()
        {
        }

        public virtual void UpdateModelUniforms(IModelRaw model)
        {
        }

        public abstract void UpdateInstanceUniforms(Matrix4 transform, TRenderable renderable);
    }
}