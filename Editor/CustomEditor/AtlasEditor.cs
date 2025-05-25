using LF.Runtime;
using UnityEditor;
using UnityEngine;

namespace LF.Editor
{
    public static class AtlasEditor
    {
        [MenuItem("Assets/图集/创建或刷新图集")]
        public static void CreateAtlas()
        {
            foreach (var obj in Selection.objects)
            {
                if (obj is Texture2D texture)
                {
                    CreateOrGetAtlas(texture.name, texture).RefreshSprites();
                }
            }
        }
        [MenuItem("Assets/图集/创建或刷新图集",true)]
        public static bool CreateAtlasCheck()
        {
            foreach (var obj in Selection.objects)
            {
                if (obj is Texture2D)
                {
                    return true;
                }
            }
            return false;
        }

        private static AtlasAsset CreateOrGetAtlas(string name,Texture2D texture)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Res/Atlas"))
            {
                AssetDatabase.CreateFolder("Assets/Res", "Atlas");   
            }
            
            var atlas = AssetDatabase.LoadAssetAtPath<AtlasAsset>($"Assets/Res/Atlas/{name}.asset");
            if (atlas == null)
            {
                atlas = ScriptableObject.CreateInstance<AtlasAsset>();
                atlas.name = name;
                atlas.SetTexture(texture);
                AssetDatabase.CreateAsset(atlas, $"Assets/Res/Atlas/{name}.asset");
            }

            return atlas;
        }
    }
}