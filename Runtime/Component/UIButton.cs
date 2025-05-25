using System;
using UnityEngine.UI;

namespace LF.Runtime
{
    public sealed class UIButton:Button
    {
        /// <summary>
        /// 可交互点击回调
        /// </summary>
        public Action OnActiveClick { get; set; }
    
        /// <summary>
        /// 不可交互点击回调
        /// </summary>
        public Action OnInActiveClick { get; set; }

        /// <summary>
        /// 是否可交互
        /// </summary>
        public bool Active { get; set; } = true;  
    
        protected override void Awake()
        {
            base.Awake();
            onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            if (Active)
            {
                OnActiveClick?.Invoke();
            }
            else
            {
                OnInActiveClick?.Invoke();
            }
        }
    }
}