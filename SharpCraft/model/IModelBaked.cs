using SharpCraft.render.shader;

namespace SharpCraft.model
{
    internal interface IModelBaked
    {
        IModelRaw RawModel { get; }

        Shader Shader { get; }

        void Bind();

        void Unbind();

        void Destroy();
    }
}