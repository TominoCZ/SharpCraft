using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpCraft
{
    class ModelCubeOutline : ModelBaked
    {
        public ModelCubeOutline(ShaderProgram shader): base(null, shader)
        {
            rawModel = ModelManager.loadModelToVAO(ModelHelper.createCubeModel(), 3);
        }
    }
}
