﻿namespace SharpCraft
{
    internal interface IModelRaw
    {
        int vaoID { get; }
        int vertexCount { get; }
        int[] bufferIDs { get; }
    }
}