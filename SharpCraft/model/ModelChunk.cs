using System.Collections.Generic;

namespace SharpCraft
{
    internal class ModelChunk
    {
        private Dictionary<ShaderProgram, ModelChunkFragment> fragmentPerShader;

        private List<ShaderProgram> shaders;

        public bool isGenerated => shaders.Count > 0;

        public ModelChunk()
        {
            fragmentPerShader = new Dictionary<ShaderProgram, ModelChunkFragment>();
            shaders = new List<ShaderProgram>();
        }

        public void setFragmentModelWithShader(ShaderProgram shader, ModelChunkFragment model)
        {
            //if (fragmentPerShader.ContainsKey(shader))
            fragmentPerShader.Remove(shader);

            fragmentPerShader.Add(shader, model);
            shaders.Add(shader);
        }

        public ModelChunkFragment getFragmentModelWithShader(ShaderProgram shader)
        {
            return fragmentPerShader[shader];
        }

        public List<ShaderProgram> getShadersPresent()
        {
            return shaders;
        }
    }
}