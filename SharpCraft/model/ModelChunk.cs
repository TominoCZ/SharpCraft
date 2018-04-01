using System.Collections.Concurrent;
using SharpCraft.render.shader;

namespace SharpCraft.model
{
    internal class ModelChunk
    {
        public ConcurrentDictionary<Shader<ModelBlock>, ModelChunkFragment> fragmentPerShader { get; }

        public bool isGenerated => fragmentPerShader.Keys.Count > 0;

        public ModelChunk()
        {
            fragmentPerShader = new ConcurrentDictionary<Shader<ModelBlock>, ModelChunkFragment>();
            //_shaders = new List<Shader>();
        }

        public void setFragmentModelWithShader(Shader<ModelBlock> shader, ModelChunkFragment model)
        {
            fragmentPerShader.TryRemove(shader, out var removed);

            //_shaders.Add(shader);

            fragmentPerShader.TryAdd(shader, model);
        }

        public ModelChunkFragment getFragmentModelWithShader(Shader<ModelBlock> shader)
        {
            return fragmentPerShader.TryGetValue(shader, out var model) ? model : null;
        }

        public void destroyFragmentModelWithShader(Shader<ModelBlock> shader)
        {
            if (fragmentPerShader.TryRemove(shader, out var removed))
            {
                removed.destroy();
                //_shaders.Remove(shader);
            }
        }

        //public List<Shader> getShadersPresent()
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