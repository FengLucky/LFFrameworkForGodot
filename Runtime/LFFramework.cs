using Config;
using Cysharp.Threading.Tasks;
using GDLog;
using Godot;

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
        InitLog();
        Tables.LoadTables();
        PageManager.Instantiate();
        Localization.Init();
    }

    private static async UniTask InitPackage(string packageName)
    {

    }

    private static void InitLog()
    {
        var fileLogAgent = new FileLogAgent(); // File Logging
        fileLogAgent.Cleanup(2); // Keep 2 log files, delete the rest
        GLog.AddAgent(fileLogAgent);

        if (EngineDebugger.IsActive())
        {
            var debuggerLogAgent = new DebuggerLogAgent(); // Output log information to Godot's debugger panel
            GLog.AddAgent(debuggerLogAgent);
        }
        var godotLogAgent = new GodotLogAgent(); // Output log information to Godot's output panel
        GLog.AddAgent(godotLogAgent);

        var builtinLogAgent = new BuiltinLogAgent(); // Built-in Logging
        GLog.AddAgent(builtinLogAgent);
    }
}