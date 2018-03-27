using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SharpCraft
{
    internal class ModelChunk
    {
        public ConcurrentDictionary<ShaderProgram, ModelChunkFragment> fragmentPerShader { get; }

        public bool isGenerated => fragmentPerShader.Keys.Count > 0;

        public ModelChunk()
        {
            fragmentPerShader = new ConcurrentDictionary<ShaderProgram, ModelChunkFragment>();
            //_shaders = new List<ShaderProgram>();
        }

        public void setFragmentModelWithShader(ShaderProgram shader, ModelChunkFragment model)
        {
            fragmentPerShader.TryRemove(shader, out var removed);

            //_shaders.Add(shader);

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
                //_shaders.Remove(shader);
            }
        }

        //public List<ShaderProgram> getShadersPresent()
        //{
            //return _shaders;
       //}

        public void destroy()
        {
            foreach (var shader in fragmentPerShader.Keys)
            {
                destroyFragmentModelWithShader(shader);
            }
            /*
            while (_shaders.Count > 0)
            {
                destroyFragmentModelWithShader(_shaders[0]);
            }*/
        }
    }
}