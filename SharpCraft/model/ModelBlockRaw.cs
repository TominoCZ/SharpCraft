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

        public void AppendAllVertexData(List<float> vertexes, List<float> normals, List<float> uvs, BlockPos offset)
        {
            for (int i = 0; i < _vertexes.Length; i += 3)
            {
                vertexes.Add(_vertexes[i] + offset.X);
                vertexes.Add(_vertexes[i + 1] + offset.Y);
                vertexes.Add(_vertexes[i + 2] + offset.Z);
            }

            normals.AddRange(_normals);
            uvs.AddRange(_uvs);
        }

        public void AppendVertexDataForSide(FaceSides side, List<float> vertexes, List<float> normals, List<float> uvs, BlockPos offset)
        {
            if (!FaceSides.Parse(side, out var parsed)) //FaceSides is a struct containing Vector2 values (normals)
                return;

            int faceIndex = (int)parsed * 18;

            for (int i = 0; i < 18; i += 3)
            {
                vertexes.Add(_vertexes[faceIndex + i] + offset.X);
                vertexes.Add(_vertexes[faceIndex + i + 1] + offset.Y);
                vertexes.Add(_vertexes[faceIndex + i + 2] + offset.Z);

                normals.Add(_normals[faceIndex + i]);
                normals.Add(_normals[faceIndex + i + 1]);
                normals.Add(_normals[faceIndex + i + 2]);
            }

            faceIndex = (int)parsed * 12;

            for (int i = 0; i < 12; i += 2)
            {
                uvs.Add(_uvs[faceIndex + i]);
                uvs.Add(_uvs[faceIndex + i + 1]);
            }
        }

        //unused for now
        public void AppendVertexesForSide(FaceSides side, List<float> vertexes, BlockPos offset)
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
            if (!FaceSides.Parse(side, out var parsed)) //FaceSides is a struct containing Vector2 values (normals)
                return;

            int faceIndex = (int)parsed * 18;

            for (int i = 0; i < 18; i += 3)
            {
                vertexes.Add(_vertexes[faceIndex + i] + offset.X);
                vertexes.Add(_vertexes[faceIndex + i + 1] + offset.Y);
                vertexes.Add(_vertexes[faceIndex + i + 2] + offset.Z);
            }
        }

        //unused for now
        public void AppendNormalsForSide(FaceSides side, List<float> normals)
        {
            if (!FaceSides.Parse(side, out var parsed))
                return;

            int faceIndex = (int)parsed * 18;

            for (int i = 0; i < 18; i += 3)
            {
                normals.Add(_normals[faceIndex + i]);
                normals.Add(_normals[faceIndex + i + 1]);
                normals.Add(_normals[faceIndex + i + 2]);
            }
        }

        public void AppendUvsForSide(FaceSides side, List<float> uvs)
        {
            if (!FaceSides.Parse(side, out var parsed))
                return;

            int faceIndex = (int)parsed * 12;

            for (int i = 0; i < 12; i += 2)
            {
                uvs.Add(_uvs[faceIndex + i]);
                uvs.Add(_uvs[faceIndex + i + 1]);
            }
        }
    }
}