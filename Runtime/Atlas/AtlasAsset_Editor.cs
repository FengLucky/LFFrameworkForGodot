using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace LF.Runtime
{
    public partial class AtlasAsset
    {
        [Button("刷新图集")]
        public void RefreshSprites()
        {
            if (sprites == null)
            {
                sprites = new List<KeyValueNgc<string, Sprite>>();
            }
            sprites.Clear();
            if (texture)
            {
                var allAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture));
                foreach (var asset in allAssets)
                {
                    if (asset is not Sprite sprite)
                    {
                        continue;
                    }
                    sprites.Add(new KeyValueNgc<string, Sprite>(sprite.name,sprite));
                }
            }
            
            sprites.Sort((a,b)=>string.Compare(a.Key, b.Key, StringComparison.Ordinal));
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }

        public void SetTexture(Texture2D texture2D)
        {
            texture = texture2D;
        }
    }
}
#endif