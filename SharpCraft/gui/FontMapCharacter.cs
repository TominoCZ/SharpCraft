using SharpCraft.texture;

namespace SharpCraft.gui
{
    internal class FontMapCharacter
    {
        public readonly FontMapCharacterNode Character;
        public readonly TextureUVNode TextureUv;

        public FontMapCharacter(FontMapCharacterNode character, TextureUVNode textureUv)
        {
            Character = character;
            TextureUv = textureUv;
        }
    }
}