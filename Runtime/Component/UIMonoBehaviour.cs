using UnityEngine;

namespace LF.Runtime
{
    public class UIMonoBehaviour:MonoBehaviourEx
    {
        public RectTransform RectTransform => transform as RectTransform;
    }
}