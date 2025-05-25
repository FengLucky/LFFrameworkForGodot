using LF.Runtime;

namespace Config
{
    public partial class LanguageBean
    {
        public string GetText(params object[] param)
        {
            return Localization.GetText(Id, param);
        }
    }
}