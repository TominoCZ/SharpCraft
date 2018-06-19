using SharpCraft.block;
using System.Collections.Generic;

namespace SharpCraft.model
{
    public class ModelBlockRaw : ModelRaw
    {
        private readonly float[] _vertexes;
        private readonly float[] _normals;
        private readonly float[] _uvs;

        public ModelBlockRaw(int vaoID, float[] vertexes, float[] normals, float[] uvs, params int[] bufferIDs) : base(vaoID, vertexes.Length / 3, bufferIDs)
        {
            _vertexes = vertexes;
            _normals = normals;
            _uvs = uvs;
        }

        public void AppendAllVertexData(ref List<float> vertexes, ref List<float> normals, ref List<float> uvs, BlockPos offset)
        {
            for (int i = 0; i < 72; i += 3)
            {
                vertexes.Add(_vertexes[i] + offset.X);
                vertexes.Add(_vertexes[i + 1] + offset.Y);
                vertexes.Add(_vertexes[i + 2] + offset.Z);
            }

            normals.AddRange(_normals);
            uvs.AddRange(_uvs);
        }

        //TODO - only use these when the block model is one 1x1x1 big cube
        public void AppendVertexesForSide(FaceSides side, ref List<float> vertexes, BlockPos offset)
        {
            /*
               top = 0
               bottom = 1
               north = 2
               south = 3
               west = 4
               east = 5
               --> these are used as indexes of the faces, since the model vertex data is added in the same exact order of the TextureType enum values
            */
            TextureType parsed = FaceSides.Parse(side);//FaceSides is a struct containing Vector2 values (normals)
            int faceIndex = (int)parsed * 12;

            for (int i = 0; i < 12; i += 3)
            {
                vertexes.Add(_vertexes[faceIndex + i] + offset.X);
                vertexes.Add(_vertexes[faceIndex + i + 1] + offset.Y);
                vertexes.Add(_vertexes[faceIndex + i + 2] + offset.Z);
            }
        }

        public void AppendNormalsForSide(FaceSides side, ref List<float> normals)
        {
            TextureType parsed = FaceSides.Parse(side);
            int faceIndex = (int)parsed * 12;

            for (int i = 0; i < 12; i += 3)
            {
                normals.Add(_normals[faceIndex + i]);
                normals.Add(_normals[faceIndex + i + 1]);
                normals.Add(_normals[faceIndex + i + 2]);
            }
        }

        public void AppendUvsForSide(FaceSides side, ref List<float> uvs)
        {
            TextureType parsed = FaceSides.Parse(side);
            int faceIndex = (int)parsed * 8;

            for (int i = 0; i < 8; i += 2)
            {
                uvs.Add(_uvs[faceIndex + i]);
                uvs.Add(_uvs[faceIndex + i + 1]);
            }
        }
    }
}