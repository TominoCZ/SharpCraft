using OpenTK.Graphics.OpenGL;

namespace SharpCraft.model
{
    public interface IModelRaw
    {
        int VaoID { get; }
        int VertexCount { get; }
        int[] BufferIDs { get; }

        bool hasLocalData();

        void Render(PrimitiveType shaderRenderType);
    }
}