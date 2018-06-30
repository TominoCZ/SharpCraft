namespace SharpCraft.model
{
    public class ModelItemRaw : ModelRaw
    {
        public ModelItemRaw(int vaoID, int vertexCount, params int[] bufferIDs) : base(vaoID, vertexCount, bufferIDs)
        {
        }
    }
}