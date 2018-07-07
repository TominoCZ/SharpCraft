using SharpCraft.texture;

namespace SharpCraft.gui
{
    public struct FontMapCharacter
    {
        public readonly FontMapCharacterNode Character;
        public readonly TextureUvNode TextureUv;
        public readonly bool HasValue;

        public FontMapCharacter(FontMapCharacterNode character, TextureUvNode textureUv)
        {
            Character = character;
            TextureUv = textureUv;
            HasValue = true;
        }
    }
}