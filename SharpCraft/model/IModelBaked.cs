using SharpCraft.render.shader;

namespace SharpCraft.model
{
    internal interface IModelBaked<T>
    {
        IModelRaw rawModel { get; }
	    Shader<T> shader { get; }

        void bind();

        void unbind();

        void destroy();
    }
}