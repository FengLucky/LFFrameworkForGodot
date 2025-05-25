using Config;
using TMPro;
using UnityEngine;

namespace LF.Runtime
{
    public class UIText:TextMeshProUGUI
    {
        [SerializeField]
        private int localizationId;
        [SerializeField]
        private bool autoRefreshOnLanguageChanged;

        private object[] _param;

        protected override void Awake()
        {
            base.Awake();
            RefreshText();
            if (autoRefreshOnLanguageChanged)
            {
                Localization.OnLanguageChanged += OnLanguageChanged;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (autoRefreshOnLanguageChanged)
            {
                Localization.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        public void SetText(int textId,params object[] param)
        {
            _param = param;
            localizationId = textId;
            RefreshText();
        }

        public new void SetText(string content)
        {
            localizationId = 0;
            base.SetText(content);
        }
        
        public void RefreshText()
        {
            if (localizationId > 0)
            {
                base.SetText(Localization.GetText(localizationId, _param));
            }
        }

        private void OnLanguageChanged(LanguageType odlType,LanguageType newType)
        {
            RefreshText();
        }
    }
}