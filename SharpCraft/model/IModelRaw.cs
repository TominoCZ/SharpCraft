namespace SharpCraft
{
    interface IModelRaw
    {
        int vaoID { get; }
        int vertexCount { get; }
        int[] bufferIDs { get; }
        bool hasUVs { get; }
        bool hasNormals { get; }
    }
}