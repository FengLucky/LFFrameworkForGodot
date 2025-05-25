using UnityEngine;

namespace LF.Runtime
{
    public static class MonoBehaviourExpansion
    {
        public static void Show(this MonoBehaviour obj)
        {
            SafeSetActive(obj,true);
        }
    
        public static void Hide(this MonoBehaviour obj)
        {
            SafeSetActive(obj,false);
        }
        
        public static void InvertVisible(this MonoBehaviour obj)
        {
            if (obj)
            {
                obj.gameObject.SetActive(!obj.gameObject.activeSelf);
            }
        }
        public static void SafeSetActive(this MonoBehaviour obj, bool active)
        {
            if (obj && obj.gameObject.activeSelf != active)
            {
                obj.gameObject.SetActive(active);
            }
        }
    }
}