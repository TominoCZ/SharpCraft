using OpenTK;
using SharpCraft.render.shader;
using SharpCraft.util;

namespace SharpCraft.model
{
    internal class ModelCubeOutline : ModelBaked<ModelCubeOutline>
    {
        public ModelCubeOutline() : base(ModelManager.loadModelToVAO(ModelHelper.createCubeModel(), 3), new Shader<ModelCubeOutline>("color"))
        {
        }

	    public Vector3 GetColor()
	    {
			return Vector3.One;
	    }
    }
}