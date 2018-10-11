using SharpCraft_Client.texture;

namespace SharpCraft_Client.gui
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