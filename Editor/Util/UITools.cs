using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LF.Editor
{
    public static class UITools
    {
        [MenuItem("GameObject/清除子物体光线投射目标(RayCastTarget)",priority = int.MinValue)]
        private static void ClearRayCastTarget(MenuCommand menuCommand)
        {
            if (menuCommand.context is not GameObject gameObject)
            {
                return;
            }
            
            foreach (var graphic in gameObject.GetComponentsInChildren<Graphic>(true))
            {
                graphic.raycastTarget = false;
            }
            
            EditorUtility.SetDirty(gameObject);
        }
    }
}