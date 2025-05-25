using UnityEngine;

namespace LF.Runtime
{
    public static class GameObjectExpansion
    {
        public static void Show(this GameObject obj)
        {
            SafeSetActive(obj, true);
        }
    
        public static void Hide(this GameObject obj)
        {
            SafeSetActive(obj, false);
        }
    
        public static void InvertVisible(this GameObject obj)
        {
            if (obj)
            {
                obj.SetActive(!obj.activeSelf);
            }
        }
        
        public static void SafeSetActive(this GameObject obj, bool active)
        {
            if (obj && obj.activeSelf != active)
            {
                obj.SetActive(active);
            }
        }
    }
}