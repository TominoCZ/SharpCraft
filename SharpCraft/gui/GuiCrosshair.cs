using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.texture;
using SharpCraft.util;

namespace SharpCraft.gui
{
    internal class GuiCrosshair : Gui
    {
        private static GuiTexture _texture;

        public GuiCrosshair()
        {
            Texture texture = TextureManager.LoadTexture("gui/cross");

            _texture = new GuiTexture(texture, Vector2.Zero, new Vector2(texture.TextureSize.Width, texture.TextureSize.Height), 1.4f);
        }

        public override void Render(int mouseX, int mouseY)
        {
            GL.BlendFunc(BlendingFactor.OneMinusDstColor, BlendingFactor.OneMinusSrcColor);
            RenderTexture(_texture, SharpCraft.Instance.ClientSize.Width / 2, SharpCraft.Instance.ClientSize.Height / 2, 0, 0, _texture.TextureSize.Width, _texture.TextureSize.Height, 1.25f, true);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }
    }
}