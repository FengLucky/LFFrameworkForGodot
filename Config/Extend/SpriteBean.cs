using System;

namespace Config
{
    public partial struct SpriteBean:IEquatable<SpriteBean>
    {
        public bool Equals(SpriteBean other)
        {
            return Atlas == other.Atlas && Sprite == other.Sprite;
        }

        public override bool Equals(object obj)
        {
            return obj is SpriteBean other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Atlas, Sprite);
        }
        
        public static bool operator ==(SpriteBean left, SpriteBean right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(SpriteBean left, SpriteBean right)
        {
            return !left.Equals(right);
        }

        private string _atlasPath;
        public string GetAtlasPath()
        {
#if !UNITY_EDITOR
            if (_atlasPath == null)
#endif
            {
                _atlasPath = $"Assets/Res/Atlas/{Atlas}";
            }

            return _atlasPath;
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Atlas) && !string.IsNullOrWhiteSpace(Sprite);
        }
    }
}