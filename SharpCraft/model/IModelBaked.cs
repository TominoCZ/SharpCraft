using SharpCraft.render.shader;

namespace SharpCraft.model
{
    internal interface IModelBaked<T>
    {
        IModelRaw RawModel { get; }
	    Shader<T> Shader { get; }

        void Bind();

        void Unbind();

        void Destroy();
    }
}