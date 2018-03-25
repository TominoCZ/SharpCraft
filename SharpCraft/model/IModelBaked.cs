namespace SharpCraft
{
    internal interface IModelBaked
    {
        IModelRaw rawModel { get; }
        ShaderProgram shader { get; }

        void bind();

        void unbind();

        void destroy();
    }
}