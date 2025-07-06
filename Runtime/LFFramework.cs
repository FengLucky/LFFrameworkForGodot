using System;
using Config;
using Cysharp.Threading.Tasks;
using GDLog;
using Godot;

namespace LF;

public class LFInitializationParam
{
    public string ThemePath;
}

public static class LFFramework
{
    public static async UniTask<bool> Initialization(LFInitializationParam param = null)
    {
        var hasError = false;
        param ??= new LFInitializationParam();
        hasError |= await InitPackage();
        hasError |= InitLog();
        hasError |= InitTables();
        hasError |= await InitPageManager(param);
        hasError |= InitLocalization();

        return hasError;
    }

    private static UniTask<bool> InitPackage()
    {
        return UniTask.FromResult(true);
    }

    private static bool InitLog()
    {
        try
        {
            var fileLogAgent = new FileLogAgent(); 
            fileLogAgent.Cleanup();
            GLog.AddAgent(fileLogAgent);

            if (EngineDebugger.IsActive())
            {
                var debuggerLogAgent = new DebuggerLogAgent(); 
                GLog.AddAgent(debuggerLogAgent);
            }
            var godotLogAgent = new GodotLogAgent();
            GLog.AddAgent(godotLogAgent);

            var builtinLogAgent = new BuiltinLogAgent(); 
            GLog.AddAgent(builtinLogAgent);
        }
        catch (Exception e)
        {
            GD.PrintErr($"GLog 初始化失败:{e.Message}");
        }

        return false;
    }

    private static bool InitTables()
    {
        try
        {
            Tables.LoadTables();
            return true;
        }
        catch (Exception e)
        {
            GLog.Error($"表格数据初始化失败:{e.Message}");
        }

        return false;
    }

    private static async UniTask<bool> InitPageManager(LFInitializationParam param)
    {
        try
        {
            PageManager.Instantiate();
           
        }
        catch (Exception e)
        {
            GLog.Error($"界面管理器初始化失败:{e.Message}");
            return false;
        }

        if (param.ThemePath.IsNotNullOrWhiteSpace())
        {
            try
            {
                await PageManager.Instance.SetTheme(param.ThemePath);
            }
            catch (Exception e)
            {
                GLog.Error($"设置界面主题失败:{e.Message}");
                return false;
            }
        }

        return true;
    }

    private static bool InitLocalization()
    {
        try
        {
            Localization.Init();
            return true;
        }
        catch (Exception e)
        {
            GLog.Error($"本地化初始化失败:{e.Message}");
        }
        return false;
    }
}