using UnityEngine;
using UnityEngine.UI;

namespace LF.Runtime
{
    public class UISlider:Slider
    {
        [SerializeField]
        private UIText text;
        [SerializeField]
        private string intTextFormat = "{0}/{1}";
        [SerializeField]
        private string floatTextFormat = "{0:F1}/{1:F1}";
        
        protected override void Awake()
        {
            base.Awake();
            if (text)
            {
                onValueChanged.AddListener(OnValueChanged);
            }
        }

        public void SetRange(float min,float max,bool fixValue = true)
        {
            minValue = min;
            maxValue = max;
            if (fixValue)
            {
                var v = Mathf.Min(value, max);
                if (Mathf.Approximately(v, value))
                {
                    OnValueChanged(v);
                }
                else
                {
                    value = v;
                }
            }
        }

        private void OnValueChanged(float v)
        {
            if (wholeNumbers)
            {
                text.SetText(string.Format(intTextFormat,(int)v,(int)maxValue));
            }
            else
            {
                text.SetText(string.Format(floatTextFormat,v,maxValue));
            }
        }
    }
}