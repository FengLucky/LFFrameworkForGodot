using Config;
using Cysharp.Threading.Tasks;

namespace LF;

public class LFInitializationParam
{
    public string YooAssetPackageName { get; set; } = "Res";
}

public static class LFFramework
{
    public static async UniTask Initialization(LFInitializationParam param = null)
    {
        param ??= new LFInitializationParam();
        await InitPackage(param.YooAssetPackageName);
        Tables.LoadTables();
        Localization.Init();
    }

    private static async UniTask InitPackage(string packageName)
    {

    }
}