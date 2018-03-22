namespace SharpCraft
{
    interface IModelBaked
    {
        IModelRaw rawModel { get; }
        ShaderProgram shader { get; }

        void bind();
        void unbind();
    }
}