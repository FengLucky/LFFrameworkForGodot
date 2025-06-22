using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using Cysharp.Threading.Tasks;
using GDLog;
using Luban;

namespace Config;

public partial class Tables
{
    private static bool _watched = false;
    private static FileSystemWatcher _watcher;
    private static readonly List<string> ChangedList = new();

    private static readonly string BytePathRoot = "res://Data/Bin";
    private static readonly string JsonPathRoot = "res://Data/Json";

    private static bool _inited = false;

    public static void EditorInit()
    {
        if (!_inited)
        {
            LoadTables(false);
        }
    }

    public static void LoadTables(bool watchChange = true)
    {
        var tablesCtor = typeof(Tables).GetMethod("LoadData", BindingFlags.NonPublic | BindingFlags.Static);
        var loaderReturnType = tablesCtor.GetParameters()[0].ParameterType.GetGenericArguments()[1];
        // 根据 Tables 的构造函数的Loader的返回值类型决定使用json还是ByteBuf Loader
        System.Delegate loader = loaderReturnType == typeof(ByteBuf)
            ? new System.Func<string, ByteBuf>(LoadByteBuf)
            : new System.Func<string, JsonNode>(LoadJson);
        tablesCtor.Invoke(null, new object[] { loader });
#if TOOLS
            _inited = true;
            if (watchChange)
            {
                if (!_watched)
                {
                    if (loaderReturnType == typeof(ByteBuf))
                    {
                        _watcher = new(BytePathRoot);
                        _watcher.Filter = "*.bytes";
                        _watcher.Changed += ByteChanged;
                    }
                    else
                    {
                        _watcher = new(JsonPathRoot);
                        _watcher.Filter = "*.json";
                        _watcher.Changed += JsonChanged;
                    }

                    _watcher.IncludeSubdirectories = true;
                    _watcher.EnableRaisingEvents = true;
                    _watched = true;
                }
            }
#endif
    }

    private static JsonNode LoadJson(string file)
    {
        var path = Path.Combine(JsonPathRoot, file + ".json");
#if TOOLS
        var json = File.ReadAllText(path);
#else
        var handle = YooAssets.LoadAssetSync<TextAsset>(path);
        var json = handle.GetAssetObject<TextAsset>().text;
        handle.Release();
#endif
        return JsonNode.Parse(json);
    }

    private static ByteBuf LoadByteBuf(string file)
    {
        var path = Path.Combine(BytePathRoot, file + ".bytes");
#if TOOLS
        var bytes = File.ReadAllBytes(path);
#else
        var handle = YooAssets.LoadAssetSync<TextAsset>(path);
        var bytes = handle.GetAssetObject<TextAsset>().bytes;
        handle.Release();
#endif
        return new ByteBuf(bytes);
    }

    private static void JsonChanged(object sender, FileSystemEventArgs e)
    {
        var name = e.Name.Replace(".json", "");
        ApplyChange(name);
    }

    private static void ByteChanged(object sender, FileSystemEventArgs e)
    {
        var name = e.Name.Replace(".bytes", "");
        ApplyChange(name);
    }

    private static async void ApplyChange(string name)
    {
#if TOOLS
            await UniTask.SwitchToMainThread();
            ChangedList.Add(name);
            int count = ChangedList.Count;
            await UniTask.WaitForSeconds(0.1f);
            if (count == ChangedList.Count)
            {
                var distinct = ChangedList.Distinct().ToList();
                var sb = new StringBuilder();
                foreach (var file in distinct)
                {
                    sb.AppendLine($"{file}、");
                }

                sb.Remove(sb.Length - 1, 1);
                GLog.DebugInfo($"配置文件更新:{'{'}{sb}{'}'}");
                IncrementalUpdate(distinct);
                ChangedList.Clear();
            }
#endif
    }
}