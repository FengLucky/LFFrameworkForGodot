using System;
using Config;

namespace LF.Runtime
{
    public sealed class Localization
    {
        public static LanguageType CurrentLanguage { get; private set; } = LanguageType.Chinese;
        public static LanguageType DefaultLanguage { get; private set; } = LanguageType.Chinese;

        public static event Action<LanguageType,LanguageType> OnLanguageChanged;
        
        public static void Init(LanguageType type = LanguageType.Chinese,LanguageType defaultType = LanguageType.Chinese)
        {
            CurrentLanguage = type;
            DefaultLanguage = defaultType;
        }
        
        public static void ChangeLanguage(LanguageType type)
        {
            var old = CurrentLanguage;
            CurrentLanguage = type;
            OnLanguageChanged.SafeInvoke(old,type);
        }
        
        public static string GetText(int textId,params object[] param)
        {
            #if UNITY_EDITOR
            Tables.EditorInit();
            #endif
            return Tables.LanguageTable.GetText(textId, CurrentLanguage, DefaultLanguage, param);
        }
        
        
    }
}