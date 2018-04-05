using OpenTK;
using SharpCraft.render.shader;
using SharpCraft.render.shader.shaders;
using SharpCraft.texture;

namespace SharpCraft.gui
{
    internal class GuiCrosshair : Gui
    {
        private static GuiTexture _texture;

        public GuiCrosshair()
        {
            var texture = TextureManager.LoadTexture("gui/cross");

            _texture = new GuiTexture(texture, Vector2.Zero, new Vector2(texture.textureSize.Width, texture.textureSize.Height), 1.4f);
        }

        public override void Render(int mouseX, int mouseY)
        {
            RenderTexture(_texture, SharpCraft.Instance.ClientSize.Width / 2, SharpCraft.Instance.ClientSize.Height / 2, 0, 0, _texture.textureSize.Width, _texture.textureSize.Height, 1.25f, true);
        }
    }
}