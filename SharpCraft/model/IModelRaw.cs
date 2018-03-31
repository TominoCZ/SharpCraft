using OpenTK.Graphics.OpenGL;

namespace SharpCraft.model
{
    public interface IModelRaw
    {
        int vaoID { get; }
        int vertexCount { get; }
        int[] bufferIDs { get; }

        bool hasLocalData();

        void Render(PrimitiveType shaderRenderType);
    }
}