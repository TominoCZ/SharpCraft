using SharpCraft_Client.render.shader;

namespace SharpCraft_Client.model
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