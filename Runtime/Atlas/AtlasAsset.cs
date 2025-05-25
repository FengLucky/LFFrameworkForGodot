using System.Collections.Generic;
using UnityEngine;

namespace LF.Runtime
{
    public partial class AtlasAsset:ScriptableObject,ISerializationCallbackReceiver
    {
        [Label("图集")]
        [SerializeField]
        private Texture2D texture;
        [Label("精灵图")]
        [SerializeField]
        private List<KeyValueNgc<string,Sprite>> sprites;
        
        private readonly Dictionary<string, Sprite> _dict = new();
        
        public  Sprite GetSprite(string spriteName)
        {
            return _dict.GetValueOrDefault(spriteName);
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            _dict.Clear();
            foreach (var kv in sprites)
            {
                if (!_dict.TryAdd(kv.Key, kv.Value))
                {
                    Debug.LogError($"图集 {texture.name} 中存在同名 sprite {kv.Key}");
                }
            }
        }
    }
}