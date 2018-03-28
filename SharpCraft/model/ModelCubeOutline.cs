using SharpCraft.shader;
using SharpCraft.util;

namespace SharpCraft.model
{
    internal class ModelCubeOutline : ModelBaked
    {
        public ModelCubeOutline(ShaderProgram shader) : base(null, shader)
        {
            rawModel = ModelManager.loadModelToVAO(ModelHelper.createCubeModel(), 3);
        }
    }
}