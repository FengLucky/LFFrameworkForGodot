using System;
using LF.Runtime;
using UnityEngine;

namespace Config
{
    public partial class LanguageTable
    {
        public string GetText(int textId,LanguageType type,LanguageType defaultType,params object[] param)
        {
            var bean = GetOrDefault(textId);
            if (bean == null)
            {
                return $"{type}_{textId}";
            }

            var originalText = GetOriginalText(bean, type, defaultType);
            if (originalText.IsNullOrWhiteSpace())
            {
                return $"{type}_{textId}";
            }
            if (param.IsNullOrEmpty())
            {
                return originalText;
            }

            try
            {
                var result = string.Format(originalText, param);
                return result;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return originalText;
            }
        }

        private string GetOriginalText(LanguageBean bean, LanguageType type, LanguageType defaultType)
        {
            var text = GetOriginalText(bean,type);
            return text.IsNullOrWhiteSpace() ? GetOriginalText(bean,defaultType) : text;
        }
        
        private string GetOriginalText(LanguageBean bean, LanguageType type)
        {
            switch (type)
            {
                case LanguageType.Chinese:
                    return bean.Chinese;
            }
            return String.Empty;
        }
    }
}