using SharpCraft.render.shader;
using System.Collections.Generic;

namespace SharpCraft.model
{
    internal class ModelGuiItem : ModelBaked<object>
    {
        public ModelGuiItem(Shader<object> shader) : base(null, shader)
        {
            RawQuad rawQuad = new RawQuad(new float[] {
                -1,  1,
                -1, -1,
                1, -1,
                1, 1 },
                new float[] {
                0, 0,
                1, 0,
                1, 1,
                0, 1}, 2);

            RawModel = ModelManager.LoadModelToVAO(new List<RawQuad> { rawQuad }, 2);
        }
    }
}