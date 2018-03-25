using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SharpCraft
{
    internal class ModelChunk
    {
        private ConcurrentDictionary<ShaderProgram, ModelChunkFragment> fragmentPerShader;

        private List<ShaderProgram> _shaders;

        public bool isGenerated => fragmentPerShader.Keys.Count > 0;

        public ModelChunk()
        {
            fragmentPerShader = new ConcurrentDictionary<ShaderProgram, ModelChunkFragment>();
            _shaders = new List<ShaderProgram>();
        }

        public void setFragmentModelWithShader(ShaderProgram shader, ModelChunkFragment model)
        {
            fragmentPerShader.TryRemove(shader, out var removed);

            _shaders.Add(shader);

            fragmentPerShader.TryAdd(shader, model);
        }

        public ModelChunkFragment getFragmentModelWithShader(ShaderProgram shader)
        {
            return fragmentPerShader.TryGetValue(shader, out var model) ? model : null;
        }

        public void destroyFragmentModelWithShader(ShaderProgram shader)
        {
            if (fragmentPerShader.TryRemove(shader, out var removed))
            {
                removed.destroy();
                _shaders.Remove(shader);
            }
        }

        public List<ShaderProgram> getShadersPresent()
        {
            return _shaders;
        }

        public void destroy()
        {
            while (_shaders.Count > 0)
            {
                destroyFragmentModelWithShader(_shaders[0]);
            }
        }
    }
}