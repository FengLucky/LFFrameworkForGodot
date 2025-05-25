using UnityEditor;

namespace LF.Editor
{
    public class ResTools
    {
        [MenuItem("Assets/资源/复制资源路径")]
        private static void CopyAssetPath()
        {
            if (Selection.gameObjects.Length == 0)
            {
                return;
            }
            
            EditorGUIUtility.systemCopyBuffer = AssetDatabase.GetAssetPath(Selection.gameObjects[0]);
        }
    }
}