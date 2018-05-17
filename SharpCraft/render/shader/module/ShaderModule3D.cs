using OpenTK;
using SharpCraft.render.shader.uniform;

namespace SharpCraft.render.shader.module
{
    public class ShaderModule3D<T> : ShaderModule<T>
    {
        private UniformMat4 Projection;
        private UniformMat4 Transform;
        private UniformMat4 View;

        public ShaderModule3D(Shader<T> parent) : base(parent)
        {
        }

        public override void InitUniforms()
        {
            Projection = Parent.GetUniformMat4("projectionMatrix");
            View = Parent.GetUniformMat4("viewMatrix");
            Transform = Parent.GetUniformMat4("transformationMatrix");
        }

        public override void UpdateGlobalUniforms()
        {
            Projection?.Update(SharpCraft.Instance.Camera.Projection);
            View?.Update(SharpCraft.Instance.Camera.View);
        }

        public override void UpdateInstanceUniforms(Matrix4 transform, T renderable)
        {
            UpdateGlobalUniforms();
            Transform?.Update(transform);
        }
    }
}